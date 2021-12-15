using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    public float lifeTimer = 2;
    public float collisionTimer = 0.1f;
    public TankController _tank;
    public TowerAI _tower;
    private WaitForSeconds _waitForSeconds;
    private WaitForSeconds _waitForDestroy;
    // Start is called before the first frame update
    private void Awake()
    {
        /*_waitForSeconds = new WaitForSeconds(333f);
        _waitForDestroy = new WaitForSeconds(333);*/

        _waitForSeconds = new WaitForSeconds(collisionTimer);
        _waitForDestroy = new WaitForSeconds(lifeTimer);
        StartCoroutine(CollisionTimer());
        StartCoroutine(LifeTimer());
    }
    private IEnumerator CollisionTimer()
    {
        yield return _waitForSeconds;
        sphereCollider.enabled = false;
    }
    private IEnumerator LifeTimer()
    {
        yield return _waitForDestroy;
        Destroy(gameObject);
    }

    [ClientRpc]
    public void SetTank(GameObject tank)
    {
        _tank = tank.GetComponent<TankController>();
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        GameUI gameUI = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>();
        GameManager gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
        if (other.CompareTag("Player"))
        {
            float proximity = (other.transform.position - transform.position).magnitude;
            float effect = 1.4f - (proximity / sphereCollider.radius);
            
            if(_tank != null)
            {
                /*_tank.score += 10;
                Debug.Log(_tank + ": 10");*/
                
                if (other.GetComponent<TankController>().health <= effect * 10)
                {
                    if (_tank.team != other.GetComponent<TankController>().team)
                    {
                        _tank.score += 100;
                        _tank.kills += 1;
                        gameUI.AddTeamScore(100, _tank.team);
                        //Debug.Log(_tank + " explosion: 100");
                    } else
                    {
                        _tank.score -= 100;
                        _tank.kills -= 1;
                        gameUI.AddTeamScore(-100, _tank.team);
                        //Debug.Log(_tank + " explosion: -100");
                    }
                    
                }
            }
            other.GetComponent<TankController>().TakeDamage(effect * 10);
            Debug.Log("hit Tank: " + other.GetComponent<TankController>().colorId);

        }

        if (other.CompareTag("TowerTurret"))
        {
            //Debug.Log("Tower hiy");
            float proximity = (other.transform.position - transform.position).magnitude;
            float effect = 1.4f - (proximity / sphereCollider.radius);
            if (_tank != null)
            {
                /*_tank.score += 10;
                Debug.Log(_tank + ": 10");*/

                if (other.GetComponent<TowerAI>().health <= effect * 10)
                {
                    if (_tank.team != other.GetComponent<TowerAI>().team)
                    {
                        _tank.score += 100;
                        gameUI.AddTeamScore(100, _tank.team);
                        //Debug.Log(_tank + " explosion: 100");
                    }
                    else
                    {
                        _tank.score -= 100;
                        gameUI.AddTeamScore(-100, _tank.team);
                        //Debug.Log(_tank + " explosion: -100");
                    }

                }
            }
            other.GetComponent<TowerAI>().TakeDamage(effect * 10);
            //Debug.Log("hit tower");
        }

        if(other.CompareTag("AI Bots"))
        {
            Debug.Log("hit bot");
            float proximity = (other.transform.position - transform.position).magnitude;
            float effect = 1.4f - (proximity / sphereCollider.radius);
            if (_tank != null)
            {
                /*_tank.score += 10;
                Debug.Log(_tank + ": 10");*/

                if (other.GetComponent<AIController>().health <= effect * 10)
                {
                    if (_tank.team != other.GetComponent<AIController>().team)
                    {
                        _tank.score += 2;
                        gameUI.AddTeamScore(2, _tank.team);
                        //Debug.Log(_tank + " explosion: 100");
                    }
                    else
                    {
                        _tank.score -= 2;
                        gameUI.AddTeamScore(-2, _tank.team);
                        //Debug.Log(_tank + " explosion: -100");
                    }

                }
            }
            other.GetComponent<AIController>().TakeDamage(effect * 10);
            //Debug.Log(effect * 10);
        }
        //CmdSpawnExplosion(transform.position, transform.rotation);
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereCollider.radius);
        //Gizmos.DrawSphere(transform.position, sphereCollider.radius);
    }
}
