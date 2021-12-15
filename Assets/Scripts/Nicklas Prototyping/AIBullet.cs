using Mirror;
using System.Collections;
using UnityEngine;

public class AIBullet : NetworkBehaviour
{
    #region Variables
    
    [SerializeField] private float range = 10f;
    [SerializeField] private GameObject explosionPrefab;
    private AIController _bot;
    public Vector3 initialPosition;

    #endregion

    #region Start Methods

    private void Awake()
    {
        initialPosition = transform.position;
    }

    [ServerCallback]
    private void Update()
    {
        var distance = Vector3.Distance(initialPosition, transform.position);

        if (distance < range) return;
        //CmdSpawnExplosion(transform.position, transform.rotation);
        Destroy(gameObject);
    }
    

    #endregion
    
    #region Trigger Events

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == null) return;
        if (_bot == null) return;
        if (other.gameObject == _bot.gameObject) return;

        //var score = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().nullScore;

        if (other.CompareTag("Player"))
        {
            //score += 1;
            if (_bot.team != other.GetComponent<TankController>().team)
            {
                GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(1, _bot.team);
                //Debug.Log(_tank + " bullet: 10");
                if (other.GetComponent<TankController>().health <= _bot.damage)
                {
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(50, _bot.team);
                    //Debug.Log(_tank + " bullet: 100");
                }

                other.GetComponent<TankController>().TakeDamage(_bot.damage);
            }

            
            
        }

        if (other.CompareTag("AI Bots"))
        {
            if (_bot.team != other.GetComponent<AIController>().team)
            {

                if (other.GetComponent<AIController>().health <= _bot.damage)
                {
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(1, _bot.team);
                }

                other.GetComponent<AIController>().TakeDamage(_bot.damage);
            }
            
            
        }

        if (other.CompareTag("TowerTurret"))
        {
            if (_bot.team != other.GetComponent<TowerAI>().team)
            {
                GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(1, _bot.team);
                //Debug.Log(_tank + " bullet: 10");
                if (other.GetComponent<TowerAI>().health <= _bot.damage)
                {
                    GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().AddTeamScore(50, _bot.team);
                    //Debug.Log(_tank + " bullet: 100");
                }
                other.GetComponent<TowerAI>().TakeDamage(_bot.damage);
            }

            
        }

        SpawnExplosion(transform.position, transform.rotation);
        if (other.CompareTag("Player") && other.gameObject.layer == _bot.gameObject.layer) return;
        RpcHandleTriggerEnter();
    }

    [ClientRpc]
    private void RpcHandleTriggerEnter()
    {
        Destroy(gameObject);
    }
    
    private void SpawnExplosion(Vector3 position, Quaternion rotation)
    {
        var explosion = Instantiate(explosionPrefab, position, rotation);
        NetworkServer.Spawn(explosion.gameObject);
    }

    #endregion

    #region Public Rpc Methods

    [ClientRpc]
    public void SetBot(GameObject bot) //Sets the bot owner of the bullet
    {
        _bot = bot.GetComponent<AIController>();
    }

    [ClientRpc]
    public void RpcLaunchBullet(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        transform.position = position;
        transform.rotation = rotation;
        GetComponent<Rigidbody>().velocity = velocity;
    }

    #endregion
}
