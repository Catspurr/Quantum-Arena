using System.Collections;
using Mirror;
using UnityEngine;

public class AIAttackState : AIBaseState
{
    public override void EnterState(AIController aiController)
    {
        aiController.animationController.IdleAim();
        aiController.stateText.text = "Attack";

        var difference = aiController.target.transform.position - aiController.transform.position;
        difference *= 0.1f;
        
        //Set the agent destination to our current destination
        aiController.navMeshAgent.SetDestination(aiController.transform.position + difference);
    }

    public override void UpdateState(AIController aiController)
    {
        aiController.GetClosestTarget();
        
        if (!aiController.targetInAttackRange)
        {
            if (aiController.targetInAggroRange)
            {
                aiController.TransitionToState(aiController.chaseState);
                return;
            }
            //aiController.TransitionToState(aiController.returnFromChaseState);
            aiController.TransitionToState(aiController.runState);
        }

        if (aiController.target != null)
        {
            //Always look at our target
            aiController.transform.LookAt(aiController.target.transform);
        }
        
        //If we cant fire then don't fire
        if (!aiController.canFire) return;
        Fire(aiController);
    }

    private void Fire(AIController aiController)
    {
        aiController.canFire = false;
        aiController.StartCoroutine(FireCooldown(aiController));
        aiController.animationController.ArmShot();
        var bullet = aiController.InstantiateBullet();
        NetworkServer.Spawn(bullet);
        bullet.GetComponent<AIBullet>().SetBot(aiController.gameObject);
        bullet.GetComponent<AIBullet>().RpcLaunchBullet(
            aiController.bulletStartPoint.transform.position,
            aiController.bulletStartPoint.transform.rotation,
            aiController.bulletStartPoint.transform.TransformDirection(Vector3.forward * aiController.fireForce));
        var soundInstance = aiController.InstantiateSound();
        NetworkServer.Spawn(soundInstance);
        soundInstance.GetComponent<SoundPrefab>().RpcPlayRobotShootSound();
    }

    private IEnumerator FireCooldown(AIController aiController)
    {
        yield return aiController.fireCooldown;
        aiController.canFire = true;
    }
}