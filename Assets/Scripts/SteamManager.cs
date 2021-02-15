
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
        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequestServer;
#else
        StartClient();

        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequestClient;
        SteamUser.OnValidateAuthTicketResponse += OnClientValidateAuthTicketResponse;
        
#endif
    }






#if !UNITY_SERVER

    private void OnClientValidateAuthTicketResponse(SteamId myid, SteamId ownerid, AuthResponse response)
    {
        switch (response)
        {
            case AuthResponse.OK:
                // We were authenticated by the server
                Mirror.NetworkManager.singleton.StartClient();

                Debug.Log("[OnValidateAuthTicketResponse] Our auth ticket was validated!");
                break;
            case AuthResponse.UserNotConnectedToSteam:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. Client not connected to steam.");
                break;
            case AuthResponse.NoLicenseOrExpired:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. Game is not licensed or expired.");
                break;
            case AuthResponse.VACBanned:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. Client is VAC banned.");
                break;
            case AuthResponse.LoggedInElseWhere:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. Logged in elsewhere.");
                break;
            case AuthResponse.VACCheckTimedOut:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. VAC check timed out.");
                break;
            case AuthResponse.AuthTicketCanceled:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. Auth ticket was cancelled.");
                break;
            case AuthResponse.AuthTicketInvalidAlreadyUsed:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. Auth ticket was already used.");
                break;
            case AuthResponse.AuthTicketInvalid:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid.");
                break;
            case AuthResponse.PublisherIssuedBan:
                Debug.LogWarning("[OnValidateAuthTicketResponse] Our auth ticket was invalid. Client has a publisher issued ban.");
                break;
            default:
                break;
        }

        SteamUser.EndAuthSession(SteamClient.SteamId);

    }


#endif


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

        
        Mirror.NetworkManager.singleton.StartServer();

        Mirror.NetworkManager.singleton.maxConnections = SteamServer.MaxPlayers;

        LogServerDetails();


        SceneManager.LoadScene(1);
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




        Debug.Log("[Aaron] Server successfuly connected. Connect using Server IP: " + SteamServer.PublicIp);
    }


    private void OnP2PSessionRequestServer(SteamId id)
    {
        if(!IsBanned(id))
            SteamNetworking.AcceptP2PSessionWithUser(id);
    }

    private void Update()
    {
        if (SteamNetworking.IsP2PPacketAvailable(0))
        {
            Steamworks.Data.P2Packet? p = SteamNetworking.ReadP2PPacket(0);

            if (p.HasValue)
            {
                string s = System.Text.Encoding.ASCII.GetString(p.Value.Data);

                if (s.Contains("join "))
                {
                    string name = s.Remove(0, 5);


                    Debug.Log(name + " has joined the game! - " + p.Value.SteamId);

                    SteamNetworking.SendP2PPacket(p.Value.SteamId, System.Text.Encoding.ASCII.GetBytes("join"), -1, 0, P2PSend.Reliable);
                }

                
            }
        }
    }


    public void Kick(Steamworks.SteamId x)
    {
        // Send a message to the user to tell them they were kicked

        Debug.Log("Kicking user with steam id: " + x);

        SteamNetworking.SendP2PPacket(x, System.Text.Encoding.ASCII.GetBytes("kicked"), -1, 0, P2PSend.Reliable);


        SteamServer.EndSession(x);


    }


    public bool IsBanned(Steamworks.SteamId id)
    {

        return false;
    }

    /// <summary>
    /// Returns true if user's steamid is on the server's moderator list
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsModerator(Steamworks.SteamId id)
    {
        return false;
    }



    /// <summary>
    /// Add a moderator to moderators.cfg
    /// </summary>
    public void AddMod(Steamworks.SteamId id)
    {

    }


    /// <summary>
    /// Remove a moderator to moderators.cfg
    /// </summary>
    public void RemoveMod(Steamworks.SteamId id)
    {

    }


    /// <summary>
    /// Add a moderator from moderators.cfg
    /// </summary>
    public void AddMod(string name)
    {

    }


    /// <summary>
    /// Remove a moderator from moderators.cfg
    /// </summary>
    public void RemoveMod(string name)
    {

    }

    /// <summary>
    /// Adds the steamid to bans.cfg
    /// </summary>
    /// <param name="id"></param>
    public void Ban(Steamworks.SteamId id)
    {
        // Write the id as an entry to bans.cfg

    }

    /// <summary>
    /// Adds the player's id to bans.cfg
    /// </summary>
    /// <param name="name"></param>
    public void Ban(string steamName)
    {
        // Find id of player with that steam name
        // then call the Ban(id)

    }

    /// <summary>
    /// Removes the steamid from bans.cfg
    /// </summary>
    /// <param name="id"></param>
    public void UnBan(Steamworks.SteamId id)
    {
        // Delete id from banscfg


    }


    /// <summary>
    /// Removes the associated steamid from bans.cfg
    /// </summary>
    /// <param name="steamName"></param>
    public void UnBan(string steamName)
    {
        // Find id of player with that steam name
        // then remove that id from bans.cfg


    }


    #endregion


#else


    #region Client Logic


    void StartClient()
    {

#if !UNITY_EDITOR
        if (Steamworks.SteamClient.RestartAppIfNecessary(1551700))
            Application.Quit();
#endif


        // Initialize steam client
        try
        {
            SteamClient.Init(1551700, true);
            Debug.Log("Steam client initialized successfully.");

            SceneManager.LoadScene(1);
        }
        catch
        {
            Debug.Log("Could not initialize steam client. Is steam not open?");


            // Here you should throw an error and prompt user to restart


        }
    }

    private void OnP2PSessionRequestClient(SteamId id)
    {
        
        SteamNetworking.AcceptP2PSessionWithUser(id);
    }

    private void OnDisable()
    {
        Mirror.NetworkManager.singleton.StopClient();


#if !UNITY_EDITOR
        SteamClient.Shutdown();
#endif
    }


#endregion

#endif

    }
