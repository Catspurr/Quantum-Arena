using UnityEngine;
public class AIIdleState : AIBaseState
{
    public override void EnterState(AIController aiController)
    {
        aiController.animationController.IdleAim();
        aiController.stateText.text = "Idle";
        //Set the agent destination to our current destination
        aiController.navMeshAgent.SetDestination(aiController.transform.position);
    }

    public override void UpdateState(AIController aiController)
    {
        //Check for a close target and chase/attack that, if none is close enough we look at the closest one instead
        
        aiController.GetClosestTarget();
        
        if (aiController.targetInAttackRange)
        {
            aiController.TransitionToState(aiController.attackState);
            return;
        }

        /*if (aiController.targetInAggroRange)
        {
            aiController.TransitionToState(aiController.chaseState);
            return;
        }*/

        if (aiController.target != null)
        {
            aiController.transform.LookAt(aiController.target.transform);
        }
        if(Vector3.Distance(aiController.transform.position, aiController.targetGuardPosition) > 1f)
        {
            aiController.TransitionToState(aiController.runState);
            return;
        }
        
        //Check if there is an enemy inside the capture point if we didnt find any targets close enough to start chasing
        //This method will automatically switch us to the ChasePointProximity state if it finds an enemy
        aiController.CheckCapturePointProximity();
    }
}