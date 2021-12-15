using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;

public class TankController : NetworkBehaviour
{
    #region Variables

    [Header("Editor Drag-in")]
    public GameObject aim;
    public BaseInput baseInput;
    public Transform bulletStartPoint;
    public Transform lazer;
    public Bullet[] bulletPrefab;
    public GameObject shield;
    [SerializeField] private TextMeshProUGUI tankNameText;
    [SerializeField] private TextMeshProUGUI tankScoreText;
    [SerializeField] private Slider healthSlider;
    public GameObject[] deathPrefab;
    [Space] 
    [Header("Tank Settings")] 
    [SerializeField] private float turnSpeed = 200f;
    [SerializeField] private float moveSpeed = 200f, aimSpeed = 500f, fireCooldown = 0.5f, fireForce = 20f;
    public float range = 5f;
    public float damage = 10f;
    [Range(1, 4)]public int team;

    public int colorId;
    public Color[] colorArray;

    public bool isDead;
    private WaitForSeconds _waitForSeconds;
    //SyncVar hook means whenever the synced variable is changed by the server the hook method is called on all clients
    [SyncVar (hook = nameof(HandleTakeDamage))]
    public float health = 100f;
    
    [SyncVar(hook = nameof(HandleNameUpdated))]
    public string _tankDisplayName;

    private MyNetworkManager _networkManager;
    private PlayerInput _playerInput;
    private Rigidbody _rigidbody;
    private float _nextShootTime;
    [SerializeField] private GameObject[] wheels;
    [SerializeField] private Animator animator;
    [SyncVar] public bool isMoving;
    public bool isAI;
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private Quaternion spawnRot;
    [SyncVar(hook = nameof(HandleScoreUpdated))]
    public float score;
    [SyncVar] public float kills;
    public Color color;
    public bool isFlameThrower;
    public VisualEffect flameEffect;
    public GameObject soundPrefab;
    
    #endregion

    #region Bullet Pooling
    //Probably overkill to pool objects for this particular game but I wanted to give object pooling a try.
    private Queue<Bullet> _pool = new Queue<Bullet>();
    private Bullet GetBullet() //Pool of bullets, instantiates a new bullet only if there is none in the pool available
    {
        if (_pool.Count > 0)
        {
            var bullet = _pool.Dequeue();
            bullet.SetTank(gameObject); //Sets the tank owner of the bullet
            return bullet;
        }
        else
        {
            var bullet = Instantiate(
                bulletPrefab[colorId], 
                bulletStartPoint.position, 
                bulletStartPoint.rotation);
            bullet.range = range;
            NetworkServer.Spawn(bullet.gameObject);
            bullet.SetTank(gameObject); 
            return bullet;
        }
    }
    public void AddToPool(Bullet bullet) //Called from Bullet script, adds the bullet back to the pool after its lifetime
    {
        _pool.Enqueue(bullet);
    }
    #endregion

    #region Start Methods
    private void Awake() //Sets things on the server side as well in order to do movement and shooting on server
    {
        //baseInput.enabled = false;
        lazer.localScale = new Vector3(0.01f, range / 2, 0.01f);
        lazer.transform.localPosition = new Vector3(0, bulletStartPoint.localPosition.y, (range / 2) + bulletStartPoint.localPosition.z);
        spawnPos = transform.position;
        spawnRot = transform.rotation;
        _rigidbody = GetComponent<Rigidbody>();
        //flameEffect.Stop();
        //isFlaming = false;
    }
    
    public override void OnStartAuthority() //Only called on the game object the client has authority over
    {
        //enabled = true; //Enables the controller on the local players tank
        _networkManager = GameObject.FindWithTag("NetworkManager").GetComponent<MyNetworkManager>();
        _playerInput = GetComponent<PlayerInput>();
        //flameEffect.Stop();
        if (_playerInput != null)
        {
            _playerInput.enabled = true;
        }
        
        if (isClientOnly)
        {
            NetworkServer.SpawnObjects(); //Spawns GameObjects that were spawned before the client joined
        }
        if (_networkManager == null)
        {
            CmdSetName("Nameless");
            return;
        }
        foreach (var gamePlayer in _networkManager.GamePlayers) //Looks through all GamePlayers in the network manager
        {
            if (!gamePlayer.isLocalPlayer) continue; //If the GamePlayer is not ours we skip to next
            CmdSetName(gamePlayer.displayName); //If the GamePlayer is ours we get the name from it
        }
    }
    [ClientRpc]
    public void CanWalk(bool enable)
    {
        if (hasAuthority || isAI)
        {
            enabled = enable;
        }
        
    }

    [ClientRpc]
    public void RpcSetTankInfo(int networkTeam, int networkColorId)
    {
        team = networkTeam;
        colorId = networkColorId;
        shield.SetActive(true);
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            render.material.color = colorArray[networkColorId];
        }

        color = colorArray[networkColorId];
        shield.SetActive(false);
    }
    
    #endregion

    #region Tank movement and rotation

    [ClientCallback]
    private void FixedUpdate() //Fixed update for physics
    {
        _rigidbody.velocity = transform.forward * baseInput.moveInput.z * moveSpeed * Time.deltaTime;
        if(baseInput.moveInput.z != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        //RotateWheels(baseInput.moveInput.z, moveSpeed);
        RotateWheel();
        
    }

    [ClientCallback]
    private void Update() //Fixed update for physics
    {

        if (baseInput.fireIsPressed)
        {
            if (!isAI)
            {
                Shoot();

            }
            else
            {
                AIShoot();
            }
        }
    }

    [ClientCallback]
    private void LateUpdate() //Late update to prevent stuttering since movement is handled in fixed update
    {
        transform.Rotate(baseInput.steerInput * turnSpeed * Time.deltaTime);
        aim.transform.rotation = Quaternion.RotateTowards(
            aim.transform.rotation,
            baseInput.aimInput,
            aimSpeed * Time.deltaTime);
    }

    public void RpcRotateWheels()
    {

    }

    [Command]
    public void CmdRotateWheels(float input, float speed)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] != null)
            {
                wheels[i].transform.Rotate(input * speed * Time.deltaTime, 0, 0);
                //wheels[i].transform.Rotate(baseInput.moveInput.z * moveSpeed * Time.deltaTime, 0, 0);
            }
        }
    }

    public void RotateWheel()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] != null)
            {
                //wheels[i].transform.Rotate(input * speed * Time.deltaTime, 0, 0);
                wheels[i].transform.Rotate(baseInput.moveInput.z * moveSpeed * Time.deltaTime, 0, 0);
            }
        }
    }

    #endregion

    #region Shooting

    public void Shoot() //Click event
    {
        if(Time.time < _nextShootTime) return; //Client check to not be able to spam messages to server
        //flameEffect.Play();
        CmdShoot(
            bulletStartPoint.position, 
            bulletStartPoint.rotation, 
            aim.transform.TransformDirection(Vector3.forward * fireForce)); //sends client position and rotation to the server to shoot in that direction
    }

    [Command] //Client calls this to run on the server
    private void CmdShoot(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (Time.time < _nextShootTime) return; //Server check if we are allowed to shoot to prevent cheating
        _nextShootTime = Time.time + fireCooldown; //Sets next allowed shoot time on the server
        //flameEffect.Play();
        var bullet = GetBullet(); //Gets a bullet from the pool or instantiates a new one if needed
        bullet.RpcLaunchBullet(position, rotation, velocity); //Gives the bullet itself the position and velocity
        var sound = Instantiate(soundPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(sound);
        sound.GetComponent<SoundPrefab>().RpcPlayTankShootSound();
    }


    public void AIShoot() //Click event
    {
        if (Time.time < _nextShootTime) return; //Client check to not be able to spam messages to server
        AIClientShoot(
            bulletStartPoint.position,
            bulletStartPoint.rotation,
            aim.transform.TransformDirection(Vector3.forward * fireForce)); //sends client position and rotation to the server to shoot in that direction
    }

    
    private void AIClientShoot(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (Time.time < _nextShootTime) return; //Server check if we are allowed to shoot to prevent cheating
        _nextShootTime = Time.time + fireCooldown; //Sets next allowed shoot time on the server
        var bullet = GetBullet(); //Gets a bullet from the pool or instantiates a new one if needed
        bullet.RpcLaunchBullet(position, rotation, velocity); //Gives the bullet itself the position and velocity
        var sound = Instantiate(soundPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(sound);
        sound.GetComponent<SoundPrefab>().RpcPlayTankShootSound();
    }
    #endregion

    #region SyncVar and Hooks

    [Command]
    private void CmdSetName(string tankName) //sets name server side to update SyncVar
    {
        _tankDisplayName = tankName;
    }
    private void HandleNameUpdated(string oldName, string newName) //Hook for name change
    {
        tankNameText.text = newName;
    }

    private void HandleScoreUpdated(float oldScore, float newScore) //Hook for name change
    {
        tankScoreText.text = newScore.ToString();
    }
    public void GiveScore(float value) //updates the health server side to update the SyncVar
    {
        score += value;

    }

    public void TakeDamage(float value) //updates the health server side to update the SyncVar
    {
        health -= value;
        if (health <= 0)
        {
            //Debug.Log("Ded");
            Die();
        }

    }

    private void HandleTakeDamage(float oldHealth, float newHealth) //Hook for health change
    {
        healthSlider.value = newHealth;
    }

    private void Die()
    {
        if (!isServer)
        {
            CmdDie(/*deathPrefab, */transform.position, transform.rotation);
            return;
        }
        AIDie(transform.position, transform.rotation);
        
    }

    private void AIDie(/*GameObject prefab, */Vector3 position, Quaternion rotation)
    {
        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>().RespawnObj(gameObject);

        var death = Instantiate(deathPrefab[colorId], position, rotation);
        NetworkServer.Spawn(death.gameObject);
        var soundInstance = Instantiate(soundPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(soundInstance);
        soundInstance.GetComponent<SoundPrefab>().RpcPlayTankExplosionSound();
        /*isDead = true;
        gameObject.SetActive(false);*/
        RpcDie();
    }


    [Command]
    private void CmdDie(/*GameObject prefab, */Vector3 position, Quaternion rotation)
    {
        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>().RespawnObj(gameObject);

        var death = Instantiate(deathPrefab[colorId], position, rotation);
        NetworkServer.Spawn(death.gameObject);
        var soundInstance = Instantiate(soundPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(soundInstance);
        soundInstance.GetComponent<SoundPrefab>().RpcPlayTankExplosionSound();
        RpcDie();
    }

    [ClientRpc]
    private void RpcDie()
    {
        isDead = true;
        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void Respawn()
    {
        transform.position = spawnPos;
        transform.rotation = spawnRot;
        gameObject.SetActive(true);
        //NetworkServer.Spawn(gameObject);
        //isDead = false;
        gameObject.layer = 18;
        shield.SetActive(true);
        //StartCoroutine(InvisFrame());
    }
    //[ClientRpc]
    IEnumerator InvisFrame()
    {
        yield return new WaitForSeconds(1f);
        RemoveInvis();
    }

    [ClientRpc]
    public void RemoveInvis()
    {
        gameObject.layer = team + 5;
        shield.SetActive(false);
        isDead = false;
    }
    #endregion
}