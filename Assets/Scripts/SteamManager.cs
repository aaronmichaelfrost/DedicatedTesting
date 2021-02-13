
using UnityEngine;
using Steamworks;




public class SteamManager : MonoBehaviour
{

    public static SteamManager Instance;

    // Server listing UI
    public GameObject serverListingPrefab;
    public Transform serverListingSpawnPosition;




    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);



#if UNITY_SERVER
        StartDedicatedServer();
#else
        StartClient();
#endif

        DontDestroyOnLoad(this.gameObject);
    }


#if UNITY_SERVER


#region Dedicated Server Logic

    void StartDedicatedServer(){



        SteamServer.OnSteamServersConnected += OnSteamServerConnected;


        SteamServerInit init = new SteamServerInit
        {
            IpAddress = System.Net.IPAddress.Any,
            Secure = true,
            DedicatedServer = true,
            GameDescription = "Game Description",
            GamePort = 28015,
            QueryPort = 27016,
            SteamPort = 27015,
            ModDir = "DedicatedTest",
            VersionString = "1.0.0.0"
        };

        SteamServer.Init(1551700, init, true);


        SteamServer.LogOnAnonymous();

        SteamServer.MapName = "Map name";
        SteamServer.Passworded = false;
        SteamServer.ServerName = "Server Name Here";

    }


    void OnSteamServerConnected()
    {
        LogServerDetails();
    }


    void LogServerDetails()
    {
        Debug.Log("[OnSteamServerConnected] server now is logged on and has a working connection to the Steam master server.");



        if (SteamServer.LoggedOn)
            Debug.Log("[Aaron] Server is connected and registered with the Steam master server.");
        else
            Debug.Log("[Aaron] Steam server is not logged on and registered with steam master server.");


        if (SteamServer.IsValid)
            Debug.Log("[Aaron] Steam server is valid.");
        else
            Debug.Log("[Aaron] Steam server is not valid.");




        Debug.Log("[Aaron] Server IP: " + SteamServer.PublicIp);
    }


#endregion


#else


#region Client Logic


    void StartClient()
    {

        // Initialize steam client
        try
        {
            SteamClient.Init(1551700, true);
            Debug.Log("Steam client initialized.");

        }
        catch
        {
            Debug.Log("Could not initialize steam client. Is steam not open?");
        }


        Debug.Log("Client started!");
    }




    async void RefreshServerList()
    {
        // Clear the current list
        foreach (Transform child in serverListingSpawnPosition.transform)
        {
            Destroy(child.gameObject);
        }


        // Add all servers from the local network to the list

        using (var list = new Steamworks.ServerList.LocalNetwork())
        {
            await list.RunQueryAsync();

            foreach (var server in list.Responsive)
            {
                SpawnServerListing(server);

                Debug.Log($"Server found with address: {server.Address} name: {server.Name}");
            }
                

            Debug.Log("Found " + list.Responsive.Count + " local network servers.");
        }


        // Add all servers from the internet to the list

        using (var list = new Steamworks.ServerList.Internet())
        {
            await list.RunQueryAsync();

            foreach (var server in list.Responsive)
            {
                SpawnServerListing(server);

                Debug.Log($"Server found with address: {server.Address} name: {server.Name}");
            }


            Debug.Log("Found " + list.Responsive.Count + " internet servers.");
        }
    }


    void SpawnServerListing(Steamworks.Data.ServerInfo server)
    {

        // Spawn server listing
        ServerListingUI s = Instantiate(serverListingPrefab, serverListingSpawnPosition).GetComponent<ServerListingUI>();

        s.players.text = server.Players.ToString();
        s.steamId.text = server.SteamId.ToString();
        s.address.text = server.Address.ToString();
        s.serverName.text = server.Name;
    }






    private void OnGUI()
    {

        GUILayout.BeginArea(new Rect(40, 40, 215, 9999));


        if (SteamClient.IsLoggedOn)
        {
            if (GUILayout.Button("Refresh Server List"))
                RefreshServerList();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Connect"))
                Mirror.NetworkManager.singleton.StartClient();

            Mirror.NetworkManager.singleton.networkAddress = GUILayout.TextField(Mirror.NetworkManager.singleton.networkAddress);

        }
        else
        {
            GUILayout.Label("Not connected to steam.");
        }


        GUILayout.EndArea();

    }


    private void OnDisable()
    {
        Mirror.NetworkManager.singleton.StopClient();

        SteamClient.Shutdown();
    }


    #endregion

#endif

}
