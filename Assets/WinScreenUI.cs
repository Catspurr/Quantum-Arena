using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//[RequireComponent(typeof(GameManager))]
public class WinScreenUI : NetworkBehaviour
{
    public GameManager gameManager;
    //private GameUI gameUI;
    public TMP_Text winMessage;
    public TMP_Text[] nameText, teamScoreText, playerScoreText, killsText;

    public Image[] scoreBoardBackgrounds;

    public Color[] scoreBoardColors, textColors;

    // Start is called before the first frame update
    
    void Start()
    {
        //gameManager = GetComponent<GameManager>();
        //gameUI.GetComponent<GameUI>();

        //gameManager.CmdFindNetworkManager();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.TeamGame)
        {
            UpdateScoreBoard();
        } else
        {
            UpdateScoreBoardTeams();
        }
        
    }

    [ClientCallback]
    public void UpdateScoreBoard()
    {
        /*if (gameManager.winScreenTanks[0].inGame)
        {
            nameText[0].text = gameManager.winScreenTanks[0].tankName.ToString();
            teamScoreText[0].text = gameManager.winScreenTanks[0].teamScore.ToString();
            playerScoreText[0].text = gameManager.winScreenTanks[0].playerScore.ToString();
            killsText[0].text = gameManager.winScreenTanks[0].kills.ToString();
            scoreBoardBackgrounds[0].color = colors[gameManager.winScreenTanks[0].color];
        }*/
        if (gameManager.winScreenTanks[0].inGame)
        {
            winMessage.text = gameManager.winScreenTanks[0].tankName.ToString() + " Win!";
            winMessage.color = textColors[gameManager.winScreenTanks[0].color];
        }

        for (var i = 0; i < gameManager.winScreenTanks.Length; i++)
        {
            if (gameManager.winScreenTanks[i].inGame)
            {
                nameText[i].text = gameManager.winScreenTanks[i].tankName.ToString();
                teamScoreText[i].text = gameManager.winScreenTanks[i].teamScore.ToString();
                playerScoreText[i].text = gameManager.winScreenTanks[i].playerScore.ToString();
                killsText[i].text = gameManager.winScreenTanks[i].kills.ToString();
                scoreBoardBackgrounds[i].color = scoreBoardColors[gameManager.winScreenTanks[i].color];
            }
        }

    }

    [ClientCallback]
    public void UpdateScoreBoardTeams()
    {
        if(gameManager.gameUI.first == 1)
        {
            winMessage.text = "Team1 Win!";
            winMessage.color = textColors[0];
        }
        else
        {
            winMessage.text = "Team2 Win!";
            winMessage.color = textColors[1];
        }

        for (var i = 0; i < gameManager.winScreenTanksTeam.Length; i++)
        {
            if (gameManager.winScreenTanksTeam[i].inGame)
            {
                nameText[i].text = gameManager.winScreenTanksTeam[i].tankName.ToString();
                teamScoreText[i].text = gameManager.winScreenTanksTeam[i].teamScore.ToString();
                playerScoreText[i].text = gameManager.winScreenTanksTeam[i].playerScore.ToString();
                killsText[i].text = gameManager.winScreenTanksTeam[i].kills.ToString();
                //scoreBoardBackgrounds[i].color = scoreBoardColors[gameManager.winScreenTanks[i].color];
            }
        }

    }


    public void ReturnToTitleScreen()
    {

        if (isServer)
        {
            //Destroy(gameManager.networkManager.gameObject);
            gameManager.networkManager.StopHost();
            //gameManager.networkManager.usingSteam.
        }
        else
        {
            //gameManager.networkManager.StopClient();
            GameObject.FindWithTag("NetworkManager").GetComponent<MyNetworkManager>().StopClient();
        }
    }

}
