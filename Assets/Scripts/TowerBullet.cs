using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBullet : NetworkBehaviour
{
    #region Variables

    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float fireSpeed = 20f;
    public float range = 10f;
    [SerializeField] private GameObject explosionPrefab;
    public TowerAI _tower;
    private WaitForSeconds _waitForSeconds;
    public Vector3 initialPosition;

    #endregion

    #region Start Methods

    private void Awake()
    {
        //initialposition = transform.position;
        _waitForSeconds = new WaitForSeconds(lifeTime / fireSpeed);
    }
    //[ServerCallback]
    private void Update()
    {
        var distance = Vector3.Distance(initialPosition, transform.position);
        //Debug.Log(distance);
        if (distance >= range)
        {
            //Debug.Log("it is range");
            //gameObject.SetActive(false);
            Destroy(gameObject);
            //_tank?.AddToPool(this);
            //RpcHandleTriggerEnter();
            /*if (explosionPrefab != null)
                CmdSpawnExplosion(transform.position, transform.rotation);*/
        }
    }
    private void OnEnable()
    {
        initialPosition = transform.position;
        //StartCoroutine(LifeTimer());
    }

    private IEnumerator LifeTimer()
    {
        yield return _waitForSeconds;
        //gameObject.SetActive(false);
        Destroy(gameObject);
        //_tank?.AddToPool(this);
    }

    #endregion

    #region Trigger Events

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == null) return;
        if (_tower.gameObject == null) return;
        if (other.gameObject == _tower.gameObject) return;
        if (other.CompareTag("Player"))
        {

            if (_tower.team != other.GetComponent<TankController>().team)
            {
                GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(10, _tower.team);
                if (other.GetComponent<TankController>().health <= _tower.bulletDamage)
                {
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(100, _tower.team);
                }
            }
            else
            {
                GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(-10, _tower.team);
                if (other.GetComponent<TankController>().health <= _tower.bulletDamage)
                {
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(-100, _tower.team);
                }
            }

            other.GetComponent<TankController>().TakeDamage(_tower.bulletDamage);
        }

        if (other.CompareTag("TowerTurret"))
        {
            other.GetComponent<TowerAI>().TakeDamage(_tower.bulletDamage);
            //Debug.Log("hit tower");
        }

        if (other.CompareTag("AI Bots"))
        {
            other.GetComponent<AIController>().TakeDamage(_tower.bulletDamage);
        }
        //CmdSpawnExplosion(transform.position, transform.rotation);
        RpcHandleTriggerEnter();
    }

    [ClientRpc]
    private void RpcHandleTriggerEnter()
    {
        /*var explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(explosion.gameObject);*/
        if (explosionPrefab != null)
            CmdSpawnExplosion(transform.position, transform.rotation);
        //gameObject.SetActive(false);
        Destroy(gameObject);
        //_tank?.AddToPool(this);
    }

    [ServerCallback]
    private void CmdSpawnExplosion(Vector3 position, Quaternion rotation)
    {
        var explosion = Instantiate(explosionPrefab, position, rotation);
        NetworkServer.Spawn(explosion.gameObject);
    }

    #endregion

    #region Public Rpc Methods

    [ClientRpc]
    public void SetTowerTurret(GameObject tower) //Sets the tank owner of the bullet
    {
        _tower = tower.GetComponent<TowerAI>();
    }

    [ClientRpc]
    public void RpcLaunchBullet(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        transform.position = position;
        transform.rotation = rotation;
        /*gameObject.SetActive(true);
        Destroy(gameObject);*/
        GetComponent<Rigidbody>().velocity = velocity;
    }

    #endregion
}
