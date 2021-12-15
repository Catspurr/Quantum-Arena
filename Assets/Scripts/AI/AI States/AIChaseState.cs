public class AIChaseState : AIBaseState
{
    public override void EnterState(AIController aiController)
    {
        aiController.animationController.RunAim();
        aiController.stateText.text = "Chase";
        
        //Set the point where we started chasing from so we can return here
        aiController.chaseStartPosition = aiController.transform.position;
    }

    public override void UpdateState(AIController aiController)
    {
        aiController.GetClosestTarget();
        if (!aiController.targetInAggroRange)
        {
            //aiController.TransitionToState(aiController.returnFromChaseState);
            aiController.TransitionToState(aiController.runState);
            return;
        }

        //Start moving the AI towards the target
        if(aiController.target != null)
            aiController.navMeshAgent.SetDestination(aiController.target.transform.position);
        
        if(aiController.targetInAttackRange)
            aiController.TransitionToState(aiController.attackState);
    }
}