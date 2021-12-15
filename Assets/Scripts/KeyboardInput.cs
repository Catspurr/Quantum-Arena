using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Mathematics;
using Mirror;

public class KeyboardInput : BaseInput
{
    /*public Vector3 moveInput;
    public Vector3 steerInput;
    //public float fireIsPressed;
    public Quaternion aimAngle = Quaternion.identity;*/
    public float firePress;

    public Vector2 mousePos;
    public Quaternion targetRotation;
    public Vector3 targetPoint;

    [ClientCallback]
    void Update()
    {
        targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
        targetRotation.x = targetRotation.z = 0;
        aimInput = targetRotation;
    }

    [Client]
    public void OnMouseInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        mousePos = new Vector2(inputValue.x, inputValue.y);

        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        //Debug.Log("mouse:" + Mouse.current.position.ReadValue());
        float hitDist = 0.0f;
        if (playerPlane.Raycast(ray, out hitDist))
        {
            targetPoint = ray.GetPoint(hitDist);
            //Debug.Log(targetPoint);
            
            
            //Debug.DrawRay(transform.position, targetPoint);
        }
    }

    [Client]
    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        moveInput = new Vector3(0, 0, inputValue.y);
    }

    [Client]
    public void OnSteerInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        steerInput = new Vector3(0, inputValue.x, 0);
    }

    public void OnFireInput(InputAction.CallbackContext ctx)
    {
        //FireInput(ctx.ReadValueAsButton());
        firePress = ctx.ReadValue<float>();
        if(firePress > 0)
        {
            fireIsPressed = true;
        } else
        {
            fireIsPressed = false;
        }
    }


    [Server]
    public void OnDebugInput(InputAction.CallbackContext ctx)
    {
        GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameUI>().time = 3;
    }
}
