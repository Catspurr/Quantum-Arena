using UnityEngine;

public class CapturePoint : MonoBehaviour
{
    public float height = 12f, width = 10f;

    //Values for the bots to know where to go
    [HideInInspector] public Vector3 pointA1, pointA2, pointB1, pointB2;
    private void Awake()
    {
        var objTransform = transform;
        var transformPos = objTransform.position;
        var transformForward = objTransform.forward;
        var transformRight = objTransform.right;
        pointA1 = transformPos + transformForward * (height / 2) - transformRight * (width / 2);
        pointB1 = transformPos + transformForward * (height / 2) + transformRight * (width / 2);
        pointA2 = transformPos - transformForward * (height / 2) - transformRight * (width / 2);
        pointB2 = transformPos - transformForward * (height / 2) + transformRight * (width / 2);
    }

    
    //For editor, easier to see where the boundaries of the point is
    private void OnDrawGizmos()
    {
        var x = height / 2;
        var z = width / 2;
        var gameObjectTransform = gameObject.transform;
        var transformPosition = gameObjectTransform.position;
        var forwardTransform = gameObjectTransform.forward;
        var rightTransform = gameObjectTransform.right;

        var point1 = transformPosition + forwardTransform * x - rightTransform * z; //Top left
        var point2 = transformPosition + forwardTransform * x + rightTransform * z; //Top right
        var point3 = transformPosition - forwardTransform * x - rightTransform * z; //Bottom left
        var point4 = transformPosition - forwardTransform * x + rightTransform * z; //bottom right
          
        Gizmos.DrawLine(point1,point2);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(point2,point4);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(point4,point3);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(point3,point1);
    }
}