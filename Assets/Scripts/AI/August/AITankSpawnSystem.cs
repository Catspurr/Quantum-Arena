using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(GameManager))]
public class AITankSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject[] teamAIPrefabs = new GameObject[4];

    [SerializeField] private GameManager gameManager;

    [SerializeField] private List<Transform> aiSpawnPoints = new List<Transform>();

    public List<List<GameObject>> botsPerTeam = new List<List<GameObject>>(2);

    [ServerCallback]
    public void FirstSpawn()
    {

        //var list2 = new List<GameObject>();
        //botsPerTeam.Add(list2);

        //StartCoroutine(SpawnDelay());
        for (var i = 0; i < aiSpawnPoints.Count; i++) //Goes through each spawn point on the map
        {
            var list = new List<GameObject>();
            botsPerTeam.Add(list);
            var aiSpawnPoint = aiSpawnPoints[i].GetComponent<AISpawnPoint>(); //Gets the specific spawn point we are spawning from currently
            var botInstance = Instantiate(teamAIPrefabs[i], aiSpawnPoints[i].position, aiSpawnPoints[i].rotation); //Instantiates the prefab for the right team depending on which spawn point we are on and sets the position to be the spawn points position plus the random value
            botInstance.GetComponent<AITankController>().GetTankSpawnSystem(this); //Sends the spawn system to the ai so they can reference the other bots
            NetworkServer.Spawn(botInstance);
        }
    }

    private bool ShouldSpawn(int teamNumber)
    {
        if (botsPerTeam[teamNumber].Count == 0)
            return true;
        print(botsPerTeam[teamNumber].Count);
        return botsPerTeam[teamNumber].Count < gameManager.maxBotsPerTeamAtOnce;
    }

}