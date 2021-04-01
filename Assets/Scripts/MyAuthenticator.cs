using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MyAuthenticator : NetworkAuthenticator
{

    public static Steamworks.AuthTicket localClientTicket;


    /// <summary>
    /// I created this unit just because the steam OnValidateAuthResponse is how I hear back about if a steamid got accepted or rejected.
    /// 
    /// The problem was that it only told me the steamid of the user, and I had to find the mirror NetworkConnection related to that steamid sent in the callback
    /// 
    /// 
    /// </summary>
    private struct AuthUnit
    {
        public PlayerData playerData;
        public NetworkConnection connection;
    }


    private List<AuthUnit> currentAuthUnits = new List<AuthUnit>();


    /// <summary>
    /// The client sends this to the server when they try to connect
    /// </summary>
    struct AuthRequest : NetworkMessage
    {
        public Steamworks.AuthTicket ticket;
        public PlayerData playerData;
    }


    /// <summary>
    /// Server sends this to client after checking the data in the request
    /// </summary>
    public struct AuthResponse : NetworkMessage
    {
        public bool accepted;

        // This message you give the reason they could /  couldnt be authenticated
        public string message;

    }



    public void Kick(Steamworks.SteamId id)
    {
        AuthResponse msg = new AuthResponse
        {
            message = "Kicked",
            accepted = false,
            

        };

        foreach (var connection in NetworkServer.connections)
        {
            if(((PlayerData)connection.Value.authenticationData).id == id)
            {

                Debug.Log("Kicking player with steamid " + id + "  name: " + ((PlayerData)connection.Value.authenticationData).steamName);


                connection.Value.Send(msg);

                Mirror.NetworkServer.RemovePlayerForConnection(connection.Value, true);

                StartCoroutine(DelayedDisconnect(connection.Value, 2f));


                return;
            }
        }
    }




    private void ClientAuthMessageHandler(NetworkConnection connection, AuthRequest authMessage)
    {
        Debug.Log("Client wants to be authenticated: " + authMessage.playerData.steamName + " - " + authMessage.playerData.id);


        


        Debug.Log("Checking for server ban from the path " + ServerData.bansPath);

        connection.authenticationData = authMessage.playerData;



        if (ServerData.Config.IdPresent(authMessage.playerData.id, ServerData.bansPath))
        {
            Debug.Log("Player is banned. Kicking them.");

            AuthResponse response = new AuthResponse
            {
                message = "Server banned!",
                accepted = false,

            };

            FailAuthentication(connection, response);
        }
        else
        {
            Debug.Log("They aren't server banned.");



            Debug.Log("Authenticating with steam...");


            // Keep track of this client's information temporarily so we can trace back to the connection using the steam id returned in the validate callback
            AuthUnit u = new AuthUnit
            {
                connection = connection,
                playerData = authMessage.playerData
            };

            Debug.Log("Created authUnit");


            Debug.Assert(authMessage.ticket.Data != null);

            Debug.Assert(authMessage.playerData.id != 0);


            if (Steamworks.SteamServer.IsValid)
            {
                if (!Steamworks.SteamServer.BeginAuthSession(authMessage.ticket.Data, authMessage.playerData.id))
                {
                    Debug.Log("BeginAuthSession returned false, called bullshit without even having to check with Gabe");

                    return;
                }
                else
                {
                    Debug.Log("Beginning authentication session.");

                    currentAuthUnits.Add(u);
                }
            }
            else
            {
                if (Steamworks.SteamUser.BeginAuthSession(authMessage.ticket.Data, authMessage.playerData.id) != Steamworks.BeginAuthResult.OK)
                {
                    Debug.Log("BeginAuthSession returned false, called bullshit without even having to check with Gabe");


                    switch (Steamworks.SteamUser.BeginAuthSession(authMessage.ticket.Data, authMessage.playerData.id))
                    {
                        case Steamworks.BeginAuthResult.OK:
                            Debug.Log("");
                            break;
                        case Steamworks.BeginAuthResult.InvalidTicket:
                            Debug.Log("Invalid");
                            break;
                        case Steamworks.BeginAuthResult.DuplicateRequest:
                            Debug.Log("Dup");
                            break;
                        case Steamworks.BeginAuthResult.InvalidVersion:
                            Debug.Log("Invalid version");
                            break;
                        case Steamworks.BeginAuthResult.GameMismatch:
                            Debug.Log("Game mismatch");
                            break;
                        case Steamworks.BeginAuthResult.ExpiredTicket:
                            Debug.Log("expired ticket");
                            break;
                        default:
                            break;
                    }

                    return;
                }
                else
                {
                    Debug.Log("Beginning authentication session.");

                    currentAuthUnits.Add(u);
                }
            }




        }
    }


    // This gets called on client when they recieve authentication reponse from server
    private void ClientAuthResponseHandler(NetworkConnection conn, AuthResponse authResponse)
    {
        Debug.Log("[Client] Auth Response recieved: Accepted: " + authResponse.accepted + ", Message: " + authResponse.message);


        if (authResponse.accepted)
        {
            ClientAccept(conn);
        }
        else
        {
            ClientReject(conn);

            Mirror.NetworkClient.Disconnect();

            SceneManager.LoadScene("MainMenu");


            Mirror.NetworkClient.Shutdown();
        }
    }

  
    // This gets called on server once steam has checked the auth ticket
    private void OnValidateAuthTicketResponse(Steamworks.SteamId userId, Steamworks.SteamId ownerId, Steamworks.AuthResponse response)
    {

        string message = "";
        bool accepted = false;

        Debug.Log("[Server/Host] Validation response recieved: ");

        switch (response)
        {
            case Steamworks.AuthResponse.OK:
                message = "Accepted!";
                accepted = true;
                break;
            case Steamworks.AuthResponse.UserNotConnectedToSteam:
                message = "Not connected to Steam!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.NoLicenseOrExpired:
                message = "No liscense or expired!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.VACBanned:
                message = "VAC banned!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.LoggedInElseWhere:
                message = "Logged in elsewhere!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.VACCheckTimedOut:
                message = "VAC check timed out!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.AuthTicketCanceled:
                message = "Auth ticket canceled!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.AuthTicketInvalidAlreadyUsed:
                message = "Auth tiekcet invalid or already used!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.AuthTicketInvalid:
                message = "Invalid auth ticket!";
                accepted = false;
                break;
            case Steamworks.AuthResponse.PublisherIssuedBan:
                message = "Game ban!";
                accepted = false;
                break;
            default:
                break;
        }

        if (NetworkServer.active && !accepted)
            return;

        Debug.Log("[Server] Accepted: " + accepted + ", Message: " + message);


        AuthResponse msg = new AuthResponse
        {
            message = message,
            accepted = accepted
        };


        // Now we need to find the connection associated with the steamid we need to respond to, and send the reponse

        foreach (var authUnit in currentAuthUnits)
        {
            if(userId == authUnit.playerData.id)
            {

                if (accepted)
                    StartCoroutine(DelayAuthenticationApproval(.5f, authUnit, msg));
                else
                    FailAuthentication(authUnit.connection, msg);


                // Remove auth unit from temporary list
                currentAuthUnits.Remove(authUnit);
                break;
            }
        }
    }



    private IEnumerator DelayAuthenticationApproval(float delayTime, AuthUnit authUnit, AuthResponse msg)
    {

        Debug.Log("Delaying authentication response");

        yield return new WaitForSeconds(delayTime);

        ApproveAuthentication(authUnit.connection, msg);
    }



    private void ApproveAuthentication(NetworkConnection conn, AuthResponse response)
    {
        Debug.Log("[Server] Authentication approved. Sending response to client: [Accepted: " + response.accepted + "  Message: " + response.message + "]");

        conn.Send(response, 0);

        Debug.Log("[Server] Result sent to client: " + conn.address);

        conn.isAuthenticated = true;


        ServerAccept(conn);
    }



    private void FailAuthentication(NetworkConnection conn, AuthResponse response)
    {
        Debug.Log("[Server] Authentication failed. Sending result to client.");


        conn.Send(response, 0);

        // must set NetworkConnection isAuthenticated = false
        conn.isAuthenticated = false;

        // disconnect the client after 1 second so that response message gets delivered
        StartCoroutine(DelayedDisconnect(conn, 1));

        Debug.Log("[Server] Rejected connection and responded to client: " + ((PlayerData)conn.authenticationData).steamName + " - " + ((PlayerData)conn.authenticationData).id);
    }


    private IEnumerator DelayedDisconnect(NetworkConnection conn, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);



        // Reject the unsuccessful authentication
        if (conn.identity != null && conn.identity.isClient)
            ServerReject(conn);
        else
            Debug.Log("Delayed disconnect returned conn.identity was null and identity wasn't a client");

    }


    // Called on client when we try to connect to the mirror server
    public override void OnClientAuthenticate(NetworkConnection conn)
    {

        Debug.Log("[Client] ON CLIENT AUTHENTICATE CALLED");

        AuthRequest authRequest = new AuthRequest
        {
            ticket = Steamworks.SteamUser.GetAuthSessionTicket(),

            playerData = new PlayerData
            {
                steamName = Steamworks.SteamClient.Name,
                id = Steamworks.SteamClient.SteamId,
            }
        };

        localClientTicket = authRequest.ticket;


        NetworkClient.Send(authRequest, 0);

    }


    public override void OnStartClient()
    {
        Debug.Log("Registered handler for auth responses.");

        NetworkClient.RegisterHandler<AuthResponse>(ClientAuthResponseHandler, false);

        base.OnStartClient();
    }


    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequest>(ClientAuthMessageHandler, false);


        if(Steamworks.SteamServer.IsValid)
            Steamworks.SteamServer.OnValidateAuthTicketResponse += OnValidateAuthTicketResponse;
        else
            Steamworks.SteamUser.OnValidateAuthTicketResponse += OnValidateAuthTicketResponse;

        base.OnStartServer();
    }

    public override void OnStopClient()
    {
        Debug.Log("Unregistered handler for auth responses.");

        NetworkClient.UnregisterHandler<AuthResponse>();

        base.OnStopClient();
    }

    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<AuthRequest>();


        if (Steamworks.SteamServer.IsValid)
            Steamworks.SteamServer.OnValidateAuthTicketResponse -= OnValidateAuthTicketResponse;
        else
            Steamworks.SteamUser.OnValidateAuthTicketResponse -= OnValidateAuthTicketResponse;


        base.OnStopServer();
    }


    public override void OnServerAuthenticate(NetworkConnection conn)
    {
        // Do nothing. The message handler callback will handle authrequests.
    }
}


[System.Serializable]
public struct PlayerData
{
    public Steamworks.SteamId id;
    public string steamName;

    // Other stuff like character selection etc
}