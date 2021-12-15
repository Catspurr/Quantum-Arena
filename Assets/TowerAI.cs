using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class TowerAI : NetworkBehaviour
{
    public GameObject target;
    public GameObject turret;
    public Quaternion aim;
    public float range;
    public LayerMask enemyTeamLayerMasks;
    public TowerBullet[] bulletPrefab;
    public GameObject[] muzzleFlare;
    public Transform bulletStartPoint;

    public float shootCooldown;
    public float bulletForce;
    public float bulletRange;
    public float bulletDamage;
    private float _nextShootTime;
    [SerializeField] private GameObject[] deathPrefab;
    [SyncVar(hook = nameof(HandleTakeDamage))]
    public float health = 100;
    public bool isDead;
    [Range(1, 4)] public int team;
    [SerializeField] private Slider healthSlider;
    public int colorId;
    public Color[] colorArray;
    public GameObject soundPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        _nextShootTime = Time.time + shootCooldown; //Sets next allowed shoot time on the server
    }

    /*public override void OnStartAuthority()
    {
        enabled = true; //Enables the controller on the local players tank

        if (isClientOnly)
        {
            NetworkServer.SpawnObjects(); //Spawns GameObjects that were spawned before the client joined
        }
    }*/
    [ServerCallback]
    void Update()
    {
        if(target != null)
        {
            try
            {
                if (target.GetComponent<TankController>().isDead)
                {
                    target = null;
                }
            }
            catch
            {
                //Do nada
            }
        }
        

        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= bulletRange)
        {
            Shoot();
        }
        
        FindTarget(range);
    }

    [ServerCallback]
    void LateUpdate()
    {
        if(target != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            targetRotation.x = targetRotation.z = 0;
            aim = targetRotation;
            turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, aim, 500 * Time.deltaTime);
        }
        
    }
    [ClientRpc]
    public void SetTowerInfo(int id)
    {
        colorId = id;
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            render.material.color = colorArray[id];
        }
        healthSlider.fillRect.GetComponent<Image>().color = colorArray[id];
        //colors = colorArray[id]
        /*team = networkTeam;
        colorId = networkColorId;
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            render.material.color = colorArray[networkColorId];
        }

        color = colorArray[networkColorId];*/
    }
    private TowerBullet GetBullet()
    {
        var bullet = Instantiate(bulletPrefab[colorId], bulletStartPoint.position, bulletStartPoint.rotation);
        bullet.range = bulletRange;
        NetworkServer.Spawn(bullet.gameObject);
        bullet.SetTowerTurret(gameObject);
        return bullet;
    }
    public void Shoot() //Click event
    {
        if (Time.time < _nextShootTime) return; //Client check to not be able to spam messages to server
        bulletStartPoint.localRotation = new Quaternion(0, Random.Range(-0.05f, 0.05f), 0, 1f);
        CmdShoot(bulletStartPoint.position, bulletStartPoint.rotation, bulletStartPoint.transform.TransformDirection(Vector3.forward * bulletForce)); //sends client position and rotation to the server to shoot in that direction
    }

    
    private void CmdShoot(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        if (Time.time < _nextShootTime) return; //Server check if we are allowed to shoot to prevent cheating
        _nextShootTime = Time.time + shootCooldown; //Sets next allowed shoot time on the server
        var flare = Instantiate(muzzleFlare[colorId], position, rotation);
        NetworkServer.Spawn(flare.gameObject);
        var bullet = GetBullet(); //Gets a bullet from the pool or instantiates a new one if needed
        bullet.RpcLaunchBullet(position, rotation, velocity); //Gives the bullet itself the position and velocity
        var soundInstance = Instantiate(soundPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(soundInstance);
        soundInstance.GetComponent<SoundPrefab>().RpcPlayTurretShootSound();
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

    private void Die()
    {
        isDead = true;
        var death = Instantiate(deathPrefab[colorId], transform.position, transform.rotation);
        NetworkServer.Spawn(death.gameObject);
        NetworkServer.UnSpawn(gameObject);
        //Destroy(gameObject);
        gameObject.SetActive(false);

    }

    private void HandleTakeDamage(float oldHealth, float newHealth) //Hook for health change
    {
        healthSlider.value = newHealth;
    }

    public void FindTarget(float alarmRange)
    {
        var foundColliders = Physics.OverlapSphere(transform.position, alarmRange, enemyTeamLayerMasks);
        var closesetDistance = alarmRange;
        foreach (var collider in foundColliders)
        {
            var tank = collider.GetComponent<TankController>();
            var soldier = collider.GetComponent<AIController>();
            if (tank != null)
            {
                if (tank.isDead || tank.team == team)
                {
                    continue;
                }
                var distance = Vector3.Distance(transform.position, tank.transform.position);

                if (distance < closesetDistance)
                {
                    closesetDistance = distance;
                    target = tank.gameObject;
                }
            }

            if (soldier != null)
            {
                if (soldier.team == team) continue;
                var distance = Vector3.Distance(transform.position, soldier.transform.position);

                if (distance < closesetDistance)
                {
                    closesetDistance = distance;
                    target = soldier.gameObject;
                }
            }
        }
    }
}
