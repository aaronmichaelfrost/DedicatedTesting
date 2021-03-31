#if !UNITY_SERVER


using UnityEngine;
using TMPro;


public class MainMenu : MonoBehaviour
{

    public static MainMenu singleton;

    private bool refreshing = false;

    public Transform serverListingParent;
    public GameObject serverListingPrefab;


    public bool pingSortLowToHigh = true;
    public bool populationSortLowToHigh = false;


    public TextMeshProUGUI roomModeText;


    public enum RoomDisplayMode
    {
        dedicated,
        hosted,
        both
    }


    /// <summary>
    /// Which room types does the client want to see in the server browser?
    /// </summary>
    public RoomDisplayMode roomMode;





    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);
    }


    private void Start()
    {
        UpdateRoomDisplayText();
        Refresh();
    }


    public void Refresh()
    {
        if (!refreshing) 
        {
            ServerListingUI.Clear();


            switch (roomMode)
            {
                case RoomDisplayMode.dedicated:
                    AddServers();
                    break;
                case RoomDisplayMode.hosted:
                    AddLobbies();
                    break;
                case RoomDisplayMode.both:
                    AddServers();
                    AddLobbies();
                    break;
                default:
                    break;
            }
                
        }
    }


    private void UpdateRoomDisplayText()
    {
        // Update the toggle text
        switch (roomMode)
        {
            case RoomDisplayMode.dedicated:
                roomModeText.text = "Dedicated";
                break;
            case RoomDisplayMode.hosted:
                roomModeText.text = "Player Hosted";
                break;
            case RoomDisplayMode.both:
                roomModeText.text = "Dedicated & Player Hosted";
                break;
            default:
                break;
        }
    }




    public void ToggleRoomDisplayMode()
    {

        // Switch to the next room display mode
        switch (roomMode)
        {
            case RoomDisplayMode.dedicated:
                roomMode = RoomDisplayMode.hosted;
                break;
            case RoomDisplayMode.hosted:
                roomMode = RoomDisplayMode.both;
                break;
            case RoomDisplayMode.both:
                roomMode = RoomDisplayMode.dedicated;
                break;
            default:
                break;
        }

        UpdateRoomDisplayText();


        Refresh();
    }


    public void TogglePingSort()
    {
        pingSortLowToHigh = !pingSortLowToHigh;

        Refresh();
    }


    public void TogglePopulationSort()
    {
        populationSortLowToHigh = !populationSortLowToHigh;

        Refresh();
    }


    #region Server Sorting Comparers

    class ServerPopLowToHighComparer : System.Collections.Generic.IComparer<Steamworks.Data.ServerInfo>
    {
        public int Compare(Steamworks.Data.ServerInfo x, Steamworks.Data.ServerInfo y)
        {
            return x.Players < y.Players ? 1 : -1;
        }
    }

    class ServerPopHighToLowComparer : System.Collections.Generic.IComparer<Steamworks.Data.ServerInfo>
    {
        public int Compare(Steamworks.Data.ServerInfo x, Steamworks.Data.ServerInfo y)
        {
            return x.Players > y.Players ? 1 : -1;
        }
    }


    class ServerPingLowToHighComparer : System.Collections.Generic.IComparer<Steamworks.Data.ServerInfo>
    {
        public int Compare(Steamworks.Data.ServerInfo x, Steamworks.Data.ServerInfo y)
        {
            return x.Ping < y.Ping ? 1 : -1;
        }
    }


    class ServerPingHighToLowComparer : System.Collections.Generic.IComparer<Steamworks.Data.ServerInfo>
    {
        public int Compare(Steamworks.Data.ServerInfo x, Steamworks.Data.ServerInfo y)
        {
            return x.Ping > y.Ping ? 1 : -1;
        }
    }


    class LobbyPopLowToHighComparer : System.Collections.Generic.IComparer<Steamworks.Data.Lobby>
    {
        public int Compare(Steamworks.Data.Lobby x, Steamworks.Data.Lobby y)
        {
            return x.MemberCount < y.MemberCount ? 1 : -1;
        }
    }

    class LobbyPopHighToLowComparer : System.Collections.Generic.IComparer<Steamworks.Data.Lobby>
    {
        public int Compare(Steamworks.Data.Lobby x, Steamworks.Data.Lobby y)
        {
            return x.MemberCount > y.MemberCount ? 1 : -1;
        }
    }


    class LobbyPingLowToHighComparer : System.Collections.Generic.IComparer<Steamworks.Data.Lobby>
    {
        public int Compare(Steamworks.Data.Lobby x, Steamworks.Data.Lobby y)
        {


            Steamworks.Data.NetPingLocation? _xloc = Steamworks.Data.NetPingLocation.TryParseFromString(x.GetData("pingLocation"));

            Steamworks.Data.NetPingLocation xloc;

            if (_xloc != null)
                xloc = (Steamworks.Data.NetPingLocation)_xloc;
            else
                Debug.LogError("Ping to lobby was null.");




            Steamworks.Data.NetPingLocation? _yloc = Steamworks.Data.NetPingLocation.TryParseFromString(y.GetData("pingLocation"));

            Steamworks.Data.NetPingLocation yloc;

            if (_yloc != null)
                yloc = (Steamworks.Data.NetPingLocation)_yloc;
            else
                Debug.LogError("Ping to lobby was null.");



            return Steamworks.SteamNetworkingUtils.EstimatePingTo(xloc) > Steamworks.SteamNetworkingUtils.EstimatePingTo(yloc) ? 1 : -1;
        }
    }


    class LobbyPingHighToLowComparer : System.Collections.Generic.IComparer<Steamworks.Data.Lobby>
    {
        public int Compare(Steamworks.Data.Lobby x, Steamworks.Data.Lobby y)
        {


            Steamworks.Data.NetPingLocation? _xloc = Steamworks.Data.NetPingLocation.TryParseFromString(x.GetData("pingLocation"));

            Steamworks.Data.NetPingLocation xloc;

            if (_xloc != null)
                xloc = (Steamworks.Data.NetPingLocation)_xloc;
            else
                Debug.LogError("Ping to lobby was null.");




            Steamworks.Data.NetPingLocation? _yloc = Steamworks.Data.NetPingLocation.TryParseFromString(y.GetData("pingLocation"));

            Steamworks.Data.NetPingLocation yloc;

            if (_yloc != null)
                yloc = (Steamworks.Data.NetPingLocation)_yloc;
            else
                Debug.LogError("Ping to lobby was null.");



            return Steamworks.SteamNetworkingUtils.EstimatePingTo(xloc) < Steamworks.SteamNetworkingUtils.EstimatePingTo(yloc) ? 1 : -1;
        }
    }


    #endregion

    #region Sorting Functions

    /// <summary>
    /// Ping sort servers
    /// </summary>
    /// <param name="responsive"></param>
    private void PingSort(ref System.Collections.Generic.List<Steamworks.Data.ServerInfo> responsive)
    {
        if (pingSortLowToHigh)
        {
            Debug.Log("Ping sorting low to high.");

            ServerPingLowToHighComparer s = new ServerPingLowToHighComparer();
            responsive.Sort(s);
        }
        else
        {
            Debug.Log("Ping sorting high to low.");


            ServerPingHighToLowComparer s = new ServerPingHighToLowComparer();
            responsive.Sort(s);
        }
    }


    /// <summary>
    /// Population sort servers
    /// </summary>
    /// <param name="responsive"></param>
    private void PopulationSort(ref System.Collections.Generic.List<Steamworks.Data.ServerInfo> responsive)
    {
        if (populationSortLowToHigh)
        {
            Debug.Log("Population sorting low to high");

            ServerPopLowToHighComparer s = new ServerPopLowToHighComparer();
            responsive.Sort(s);
        }
        else
        {
            Debug.Log("Population sorting high to low");


            ServerPopHighToLowComparer s = new ServerPopHighToLowComparer();
            responsive.Sort(s);
        }
    }


    /// <summary>
    /// Ping sort lobbies
    /// </summary>
    /// <param name="responsive"></param>
    private void PingSort(ref System.Collections.Generic.List<Steamworks.Data.Lobby> responsive)
    {
        if (pingSortLowToHigh)
        {
            Debug.Log("Ping sorting low to high.");

            LobbyPingLowToHighComparer s = new LobbyPingLowToHighComparer();
            responsive.Sort(s);
        }
        else
        {
            Debug.Log("Ping sorting high to low.");

            LobbyPingHighToLowComparer s = new LobbyPingHighToLowComparer();
            responsive.Sort(s);
        }
    }


    /// <summary>
    /// Population sort lobbies
    /// </summary>
    /// <param name="responsive"></param>
    private void PopulationSort(ref System.Collections.Generic.List<Steamworks.Data.Lobby> responsive)
    {
        if (populationSortLowToHigh)
        {
            Debug.Log("Population sorting low to high");

            LobbyPopLowToHighComparer s = new LobbyPopLowToHighComparer();
            responsive.Sort(s);
        }
        else
        {
            Debug.Log("Population sorting high to low");

            LobbyPopHighToLowComparer s = new LobbyPopHighToLowComparer();
            responsive.Sort(s);
        }
    }


#endregion

    /// <summary>
    /// Refreshes the list of servers. This is usually connected to a button.
    /// </summary>
    private async void AddServers()
    {
        refreshing = true;

        var responsive = await SteamManager.singleton.ResponsiveServers();


        // Now sort based on pingSortLowToHigh bool and populationSortLowToHighBool
        PingSort(ref responsive);
        PopulationSort(ref responsive);

        foreach (var server in responsive)
            ServerListingUI.CreateServerListing(server);

        refreshing = false;
    }



    /// <summary>
    /// Refreshes the list of lobbies. This is usually connected to a button.
    /// </summary>
    private async void AddLobbies()
    {
        var responsive = await SteamManager.singleton.ResponsiveLobbies();


        // Now sort based on pingSortLowToHigh bool and populationSortLowToHighBool
        PingSort(ref responsive);
        PopulationSort(ref responsive);

        foreach (var lobby in responsive)
            ServerListingUI.CreateLobbyListing(lobby);
    }
}
#endif