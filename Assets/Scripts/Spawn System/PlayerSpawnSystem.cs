using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerSpawnSystem : NetworkBehaviour
{
    //[SerializeField] private List<GameObject> playerPrefab = new List<GameObject>();
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiTankPrefab;
    private bool _allPlayersSpawned;
    private bool isCoroutineExecuting = false;

    private MyNetworkManager _networkManager;

    private static List<Transform> _spawnPoints = new List<Transform>();

    public static void AddSpawnPoint(Transform transform) //adds spawn points to the list, called from the actual spawn point
    {
        _spawnPoints.Add(transform);
        _spawnPoints = _spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList(); //makes sure the list is ordered
    }

    public static void RemoveSpawnPoint(Transform transform) => _spawnPoints.Remove(transform); //removed spawn points form the list, called from the spawn point

    public override void OnStartServer() => MyNetworkManager.OnServerReadied += SpawnPlayer; // once the server is ready, spawn players

    [ServerCallback]
    private void OnDestroy() => MyNetworkManager.OnServerReadied -= SpawnPlayer; //unsubscribe from the event when game object is destroyed


    [ServerCallback]
    private void Awake()
    {
        _networkManager = GameObject.FindWithTag("NetworkManager").GetComponent<MyNetworkManager>();
    }

    [ServerCallback]
    private void Update()
    {
        if (_allPlayersSpawned) return;
        _allPlayersSpawned = AllPlayersSpawned();
        if (_allPlayersSpawned)
        {
            Debug.Log("Spawn AI");
            //SpawnAiTanks();
            StartCoroutine(SpawnAiTanks());
            //StartCoroutine(ExecuteAfterTime(0.01f));
        }
    }

    private bool AllPlayersSpawned()
    {
        //Returns true if all spawned variables are true in the list
        return _networkManager.GamePlayers.All(gamePlayer => gamePlayer.spawned);
    }

    
    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        //_networkManager = GameObject.FindWithTag("NetworkManager").GetComponent<MyNetworkManager>();
        foreach (var networkGamePlayer in _networkManager.GamePlayers)
        {
            if (networkGamePlayer.netIdentity != conn.identity) continue;
            foreach (var point in _spawnPoints)
            {
                if (point.GetComponent<PlayerSpawnPoint>().team != networkGamePlayer.team) continue;
                if (point.GetComponent<PlayerSpawnPoint>().alreadyUsed) continue;
                point.GetComponent<PlayerSpawnPoint>().alreadyUsed = true;
                var playerInstance = Instantiate(playerPrefab,point.position,point.rotation);

                NetworkServer.Spawn(playerInstance, conn);
                if(_networkManager.gameMode == 2)
                {
                    playerInstance.GetComponent<TankController>().RpcSetTankInfo(networkGamePlayer.team, _networkManager.teamColors[networkGamePlayer.lobbySlot]);
                }
                else
                {
                    playerInstance.GetComponent<TankController>().RpcSetTankInfo(networkGamePlayer.team, networkGamePlayer.colorId);
                }
                
                playerInstance.layer = playerInstance.GetComponent<TankController>().team + 5; //+5 because the team layers start at index 6
                networkGamePlayer.spawned = true;
                return;
            }
        }
    }
    
    /*public IEnumerator Testt()
    {

    }*/
    [Server]
    public IEnumerator SpawnAiTanks()
    {
        //Set up some variables
        var gameMode = 1;
        var amountToSpawn = 0;
        //Look for the leader, set the game mode to be what the leader has
        foreach (var gamePlayer in _networkManager.GamePlayers)
        {
            if (gamePlayer.isLeader) gameMode = gamePlayer.gameMode;
        }
        Debug.Log($"Game mode: {gameMode}");

        //Depending on the game mode we want to spawn different amounts of bots
        switch (gameMode)
        {
            case 1: //1v1
                amountToSpawn = 2 - _networkManager.GamePlayers.Count;
                break;
            case 2: //2v2
                amountToSpawn = 4 - _networkManager.GamePlayers.Count;
                break;
            case 3: //FFA
                amountToSpawn = 4 - _networkManager.GamePlayers.Count;
                break;
        }
        Debug.Log($"{amountToSpawn} bots to spawn");
        //If we dont have any bots to spawn then cancel out of this function
        if (amountToSpawn >= 1)
        {
            
            for (var i = 0; i < amountToSpawn; i++)
            {
                //Setup some Variables
                var team = 1;
                //Slot means which slot the bot would have filled in the lobby to calculate which team it should join
                //5 because we subtract the amount of bots to spawn subtracted by which iteration we are on
                //Example, we need 2 bots, 5-(2-0) = 3 AND 5-(2-1) = 4 so we get slot 3 and 4 respectively
                var slot = 5 - (amountToSpawn - i);
                switch (gameMode)
                {
                    case 1: //1v1
                        team = 2;
                        break;
                    case 2: //2v2
                        team = slot == 2 ? 1 : 2;
                        break;
                    case 3: //FFA
                        team = slot;
                        break;
                }

                //Go through all the spawn points
                foreach (var point in _spawnPoints)
                {
                    var spawnPoint = point.GetComponent<PlayerSpawnPoint>();
                    //If the spawn point is not on our team go to next one
                    if (spawnPoint.team != team) continue;
                    //If the spawn point is already being used go to next one
                    if (spawnPoint.alreadyUsed) continue;
                    spawnPoint.alreadyUsed = true;

                    //Find all the spawned tanks and find a color id that is free to use for the bot
                    yield return new WaitForSeconds(0.1f);
                    var spawnedTanks = GameObject.FindGameObjectsWithTag("Player");
                    var colorId = FindFreeColorID(spawnedTanks);

                    //Spawn the ai tank on the right position, with the right team, color and layer.
                    var aiTankInstance = Instantiate(aiTankPrefab, point.position, point.rotation);
                    NetworkServer.Spawn(aiTankInstance);
                    //aiTankInstance.GetComponent<TankController>().RpcSetTankInfo(team, colorId);
                    if (gameMode == 2)
                    {
                        aiTankInstance.GetComponent<TankController>().RpcSetTankInfo(team, _networkManager.teamColors[slot - 1]);
                        aiTankInstance.GetComponent<TankController>()._tankDisplayName = "Tank" + slot.ToString();
                        //aiTankInstance.GetComponent<TankController>().RpcSetTankInfo(team, team - 1);
                    }
                    else
                    {
                        aiTankInstance.GetComponent<TankController>().RpcSetTankInfo(team, colorId);
                        aiTankInstance.GetComponent<TankController>()._tankDisplayName = "Tank" + team.ToString();
                    }

                    aiTankInstance.layer = team + 5;
                    //yield return new WaitForSeconds(0.1f);
                    //colorId = FindFreeColorID(spawnedTanks);
                    //StartCoroutine(ExecuteAfterTime(0.2f, spawnedTanks));
                    break;
                }
            }
        }
        
        //Actual spawn loop
        
    }


    //[Server]
    /*IEnumerator ExecuteAfterTime(float time, GameObject[] players)
    {
        if (isCoroutineExecuting)
        {
            //SpawnAiTanks();
            FindFreeColorID(players);
            yield break;
        }



        isCoroutineExecuting = true;

        yield return new WaitForSeconds(time);

        // Code to execute after the delay

        isCoroutineExecuting = false;
    }*/

    private int FindFreeColorID(GameObject[] players)
    {
        //Literally brainfreezed and put in this ugly unoptimized shit cause I couldn't figure out how to do it better
        var id0 = false;
        foreach (var player in players)
        {
            if (player.GetComponent<TankController>().colorId == 0) id0 = true;
        }
        if (!id0) return 0;
        
        var id1 = false;
        foreach (var player in players)
        {
            if (player.GetComponent<TankController>().colorId == 1) id1 = true;
        }
        if (!id1) return 1;
        
        var id2 = false;
        foreach (var player in players)
        {
            if (player.GetComponent<TankController>().colorId == 2) id2 = true;
        }
        if (!id2) return 2;
        
        var id3 = false;
        foreach (var player in players)
        {
            if (player.GetComponent<TankController>().colorId == 3) id3 = true;
        }
        if (!id3) return 3;
        
        var id4 = false;
        foreach (var player in players)
        {
            if (player.GetComponent<TankController>().colorId == 4) id4 = true;
        }
        if (!id4) return 4;

        //If something goes wrong
        return 0;
    }
    
    
    
    [ClientRpc]
    private void SetTankStuff(TankController tankController, int gamePlayerTeam)
    {
        /*foreach (var render in renderers)
        {
            render.material.color = tankController.colorArray[networkColorId];
        }*/

        tankController.team = gamePlayerTeam;
    }

    [ClientRpc]
    private void SetTankColor(TankController tankController, int networkColorId)
    {
        //tankController.gameObject.GetComponent<Ren>().material.color = tankController.colorArray[networkColorId];
        var renderers = tankController.gameObject.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            render.material.color = tankController.colorArray[networkColorId];
        }
    }

    

    [ClientRpc]
    private void SetTankInfo(TankController tankController, NetworkGamePlayer gamePlayer)
    {
        //var tankController = playerInstance.GetComponent<TankController>();
        var renderers = tankController.gameObject.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            render.material.color = tankController.colorArray[gamePlayer.colorId];
        }
        tankController.team = gamePlayer.team;
    }
}