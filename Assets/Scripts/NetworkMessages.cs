using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMessages : MonoBehaviour
{
    public struct JoinRequest : Mirror.NetworkMessage
    {
        public Steamworks.SteamId id;
        public byte[] authTicket;
    }

    public struct JoinAccept : Mirror.NetworkMessage
    {

    }
}
