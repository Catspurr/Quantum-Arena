using System.Collections;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class AITankController : BaseInput
{
    public NavMeshAgent navMeshAgent;
    public AITankBaseState aiState;
    public GameObject target;
    public float chaseRange = 20;
    public float attackRange = 5;
    public LayerMask enemyTeamLayerMasks;
    public AITankSpawnSystem aiTankSpawnSystem;
    public TankController tankController;
    public Vector3 node;
    public TextMeshProUGUI stateText;

    [ServerCallback]
    void Start()
    {
        tankController.enabled = true;
    }

    // Update is called once per frame
    [ServerCallback]
    void Update()
    {
        /*if(target != null)
        {
            moveInput.z = 1;
            //navMeshAgent.SetDestination(target.position);
        }*/
        /*if (fireIsPressed)
        {
            Shoot();
        }*/
        /*if (target == null)
        {
            FindTarget(chaseRange);

        }*/

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
                /*if (target.GetComponent<AIController>().isDead)
                {
                    target = null;
                }*/

                if (target.GetComponent<TowerAI>().isDead)
                {
                    target = null;
                }
            }
        }
        
        FindTarget(chaseRange);
        aiState.UpdateState(this);
        
    }

    [ServerCallback]

    void LateUpdate()
    {
        aiState.LateUpdateState(this);
    }

    [Server]
    public void TransitionToState(AITankBaseState state)
    {
        aiState = state;
        aiState.EnterState(this);
    }
    //[Command]
    public void GetTankSpawnSystem(AITankSpawnSystem tankSpawnSystem)
    {
        if (aiTankSpawnSystem != null)
        {
            aiTankSpawnSystem = tankSpawnSystem;
        }

    }

    public GameObject InstantiateBullet()
    {
        var bulletInstance = Instantiate(tankController.bulletPrefab[tankController.colorId].gameObject, tankController.bulletStartPoint.position, tankController.bulletStartPoint.rotation);

        return bulletInstance;
    }
    //[Command]
    /*public void Shoot()
    {
        //tankController.CmdShoot(transform.position, transform.rotation, tankController.aim.transform.TransformDirection(Vector3.forward * 20));
        //tankController.Shoot();
        //isShooting = true;
        var bullet = InstantiateBullet();
        NetworkServer.Spawn(bullet);
    }*/

    public void LookAtTarget(float closeAngle = 0, float midCloseAngle = 150, float farAngle = 20)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        targetRotation.x = targetRotation.z = 0;
        //aimInput = targetRotation;
        //aimInput.y = 90;
        //attackRange = 3;
        

        


        if (target.gameObject.GetComponent<TankController>() != null && target.gameObject.GetComponent<TankController>().isMoving == true)
        {
            Vector3 directionToTarget = target.transform.position - transform.position;
            float signedAngle = Vector3.SignedAngle(tankController.aim.transform.forward, directionToTarget, Vector3.one);

            if (signedAngle > -2 && signedAngle < 2)
            {
                //Debug.Log("infront");
                //Debug.Log("signedAngle > -5 && signedAngle < 5");
                aimInput = Quaternion.RotateTowards(tankController.aim.transform.rotation, targetRotation, closeAngle * Time.deltaTime);
            }
            else if (signedAngle > 12)
            {
                //Debug.Log("signedAngle > 12");
                aimInput = Quaternion.RotateTowards(tankController.aim.transform.rotation, targetRotation, midCloseAngle * Time.deltaTime);
                /*if (Random.Range(0f, 300f) > 299f) //changes movingRight bool radomly
                {
                    aimInput = targetRotation;
                    Debug.Log("yes");
                }*/
            }
            else if (signedAngle < -12)
            {
                //Debug.Log("signedAngle < -12");
                aimInput = Quaternion.RotateTowards(tankController.aim.transform.rotation, targetRotation, midCloseAngle * Time.deltaTime);
                /*if (Random.Range(0f, 300f) > 299f) //changes movingRight bool radomly
                {
                    aimInput = targetRotation;
                    Debug.Log("yes");
                }*/
            }
            else
            {
                //Debug.Log("else");
                aimInput = Quaternion.RotateTowards(tankController.aim.transform.rotation, targetRotation, farAngle * Time.deltaTime);
            }
        }
        else
        {
            //aimInput = Quaternion.RotateTowards(tankController.aim.transform.rotation, targetRotation, 150 * Time.deltaTime);
            aimInput = targetRotation;
        }
    }

    public void GetPath()
    {
        navMeshAgent.destination = target.transform.position;
        //Debug.Log(navMeshAgent.path.corners.Length);
        if(navMeshAgent.path.corners.Length > 1)
        {
            node = navMeshAgent.path.corners[1];
        }
        /*else if(navMeshAgent.path.corners.Length == 1)
        {
            //node = Vector3.forward;
            node = navMeshAgent.path.corners[0];
        }*/
    }

    public void MoveToTarget(float angle)
    {
        Vector3 directionToTarget = node - transform.position;
        float signedAngle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.one);

        if (signedAngle > -10 && signedAngle < 10)
        {
            steerInput.y = 0;
            moveInput.z = 1;
            //aiController.moveInput.z = 0.5f;
            //Debug.Log("signedAngle > -10 && signedAngle < 10");
        }
        else if (signedAngle > 25)
        {
            steerInput.y = 1;
            moveInput.z = 0.7f;
            //aiController.moveInput.z = 0.3f;
            //Debug.Log("signedAngle > 25");
        }
        else if (signedAngle < -25)
        {
            steerInput.y = -1;
            moveInput.z = 0.7f;
            //aiController.moveInput.z = 0.3f;
            //Debug.Log("signedAngle < -25");
        }
    }

    //[Command]
    public void FindTarget(float alarmRange)
    {
        var foundColliders = Physics.OverlapSphere(transform.position, alarmRange, enemyTeamLayerMasks);
        var closesetDistance = alarmRange;
        var closesetDistance2 = alarmRange;
        foreach (var collider in foundColliders)
        {
            var tank = collider.GetComponent<TankController>();
            var soldier = collider.GetComponent<AIController>();
            var tower = collider.GetComponent<TowerAI>();
            //if (tank.team == tankController.team || tower.team == tankController.team) return;
            /*var tankDistance = Vector3.Distance(transform.position, tank.transform.position);
            var towerDistance = Vector3.Distance(transform.position, tower.transform.position);*/

            if (tank != null)
            {
                
                if (tank.isDead || tank.team == tankController.team)
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
            if (tower != null)
            {

                if (tower.isDead || tower.team == tankController.team)
                {
                    continue;
                }
                var distance = Vector3.Distance(transform.position, tower.transform.position);

                if (distance < closesetDistance && tank == null)
                {
                    closesetDistance = distance;
                    target = tower.gameObject;

                }
            }


        }

    }
}
