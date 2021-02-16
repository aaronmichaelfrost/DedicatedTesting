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
        RefreshServerList();
    }



    /// <summary>
    /// Refreshes the list of servers. This is usually connected to a button.
    /// </summary>
    public async void RefreshServerList()
    {


        var responsive = await SteamManager.singleton.ResponsiveServers();

        ServerListingUI.Clear();

        foreach (var server in responsive)
            ServerListingUI.CreateListing(server);
    }
}
#endif