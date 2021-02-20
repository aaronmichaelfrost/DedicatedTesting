using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAuthenticator : NetworkAuthenticator
{
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
    struct AuthResponse : NetworkMessage
    {
        public bool accepted;

        // This message you give the reason they could /  couldnt be authenticated
        public string message;
    }


    public struct PlayerData
    {
        public Steamworks.SteamId id;
        public string steamName;

        // Other stuff like character selection etc
    }


    private void ClientAuthMessageHandler(NetworkConnection connection, AuthRequest authMessage)
    {
        Debug.Log("Client wants to be authenticated: " + authMessage.playerData.steamName + " - " + authMessage.playerData.id);


        Debug.Log("Checking for server ban: ");

        if (SteamManager.singleton.IsBanned(authMessage.playerData.id))
        {
            AuthResponse response = new AuthResponse
            {
                message = "Server banned!",
                accepted = false
            };

            connection.Send(response, 0);
        }
        else
        {
            Debug.Log("Authenticating with steam.");


            // Keep track of this client's information temporarily so we can trace back to the connection using the steam id returned in the validate callback
            AuthUnit u = new AuthUnit
            {
                connection = connection,
                playerData = authMessage.playerData
            };

            currentAuthUnits.Add(u);

            // Ask steam to validate the user
            Steamworks.SteamServer.BeginAuthSession(authMessage.ticket.Data, authMessage.playerData.id);
        }
    }


    // This gets called on client when they recieve authentication reponse from server
    private void ClientAuthResponseHandler(NetworkConnection conn, AuthResponse authResponse)
    {
        Debug.Log("Accepted: " + authResponse.accepted + ", Message: " + authResponse.message);


        if (authResponse.accepted)
        {
            ClientAccept(conn);
        }
        else
        {
            ClientReject(conn);
        }
    }

  
    // This gets called on server once steam has checked the auth ticket
    private void OnValidateAuthTicketResponse(Steamworks.SteamId userId, Steamworks.SteamId ownerId, Steamworks.AuthResponse response)
    {

        string message = "";
        bool accepted = false;

        Debug.Log("Validation response recieved: ");

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

        Debug.Log("Accepted: " + accepted + ", Message: " + message);


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
                Debug.Log("Sending response to client...");


                if (accepted)
                    ApproveAuthentication(authUnit);
                else
                    FailAuthentication(authUnit.connection, msg);

                authUnit.connection.Send(msg, 0);


                // Remove auth unit from temporary list
                currentAuthUnits.Remove(authUnit);
                break;
            }
        }
    }


    private void ApproveAuthentication(AuthUnit authUnit)
    {
        // Set the authentication data like steam name etc
        authUnit.connection.authenticationData = authUnit.playerData;

        ServerAccept(authUnit.connection);
    }



    private void FailAuthentication(NetworkConnection conn, AuthResponse response)
    {
        conn.Send(response, 0);


        // must set NetworkConnection isAuthenticated = false
        conn.isAuthenticated = false;

        // disconnect the client after 1 second so that response message gets delivered
        StartCoroutine(DelayedDisconnect(conn, 1));
    }


    private IEnumerator DelayedDisconnect(NetworkConnection conn, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Reject the unsuccessful authentication
        ServerReject(conn);
    }


    // Called on client when we try to connect to the mirror server
    public override void OnClientAuthenticate(NetworkConnection conn)
    {

        AuthRequest authRequest = new AuthRequest
        {
            ticket = Steamworks.SteamUser.GetAuthSessionTicket(),

            playerData = new PlayerData
            {
                steamName = Steamworks.SteamClient.Name,
                id = Steamworks.SteamClient.SteamId
            }
        };

        NetworkClient.Send(authRequest, 0);
    }


    public override void OnStartClient()
    {
        NetworkClient.RegisterHandler<AuthResponse>(ClientAuthResponseHandler, false);

        base.OnStartClient();
    }


    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<AuthRequest>(ClientAuthMessageHandler, false);

        Steamworks.SteamServer.OnValidateAuthTicketResponse += OnValidateAuthTicketResponse;

        base.OnStartServer();
    }

    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<AuthResponse>();

        base.OnStopClient();
    }

    public override void OnStopServer()
    {
        NetworkServer.UnregisterHandler<AuthRequest>();

        base.OnStopServer();
    }


    public override void OnServerAuthenticate(NetworkConnection conn)
    {
        // Do nothing. The message handler callback will handle authrequests.
    }
}