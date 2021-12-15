using Mirror;
using System.Collections;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    #region Variables

    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float fireSpeed = 20f;
    public float range = 10f;
    [SerializeField] private GameObject explosionPrefab;
    public TankController _tank;
    private WaitForSeconds _waitForSeconds;
    public Vector3 initialPosition;
    [SyncVar] public int hitPoints = 1; //prevents explosion from activating twice
    public GameObject soundPrefab;

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
        if(distance >= range)
        {
            //Debug.Log("it is range");
            gameObject.SetActive(false);
            _tank?.AddToPool(this);
            //RpcHandleTriggerEnter();
            CmdSpawnExplosion(transform.position, transform.rotation);
        }
    }
    private void OnEnable()
    {
        initialPosition = transform.position;
        hitPoints = 1;
        //StartCoroutine(LifeTimer());
    }

    private IEnumerator LifeTimer()
    {
        yield return _waitForSeconds;
        gameObject.SetActive(false);
        _tank?.AddToPool(this);
    }

    #endregion
    
    #region Trigger Events

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (_tank == null) return;
        if (other.gameObject == _tank.gameObject) return;
        if (other.CompareTag("Player"))
        {
            if (_tank != null && !other.GetComponent<TankController>().isDead)
            {
                if (_tank.team != other.GetComponent<TankController>().team)
                {
                    _tank.score += 10;
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(10, _tank.team);
                    //Debug.Log(_tank + " bullet: 10");
                    if (other.GetComponent<TankController>().health <= _tank.damage)
                    {
                        _tank.score += 100;
                        _tank.kills += 1;
                        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(100, _tank.team);
                        //Debug.Log(_tank + " bullet: 100");
                    }
                }
                else
                {
                    _tank.score -= 10;
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(-10, _tank.team);
                    //Debug.Log(_tank + " bullet: -10");
                    if (other.GetComponent<TankController>().health <= _tank.damage)
                    {
                        _tank.score -= 100;
                        _tank.kills -= 1;
                        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(-100, _tank.team);
                        //Debug.Log(_tank + " bullet: -100 :(");
                    }
                }
                other.GetComponent<TankController>().TakeDamage(_tank.damage);
            }
            
        }

        if (other.CompareTag("TowerTurret"))
        {
            

            //Debug.Log("hit tower");
            if (_tank != null)
            {
                if(_tank.team != other.GetComponent<TowerAI>().team)
                {
                    _tank.score += 10;
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(10, _tank.team);
                    //Debug.Log(_tank + " bullet: 10");
                    if (other.GetComponent<TowerAI>().health <= _tank.damage)
                    {
                        _tank.score += 100;
                        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(100, _tank.team);
                        //Debug.Log(_tank + " bullet: 100");
                    }
                }
                else
                {
                    _tank.score -= 10;
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(-10, _tank.team);
                    //Debug.Log(_tank + " bullet: -10");
                    if (other.GetComponent<TowerAI>().health <= _tank.damage)
                    {
                        _tank.score -= 100;
                        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(-100, _tank.team);
                        //Debug.Log(_tank + " bullet: -100... dumbass");
                    }
                }
                other.GetComponent<TowerAI>().TakeDamage(_tank.damage);
            }
            
        }

        if (other.CompareTag("AI Bots"))
        {
            if (_tank != null)
            {
                if (_tank.team != other.GetComponent<AIController>().team)
                {
                    _tank.score += 1;
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(1, _tank.team);
                    //Debug.Log(_tank + " bullet: 10");
                    if (other.GetComponent<AIController>().health <= _tank.damage)
                    {
                        _tank.score += 10;
                        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(10, _tank.team);
                        //Debug.Log(_tank + " bullet: 100");
                    }

                    other.GetComponent<AIController>().TakeDamage(_tank.damage);
                }
                /*else
                {
                    _tank.score -= 1;
                    //Debug.Log(_tank + " bullet: -10");
                    if (other.GetComponent<AIController>().health <= _tank.damage)
                    {
                        _tank.score -= 10;
                        //Debug.Log(_tank + " bullet: -100... dumbass");
                    }
                }*/
            }
            
        }
        //CmdSpawnExplosion(transform.position, transform.rotation);
        if (_tank != null && other.CompareTag("AI Bots") && other.gameObject.layer == _tank.gameObject.layer) return;
        RpcHandleTriggerEnter();
    }

    [ClientRpc]
    private void RpcHandleTriggerEnter()
    {
        /*var explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(explosion.gameObject);*/
        if(hitPoints >= 1)
        {
            CmdSpawnExplosion(transform.position, transform.rotation);
            gameObject.SetActive(false);
            _tank?.AddToPool(this);
            hitPoints--;
        }
        
    }

    [ServerCallback]
    private void CmdSpawnExplosion(Vector3 position, Quaternion rotation)
    {
        var explosion = Instantiate(explosionPrefab, position, rotation);
        
        NetworkServer.Spawn(explosion.gameObject);
        explosion.GetComponent<Explosion>().SetTank(_tank.gameObject);

        var soundInstance = Instantiate(soundPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(soundInstance);
        soundInstance.GetComponent<SoundPrefab>().RpcPlayTankBulletExplosionSound();
        //Debug.Log(explosion.GetComponent<Explosion>()._tank);
    }

    #endregion

    #region Public Rpc Methods

    [ClientRpc]
    public void SetTank(GameObject tank) //Sets the tank owner of the bullet
    {
        _tank = tank.GetComponent<TankController>();
    }

    [ClientRpc]
    public void RpcLaunchBullet(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);
        GetComponent<Rigidbody>().velocity = velocity;
    }

    #endregion
}
