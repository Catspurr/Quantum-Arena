public class AIReturnFromChaseState : AIBaseState
{
    public override void EnterState(AIController aiController)
    {
        aiController.animationController.Run();
        aiController.stateText.text = "Return";
        //Return to the position where it started chasing
        aiController.navMeshAgent.SetDestination(aiController.chaseStartPosition);
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

        if (aiController.navMeshAgent.remainingDistance < 1)
        {
            aiController.TransitionToState(aiController.runState);
        }
    }
}