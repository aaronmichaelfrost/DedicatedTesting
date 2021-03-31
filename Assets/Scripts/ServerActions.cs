using UnityEngine;



/// <summary>
/// Global class for performing server side behaviors such as banning, kicking, promoting to moderator, unbanning, giving players items, changing server settings, etc
/// 
/// This is the class that connects the front end of server behavior to the backend (mirror networking commands, file I/O, etc)
/// </summary>
public static class ServerActions 
{
    public static void Ban(Steamworks.SteamId id)
    {
        ServerData.Config.AddId(id, ServerData.bansPath);
        Kick(id);

        Debug.Log("Kickbanning " + id);
    }


    public static void Ban(string name)
    {
        ulong id = ServerData.Players.GetId(name);

        if (id != 0)
        {
            ServerData.Config.AddId(id, ServerData.bansPath);
            Kick(id);
        }

        Debug.Log("Kickbanning " + id);
    }


    public static void UnBan(Steamworks.SteamId id)
    {
        ServerData.Config.RemoveId(id, ServerData.bansPath);

        Debug.Log("Unbanning " + id);
    }


    public static void UnBan(string name)
    {
        ulong id = ServerData.Players.GetId(name);

        if (id != 0)
            ServerData.Config.AddId(id, ServerData.modsPath);

        Debug.Log("Unbanning " + id);
    }


    public static void Mod(Steamworks.SteamId id)
    {
        ServerData.Config.AddId(id, ServerData.modsPath);

        Debug.Log("Modding " + id);
    }


    public static void Mod(string name)
    {
        ulong id = ServerData.Players.GetId(name);

        if (id != 0)
        {
            ServerData.Config.AddId(id, ServerData.modsPath);
        }

        Debug.Log("Modding " + id);
    }


    public static void Unmod(Steamworks.SteamId id)
    {
        ServerData.Config.RemoveId(id, ServerData.modsPath);

        Debug.Log("Unmodding " + id);
    }


    public static void Unmod(string name)
    {
        ulong id = ServerData.Players.GetId(name);

        if (id != 0)
            ServerData.Config.RemoveId(id, ServerData.modsPath);

        Debug.Log("Unmodding " + id);
    }


    public static void Kick(Steamworks.SteamId id)
    {
        if (ServerData.Players.IdPresent(id))
            ((MyAuthenticator)Mirror.NetworkManager.singleton.authenticator).Kick(id);


        Debug.Log("Kicking " + id);
    }


    public static void Kick(string name)
    {
        ulong id = ServerData.Players.GetId(name);

        if (id != 0)
            ((MyAuthenticator)Mirror.NetworkManager.singleton.authenticator).Kick(id);


        Debug.Log("Kicking " + id);
    }
}
