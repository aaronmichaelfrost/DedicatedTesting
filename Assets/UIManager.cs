using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager singleton;



    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);
    }


    public GameObject serverListingPrefab;
    public Transform serverListingSpawnPosition;


    public void ClearServerList()
    {
        foreach (Transform child in serverListingSpawnPosition.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddServerListing(Steamworks.Data.ServerInfo server)
    {
        ServerListingUI s = Instantiate(serverListingPrefab, serverListingSpawnPosition).GetComponent<ServerListingUI>();

        s.players.text = server.Players.ToString();
        s.steamId.text = server.SteamId.ToString();
        s.address.text = server.Address.ToString();
        s.serverName.text = server.Name;
    }



}
