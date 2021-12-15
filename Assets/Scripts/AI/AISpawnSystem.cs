using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class AISpawnSystem : NetworkBehaviour
{
     [Header("Editor drag-ins")]
     public GameManager gameManager;
     [SerializeField] private GameObject robotPrefab;
     [SerializeField] private List<Transform> aiSpawnPoints = new List<Transform>(2);
     [SyncVar]public Color _colorTeam1, _colorTeam2;

     //Jagged list, capacity of 2 because there is only 2 teams
     public List<List<GameObject>> botsPerTeam = new List<List<GameObject>>(2);

     public void FirstSpawn()
     {
          foreach (var player in gameManager.playerList)
          {
               var tankController = player.GetComponent<TankController>();
               if (tankController.team != 1) continue;
               _colorTeam1 = gameManager.colors[tankController.colorId];
               break;
          }
          foreach (var player in gameManager.playerList)
          {
               var tankController = player.GetComponent<TankController>();
               if (tankController.team != 2) continue;
               _colorTeam2 = gameManager.colors[tankController.colorId];
               break;
          }
          
          
          StartCoroutine(SpawnDelay()); //Starts the timer for the next spawn wave to keep spawning bots
          for (var i = 0; i < aiSpawnPoints.Count; i++) //Goes through each spawn point on the map
          {
               var list = new List<GameObject>();
               botsPerTeam.Add(list);
               var aiSpawnPoint = aiSpawnPoints[i].GetComponent<AISpawnPoint>(); //Gets the specific spawn point we are spawning from currently
               for (var j = 0; j < gameManager.amountOfBotsPerWave; j++) //Repeats the spawning process for as many bots we have set to spawn per wave
               {
                    var randomSpawnPoint = new Vector3(
                         Random.Range(-aiSpawnPoint.width / 2, aiSpawnPoint.width / 2),
                         aiSpawnPoints[i].position.y,
                         Random.Range(-aiSpawnPoint.height / 2, aiSpawnPoint.height / 2)); //Randomizes the spawn point within its boundaries
                    var botInstance = Instantiate(
                         robotPrefab, 
                         aiSpawnPoints[i].position + randomSpawnPoint, 
                         aiSpawnPoints[i].rotation); //Instantiates the prefab for the right team depending on which spawn point we are on and sets the position to be the spawn points position plus the random value
                    
                    botsPerTeam[i].Add(botInstance); //Adds the bot to the jagged list of bots for the right team
                    var aiControllerInstance = botInstance.GetComponent<AIController>();
                    aiControllerInstance.team = i+1; //Sets the team
                    botInstance.layer = aiControllerInstance.team + 5; //Team layers start at layer 6
                    aiControllerInstance.GetSpawnSystem(this); //Sends the spawn system to the ai so they can reference the other bots
                    
                    
                    
                    foreach (var render in botInstance.GetComponentsInChildren<Renderer>())
                    {
                         render.material.color = aiControllerInstance.team == 1
                              ? _colorTeam1
                              : _colorTeam2;
                    }
                    foreach (var render in aiControllerInstance.gunMeshes)
                    {
                         render.material.color = Color.black;
                    }
                    
                    NetworkServer.Spawn(botInstance); //Spawns the prefab on the server
                SetColor(botInstance);
            }
          }
     }
     
     [Server]
     private void SpawnWave()
     {
          StartCoroutine(SpawnDelay());

          for (var i = 0; i < aiSpawnPoints.Count; i++) //Goes through each spawn point on the map
          {
               if (!ShouldSpawn(i)) return; //Doesnt spawn more mobs if we have more than max allowed bots
               var aiSpawnPoint = aiSpawnPoints[i].GetComponent<AISpawnPoint>(); //Gets the specific spawn point we are spawning from currently
               for (var j = 0; j < gameManager.amountOfBotsPerWave; j++) //Repeats the spawning process for as many bots we have set to spawn per wave
               {
                    var randomSpawnPoint = new Vector3(
                         Random.Range(-aiSpawnPoint.width / 2, aiSpawnPoint.width / 2),
                         aiSpawnPoints[i].position.y,
                         Random.Range(-aiSpawnPoint.height / 2, aiSpawnPoint.height / 2)); //Randomizes the spawn point within its boundaries
                    var botInstance = Instantiate(
                         robotPrefab, 
                         aiSpawnPoints[i].position + randomSpawnPoint, 
                         aiSpawnPoints[i].rotation); //Instantiates the prefab for the right team depending on which spawn point we are on and sets the position to be the spawn points position plus the random value
                    
                    botsPerTeam[i].Add(botInstance); //Adds the bot to the jagged list of bots for the right team
                    var aiControllerInstance = botInstance.GetComponent<AIController>();
                    aiControllerInstance.team = i+1; //Sets the team
                    botInstance.layer = aiControllerInstance.team + 5; //Team layers start at layer 6
                    aiControllerInstance.GetSpawnSystem(this); //Sends the spawn system to the ai so they can reference the other bots
                    
                    foreach (var render in botInstance.GetComponentsInChildren<Renderer>())
                    {
                         render.material.color = aiControllerInstance.team == 1
                              ? _colorTeam1
                              : _colorTeam2;
                    }
                    foreach (var render in aiControllerInstance.gunMeshes)
                    {
                         render.material.color = Color.black;
                    }
                
                NetworkServer.Spawn(botInstance); //Spawns the prefab on the server
                SetColor(botInstance);

            }
          }
     }

    [ClientRpc]
    public void SetColor(GameObject bot)
    {
        var aiControllerInstance = bot.GetComponent<AIController>();
        foreach (var render in bot.GetComponentsInChildren<Renderer>())
        {
            render.material.color = aiControllerInstance.team == 1
                 ? _colorTeam1
                 : _colorTeam2;
        }
        foreach (var render in aiControllerInstance.gunMeshes)
        {
            render.material.color = Color.black;
        }
    }

     private IEnumerator SpawnDelay()
     {
          yield return new WaitForSeconds(gameManager.secondsBetweenSpawningBots);
          SpawnWave();
     }

     private bool ShouldSpawn(int teamNumber)
     {
          if (botsPerTeam[teamNumber].Count == 0)
               return true;
          return botsPerTeam[teamNumber].Count < gameManager.maxBotsPerTeamAtOnce;
     }
}