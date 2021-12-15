using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITankProximityState : AITankBaseState
{
    private bool movingRight = true;
    private bool change;
    public AITankIdleState idleState;
    public AITankChaseState chaseState;
    public override void EnterState(AITankController aiController)
    {
        change = false;
        //Debug.Log("proximity");
        //aiController.stateText.text = "proximity";
        /*yield return new WaitForSeconds(1);
        aiController.fireIsPressed = true;*/
    }

    public override void UpdateState(AITankController aiController)
    {
        if(aiController.target != null)
        {
            
            
            aiController.navMeshAgent.SetDestination(GetTargetPosition(aiController, Quaternion.LookRotation(aiController.aimInput * (movingRight ? Vector3.right : Vector3.left), Vector3.up)));
            if (Random.Range(0f, 300f) > 299f) //changes movingRight bool radomly
            {
                movingRight = !movingRight;
                //Debug.Log(movingRight);
            }
            if (aiController.navMeshAgent.path.corners.Length > 1)
            {
                aiController.node = aiController.navMeshAgent.path.corners[1];
            }

            aiController.MoveToTarget(15);
            aiController.LookAtTarget(0, 150, 60);

            /*Quaternion targetRotation = Quaternion.LookRotation(aiController.target.transform.position - aiController.transform.position);
            targetRotation.x = targetRotation.z = 0;
            aiController.aimInput = targetRotation;*/

            //aiController.Shoot();
            //aiController.fireIsPressed = true;

            if (Vector3.Distance(aiController.transform.position, aiController.target.transform.position) < aiController.attackRange + 1)
            {
                Ray rayshow = new Ray(new Vector3(aiController.transform.position.x, aiController.transform.position.y + 1, aiController.transform.position.z), aiController.tankController.lazer.up);
                RaycastHit hitinfo;
                Debug.DrawRay(new Vector3(aiController.transform.position.x, aiController.transform.position.y + 1, aiController.transform.position.z), aiController.tankController.lazer.up, Color.cyan);
                if (Physics.Raycast(rayshow, out hitinfo, 10))
                {

                    if (hitinfo.collider != null)
                    {
                        if (hitinfo.collider.gameObject.layer != aiController.target.layer && Vector3.Distance(aiController.transform.position, hitinfo.collider.gameObject.transform.position) < 4)
                        {
                            //Debug.Log(rayshow);
                            //Debug.Log("test");
                            //aiController.steerInput.y = 0;
                            //aiController.moveInput.z = 0;
                            change = true;
                            aiController.TransitionToState(chaseState);
                            //StartCoroutine(TransitionToOtherState(aiController));

                        }
                        else
                        {
                            //aiController.MoveToTarget(15);
                            aiController.fireIsPressed = true;
                            change = false;
                        }
                    }
                    /*else
                    {
                        aiController.TransitionToState(chaseState);
                        aiController.fireIsPressed = false;
                    }*/

                }
                
            }
            

            if (Vector3.Distance(aiController.transform.position, aiController.target.transform.position) > aiController.attackRange + 1)
            {
                aiController.fireIsPressed = false;
                aiController.TransitionToState(chaseState);
            }

            if (Vector3.Distance(aiController.transform.position, aiController.target.transform.position) > aiController.chaseRange)
            {
                aiController.target = null;
            }
        }


        if (aiController.target == null)
        {
            aiController.fireIsPressed = false;
            aiController.TransitionToState(idleState);
        }
    }

    IEnumerator TransitionToOtherState(AITankController aiController)
    {
        yield return new WaitForSeconds(1);
        /*if (hitinfo.collider.gameObject.layer != aiController.target.layer)
        {
            aiController.TransitionToState(chaseState);
            Debug.Log("True");
        }
        else
        {
            Debug.Log("false");
        }*/
        //Debug.Log("too long");
        if (change == true)
        {
            //Debug.Log("True");
            aiController.TransitionToState(chaseState);
            aiController.fireIsPressed = false;
            change = false;
            yield return null;
        } else
        {
            //Debug.Log("false");
        }
        //Debug.Log("aaahhhahasas");

        //gameObject.SetActive(false);
        //_tank?.AddToPool(this);
        //yield return new WaitForSeconds(0);
    }
    public override void LateUpdateState(AITankController aiController)
    {
        if (aiController.target != null)
        {
            

            //aiController.LookAtTarget();
        }

    }
    public Vector3 GetTargetPosition(AITankController character, Quaternion fleeDirection)
    {
        Vector3 fleeVector = fleeDirection * Vector3.back * 10;
        fleeVector += character.transform.position;
        NavMeshHit hit;

        NavMesh.SamplePosition(fleeVector, out hit, 10, 1);

        return hit.position;
    }
}
