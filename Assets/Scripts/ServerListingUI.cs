﻿#if !UNITY_SERVER


using TMPro;
using UnityEngine;

public class ServerListingUI : MonoBehaviour
{
    public TextMeshProUGUI serverName;

    public TextMeshProUGUI players;

    public Steamworks.Data.ServerInfo server;


    /// <summary>
    /// Connect input was true. Try to connect to the server
    /// </summary>
    public void Connect()
    {

        Debug.Log("Connecting to server " + server.Address.ToString());

        Mirror.NetworkManager.singleton.networkAddress = server.SteamId.ToString();
        //Mirror.NetworkManager.singleton.networkAddress = server.Address.ToString();
        Mirror.NetworkManager.singleton.StartClient();

        

    }


    /// <summary>
    /// Spawns the listing UI prefab and initializes it's values
    /// </summary>
    /// <param name="server"></param>
    public static void CreateListing(Steamworks.Data.ServerInfo server)
    {

        ServerListingUI s = Instantiate(MainMenu.singleton.serverListingPrefab, MainMenu.singleton.serverListingParent).GetComponent<ServerListingUI>();

        s.server = server;
        s.Init();
    }


    /// <summary>
    /// Deletes all children of the server listing transform parent
    /// </summary>
    public static void Clear()
    {
        
        foreach (Transform child in MainMenu.singleton.serverListingParent)
            if(child != null && child.gameObject) Destroy(child.gameObject);
    }


    /// <summary>
    /// Initializes UI fields
    /// </summary>
    public void Init()
    {
        players.text = server.Players + "/" + server.MaxPlayers;
        serverName.text = server.Name;
    }
}


#endif