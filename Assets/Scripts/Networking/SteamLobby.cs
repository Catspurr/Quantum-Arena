using System;
using Mirror;
using Steamworks;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private MainMenuManager mainMenuManager;
    private MyNetworkManager _networkManager;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    
    public static CSteamID LobbyId { get; private set; }
    private CSteamID _currentLobby;

    private const string HostAddressKey = "HostAddress";

    private void OnEnable()
    {
        MyNetworkManager.OnClientDisconnected += LeaveGame;
    }

    private void OnDisable()
    {
        MyNetworkManager.OnClientDisconnected -= LeaveGame;
    }

    private void Start()
    {
        _networkManager = GetComponent<MyNetworkManager>();
        if (!SteamManager.Initialized) return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) return;

        LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        _currentLobby = LobbyId;
        Debug.Log(LobbyId.m_SteamID);
        
        _networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(
            LobbyId, 
            HostAddressKey, 
            SteamUser.GetSteamID().ToString());
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        _currentLobby = callback.m_steamIDLobby;
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;
        
        var hostAddress = SteamMatchmaking.GetLobbyData(
            new CSteamID(callback.m_ulSteamIDLobby),
            HostAddressKey);

        mainMenuManager.CloseAllPanels();
        _networkManager.networkAddress = hostAddress;
        _networkManager.StartClient();
    }

    public void LeaveGame()
    {
        SteamMatchmaking.LeaveLobby(_currentLobby);
    }
}