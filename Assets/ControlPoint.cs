using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ControlPoint : NetworkBehaviour
{
    // Start is called before the first frame update
    public int amountOfOBjects;
    [SyncVar(hook = nameof(HandleObjectAmount))]
    public int amountOfTeam1;

    [SyncVar(hook = nameof(HandleObjectAmount2))]
    public int amountOfTeam2;
    //public GameObject;
    public Renderer matRenderer;
    public Material matDraw, mat1, mat2;

    public Material[] colors;
    [SyncVar] public int team1ColorId = 0, team2ColorId = 1;

    public List<AIController> aiBotLists;
    public List<TankController> tankList;
    void Start()
    {
        
    }

    // Update is called once per frame
    //[ServerCallback]
    //[ClientCallback]
    void Update()
    {
        if(amountOfTeam1 > amountOfTeam2)
        {
            matRenderer.material = colors[team1ColorId];
            /*if (gameUI != null)
                gameUI.t1Score++;*/

        } else if (amountOfTeam1 < amountOfTeam2)
        {
            matRenderer.material = colors[team2ColorId];
            /*if (gameUI != null)
                gameUI.t2Score++;*/
        } else
        {
            matRenderer.material = matDraw;
        }
    }
    //[ServerCallback]
    //[ClientCallback]
    private void OnTriggerStay(Collider other)
    {
        /*foreach(GameObject bot in GameObject.FindGameObjectsWithTag("AI Bots"))
        {
            
        }*/
        var aiBots = other.GetComponent<AIController>();
        var tanks = other.GetComponent<TankController>();

        for (int i = 0; i < aiBotLists.Count; i++)
        {
            if(aiBotLists[i] == null)
            {
                aiBotLists.RemoveAt(i);
                //amountOfOBjects--;
            }

        }
        for (int i = 0; i < tankList.Count; i++)
        {
            if (tankList[i] == null)
            {
                tankList.RemoveAt(i);
                //amountOfOBjects--;
                
            }
            if (tankList[i].isDead)
            {
                
                amountOfOBjects--;
                if (tankList[i].team == 1)
                {
                    //amountOfTeam1--;
                    RemoveFromPoint(1);
                }
                else
                {
                    //amountOfTeam2--;
                    RemoveFromPoint(2);
                }
                tankList.RemoveAt(i);
            }
        }


        if (aiBots != null && !aiBotLists.Contains(aiBots))
        {

            aiBotLists.Add(aiBots);
            amountOfOBjects++;
            if(aiBots.team == 1)
            {
                //amountOfTeam1++;
                AddToPoint(1);
            } 
            else
            {
                //amountOfTeam2++;
                AddToPoint(2);
            }

        }
        if(aiBots != null)
        {
            if (aiBots.health <= 0)
            {
                amountOfOBjects--;
                
                if (aiBots.team == 1)
                {
                    //amountOfTeam1--;
                    RemoveFromPoint(1);
                }
                else
                {
                    //amountOfTeam2--;
                    RemoveFromPoint(2);
                }
            }
        }

        if(tanks != null)
        {
            if (!tankList.Contains(tanks) && !tanks.isDead)
            {
                tankList.Add(tanks);
                amountOfOBjects++;
                if (tanks.team == 1)
                {
                    //amountOfTeam1++;
                    AddToPoint(1);
                }
                else
                {
                    //amountOfTeam2++;
                    AddToPoint(2);
                }
            }

        }
    }
    //[ServerCallback]
    //[ClientCallback]
    private void OnTriggerExit(Collider other)
    {
        var aiBots = other.GetComponent<AIController>();
        var tanks = other.GetComponent<TankController>();

        if (aiBots != null)
        {
            amountOfOBjects--;
            aiBotLists.Remove(aiBots);
            if (aiBots.team == 1)
            {
                //amountOfTeam1--;
                RemoveFromPoint(1);
            }
            else
            {
                //amountOfTeam2--;
                RemoveFromPoint(2);
            }
        }

        if (tanks != null)
        {
            amountOfOBjects--;
            tankList.Remove(tanks);
            if (tanks.team == 1)
            {
                //amountOfTeam1--;
                RemoveFromPoint(1);
            }
            else
            {
                //amountOfTeam2--;
                RemoveFromPoint(2);
            }
        }
        /*foreach (AIController bot in aiBotLists)
        {
            Debug.Log(bot.team);
            if (bot.team == 2)
            {
                amountOfTeam2++;

            }
        }*/
    }
    [ServerCallback]
    public void AddToPoint(int team)
    {
        if(team == 1)
        {
            amountOfTeam1++;
        } else
        {
            amountOfTeam2++;
        }
    }
    [ServerCallback]
    public void RemoveFromPoint(int team)
    {
        if (team == 1)
        {
            amountOfTeam1--;
        }
        else
        {
            amountOfTeam2--;
        }
    }


    public void HandleObjectAmount(int oldValue, int newValue)
    {
        amountOfTeam1 = newValue;
    }
    public void HandleObjectAmount2(int oldValue, int newValue)
    {
        amountOfTeam2 = newValue;
    }
}
