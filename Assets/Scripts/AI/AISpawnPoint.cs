using System;
using UnityEngine;

public class AISpawnPoint : MonoBehaviour
{
    public float width = 7f, height = 3f;
    [SerializeField][Range(1, 2)] private int team = 1;


    //For the editor to see where the bots can spawn
    private void OnDrawGizmos()
    {
        var x = height / 2;
        var z = width / 2;
        var gameObjectTransform = transform;
        var transformPosition = gameObjectTransform.position;
        var forwardTransform = gameObjectTransform.forward;
        var rightTransform = gameObjectTransform.right;

        var point1 = transformPosition + forwardTransform * x - rightTransform * z; //Top left
        var point2 = transformPosition + forwardTransform * x + rightTransform * z; //Top right
        var point3 = transformPosition - forwardTransform * x - rightTransform * z; //Bottom left
        var point4 = transformPosition - forwardTransform * x + rightTransform * z; //bottom right

        Gizmos.color = team switch
        {
            1 => Color.red,
            2 => Color.blue,
            _ => Color.white
        };

        Gizmos.DrawLine(point1,point2);
        Gizmos.DrawLine(point2,point4);
        Gizmos.DrawLine(point4,point3);
        Gizmos.DrawLine(point3,point1);
    }
}