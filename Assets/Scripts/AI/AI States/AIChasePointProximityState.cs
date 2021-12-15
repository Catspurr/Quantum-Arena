using UnityEngine;

public class AIChasePointProximityState : AIBaseState
{
    public override void EnterState(AIController aiController)
    {
        aiController.animationController.RunAim();
        aiController.stateText.text = "Chase point";
        //Set the point where we started chasing from so we can return here
        aiController.chaseStartPosition = aiController.transform.position;
    }

    public override void UpdateState(AIController aiController)
    {
        //If the target is null for some reason transition to ReturnFromChase state
        if (aiController.target == null)
        {
            //aiController.TransitionToState(aiController.returnFromChaseState);
            aiController.TransitionToState(aiController.runState);
            return;
        }
        
        try //Try checking if the player is dead, in that case return from chase (Try catch in case the target is an AI)
        {
            if (aiController.target.GetComponent<TankController>().isDead)
            {
                //aiController.TransitionToState(aiController.returnFromChaseState);
                aiController.TransitionToState(aiController.runState);
                return;
            }
        }
        catch
        {
            //Do nada
        }
        
        //Set the agent to chase after the target inside the control point
        aiController.navMeshAgent.SetDestination(aiController.target.transform.position);

        //If the target is within aggro range transition to chase state
        if (Vector3.Distance(aiController.transform.position, aiController.target.transform.position) <
            aiController.aggroRange)
        {
            aiController.TransitionToState(aiController.chaseState);
        }
    }
}