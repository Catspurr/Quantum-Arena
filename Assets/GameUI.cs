using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Teams
{
    public int team;
    //[SyncVar]
    public float score;

    public void AddScore(float value)
    {
        score += value;
    }
}

[RequireComponent(typeof(GameManager))]
public class GameUI : NetworkBehaviour
{
    private GameManager gameManager;
    [SyncVar(hook = nameof(HandleTimeUpdated))]
    public float time;
    public TMP_Text textTimer, maxScoreText;

    public TMP_Text[] playerScoreText, playerNameText, teamScoreText;
    public Slider[] healthSlider;

    [SyncVar(hook = nameof(HandleScoreUpdated))]
    public float score1, score2, score3, score4, health1, health2, health3, health4;

    [SyncVar(hook = nameof(HandleColors))]
    public Color color1, color2, color3, color4;

    [SyncVar(hook = nameof(HandleName))]
    public string name1, name2, name3, name4;

    //public TMP_Text team1Score, team2Score, team3Score, team4Score;
    [SyncVar(hook = nameof(HandleTeamScoreUpdated))]
    public float t1Score, t2Score, t3Score, t4Score;

    //public bool ffa;
    [SyncVar]public bool stopTimer;
    [SyncVar] public float maxScore;

    public TeamScoreManager[] teamScoreManagers; //this is for ffa and 1v1
    public TeamScoreManager[] teamGameScore; //this is for 2v2

    public float nullScore;
    [SyncVar(hook = nameof(HandleBestPlace))]
    public int first, second, third, forth;

    void Start()
    {
        gameManager = GetComponent<GameManager>();
        
        //resize the array the 1v1 and ffa teams depending on how many players are in match
        Array.Resize<TeamScoreManager>(ref teamScoreManagers, gameManager.playerList.Count); 
        for (var i = 0; i < gameManager.playerList.Count; i++)
        {
            teamScoreManagers[i].team = i + 1;
        }

    }
    public void Update()
    {

    }
    /*public int CompareInt(float a, int b)
    {

    }*/

    private void FixedUpdate()
    {
        //count down the timer
        if (time >= 0 && gameManager != null && !gameManager.gameIsOver && !stopTimer)
        {
            time -= Time.deltaTime;
        }
        
        //update the display for the tanks health score color and name
        //make this shorter if there is time

        //the reason the variables for score, color, name etc.. isn't arrays is because it wont work with syncvar
        if (gameManager.playerList.Count >= 1)
        {
            if(gameManager.playerList[0].gameObject != null)
            {
                TankController tank1 = gameManager.playerList[0].gameObject.GetComponent<TankController>();
                score1 = tank1.score;
                color1 = tank1.color;
                health1 = tank1.health;
                name1 = tank1._tankDisplayName;
            }
        }
        if (gameManager.playerList.Count >= 2)
        {
            if(gameManager.playerList[1].gameObject != null)
            {
                score2 = gameManager.playerList[1].gameObject.GetComponent<TankController>().score;
                color2 = gameManager.playerList[1].gameObject.GetComponent<TankController>().color;
                health2 = gameManager.playerList[1].gameObject.GetComponent<TankController>().health;
                name2 = gameManager.playerList[1].gameObject.GetComponent<TankController>()._tankDisplayName;
            }
        }
        if (gameManager.playerList.Count >= 3)
        {
            if (gameManager.playerList[2].gameObject != null)
            {
                score3 = gameManager.playerList[2].gameObject.GetComponent<TankController>().score;
                color3 = gameManager.playerList[2].gameObject.GetComponent<TankController>().color;
                health3 = gameManager.playerList[2].gameObject.GetComponent<TankController>().health;
                name3 = gameManager.playerList[2].gameObject.GetComponent<TankController>()._tankDisplayName;
            }
        }
        if (gameManager.playerList.Count >= 4)
        {
            if (gameManager.playerList[3].gameObject != null)
            {
                score4 = gameManager.playerList[3].gameObject.GetComponent<TankController>().score;
                color4 = gameManager.playerList[3].gameObject.GetComponent<TankController>().color;
                health4 = gameManager.playerList[3].gameObject.GetComponent<TankController>().health;
                name4 = gameManager.playerList[3].gameObject.GetComponent<TankController>()._tankDisplayName;
            }
        }

    }

    public void HandleScoreUpdated(float oldScore, float newScore)
    {

        if (playerScoreText.Length >= 1 && playerScoreText[0] != null) playerScoreText[0].text = score1.ToString();
        if (healthSlider.Length >= 1 && healthSlider[0] != null) healthSlider[0].value = health1;
        //if (teamScoreText.Length >= 1 && teamScoreText[0] != null) teamScoreText[0].text = t1Score.ToString();

        if (playerScoreText.Length >= 2 && playerScoreText[1] != null) playerScoreText[1].text = score2.ToString();
        if (healthSlider.Length >= 2 && healthSlider[1] != null) healthSlider[1].value = health2;

        if (playerScoreText.Length >= 3 && playerScoreText[2] != null) playerScoreText[2].text = score3.ToString();
        if (healthSlider.Length >= 3 && healthSlider[2] != null) healthSlider[2].value = health3;

        if (playerScoreText.Length >= 4 && playerScoreText[3] != null) playerScoreText[3].text = score4.ToString();
        if (healthSlider.Length >= 4 && healthSlider[3] != null) healthSlider[3].value = health4;


    }

    public void HandleTimeUpdated(float oldTime, float newTime)
    {
        //
        if (textTimer != null)
        {
            int minutes = Mathf.FloorToInt(time / 60.0f);
            int seconds = Mathf.FloorToInt(time - minutes * 60);
            textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            maxScoreText.text = "Playing to " + maxScore.ToString() + " pts";
            if (newTime <= 0 && gameManager != null)
            {
                stopTimer = true;
                SetTeamWinners();
                gameManager.ChangeScene();
            }
        }
    }

    public void HandleColors(Color oldColor, Color newColor)
    {

        if (playerScoreText.Length >= 1 && playerScoreText[0] != null) playerScoreText[0].color = color1;
        if (healthSlider.Length >= 1 && healthSlider[0] != null) healthSlider[0].fillRect.GetComponent<Image>().color = color1;
        if (playerNameText.Length >= 1 && playerNameText[0] != null) playerNameText[0].color = color1;

        if (playerScoreText.Length >= 2 && playerScoreText[1] != null) playerScoreText[1].color = color2;
        if (healthSlider.Length >= 2 && healthSlider[1] != null) healthSlider[1].fillRect.GetComponent<Image>().color = color2;
        if (playerNameText.Length >= 2 && playerNameText[1] != null) playerNameText[1].color = color2;

        if (playerScoreText.Length >= 3 && playerScoreText[2] != null) playerScoreText[2].color = color3;
        if (healthSlider.Length >= 3 && healthSlider[2] != null) healthSlider[2].fillRect.GetComponent<Image>().color = color3;
        if (playerNameText.Length >= 3 && playerNameText[2] != null) playerNameText[2].color = color3;

        if (playerScoreText.Length >= 4 && playerScoreText[3] != null) playerScoreText[3].color = color4;
        if (healthSlider.Length >= 4 && healthSlider[3] != null) healthSlider[3].fillRect.GetComponent<Image>().color = color4;
        if (playerNameText.Length >= 4 && playerNameText[3] != null) playerNameText[3].color = color4;

        if(playerScoreText.Length <= 0)
        {
            if (teamScoreText.Length >= 1 && teamScoreText[0] != null) teamScoreText[0].color = color1;
            if (teamScoreText.Length >= 2 && teamScoreText[1] != null) teamScoreText[1].color = color2;
            if (teamScoreText.Length >= 3 && teamScoreText[2] != null) teamScoreText[2].color = color3;
            if (teamScoreText.Length >= 4 && teamScoreText[3] != null) teamScoreText[3].color = color4;
        }

    }

    public void HandleName(string oldName, string newName)
    {

        if (playerNameText.Length >= 1 && playerNameText[0] != null) playerNameText[0].text = name1;
        if (playerNameText.Length >= 2 && playerNameText[1] != null) playerNameText[1].text = name2;
        if (playerNameText.Length >= 3 && playerNameText[2] != null) playerNameText[2].text = name3;
        if (playerNameText.Length >= 4 && playerNameText[3] != null) playerNameText[3].text = name4;
    }
    /*public static float SortByScore(GameManager., t2Score)
    {
        return t1Score.CompareTo(t2Score);
    }*/
    public void AddTeamScore(float value, int team)
    {
        if (gameManager != null && !gameManager.gameIsOver)
        {
            if (team == 1)
            {
                if (teamScoreManagers.Length >= 1) teamScoreManagers[0].score += value;
                if (teamGameScore.Length >= 1) teamGameScore[0].score += value;
                t1Score += value;
                //team1Score.text = t1Score.ToString();
                if (teamScoreText.Length >= 1 && teamScoreText[0] != null) teamScoreText[0].text = t1Score.ToString();

            }
            else if (team == 2)
            {
                if (teamScoreManagers.Length >= 2) teamScoreManagers[1].score += value;
                if (teamGameScore.Length >= 2) teamGameScore[1].score += value;
                t2Score += value;
                //team2Score.text = t2Score.ToString();
                if (teamScoreText.Length >= 2 && teamScoreText[1] != null) teamScoreText[1].text = t2Score.ToString();

            }
            else if (team == 3)
            {
                if (teamScoreManagers.Length >= 3) teamScoreManagers[2].score += value;
                t3Score += value;
            }
            else if (team == 4)
            {
                if (teamScoreManagers.Length >= 4) teamScoreManagers[3].score += value;
                t4Score += value;
            }
            else
            {
                Debug.Log("not added");
            }


            if (t1Score >= maxScore || t2Score >= maxScore || t3Score >= maxScore || t4Score >= maxScore)
            {
                stopTimer = true;
                gameManager.gameIsOver = true;
                SetTeamWinners();
                StartCoroutine(MaxScoreReached());

            }
        }
        

        
    }

    //[ClientRpc]
    public IEnumerator MaxScoreReached()
    {
        yield return new WaitForSeconds(0.2f);
        gameManager.ChangeScene();
    }


    public void HandleTeamScoreUpdated(float oldValue, float newValue)
    {
        //if (team1Score != null) team1Score.text = t1Score.ToString();

        //if (team2Score != null) team2Score.text = t2Score.ToString();

        if (teamScoreText.Length >= 1 && teamScoreText[0] != null) teamScoreText[0].text = t1Score.ToString();

        if (teamScoreText.Length >= 2 && teamScoreText[1] != null) teamScoreText[1].text = t2Score.ToString();

        if (teamScoreText.Length >= 3 && teamScoreText[2] != null) teamScoreText[2].text = t3Score.ToString();

        if (teamScoreText.Length >= 4 && teamScoreText[3] != null) teamScoreText[3].text = t4Score.ToString();
    }


    [Server]
    public void SetTeamWinners()
    {
        //if (!isServer) return;
        /*int eee = teamList[0].team;
        CmdSetWinners(eee);*/
        if (!gameManager.TeamGame)
        {
            Array.Sort(teamScoreManagers, delegate (TeamScoreManager x, TeamScoreManager y) { return y.score.CompareTo(x.score); });
            if (teamScoreManagers.Length >= 1) first = teamScoreManagers[0].team;
            if (teamScoreManagers.Length >= 2) second = teamScoreManagers[1].team;
            if (teamScoreManagers.Length >= 3) third = teamScoreManagers[2].team;
            if (teamScoreManagers.Length >= 4) forth = teamScoreManagers[3].team;
            
        } else
        {
            Array.Sort(teamGameScore, delegate (TeamScoreManager x, TeamScoreManager y) { return y.score.CompareTo(x.score); });
            first = teamGameScore[0].team;
            second = teamGameScore[1].team;
            third = 3;
            forth = 4;
        }
        

        CmdSetWinners(first, second, third, forth);
    }
    [ClientRpc]
    public void CmdSetWinners(int value1st, int value2nd, int value3rd, int value4th)
    {
        first = value1st;
        second = value2nd;
        third = value3rd;
        forth = value4th;
    }

    public void HandleBestPlace(int oldValue, int newValue)
    {
        //first = newValue;
    }
    
}
