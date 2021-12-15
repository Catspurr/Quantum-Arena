using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MyNetworkManager : NetworkManager
{
    [Tooltip("Dont touch, specifically set for each network manager.")]
    public bool usingSteam;
    [Tooltip("Minimum amount of players allowed in the lobby before the game can be started.")]
    [SerializeField] private int minPlayers = 1;
    [Space]
    [Header("Scenes")]
    [Tooltip("The main menu scene")]
    [Scene] [SerializeField] private string menuScene = string.Empty;
    [Tooltip("The game scene (MUST CONTAIN 'Game' IN THE NAME), This will be changed later to instead be selected from a list of scenes for the specific game mode")]
    [Scene] [SerializeField] private string gameScene = string.Empty;
    [Space]
    [Header("Lobby Room")] 
    public NetworkLobbyRoomPlayer lobbyRoomPlayerPrefab;
    [Space]
    [Header("Game Room")] 
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab;
    [SerializeField] private GameObject playerSpawnSystem;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    public int gameMode;

    public int[] teamColors;

    [Space][Header("Maps")]
    [Scene][SerializeField] private List<string> oneVersusOneMaps = new List<string>();
    [Scene][SerializeField] private List<string> twoVersusTwoMaps = new List<string>();
    [Scene][SerializeField] private List<string> freeForAllMaps = new List<string>();
    [Scene][SerializeField] private List<string> oneVersusOneRobotMaps = new List<string>();
    [Scene][SerializeField] private List<string> twoVersusTwoRobotMaps = new List<string>();

    [Space][Header("Music")] 
    [SerializeField] private List<AudioClip> menuMusic = new List<AudioClip>();
    [SerializeField] private List<AudioClip> gameMusic = new List<AudioClip>();
    private AudioSource _musicSource;


    public List<NetworkLobbyRoomPlayer> RoomPlayers { get; } = 
        new List<NetworkLobbyRoomPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = 
        new List<NetworkGamePlayer>();

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        OnClientConnected?.Invoke();
    } //Does the base functions plus invokes an event whenever a client connects to the server client side

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke();
        try
        {
            _musicSource = GameObject.FindWithTag("Persistent Sound").GetComponent<AudioSource>();
        }
        catch
        {
            _musicSource = null;
        }
        
        if (_musicSource != null && menuMusic != null)
        {
            _musicSource.clip = menuMusic[Random.Range(0, menuMusic.Count)];
            _musicSource.volume = 1f;
            _musicSource.Play();
        }
    } //Does the base functions plus invokes an event whenever a client disconnects to the server client side

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        } //Disconnects the new connection if the current amount of connections is the same or more than the max amount of connections

        if (SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        } //Disconnects the connection if they arent on the correct scene
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            var isLeader = RoomPlayers.Count == 0;
            var roomPlayerInstance = Instantiate(lobbyRoomPlayerPrefab);
            roomPlayerInstance.IsLeader = isLeader; //sets the roomPlayer (for lobby) to be leader or not depending on their place in the list. 0 = leader
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject); //adds the instanced game object to the current connection
            
            if (!usingSteam) return;
            var steamID = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyId, numPlayers - 1);
            conn.identity.GetComponent<NetworkLobbyRoomPlayer>().steamId = steamID.m_SteamID;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkLobbyRoomPlayer>();

            RoomPlayers.Remove(player); //removes the player from the list

            NotifyPlayersOfReadyState(); //Updates all client but only runs on the leader client to enable or disable the start button
        }
        
        base.OnServerDisconnect(conn); //Runs the base function of disconnecting
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear(); //clears the list to prevent overlapping
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    /*public void NotifyPlayersOfColorChange()
    {
        foreach (var player in RoomPlayers)
        {
            player.CmdSetColor()
        }
    }*/

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) return false; //if there is not enough players we are not ready to start
        foreach (var player in RoomPlayers)
        {
            if (!player.isReady) return false;
        } //if any player is not ready we are not ready

        return true; //if we have enough players and all players are ready then we are ready
    }

    public void StartGame(int gMode, bool useRobots)
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) return;
            gameMode = gMode;

            switch (gameMode)
            {
                case 1: //1v1
                    gameScene = useRobots ? 
                        oneVersusOneRobotMaps[Random.Range(0, oneVersusOneRobotMaps.Count)] : 
                        oneVersusOneMaps[Random.Range(0, oneVersusOneMaps.Count)];
                    break;
                case 2: //2v2
                    gameScene = useRobots ?
                        twoVersusTwoRobotMaps[Random.Range(0, twoVersusTwoRobotMaps.Count)] :
                        twoVersusTwoMaps[Random.Range(0, twoVersusTwoMaps.Count)];
                    break;
                case 3: //FFA
                    gameScene = freeForAllMaps[Random.Range(0, freeForAllMaps.Count)];
                    break;
                default:
                    Debug.Log($"{gameMode} is not a valid game mode.");
                    break;
            }
            
            
            
            ServerChangeScene(gameScene); //Change for the proper scene
            //Calls the server to change the scene into the game scene if we are on the right scene and all players are ready
        }
    }

    public override void ServerChangeScene(string sceneName)
    {
        Debug.Log(sceneName);
        //if we move from lobby to a game scene
        if (SceneManager.GetActiveScene().path == menuScene && sceneName.Contains("Game"))
        {
            for (var i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var gamePlayerInstance = Instantiate(gamePlayerPrefab); //Instantiates the game player
                if(gameMode == 2)
                {
                    gamePlayerInstance.SetUpTank(
                        RoomPlayers[i].displayName,
                        RoomPlayers[i].team - 1,
                        RoomPlayers[i].team,
                        RoomPlayers[i].tankType,
                        RoomPlayers[i].isLeader,
                        RoomPlayers[i].gameMode,
                        RoomPlayers[i].lobbySlot);


                    var conn = RoomPlayers[i].connectionToClient;
                    NetworkServer.Destroy(conn.identity.gameObject); //removes the room player
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject); //sets the instantiated game player to be the new game object for the client

                    
                } else
                {
                    //Pass in info from room player into the game player here!
                    gamePlayerInstance.SetUpTank(
                        RoomPlayers[i].displayName,
                        RoomPlayers[i].colorId,
                        RoomPlayers[i].team,
                        RoomPlayers[i].tankType,
                        RoomPlayers[i].isLeader,
                        RoomPlayers[i].gameMode,
                        RoomPlayers[i].lobbySlot);


                    var conn = RoomPlayers[i].connectionToClient;
                    NetworkServer.Destroy(conn.identity.gameObject); //removes the room player
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject); //sets the instantiated game player to be the new game object for the client
                }
                


            }
        }
        base.ServerChangeScene(sceneName); //runs the base function after creating the new game player
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        try
        {
            _musicSource = GameObject.FindWithTag("Persistent Sound").GetComponent<AudioSource>();
        }
        catch
        {
            _musicSource = null;
        }
        if (SceneManager.GetActiveScene().name.Contains("Game"))
        {
            if (_musicSource != null && gameMusic != null)
            {
                _musicSource.clip = gameMusic[Random.Range(0, gameMusic.Count)];
                _musicSource.volume = 0.5f;
                _musicSource.Play();
                return;
            }
        }
        if (_musicSource != null && menuMusic != null)
        {
            _musicSource.clip = menuMusic[Random.Range(0, menuMusic.Count)];
            _musicSource.volume = 1f;
            _musicSource.Play();
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.Contains("Game"))
        {
            var playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance); //Instantiates and spawns the spawn system to spawn players
        }
    }


    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        OnServerReadied?.Invoke(conn);
    } //runs the base function and invokes an event

    public void SetGameRuleRequiredPlayers(int value)
    {
        //TEMPORARY COMMENT minPlayers = value;
    }

    public void SetGameRuleMaxPlayers(int value)
    {
        //TEMPORARY COMMENT maxConnections = value;
    }

    public void SetGameRuleSpawnRobotGrunts(bool value)
    {
        //determine whether or not to spawn bots
    }

    public void SetGameRuleFreeForAll(bool value)
    {
        //Set whether we spawn in only two teams 
    }
    
    
}