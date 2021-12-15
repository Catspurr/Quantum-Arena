using System;
using System.Linq.Expressions;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AIController : NetworkBehaviour
{
    [Header("Editor Drag-ins")]
    public GameObject deathPrefab;
    public Renderer[] gunMeshes;
    public Transform bulletStartPoint;
    public GameObject aiBulletPrefab;
    public TextMeshProUGUI stateText;
    public GameObject soundPrefab;
    [Space]
    [Header("Bots stats")] [SyncVar]
    public float health = 20f;
    public float damage = 2f, moveSpeed = 5f, secondsBetweenAttacks = 1;
    [Space]
    [Header("Attack behaviour")]
    [Tooltip("How far away the robot can see and select a target")]
    public float aggroRange = 10;
    [Tooltip("How far away the robot can attack from")]
    public float attackRange = 5;
    [Tooltip("How fast the bullet travels")]
    public float fireForce = 20;
    [SyncVar] public int team;
    
    [HideInInspector] public GameObject target;
    [HideInInspector] public bool canFire = true, targetInAggroRange, targetInAttackRange;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public Vector3 targetGuardPosition = new Vector3(0f,0f,0f), chaseStartPosition = new Vector3(0f,0f,0f);
    [HideInInspector] public AISpawnSystem aiSpawnSystem;
    [HideInInspector] public AIAnimationController animationController;
    [HideInInspector] public CapturePoint capturePoint;
    public WaitForSeconds fireCooldown;
    
    private AIBaseState _currentState;

    public readonly AIRunState runState = new AIRunState();
    public readonly AIChaseState chaseState = new AIChaseState();
    public readonly AIAttackState attackState = new AIAttackState();
    public readonly AIIdleState idleState = new AIIdleState();
    private readonly AIChasePointProximityState _chasePointProximityState = new AIChasePointProximityState();
    private readonly AIDeadState _deadState = new AIDeadState();

    [ServerCallback]
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        capturePoint = GameObject.FindWithTag("Capture Point").GetComponent<CapturePoint>();
        fireCooldown = new WaitForSeconds(secondsBetweenAttacks);
        aiSpawnSystem = GameObject.FindWithTag("Game Manager").GetComponent<AISpawnSystem>();
        animationController = GetComponent<AIAnimationController>();
    }
    
    [ServerCallback]
    private void Start()
    {
        CalculateGuardPosition();
        TransitionToState(runState);
    }
    
    [ServerCallback]
    private void Update()
    {
        _currentState.UpdateState(this);
    }
    
    [Server]
    public void TransitionToState(AIBaseState state)
    {
        _currentState = state;
        _currentState.EnterState(this);
    }

    public void GetClosestTarget() //Changes target to the closest enemy robot or tank, favors tanks
    {
        //If we still have a target and it is dead we want to reset the target to look for a new one
        if (target != null)
        {
            var tankController = target.GetComponent<TankController>();
            if (tankController != null)
            {
                if(tankController.isDead) target = null;
            }
            
            /*try //Try getting the AIController and check if it's dead
            {
                if (target.GetComponent<TankController>().isDead)
                    target = null;
            }
            catch //If its not an AI then get the TankController and check if it's dead
            {
                //Do nada
            }*/
        }
        
        
        //Checks all enemy AI in a jagged list
        for (var i = 0; i < aiSpawnSystem.botsPerTeam.Count; i++)
        {
            //Skip our team
            if (i == team - 1) continue;
            foreach (var go in aiSpawnSystem.botsPerTeam[i])
            {
                //Sets the target as a failsafe
                if (target == null)
                {
                    target = go;
                    continue;
                }
                //If the distance between us and "go" is shorter than the distance between us and the current target
                if (Vector3.Distance(transform.position, target.transform.position) >
                    Vector3.Distance(transform.position, go.transform.position))
                {
                    target = go;
                }
            }
        }
        
        //Check if any enemy player is closer
        foreach (var go in aiSpawnSystem.gameManager.playerList)
        {
            if (go == null) continue;
            //Skip if player is dead or on the same team
            if (go.GetComponent<TankController>().isDead) continue;
            if (go.GetComponent<TankController>().team == team) continue;

            //If there are no enemy bots the target will be null, therefore we gotta temporarily set the first enemy tank as the target to compare the rest
            if (target == null)
            {
                target = go;
                continue;
            }
            
            //If the distance between us and "go" is shorter than it is to our current target, set new target
            if (Vector3.Distance(transform.position, target.transform.position) >
                Vector3.Distance(transform.position, go.transform.position))
            {
                target = go;
            }
        }

        //Failsafe in case it didnt find any new targets
        if (target == null)
        {
            targetInAggroRange = targetInAttackRange = false;
            return;
        }
        
        //If the distance to the target is less than aggro range set bool to true
        targetInAggroRange = Vector3.Distance(transform.position, target.transform.position) < aggroRange;
        
        //If the distance to the target is less than attack range set bool to true
        targetInAttackRange = Vector3.Distance(transform.position, target.transform.position) < attackRange;
            
    }

    public void CheckCapturePointProximity() //This will prioritize hunting down bots in the proximity of the capture point before players
    {
        //IMPORTANT!!! TARGET WILL REMAIN NULL IF NO ONE IS CLOSE ENOUGH TO THE CAPTURE POINT!!!
        target = null;
        //Check all enemy ai teams
        for (var i = 0; i < aiSpawnSystem.botsPerTeam.Count; i++)
        {
            //Skip our own team
            if (i == team - 1) continue;
            
            //Check all bots distance to the capture point in this team
            foreach (var go in aiSpawnSystem.botsPerTeam[i])
            {
                //If the distance between the bot and the capture point is longer than the max size of the point, skip to next bot
                if (Vector3.Distance(capturePoint.transform.position, go.transform.position) >
                    Mathf.Max(capturePoint.height, capturePoint.width)) continue;
                
                target = go;
                TransitionToState(_chasePointProximityState);
                return;
            }
        }
        
        //Check all enemy players distance to the capture point
        foreach (var go in aiSpawnSystem.gameManager.playerList)
        {
            //Skip players that are dead or on our team
            if (go.GetComponent<TankController>().team == team || go.GetComponent<TankController>().isDead) continue;
            
            //if the distance between the player and the capture point is longer than the max size of the capture point, skip to next player
            if (Vector3.Distance(capturePoint.transform.position, go.transform.position) >
                Mathf.Max(capturePoint.height, capturePoint.width)) continue;
            
            target = go;
            TransitionToState(_chasePointProximityState);
        }
    }

    private void CalculateGuardPosition() //Calculates which end of the capture point they should go towards and sets a random position along the far line of it
    {
        var dist1 = Vector3.Distance(transform.position, capturePoint.pointA1);
        var dist2 = Vector3.Distance(transform.position, capturePoint.pointB1);
        if (dist1 > dist2)
        {
            var difference = capturePoint.pointA1 - capturePoint.pointA2;
            var newDifference = difference * Random.Range(0f, 1f);
            targetGuardPosition = capturePoint.pointA2 + newDifference;
        }
        else
        {
            var difference = capturePoint.pointB1 - capturePoint.pointB2;
            var newDifference = difference * Random.Range(0f, 1f);
            targetGuardPosition = capturePoint.pointB2 + newDifference;
        }
    }

    public void GetSpawnSystem(AISpawnSystem spawnSystem)
    {
        aiSpawnSystem = spawnSystem;
    }

    public GameObject InstantiateBullet()
    {
        var bulletInstance = Instantiate(aiBulletPrefab, bulletStartPoint.position, bulletStartPoint.rotation);

        return bulletInstance;
    }

    public GameObject InstantiateSound()
    {
        var soundInstance = Instantiate(soundPrefab, transform.position, transform.rotation);
        return soundInstance;
    }
    
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health <= 0f)
        {
            TransitionToState(_deadState);
        }
    }

    public void DestroyBot()
    {
        var deathPrefabInstance = Instantiate(deathPrefab, transform.position, transform.rotation);
        NetworkServer.Spawn(deathPrefabInstance);
        aiSpawnSystem.botsPerTeam[team-1].Remove(gameObject);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = team switch
        {
            1 => Color.red,
            2 => Color.blue,
            3 => Color.green,
            4 => Color.yellow,
            _ => Color.white
        };
        Gizmos.DrawWireSphere(transform.position,aggroRange);
    }
    
}