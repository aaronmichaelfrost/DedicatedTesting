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
        SteamLobby.singleton.LeaveLobby();


        
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
            

        SceneManager.LoadScene("MainMenu");
    }

}
