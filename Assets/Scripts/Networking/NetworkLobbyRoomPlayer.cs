using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;
using TMPro;

public class NetworkLobbyRoomPlayer : NetworkBehaviour
{
    [Header("UI")] 
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
    [SerializeField] private RawImage[] playerProfileImages = new RawImage[4];
    [SerializeField] private Image[] playerColors = new Image[4];
    [SerializeField] private Color[] teamColors = new Color[5];
    [SerializeField] private Button startGameButton, inviteButton;
    [SerializeField] private GameObject hostUI;
    [SerializeField] private GameObject player1Collection ,player2Collection, player3Collection, player4Collection;
    [SerializeField] private Image useRobotLightShowcase;

    [SyncVar(hook = nameof(HandleGameModeChanged))]
    public int gameMode = 1;

    public RawImage profileImage;
    private Callback<AvatarImageLoaded_t> _avatarImageLoaded;

    private bool useRobots;
    
    public int team;
    [SyncVar] public int lobbySlot;

    [SyncVar(hook = nameof(HandleColorIdChanged))]
    public int colorId;

    public int tankType;

    [SyncVar(hook = nameof(HandleSteamIdUpdated))]
    public ulong steamId;
    
    [SyncVar(hook = nameof(HandleColorChanged))]
    public Color color;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string displayName = "Loading...";

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool isReady = false;

    
    public bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            hostUI.SetActive(value);
            inviteButton.gameObject.SetActive(Room.usingSteam);
        }
    }

    private MyNetworkManager _room;
    private MyNetworkManager Room
    {
        get
        {
            if (_room != null) return _room;
            return _room = NetworkManager.singleton as MyNetworkManager;
        }
    }

    public override void OnStartAuthority() //called for the object the client have authority over
    {
        //If we are not using Steam, take the name from the player input
        if (!Room.usingSteam) CmdSetDisplayName(PlayerNameInput.DisplayName);
        //lobbySlot = Room.RoomPlayers.Count;
        //AssignSlotValue();
        //Check all players color, if any is the same as our, call the change color method to switch to the next available color
        foreach (var roomPlayer in Room.RoomPlayers)
        {
            //Ignore our own entry
            if (roomPlayer.hasAuthority) continue;
            //Continue if the current roomPlayer does not have the same color as ours
            if (colorId != roomPlayer.colorId) continue;
            ChangeColor();
            break;
        }
        //Sets the color initially
        CmdSetColor(colorId);

        //sets the ui active for the clients own object
        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        //Add this room player to the list of room players in network manager
        Room.RoomPlayers.Add(this);
        //lobbySlot = Room.RoomPlayers.Count - 1;
        //If we are using steam set the callback to load avatars
        if (Room.usingSteam) _avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        //Initial update of the UI to show the players in the lobby
        UpdateDisplay(); 
    }
    /*[Command]
    public void AssignSlotValue()
    {
        lobbySlot = Room.RoomPlayers.Count - 1;
    }*/
    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);
        UpdateDisplay();
    } //Removes the disconnected player from the network list and updates ui

    public void HandleGameModeChanged(int oldValue, int newValue) => UpdateDisplay();
    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();
    public void HandleColorChanged(Color oldValue, Color newValue) => UpdateDisplay();
    public void HandleColorIdChanged(int oldValue, int newValue) => UpdateDisplay();

    //public void HandleTeamChanged(int oldValue, int newValue) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        } //If we dont have authority check for the object that has authority and run on that client

        for (var i = 0; i < playerNameTexts.Length; i++)
        {
            playerProfileImages[i].enabled = false;
            playerNameTexts[i].text = "Waiting for player...";
            playerReadyTexts[i].text = string.Empty;
            playerColors[i].gameObject.SetActive(false);
        } //Resets all names and removes ready text and profile pictures

        for (var i = 0; i < Room.RoomPlayers.Count; i++)
        {
            if (Room.usingSteam)
            {
                if (Room.RoomPlayers[i].profileImage != null)
                {
                    playerProfileImages[i].enabled = true;
                    playerProfileImages[i].texture = Room.RoomPlayers[i].profileImage.texture;
                }
            }
            playerNameTexts[i].text = Room.RoomPlayers[i].displayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].isReady
                ? "<color=green>Ready</color>"
                : "<color=red>Not ready</color>";
            //Room.RoomPlayers[i].team = i+1;
            playerColors[i].gameObject.SetActive(true);
            Room.RoomPlayers[i].lobbySlot = i;
            Room.RoomPlayers[i].gameMode = gameMode;
            playerColors[i].color = Room.RoomPlayers[i].color;
        } //updates all names and ready texts for each player connected


        if (Room.RoomPlayers.Count < 1) return;
        
        var player1Background = player1Collection.GetComponent<Image>();
        var player2Background = player2Collection.GetComponent<Image>();
        var player3Background = player3Collection.GetComponent<Image>();
        var player4Background = player4Collection.GetComponent<Image>();
        var redColor = Color.red;
        redColor.a = 0.392f;
        var blueColor = Color.blue;
        blueColor.a = 0.392f;
        
        switch (Room.RoomPlayers[0].gameMode)
        {
            case 1:
                for (var i = 0; i < Room.RoomPlayers.Count; i++)
                {
                    Room.RoomPlayers[i].team = i + 1;
                    //Room.RoomPlayers[i].lobbySlot = i;
                }
                player1Background.color = redColor;
                player2Background.color = blueColor;
                player3Collection.SetActive(false);
                player4Collection.SetActive(false);
                break;
            case 2:
                for (var i = 0; i < Room.RoomPlayers.Count; i++)
                {
                    Room.RoomPlayers[i].team = i < 2 ? 1 : 2;
                    //Room.RoomPlayers[i].lobbySlot = i;

                    //Room.RoomPlayers[i].slot = i;
                    //Room.RoomPlayers[i].colorId = i == 1 ? 0 : 3;

                }
                player1Background.color = redColor;
                player2Background.color = redColor;
                player3Background.color = blueColor;
                player4Background.color = blueColor;
                player3Collection.SetActive(true);
                player4Collection.SetActive(true);
                break;
            case 3:
                for (var i = 0; i < Room.RoomPlayers.Count; i++)
                {
                    Room.RoomPlayers[i].team = i + 1;
                    //Room.RoomPlayers[i].lobbySlot = i;
                }
                player1Background.color = redColor;
                player2Background.color = redColor;
                player3Background.color = redColor;
                player4Background.color = redColor;
                player3Collection.SetActive(true);
                player4Collection.SetActive(true);
                break;
        }
    }
    

    public void HandleReadyToStart(bool b)
    {
        if (!isLeader) return;

        startGameButton.interactable = b;
    } //sets the start game button to be interactable or not for the leader client

    #region Steam

    private void HandleSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        var cSteamId = new CSteamID(newSteamId);
        //CmdSetDisplayName(SteamFriends.GetFriendPersonaName(cSteamId));
        displayName = SteamFriends.GetFriendPersonaName(cSteamId);

        var imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);

        if (imageId == -1) return;

        profileImage.texture = GetSteamImageAsTexture(imageId);
        UpdateDisplay();
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID != steamId) return;
        profileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
        UpdateDisplay();
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        var isValid = SteamUtils.GetImageSize(iImage, out var width, out var height);
        if (isValid)
        {
            var image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));
            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        
        return texture;
    }
    
    public void InviteButton()
    {
        SteamFriends.ActivateGameOverlayInviteDialog(SteamLobby.LobbyId);
    }

    #endregion
    
    public void LeaveLobbyButton()
    {
        if (isServer)
        {
            Room.StopHost();
        }
        else
        {
            Room.StopClient();
            
        }
    }

    public void ChangeColor()
    {
        //Update ID to next entry
        colorId++;
        //If the ID is longer than the array set it back to 0
        if (colorId >= teamColors.Length) colorId = 0;
        //For each player in the lobby
        foreach(var player in Room.RoomPlayers)
        {
            //Continue if it's our own object
            if (player.hasAuthority) continue;
            //If our new color ID is the same as the current player in the loop
            if (colorId == player.colorId)
            {
                //Go to the next ID
                colorId++;
                if (colorId >= teamColors.Length) colorId = 0;
                //Check every player again if anyone have the same color, if they do, repeat the process and return
                foreach (var nextPlayer in Room.RoomPlayers)
                {
                    if (nextPlayer.hasAuthority) continue;
                    if (colorId == nextPlayer.colorId)
                    {
                        ChangeColor();
                        return;
                    }
                }
            }
        }
        //When we found a free color set it on the server to update all players on our new color
        CmdSetColor(colorId);
    }

    [Command]
    private void CmdSetColor(int id)
    {
        //Set the color ID and the color for the UI server side to trigger syncvar hooks
        colorId = id;
        color = teamColors[id];
    }

    [Command]
    private void CmdSetDisplayName(string dName)
    {
        displayName = dName;
    } //sets the name on server side
    

    [Command]
    public void CmdReadyUp()
    {
        isReady = !isReady;
        Room.NotifyPlayersOfReadyState();
    } //sets ready button as a toggle and updates the other clients of the state

    [Command]
    public void CmdStartGame()
    {
        //Failsafe to check that its actually the leader
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) return;
        /*for (var i = 0; i < Room.RoomPlayers.Count; i++)
        {
            if(gameMode == 2)
            {
                Room.teamColors[0] = 0;
                Room.teamColors[1] = 1;
            }
            else
            {
                Room.teamColors[i] = Room.RoomPlayers[i].colorId;
                //Room.teamColors[i + 1] = Room.RoomPlayers[i].colorId + 1;
                *//*if(Room.RoomPlayers[i].connectionToClient != connectionToClient)
                {
                    Room.teamColors[i] = i - 1;
                }*//*
            }
            
        }*/

        Room.StartGame(gameMode, useRobots);
    } //Starts the game

    [Command]
    public void CmdChangeGameMode(int mode)
    {
        //Failsafe to check that its actually the leader
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) return;
        gameMode = mode;
        UpdateDisplay();
    }

    public void UseRobotsButton()
    {
        useRobots = !useRobots;
        useRobotLightShowcase.color = useRobots ? Color.green : Color.red;
    }
}