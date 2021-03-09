﻿using UnityEngine;

public class SteamLobby : MonoBehaviour
{

    public static SteamLobby singleton;


    // The lobby that we are hosting or are a member of
    public static Steamworks.Data.Lobby myLobby;

    public string lobbyName;


    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);

        Utilities.DontDestroyOnLoad(this.gameObject);


        Steamworks.SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        Steamworks.SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;


    }


    private void OnDisable()
    {
        Steamworks.SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        Steamworks.SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;

        myLobby.SetPrivate();
        myLobby.SetInvisible();
        myLobby.Leave();
    }


    public static void CreateLobby()
    {
        Debug.Log("Creating lobby");


        Steamworks.SteamMatchmaking.CreateLobbyAsync(NetworkManagerCallbacks.singleton.maxConnections);
    }



    /// <summary>
    /// Disconnects from lobby if you are a client. If you are a host this functions shuts down the server and disconnect all players
    /// </summary>
    public void LeaveLobby()
    {

        // If we are hosting, then shut down the lobby
        if (Mirror.NetworkServer.active)
        {

            Debug.Log("Closing lobby");

            myLobby.SetPrivate();
            myLobby.SetInvisible();
        }


        Debug.Log("Leaving lobby");
        if (myLobby.Data != null)
            myLobby.Leave();
    }



    private void OnLobbyCreated(Steamworks.Result result, Steamworks.Data.Lobby lobby)
    {
        
        Debug.Log("Lobby creation result: " + result);

        if (result == Steamworks.Result.OK) 
        {
            myLobby = lobby;

            lobby.SetData("id", Steamworks.SteamClient.SteamId.ToString());

            lobby.SetData("name", lobbyName);

            lobby.SetPublic();


            Debug.Log("Starting network host");


            // Start host
            NetworkManagerCallbacks.singleton.StartHost();


            Mirror.NetworkManager.singleton.ServerChangeScene("gameplay");

        }
        else
        {
            Debug.Log("Unable to create lobby.");
        }
    }


    private void OnLobbyEntered(Steamworks.Data.Lobby lobby)
    {

        // Return if we are the host
        if (Mirror.NetworkServer.active)
            return;


        myLobby = lobby;

        Debug.Log("We joined a lobby.");

        Debug.Log("Connecting mirror client to the id associated with the lobby.");


        Mirror.NetworkManager.singleton.networkAddress = lobby.GetData("id");

        Mirror.NetworkManager.singleton.StartClient();
    }
}
