using System.Collections;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Networking")] public RawImage profileImage;
    
    [Header("UI")] 
    [SerializeField] private GameObject playerNameInputPanel;
    [SerializeField] private GameObject mainPagePanel, settingsPanel, playPagePanel, hostLobbyPanel, ipAddressPanel, mainMenuEffects;
    [SerializeField] private TextMeshProUGUI welcomeText;

    [Space][Header("Volume Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider, musicSlider, sfxSlider;
    
    private MyNetworkManager _networkManager;
    private SteamLobby _steamLobby;
    private Callback<AvatarImageLoaded_t> _avatarImageLoaded;

    //PlayerPrefs keys
    private const string PlayerPrefsMasterVolumeKey = "MasterVolume";
    private const string PlayerPrefsMusicVolumeKey = "MusicVolume";
    private const string PlayerPrefsSfxVolumeKey = "SFXVolume";
    private const string PlayerPrefsNameKey = "PlayerName";

    private void OnEnable()
    {
        MyNetworkManager.OnClientConnected += OnClientConnect;
        MyNetworkManager.OnClientDisconnected += OnClientDisconnect;
    }

    private void OnDisable()
    {
        MyNetworkManager.OnClientDisconnected -= OnClientDisconnect;
        MyNetworkManager.OnClientConnected += OnClientConnect;
    }

    private void Start()
    {
        var networkGameObject = GameObject.FindWithTag("NetworkManager");
        _networkManager = networkGameObject.GetComponent<MyNetworkManager>();
        _steamLobby = networkGameObject.GetComponent<SteamLobby>();

        //Set the ui to be correct no matter what is enabled in the editor for ease during the development
        if (_networkManager.usingSteam)
        {
            _avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
            playerNameInputPanel.SetActive(false);
            mainPagePanel.SetActive(true);
            SetWelcomeText(SteamFriends.GetPersonaName());
            GetSteamImage();
        }
        else
        {
            playerNameInputPanel.SetActive(true);
            mainPagePanel.SetActive(false);
            if (PlayerPrefs.HasKey(PlayerPrefsNameKey))
            {
                SetWelcomeText(PlayerPrefs.GetString(PlayerPrefsNameKey));
            }
        }
        playPagePanel.SetActive(false);
        ipAddressPanel.SetActive(false);
        settingsPanel.SetActive(false);
        hostLobbyPanel.SetActive(false);
        
        
        //Sets all the volume sliders if there is playerPrefs for them, also sets the volume levels for each
        if (PlayerPrefs.HasKey(PlayerPrefsMasterVolumeKey))
        {
            var value = PlayerPrefs.GetFloat(PlayerPrefsMasterVolumeKey);
            masterSlider.value = value;
            SetMasterLevel(value);
        }
        else if(!PlayerPrefs.HasKey(PlayerPrefsMasterVolumeKey))
        { //If there is no PlayerPrefs set the values to half level to not blast peoples eardrums first time starting
            masterSlider.value = 0.5f;
            SetMasterLevel(0.5f);
        }
        
        if (PlayerPrefs.HasKey(PlayerPrefsMusicVolumeKey))
        {
            var value = PlayerPrefs.GetFloat(PlayerPrefsMusicVolumeKey);
            musicSlider.value = value;
            SetMusicLevel(value);
        }
        else if(!PlayerPrefs.HasKey(PlayerPrefsMusicVolumeKey))
        {
            musicSlider.value = 0.5f;
            SetMusicLevel(0.5f);
        }
        
        if (PlayerPrefs.HasKey(PlayerPrefsSfxVolumeKey))
        {
            var value= PlayerPrefs.GetFloat(PlayerPrefsSfxVolumeKey);
            sfxSlider.value = value;
            SetSfxLevel(value);
        } 
        else if (!PlayerPrefs.HasKey(PlayerPrefsSfxVolumeKey))
        {
            sfxSlider.value = 0.5f;
            SetSfxLevel(0.5f);
        }
    }

    private void GetSteamImage()
    {
        var imageId = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());
        if (imageId == -1) return;
        profileImage.texture = GetSteamImageAsTexture(imageId);
        profileImage.gameObject.SetActive(true);
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID != SteamUser.GetSteamID()) return;
        profileImage.texture = GetSteamImageAsTexture(callback.m_iImage);
        profileImage.gameObject.SetActive(true);
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

    public void SetWelcomeText(string playerName)
    {
        welcomeText.text = $"Welcome, {playerName}";
    }

    private void OnClientConnect()
    {
        if (mainMenuEffects != null) mainMenuEffects.SetActive(false);
    }

    private void OnClientDisconnect()
    {
        if (ipAddressPanel.activeSelf) return;
        playPagePanel.SetActive(true);
    } //activates the main set of buttons if you're disconnected from the server

    public void HostLobby()
    {
        if (_networkManager.usingSteam)
        {
            if (_steamLobby != null)
            {
                _steamLobby.HostLobby();
            }
        }
        else
        {
            _networkManager.StartHost();
        }
        
        playPagePanel.SetActive(false);
    } // starts a hosted game and disables the main set of buttons

    public void HostOneVersusOneGameMode()
    {
        _networkManager.SetGameRuleRequiredPlayers(2);
        _networkManager.SetGameRuleMaxPlayers(2);
        _networkManager.SetGameRuleFreeForAll(false);
        _networkManager.SetGameRuleSpawnRobotGrunts(true);
        HostLobby();
    }

    public void HostTwoVersusTwoGameMode()
    {
        _networkManager.SetGameRuleRequiredPlayers(4);
        _networkManager.SetGameRuleMaxPlayers(4);
        _networkManager.SetGameRuleFreeForAll(false);
        _networkManager.SetGameRuleSpawnRobotGrunts(true);
        HostLobby();
    }

    public void HostFreeForAllGameMode()
    {
        _networkManager.SetGameRuleRequiredPlayers(2);
        _networkManager.SetGameRuleMaxPlayers(4);
        _networkManager.SetGameRuleFreeForAll(true);
        _networkManager.SetGameRuleSpawnRobotGrunts(false);
        HostLobby();
    }

    public void CloseAllPanels()
    {
        playerNameInputPanel.SetActive(false);
        playPagePanel.SetActive(false);
        ipAddressPanel.SetActive(false);
        settingsPanel.SetActive(false);
        hostLobbyPanel.SetActive(false);
        mainPagePanel.SetActive(false);
    }

    public void SetMasterLevel(float sliderValue)
    {
        var value = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat(PlayerPrefsMasterVolumeKey, value);
        PlayerPrefs.SetFloat(PlayerPrefsMasterVolumeKey, sliderValue);
    }

    public void SetMusicLevel(float sliderValue)
    {
        var value = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat(PlayerPrefsMusicVolumeKey, value);
        PlayerPrefs.SetFloat(PlayerPrefsMusicVolumeKey, sliderValue);
    }

    public void SetSfxLevel(float sliderValue)
    {
        var value = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat(PlayerPrefsSfxVolumeKey, value);
        PlayerPrefs.SetFloat(PlayerPrefsSfxVolumeKey, sliderValue);
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
}