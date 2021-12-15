using Mirror;
using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour //Class for storing info
{
    [SyncVar]
    public string displayName = "Loading..."; //synced on the server
    [SyncVar] 
    public int colorId;
    [SyncVar] 
    public int team;
    [SyncVar] 
    public int tankType;

    [SyncVar] 
    public int gameMode;

    [SyncVar]
    public bool isLeader;
    
    public bool spawned;

    [SyncVar]
    public int lobbySlot;
    //TO-DO: Add cached pfp sprite from lobby for steam integration


    private MyNetworkManager _room;
    private MyNetworkManager Room
    {
        get
        {
            if (_room != null) return _room;
            return _room = NetworkManager.singleton as MyNetworkManager;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        
        Room.GamePlayers.Add(this); //adds this to the GamePlayer list on the network manager
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this); //removes this from the list in the network manager
    }

    [Server]
    public void SetDisplayName(string s) //called on server to set the display name
    {
        displayName = s;
    }

    [Server]
    public void SetUpTank(string tName, int tColorId, int tTeam, int tTankType, bool tIsLead, int tGameMode, int tSlot)
    {
        displayName = tName;
        colorId = tColorId;
        team = tTeam;
        tankType = tTankType;
        isLeader = tIsLead;
        gameMode = tGameMode;
        lobbySlot = tSlot;
    } //TO-DO: Add pfp cache
}