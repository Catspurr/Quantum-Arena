using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITankIdleState : AITankBaseState
{
    public AITankChaseState chaseState;
    public AITankProximityState proximityState;
    private float timeUntilChangeDir = 0f;
    private bool changeDirDone = false;
    private float changeDirTime = 1;
    private Vector3 finalPosition = Vector3.zero;
    public override void EnterState(AITankController aiController)
    {
        //Debug.Log("idle");
        //aiController.stateText.text = "idle";
        aiController.moveInput.z = 0;
    }

    public override void UpdateState(AITankController aiController)
    {

        /*if (Physics.CheckSphere(
            aiController.transform.position,
            aiController.chaseRange,
            aiController.enemyTeamLayerMasks))
        {
            aiController.TransitionToState(chaseState);
        }*/
        
        timeUntilChangeDir = changeDirDone ? changeDirTime : timeUntilChangeDir - Time.fixedDeltaTime;
        changeDirDone = (timeUntilChangeDir <= 0);
        if (changeDirDone)
        {
            aiController.navMeshAgent.SetDestination(RandomNavmeshLocation(aiController.transform, 25f));
            //aiController.tankController.baseInput.aimInput.y = Random.Range(0, 360);
            changeDirTime = Random.Range(0.8f, 1.3f);
        }
        if (aiController.navMeshAgent.path.corners.Length > 1)
        {
            aiController.node = aiController.navMeshAgent.path.corners[1];
        }
        aiController.MoveToTarget(15);
        //aiController.LookAtTarget(0, 150, 60);
        if (aiController.target != null)
        {
            /*Quaternion targetRotation = Quaternion.LookRotation(aiController.target.transform.position - aiController.transform.position);
            targetRotation.x = targetRotation.z = 0;
            aiController.aimInput = targetRotation;*/
            aiController.TransitionToState(chaseState);
        }


        
        /*if (aiController.target != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(aiController.target.transform.position - aiController.transform.position);
            targetRotation.x = targetRotation.z = 0;
            aiController.aimInput = targetRotation;
        }*/
        //if(aiController.)


    }

    public Vector3 RandomNavmeshLocation(Transform transform, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;

        NavMesh.SamplePosition(randomDirection, out hit, radius, 1);

        finalPosition = hit.position;

        return finalPosition;
    }
}