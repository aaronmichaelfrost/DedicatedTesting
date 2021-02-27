using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{

    public static SteamLobby singleton;

    public static Steamworks.Data.Lobby ourLobby;

    public string lobbyName;


    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);


        Steamworks.SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        Steamworks.SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;

    }


    private void OnDisable()
    {
        Steamworks.SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        Steamworks.SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;

        ourLobby.SetPrivate();
        ourLobby.SetInvisible();
        ourLobby.Leave();
    }


    public void CreateLobby()
    {
        Debug.Log("Creating lobby");


        Steamworks.SteamMatchmaking.CreateLobbyAsync(NetworkManagerCallbacks.singleton.maxConnections);
    }



    private void OnLobbyCreated(Steamworks.Result result, Steamworks.Data.Lobby lobby)
    {
        
        Debug.Log("Lobby creation result: " + result);

        if (result == Steamworks.Result.OK) 
        {

            ourLobby = lobby;

            Debug.Log("Lobby successfully created.");


            lobby.SetPublic();

            Mirror.NetworkManager.singleton.ServerChangeScene("gameplay");


            Mirror.NetworkManager.singleton.StartHost();


            lobby.SetData("id", Steamworks.SteamClient.SteamId.ToString());

            lobby.SetData("name", lobbyName);

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


        Debug.Log("We joined a lobby.");

        Debug.Log("Connecting mirror client to the id associated with the lobby.");


        Mirror.NetworkManager.singleton.networkAddress = lobby.GetData("id");

        Mirror.NetworkManager.singleton.StartClient();
    }
}
