public class AIRunState : AIBaseState
{
    public override void EnterState(AIController aiController)
    {
        aiController.animationController.RunAim();
        aiController.stateText.text = "Run";
        //Set the destination to be on the far side of the control point
        aiController.navMeshAgent.SetDestination(aiController.targetGuardPosition);
    }

    public override void UpdateState(AIController aiController)
    {
        aiController.GetClosestTarget();

        if (aiController.targetInAttackRange)
        {
            aiController.TransitionToState(aiController.attackState);
            return;
        }
        
        if (aiController.targetInAggroRange)
        {
            aiController.TransitionToState(aiController.chaseState);
            return;
        }

        if (aiController.navMeshAgent.remainingDistance < 2f)
        {
            aiController.TransitionToState(aiController.idleState);
        }
    }
}