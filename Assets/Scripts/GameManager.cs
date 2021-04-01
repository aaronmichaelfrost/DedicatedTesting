using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_SERVER
        // Debug this so the dedicated server knows we've entered the gameplay scene
        Debug.Log("Game Manager started");

#endif
    }


    public void LeaveServer()
    {
        SteamLobby.LeaveLobby();


        // Cancel auth ticket
        if (MyAuthenticator.localClientTicket != null)
            MyAuthenticator.localClientTicket.Cancel();

        MyAuthenticator.localClientTicket = null;


        // Shutdown network
        if (Mirror.NetworkServer.active)
        {
            Debug.Log("Stopping host");

            Mirror.NetworkManager.singleton.StopHost();
        }

        else
        {
            Debug.Log("Disconnecting client");

            Mirror.NetworkClient.Disconnect();

            Mirror.NetworkManager.singleton.StopClient();

        }


        // Go to main menu
        SceneManager.LoadScene("MainMenu");
    }
}