using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreenFloor : MonoBehaviour
{
    public Color[] floorColors;
    public Renderer floorRenderer;
    public WinScreenTank tankFFA, tankTeams;

    public bool teamGame;

    void Start()
    {
        
    }
    void Update()
    {
        if (!teamGame)
        {
            floorRenderer.material.color = floorColors[tankFFA.color];
        }
        else
        {
            floorRenderer.material.color = floorColors[tankTeams.color];
        }
        
    }
}
