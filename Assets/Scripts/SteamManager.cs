
using UnityEngine;
using Steamworks;

using UnityEngine.SceneManagement;




public class SteamManager : MonoBehaviour
{
    


    public static SteamManager singleton;




    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);


        Utilities.DontDestroyOnLoad(this.gameObject);


#if UNITY_SERVER
        
        StartDedicatedServer();
        ServerData.Init();
#else
        StartClient();
#endif
    }



    /// <summary>
    /// Locates the argument, then returns the string that follows that argument. For example, 
    /// if your parameter was for a maximum number of players, your argument could be -maxplayers 1000.
    /// GetArg("-maxplayers") would return "1000". Using quotations in the argument works too: ex.
    /// you could pass in -description "Here is the description", and GetArg("-description") returns
    /// the string "Here is the description".
    ///
    /// </summary>
    /// <param name="name">The name of the paramater that should be succeeded by the argument value.
    /// ex. "-description".</param>
    /// <returns></returns>
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }



#if UNITY_SERVER


    #region Dedicated Server Logic




    // Initialize server settings to their defaults

    string serverDescription = "Server Description goes here";
    ushort gamePort = 28015;
    ushort queryPort = 27016;
    string serverName = "Default Server Name";
    int maxPlayers = 100;

    
    private void CreateSteamServer()
    {

        // Define batchmode parameters here

        if (GetArg("-description") != null)
            serverDescription = GetArg("-description").Truncate(100);


        if (GetArg("-gameport") != null)
            gamePort = System.Convert.ToUInt16(GetArg("-gameport"));


        if (GetArg("-queryport") != null)
            queryPort = System.Convert.ToUInt16(GetArg("-queryport"));


        if (GetArg("-name") != null)
            serverName = GetArg("-name").Truncate(24);


        if (GetArg("-maxplayers") != null)
            maxPlayers = Mathf.Clamp(System.Convert.ToUInt16(GetArg("-maxplayers")), 0, 1000);


        SteamServerInit init = new SteamServerInit
        {
            IpAddress = System.Net.IPAddress.Any,
            Secure = true,
            DedicatedServer = true,
            GameDescription = serverDescription,
            GamePort = gamePort,
            QueryPort = queryPort,
            SteamPort = 27015,
            ModDir = "DedicatedTest",
            VersionString = "1.0.0.0"
        };


        SteamServer.Init(1551700, init, true);

    }


    private void StartDedicatedServer(){


        SteamServer.OnSteamServersConnected += OnSteamServerConnected;
        

        // Initialize the steam server
        CreateSteamServer();

        SteamServer.LogOnAnonymous();

        
    }


    void OnSteamServerConnected()
    {

        SteamServer.MaxPlayers = maxPlayers;

        SteamServer.ServerName = serverName;

        SteamServer.MapName = "Map name";

        SteamServer.Passworded = false;


        Mirror.NetworkManager.singleton.maxConnections = SteamServer.MaxPlayers;

        LogServerDetails();


        SceneManager.LoadScene(1);

        Mirror.NetworkManager.singleton.StartServer();

        Mirror.NetworkManager.singleton.ServerChangeScene("gameplay");
    }


    void LogServerDetails()
    {
        Debug.Log(" server now is logged on and has a working connection to the Steam master server.");


        if (SteamServer.LoggedOn)
            Debug.Log("[OnSteamServerConnected] Server is connected and registered with the Steam master server.");
        else
            Debug.Log("[OnSteamServerConnected] Steam server is not logged on and registered with steam master server.");


        if (SteamServer.IsValid)
            Debug.Log("[OnSteamServerConnected] Steam server is valid.");
        else
            Debug.Log("[OnSteamServerConnected] Steam server is not valid.");


        Debug.Log("[OnSteamServerConnected] Server successfuly connected. Connect using Server IP: " + SteamServer.PublicIp);
    }



    #endregion


#else


    #region Client Logic


    bool initialized = false;



    /// <summary>
    /// Returns a list of responsive server's server info when finished
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<System.Collections.Generic.List<Steamworks.Data.ServerInfo>> ResponsiveServers()
    {
        var responsive = new System.Collections.Generic.List<Steamworks.Data.ServerInfo>();


        using (var list = new Steamworks.ServerList.LocalNetwork())
        {
            await list.RunQueryAsync();

            foreach (var server in list.Responsive)
                responsive.Add(server);
        }


        using (var list = new Steamworks.ServerList.Internet())
        {
            await list.RunQueryAsync();

            foreach (var server in list.Responsive)
                responsive.Add(server);
        }


        return responsive;
    }


    void StartClient()
    {

#if !UNITY_EDITOR
        if (Steamworks.SteamClient.RestartAppIfNecessary(1551700))
            Application.Quit();
#endif

        SteamUser.OnClientGameServerDeny += Mirror.NetworkClient.Disconnect;


        // Initialize steam client
        try
        {
            SteamClient.Init(1551700, true);
            Debug.Log("Steam client initialized successfully.");

            initialized = true;

            SceneManager.LoadScene(1);
        }
        catch
        {
            Debug.Log("Could not initialize steam client. Is steam not open?");


            // Here you should throw an error and prompt user to restart


        }
    }









    private void OnDisable()
    {


        if(Mirror.NetworkClient.isConnected)
            Mirror.NetworkClient.Disconnect();

        Mirror.NetworkManager.singleton.StopClient();


#if !UNITY_EDITOR
        if (initialized)
            SteamClient.Shutdown();
#endif


    }


#endregion

#endif

    }
