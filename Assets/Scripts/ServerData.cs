using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


/// <summary>
/// 
/// Class for reading and writing player and world data
/// 
/// This will hold the world save, player list, mod list, ban list, etc
/// 
/// 
/// </summary>
public class ServerData
{



    public static string bansPath = "bans.cfg";

    public static string modsPath = "mods.cfg";

    public static string playersPath = "players.cfg";


    


    
    /// <summary>
    /// Removes a steamid from the file specified
    /// </summary>
    /// <param name="id"></param>
    public static void RemoveId(Steamworks.SteamId id, string path)
    {
        // Get the current bans list

        StreamReader reader = new StreamReader(path);

        string[] bannedIds = reader.ReadLine().Split(' ');

        reader.Close();



        // Make a new string to that will exclude this players id

        string newBannedIds = "";


        foreach (var idString in bannedIds)
            if (System.Convert.ToUInt64(idString) != id)
                newBannedIds += idString + " ";




        // Write the updated id list

        StreamWriter writer = new StreamWriter(path, false);

        writer.Write(newBannedIds);

        writer.Close();
    }


    /// <summary>
    /// Checks if the steamid is in the file specified
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool IdPresent(Steamworks.SteamId id, string path)
    {
        StreamReader reader = new StreamReader(path);


        string[] bannedIds = reader.ReadLine().Split(' ');

        bool banned = false;

        foreach (var idString in bannedIds)
        {
            if (System.Convert.ToUInt64(idString) == id)
                banned = true; 
        }

        reader.Close();

        return banned;
    }



    /// <summary>
    /// Writes the steamid to the file specified
    /// </summary>
    /// <param name="id"></param>
    public static void AddId(Steamworks.SteamId id, string path)
    {
        StreamWriter writer = new StreamWriter(path, true);

        writer.Write(id + " ");

        writer.Close();
    }
}
