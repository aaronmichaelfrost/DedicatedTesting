using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu singleton;



    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(this.gameObject);


    }



#if !UNITY_SERVER




    private void Update()
    {

        
        if (SteamNetworking.IsP2PPacketAvailable(0))
        {
            Steamworks.Data.P2Packet? p = SteamNetworking.ReadP2PPacket(0);

            if (p.HasValue)
            {
                string s = System.Text.Encoding.ASCII.GetString(p.Value.Data);

                Debug.Log("We got this message: " + s);

                if (s == "join")
                {
                    
                    Debug.Log("Server accepted join request! Joining now!");

                    Mirror.NetworkManager.singleton.StartClient();
                }

                if(s == "kicked")
                {
                    Debug.Log("Server kicked us!");

                    Mirror.NetworkManager.singleton.StopClient();
                }


            }
        }

        
    }

    async void RefreshServerList()
    {

        ClearServerList();



        // Add all servers from the local network to the list

        using (var list = new Steamworks.ServerList.LocalNetwork())
        {
            await list.RunQueryAsync();


            foreach (var server in list.Responsive)
            {
                AddServerListing(server);

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
                MainMenu.singleton.AddServerListing(server);

                Debug.Log($"Server found with address: {server.Address} name: {server.Name}");
            }


            Debug.Log("Found " + list.Responsive.Count + " internet servers.");
        }
    }


    string steamIdInputField = "SteamID";

    


    private void OnGUI()
    {

        GUILayout.BeginArea(new Rect(40, 40, 215, 9999));


        if (SteamClient.IsValid)
        {

            if (SteamClient.IsLoggedOn)
            {
                if (GUILayout.Button("Refresh Server List"))
                    RefreshServerList();

                GUILayout.BeginHorizontal();

                // Set the address to connect to
                Mirror.NetworkManager.singleton.networkAddress = GUILayout.TextField(Mirror.NetworkManager.singleton.networkAddress);

                ulong desiredServerSteamId;

                steamIdInputField = GUILayout.TextField(steamIdInputField);

                if (GUILayout.Button("Connect"))
                {

                    if (ulong.TryParse(steamIdInputField, out desiredServerSteamId))
                    {

                        Debug.Log("Sending authentication ticket");

                        BeginAuthResult beginAuth = SteamUser.BeginAuthSession(SteamUser.GetAuthSessionTicket().Data, SteamClient.SteamId);
                        switch (beginAuth)
                        {
                            case BeginAuthResult.OK:
                                Debug.Log("Began auth ok");
                                break;
                            case BeginAuthResult.InvalidTicket:
                                Debug.Log("Began auth invalid");
                                break;
                            case BeginAuthResult.DuplicateRequest:
                                Debug.Log("Began auth duplicate request");
                                break;
                            case BeginAuthResult.InvalidVersion:
                                Debug.Log("Began auth invalid version");
                                break;
                            case BeginAuthResult.GameMismatch:
                                Debug.Log("Began auth game mismatch");
                                break;
                            case BeginAuthResult.ExpiredTicket:
                                Debug.Log("Began auth expired ticket");
                                break;
                            default:
                                break;
                        }


                        

                    }
                    else
                    {
                        Debug.Log("Couldnt parse: " + steamIdInputField + " into a ulong...");
                    }

                    
                    SteamNetworking.SendP2PPacket(desiredServerSteamId, System.Text.Encoding.ASCII.GetBytes("join " + SteamClient.Name), -1, 0, P2PSend.Reliable);
                }


                GUILayout.BeginHorizontal();
            }
            else
            {
                GUILayout.Label("Not connected to steam.");
            }
        }
        else
        {
            GUILayout.Label("Not connected to steam.");
        }




        GUILayout.EndArea();

    }


    public GameObject serverListingPrefab;
    public Transform serverListingSpawnPosition;


    public void ClearServerList()
    {
        foreach (Transform child in serverListingSpawnPosition.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddServerListing(Steamworks.Data.ServerInfo server)
    {
        ServerListingUI s = Instantiate(serverListingPrefab, serverListingSpawnPosition).GetComponent<ServerListingUI>();

        s.players.text = server.Players.ToString();
        s.steamId.text = server.SteamId.ToString();
        s.address.text = server.Address.ToString();
        s.serverName.text = server.Name;
    }

#endif

}
