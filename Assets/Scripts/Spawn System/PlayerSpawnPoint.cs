using System;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public Color colors = new Color(1f, 1f, 1f, 1f);
    [Range(1, 4)] public int team;
    [HideInInspector] public bool alreadyUsed;
    private void Awake() => PlayerSpawnSystem.AddSpawnPoint(transform);
    private void OnDestroy() => PlayerSpawnSystem.RemoveSpawnPoint(transform);


    private void OnDrawGizmos()
    {
        Gizmos.color = colors;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1);
    }


}