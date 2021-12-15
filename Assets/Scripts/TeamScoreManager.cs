using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[Serializable]
public class TeamScoreManager 
{
    public int team;
    //[SyncVar(hook = nameof(AddScore))]
    public float score;
    //public int colorId;

    /*public void AddScore(float oldValue, float newValue)
    {
        score = newValue;
    }*/
}
