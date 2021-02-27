#if !UNITY_SERVER


using UnityEngine;


public class MainMenu : MonoBehaviour
{

    public static MainMenu singleton;


    public Transform serverListingParent;
    public GameObject serverListingPrefab;



    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);
    }


    private void Start()
    {
        Refresh();
    }


    public void Refresh()
    {
        ServerListingUI.Clear();

        AddLobbies();
        AddServers();
    }



    /// <summary>
    /// Refreshes the list of servers. This is usually connected to a button.
    /// </summary>
    private async void AddServers()
    {
        var responsive = await SteamManager.singleton.ResponsiveServers();

        if(responsive != null)
            foreach (var server in responsive)
                ServerListingUI.CreateServerListing(server);
    }



    /// <summary>
    /// Refreshes the list of lobbies. This is usually connected to a button.
    /// </summary>
    private async void AddLobbies()
    {
        var responsive = await SteamManager.singleton.ResponsiveLobbies();

        foreach (var lobby in responsive)
            ServerListingUI.CreateLobbyListing(lobby);
    }
}
#endif