using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Game Manager started");
    }


    public void LeaveServer()
    {
        SteamLobby.LeaveLobby();

        if (MyAuthenticator.localClientTicket != null)
            MyAuthenticator.localClientTicket.Cancel();

        MyAuthenticator.localClientTicket = null;



        if (Mirror.NetworkServer.active)
        {
            Debug.Log("Stopping host");

            NetworkManagerCallbacks.singleton.StopHost();
        }

        else
        {
            Debug.Log("Disconnecting client");

            Mirror.NetworkClient.Disconnect();
        }

        Mirror.NetworkClient.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }


    private void OnDisable()
    {

        if(MyAuthenticator.localClientTicket != null)
            MyAuthenticator.localClientTicket.Cancel();

        SteamLobby.LeaveLobby();

        if(Mirror.NetworkClient.active)
            Mirror.NetworkClient.Shutdown();
    }

}
