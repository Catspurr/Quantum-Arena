using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
//using System.Linq;

public class GameManager : NetworkBehaviour
{
    [Header("General game settings")]
    [SerializeField] private int secondsBeforeGameStart = 2;
    [Space]
    
    [Header("Bots settings")]
    [Range(1, 10)] public int amountOfBotsPerWave = 7;
    [Range(1, 30)] public int maxBotsPerTeamAtOnce = 20;
    public float secondsBetweenSpawningBots = 10f;
    [Space]
    [SerializeField] private AISpawnSystem aiSpawnSystem;
    [SerializeField] private AITankSpawnSystem aiTankSpawnSystem;
    [SerializeField] private ControlPoint controlPoint;
    private Color team1Color, team2Color;

    public List<GameObject> playerList = new List<GameObject>();
    public List<GameObject> turretList = new List<GameObject>();


    public GameUI gameUI;
    public WinScreenUI winScreenUI;
    /*[SyncVar(hook = nameof(HandleTimeUpdated))]
    public float time;
    public TMP_Text textTimer;*/
    public Color[] colors;
    public Bullet[] tankBullets;
    public GameObject[] tankDeathPrefabs;
    public static event Action StartGame;

    public MyNetworkManager networkManager;

    public bool TeamGame;
    public GameObject camera1, camera2, gameCanvas, resultCanvas, loadingCanvas, loadingCamera;

    //public WinScreenTank[] tankPlace1, tankPlace2, tankPlace3, tankPlace4;

    public WinScreenTank[] winScreenTanks;
    public WinScreenTank[] winScreenTanksTeam;

    [SyncVar] public bool gameIsOver;

    public float invisTime;

    [ServerCallback]
    private void Start()
    {
        //networkManager = FindSceneObjectsOfType(MyNetworkManager);
        networkManager = GameObject.FindWithTag("NetworkManager").GetComponent<MyNetworkManager>();
        //CmdFindNetworkManager();
        //if(networkManager != null) Debug.Log(networkManager.teamColors[0] + " " + networkManager.teamColors[1]);
        /*foreach(var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            go.gameObject.GetComponent<TankController>().enabled = false;
        }*/

        for (var i = 0; i < winScreenTanks.Length; i++)
        {
            if(TeamGame) winScreenTanks[i].gameObject.SetActive(false);
        }
        for (var i = 0; i < winScreenTanksTeam.Length; i++)
        {
            if(!TeamGame)winScreenTanksTeam[i].gameObject.SetActive(false);
        }
        StartCoroutine(StartDelay());
    }

    [ClientRpc]
    public void CmdFindNetworkManager()
    {
        networkManager = GameObject.FindWithTag("NetworkManager").GetComponent<MyNetworkManager>();
    }
    /*public override void OnStartAuthority()
    {

    }*/
    /*private void FixedUpdate()
    {
        time -= Time.deltaTime;
        
    }*/
    private IEnumerator StartDelay()
    {
        if (aiTankSpawnSystem != null)
        {
            aiTankSpawnSystem.FirstSpawn();
        }
        yield return new WaitForSeconds(2f);
        TurnOnGameCanvas();

        foreach (var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            playerList.Add(go); //Sends all players to a list in the ai spawn system so they have references
            //playerList.Add(go.GetComponent<TankController>()); //Sends all players to a list in the ai spawn system so they have references
            var tank = go.gameObject.GetComponent<TankController>();
            //tank.baseInput.enabled = true;
            tank.RpcSetTankInfo(tank.team, tank.colorId);
            go.gameObject.layer = tank.team + 5;
            //tank.CanWalk(true);

            //turretList.Add(turret[playerList.Count - 1]);
        }
        //yield return new WaitForSeconds(0.1f);
        //var turret = GameObject.FindGameObjectsWithTag("TowerTurret");
        
        
        
        for (var i = 0; i < turretList.Count; i++)
        {
            //turretList.Add(turret[i]);
            //turretList = turretList.OrderBy(x => x.)
            var tower = turretList[i].gameObject.GetComponent<TowerAI>();
            //turret[i].gameObject.layer = tower.team + 5;
            tower.enabled = true;

            if (TeamGame)
            {
                if(tower.team == 1)
                {
                    tower.SetTowerInfo(0);
                } else
                {
                    tower.SetTowerInfo(1);
                }
            }
            else
            {
                foreach (var player in playerList)
                {
                    var controller = player.GetComponent<TankController>();
                    if (controller.team != tower.team) continue;
                    tower.SetTowerInfo(controller.colorId);
                    break;
                }
            }
        }
        
        if (gameUI != null) gameUI.enabled = true;

        yield return new WaitForSeconds(1);
        foreach (var go in GameObject.FindGameObjectsWithTag("Player"))
        {
            var tank = go.gameObject.GetComponent<TankController>();
            tank.baseInput.enabled = true;
            tank.CanWalk(true);
        }


        if (controlPoint != null && !TeamGame && playerList.Count == 2)
        {
            if (playerList[0] != null) controlPoint.team1ColorId = playerList[0].GetComponent<TankController>().colorId;
            if (playerList[1] != null) controlPoint.team2ColorId = playerList[1].GetComponent<TankController>().colorId;
        }

        yield return new WaitForSeconds(secondsBeforeGameStart);
        if (aiSpawnSystem != null)
        {
            aiSpawnSystem.FirstSpawn();
        }
        if(controlPoint != null)
        {
            StartCoroutine(ControlPointPoints());
        }
        


        StartGame?.Invoke();
        
    }
    [ClientRpc]
    public void TurnOnGameCanvas()
    {
        if (camera1 != null) camera1.SetActive(true);
        if (loadingCamera != null) loadingCamera.SetActive(false);
        if (gameCanvas != null) gameCanvas.SetActive(true);
        if (loadingCanvas != null) loadingCanvas.SetActive(false);
    }

    public void RespawnObj(GameObject go)
    {
        if(go != null && !gameIsOver) StartCoroutine(Respawn(go));
    }

    private IEnumerator Respawn(GameObject gameObj)
    {
        yield return new WaitForSeconds(2);
        if (gameObj != null && !gameIsOver)
        {
            var tankController = gameObj.GetComponent<TankController>();
            tankController.health = 100;
            tankController.Respawn();
            StartCoroutine(RemoveInvis(gameObj));
        }
        
    }


    private IEnumerator RemoveInvis(GameObject gameObj)
    {
        yield return new WaitForSeconds(2);
        if(gameObj != null)
        {
            var tankController = gameObj.GetComponent<TankController>();
            tankController.RemoveInvis();
        }
        
    }

    private IEnumerator ControlPointPoints()
    {
        while(controlPoint != null)
        {
            yield return new WaitForSeconds(1);
            if(controlPoint != null)
            {
                if (controlPoint.amountOfTeam1 > controlPoint.amountOfTeam2)
                {
                    //gameUI.t1Score += 10;
                    gameUI.AddTeamScore(10, 1);
                    //yield return new WaitForSeconds(0.3f);
                }
                else if (controlPoint.amountOfTeam1 < controlPoint.amountOfTeam2)
                {
                    //gameUI.t2Score += 10;
                    gameUI.AddTeamScore(10, 2);
                    //yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    //Debug.Log("noleaders");
                    //yield return new WaitForSeconds(0.3f);
                }
            }
            
        }
        
    }
    [ClientRpc]
    public void ChangeScene()
    {
        gameIsOver = true;
        SetWinScreenColors();
        camera2.SetActive(true);
        camera1.SetActive(false);
        if(gameCanvas!= null) gameCanvas.SetActive(false);
        if (resultCanvas != null) resultCanvas.SetActive(true);

        foreach (var bullet in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            Destroy(bullet);
        }
        //playerList.Clear();

        if (aiSpawnSystem != null)
        {
            aiSpawnSystem.gameObject.SetActive(false);
            aiSpawnSystem = null;
        }
        if(controlPoint != null)
        {
            controlPoint = null;
        }
        
        playerList.Clear();
        foreach (var tank in GameObject.FindGameObjectsWithTag("Player"))
        {
            //Destroy(tank);
            tank.gameObject.GetComponent<TankController>().isDead = true;
            tank.gameObject.SetActive(false);
            //tank.gameObject.GetComponent<TankController>().CanWalk(false);
        }

        foreach (var bot in GameObject.FindGameObjectsWithTag("AI Bots"))
        {
            Destroy(bot);
        }
        turretList.Clear();
        foreach (var turret in GameObject.FindGameObjectsWithTag("TowerTurret"))
        {
            Destroy(turret);
        }

        if (winScreenUI != null) winScreenUI.enabled = true;
    }
    [Server]
    public void SetWinScreenColors()
    {
        if (!TeamGame)
        {
            for(var i = 0; i < playerList.Count; i++)
            {
                if(playerList[gameUI.teamScoreManagers[i].team - 1] != null)
                {
                    winScreenTanks[i].color = playerList[gameUI.teamScoreManagers[i].team - 1].GetComponent<TankController>().colorId;
                    winScreenTanks[i].tankName = playerList[gameUI.teamScoreManagers[i].team - 1].GetComponent<TankController>()._tankDisplayName;
                    winScreenTanks[i].teamScore = gameUI.teamScoreManagers[i].score;
                    winScreenTanks[i].playerScore = playerList[gameUI.teamScoreManagers[i].team - 1].GetComponent<TankController>().score;
                    winScreenTanks[i].kills = playerList[gameUI.teamScoreManagers[i].team - 1].GetComponent<TankController>().kills;
                    winScreenTanks[i].inGame = true;
                }
                
            }

            for (var i = 0; i < winScreenTanks.Length; i++)
            {
                if (!winScreenTanks[i].inGame)
                {
                    winScreenTanks[i].RemoveTank();
                    winScreenTanks[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if(gameUI.first == 1)
            {
                winScreenTanksTeam[0].color = 0;
                winScreenTanksTeam[1].color = winScreenTanksTeam[1].winColors.Length - 2;
                winScreenTanksTeam[2].color = 1;
                winScreenTanksTeam[3].color = winScreenTanksTeam[3].winColors.Length - 1;
            }
            else
            {
                winScreenTanksTeam[0].color = 1;
                winScreenTanksTeam[1].color = winScreenTanksTeam[1].winColors.Length - 1;
                winScreenTanksTeam[2].color = 0;
                winScreenTanksTeam[3].color = winScreenTanksTeam[3].winColors.Length - 2;
            }
            for (var i = 0; i < playerList.Count; i++)
            {
                if (playerList[i] != null)
                {
                    //winScreenTanks[i].color = playerList[gameUI.teamScoreManagers[i].team - 1].GetComponent<TankController>().colorId;
                    winScreenTanksTeam[i].tankName = playerList[i].GetComponent<TankController>()._tankDisplayName;
                    winScreenTanksTeam[i].teamScore = gameUI.teamScoreManagers[i].score;
                    winScreenTanksTeam[i].playerScore = playerList[i].GetComponent<TankController>().score;
                    winScreenTanksTeam[i].kills = playerList[i].GetComponent<TankController>().kills;
                    winScreenTanksTeam[i].inGame = true;
                }

            }
        }


    }
}