using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GamepadInput : BaseInput
{
    /*public Vector3 moveInput;
    public Vector3 steerInput;
    public Quaternion aimInput = Quaternion.identity;*/


    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        moveInput = new Vector3(0, 0, inputValue.y);
    }

    public void OnAimInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();


        //inputValue = inputValue.Rotate();


        if (!(inputValue.x == 0 && inputValue.y == 0))
        {
            var tempAimInput = new Vector3(inputValue.x, 0, inputValue.y);
            aimInput = Quaternion.LookRotation(tempAimInput, Vector3.up);
        }
    }

    public void OnSteerInput(InputAction.CallbackContext ctx)
    {
        Vector2 inputValue = ctx.ReadValue<Vector2>();
        steerInput = new Vector3(0, inputValue.x, 0);
    }
}
