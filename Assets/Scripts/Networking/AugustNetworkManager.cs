using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AugustNetworkManager : NetworkManager
{
    public GameObject[] playerPrefabs;
    public int playerCount = 0;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefabs[playerCount], startPos.position, startPos.rotation)
            : Instantiate(playerPrefabs[playerCount]);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefabs[playerCount].name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
        playerCount++;
    }
}
