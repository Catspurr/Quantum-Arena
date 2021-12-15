using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITankChaseState : AITankBaseState
{
    public AITankIdleState idleState;
    public AITankProximityState proximityState;
    //bool moving = true;
    public override void EnterState(AITankController aiController)
    {

        //Debug.Log("chase");
        //aiController.stateText.text = "chase";
    }

    public override void UpdateState(AITankController aiController)
    {
        if(aiController.target != null)
        {
            
            aiController.GetPath();
            //aiController.LookAtTarget();


            Ray rayshow = new Ray(new Vector3(aiController.transform.position.x, aiController.transform.position.y + 1, aiController.transform.position.z), aiController.tankController.lazer.up);
            RaycastHit hitinfo;
            Debug.DrawRay(new Vector3(aiController.transform.position.x, aiController.transform.position.y + 1, aiController.transform.position.z), aiController.tankController.lazer.up, Color.cyan);
            if (Physics.Raycast(rayshow, out hitinfo, 15))
            {

                if (hitinfo.collider != null)
                {
                    if (hitinfo.collider.gameObject.layer == aiController.target.layer)
                    {
                        //Debug.Log(rayshow);
                        //Debug.Log("test");
                        /*aiController.steerInput.y = 0;
                        aiController.moveInput.z = 0;*/
                        //moving = false;
                        aiController.fireIsPressed = true;
                        if (Vector3.Distance(aiController.transform.position, aiController.target.transform.position) < aiController.attackRange - 1)
                        {
                            aiController.TransitionToState(proximityState);
                        }

                    }
                    else
                    {
                        aiController.fireIsPressed = false;
                    }
                }

            }

            if (Vector3.Distance(aiController.transform.position, aiController.target.transform.position) > aiController.chaseRange)
            {
                aiController.steerInput.y = 0;
                aiController.moveInput.z = 0;
                aiController.target = null;
            }

        }

        

        if (aiController.target == null)
        {
            aiController.TransitionToState(idleState);
        }

    }


    public override void LateUpdateState(AITankController aiController)
    {
        if(aiController.target != null)
        {
            aiController.MoveToTarget(15);
            /*if (moving == true)
            {
                aiController.MoveToTarget(15);
            }
            else
            {
                aiController.steerInput.y = 0;
                aiController.moveInput.z = 0;
            }*/

            aiController.LookAtTarget(0, 200, 60);
            /*Quaternion targetRotation = Quaternion.LookRotation(aiController.target.transform.position - aiController.transform.position);
            targetRotation.x = targetRotation.z = 0;
            aiController.aimInput = targetRotation;*/
        }

    }
}
