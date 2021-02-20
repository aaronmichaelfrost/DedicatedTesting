using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTransport : Mirror.Transport
{
    public override void ClientConnect(string address)
    {
        throw new System.NotImplementedException();
    }

    public override void ClientConnect(Uri uri)
    {
        base.ClientConnect(uri);
    }

    public override void ClientDisconnect()
    {
        throw new NotImplementedException();
    }

    public override bool Available()
    {
        throw new NotImplementedException();
    }

    public override bool ClientConnected()
    {
        throw new NotImplementedException();
    }

    public override void ClientSend(int channelId, ArraySegment<byte> segment)
    {
        throw new NotImplementedException();
    }

    public override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
    }



    public override void ServerSend(int connectionId, int channelId, ArraySegment<byte> segment)
    {
        throw new NotImplementedException();
    }


    public override void ServerStart()
    {
        throw new NotImplementedException();
    }



    public override void ServerStop()
    {
        throw new NotImplementedException();
    }


    public override void Shutdown()
    {
        throw new NotImplementedException();
    }


    public override int GetMaxBatchSize(int channelId)
    {
        return base.GetMaxBatchSize(channelId);
    }


    public override bool ServerActive()
    {
        throw new NotImplementedException();
    }


    public override Uri ServerUri()
    {
        throw new NotImplementedException();
    }

    public override string ServerGetClientAddress(int connectionId)
    {
        throw new NotImplementedException();
    }


    public override bool ServerDisconnect(int connectionId)
    {
        throw new NotImplementedException();
    }


    public override int GetMaxPacketSize(int channelId = 0)
    {
        throw new NotImplementedException();
    }
}
