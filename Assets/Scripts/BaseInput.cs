using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInput : MonoBehaviour
{
    public Vector3 moveInput;
    public Vector3 steerInput;
    public Quaternion aimInput = Quaternion.identity;
    public bool fireIsPressed;
}
