using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WinScreenTank : NetworkBehaviour
{
    // Start is called before the first frame update
    public Color[] winColors;
    public Color[] losingColors;

    //public Renderer render;
    public bool winner;

    public int place;
    public int team;
    [SyncVar(hook = nameof(SetColor))]
    public int color;

    [SyncVar] public string tankName;
    [SyncVar] public float teamScore;

    [SyncVar] public float playerScore;

    [SyncVar] public float kills;

    [SyncVar] public bool inGame;

    

    

    /*[SyncVar(hook = nameof(EnableOrDisable))]
    public bool active = true;*/

    //public GameUI
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //SetColor(team, team);
    }

    /*public void SetWin()
    {
        if(place == 1)
        {

        }
    }*/

    //[Command]
    public void SetColor(int oldColor, int colorId)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            if (winner)
            {
                //render.material.color(winColors[colorId]);
                render.material.color = winColors[colorId];
            } else
            {
                render.material.color = losingColors[colorId];
            }
            
        }
    }

    /*public void EnableOrDisable(bool oldValue, bool newValue)
    {
        gameObject.SetActive(newValue);
    }*/

    [ClientRpc]
    public void RemoveTank()
    {
        gameObject.SetActive(false);
    }
}
