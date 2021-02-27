#if !UNITY_SERVER


using TMPro;
using UnityEngine;

public class ServerListingUI : MonoBehaviour
{
    public TextMeshProUGUI serverName;

    public TextMeshProUGUI players;

    public Steamworks.Data.ServerInfo server;

    public Steamworks.Data.Lobby lobby;


    public bool isDedicated = false;


    /// <summary>
    /// Connect input was true. Try to connect to the server
    /// </summary>
    public async void Connect()
    {

        Debug.Log("Connecting to game!");

        if (isDedicated)
        {

            // Connect to the dedicated server

            Mirror.NetworkManager.singleton.networkAddress = server.SteamId.ToString();
            Mirror.NetworkManager.singleton.StartClient();
        }
        else
        {
            // Try to connect to the client hosted lobby

            var result = await lobby.Join();

            if (result == Steamworks.RoomEnter.Success)
                Debug.Log("Success!");

            // Now wait for the SteamLobby.OnLobbyEntered Call back to trigger so we can connect to the server
        }
    }


    /// <summary>
    /// Spawns the listing UI prefab and initializes it's values
    /// </summary>
    /// <param name="server"></param>
    public static void CreateLobbyListing(Steamworks.Data.Lobby lobby)
    {

        ServerListingUI s = Instantiate(MainMenu.singleton.serverListingPrefab, MainMenu.singleton.serverListingParent).GetComponent<ServerListingUI>();

        s.isDedicated = false;

        s.lobby = lobby;
        s.InitLobby();
    }


    /// <summary>
    /// Spawns the listing UI prefab and initializes it's values
    /// </summary>
    /// <param name="server"></param>
    public static void CreateServerListing(Steamworks.Data.ServerInfo server)
    {

        ServerListingUI s = Instantiate(MainMenu.singleton.serverListingPrefab, MainMenu.singleton.serverListingParent).GetComponent<ServerListingUI>();

        s.isDedicated = true;

        s.server = server;
        s.InitServer();
    }


    /// <summary>
    /// Deletes all children of the server listing transform parent
    /// </summary>
    public static void Clear()
    {
        if(MainMenu.singleton != null)
            foreach (Transform child in MainMenu.singleton.serverListingParent)
                if(child.gameObject) Destroy(child.gameObject);
    }



    private void InitLobby()
    {
        Debug.Log("Initializing a lobby listing prefab.");

        players.text = lobby.MemberCount + "/" + lobby.MaxMembers;
        serverName.text = lobby.GetData("name");
    }


    /// <summary>
    /// Initializes UI fields
    /// </summary>
    private void InitServer()
    {
        players.text = server.Players + "/" + server.MaxPlayers;
        serverName.text = server.Name;
    }
}


#endif