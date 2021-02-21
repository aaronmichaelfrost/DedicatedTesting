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
    // Format: steamid steamid steamid

    public static string modsPath = "mods.cfg";
    // Format: steamid steamid steamid

    public static string playersPath = "players.cfg";
    // Format:

    /*
     * steamid 
     * steamName
     * steamid 
     * steamName
     * 
     */

    public static void Init()
    {

        string projectFolder = Path.Combine(Application.dataPath, "../");

        // Create config / data files if neccecary

        playersPath = projectFolder  + playersPath;

        modsPath = projectFolder +  modsPath;

        bansPath = projectFolder +  bansPath;


        if (!File.Exists(playersPath))
        {
            File.CreateText(playersPath);

            Debug.Log("Creating players file at " + playersPath);

        }
            

        if (!File.Exists(bansPath))
        {
            File.CreateText(bansPath);

            Debug.Log("Creating bans file at " + bansPath);
        }
            

        if (!File.Exists(modsPath))
        {
            File.CreateText(modsPath);

            Debug.Log("Creating moderators file at " + modsPath);
        }
            
    }


    public static class Players
    {



        /// <summary>
        /// Appends a player entry to the players file. Used to associate player id with name in the players file
        /// Note: removes duplicate entries with duplicate ids
        /// </summary>
        /// <param name="data"></param>
        public static void AddPlayer(PlayerData data)
        {
            Debug.Log("Removing duplicates player entries.");

            // Remove duplicate copies 
            RemovePlayer(data.id);

            Debug.Log("Finished removing player from players file.");

            StreamWriter writer = new StreamWriter(playersPath, true);

            writer.Write(data.id + "\n" + data.steamName + "\n");

            Debug.Log("Finished writing new entry " + data.id + " " + data.steamName);

            writer.Close();

        }


        /// <summary>
        /// Removes a player entry from the players file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static void RemovePlayer(Steamworks.SteamId id)
        {

            Debug.Log("Removing player");

            StreamReader reader = new StreamReader(playersPath);

            Debug.Log("Opened reader");

            if (reader.EndOfStream)
            {
                Debug.Log("File empty.");

                reader.Close();
                return;
            }

            string[] players = reader.ReadToEnd().Split('\n');

            Debug.Log("Read file to end and split it");

            reader.Close();

            string updated = "";

            // Add all players to the updated string except for the player to remove
            if (players != null && players.Length > 0)
            {
                Debug.Log("There are " + (players.Length / 2) + " players on record.");

                for (int i = 0; i < players.Length; i++)
                {
                    if(players[i].Length >= 1)
                    {
                        Debug.Log("Found a player on record " + players[i + 1]);

                        if (players[i] != id.ToString())
                        {
                            Debug.Log("Adding player to updated list: " + players[i + 1]);

                            updated += players[i] + '\n' + players[i + 1] + '\n';
                        }


                        i++;
                    }
                }
            }
            else
            {
                Debug.Log("Players file was null or empty.");
            }

            Debug.Log("Now writing updated player file after player has been removed:");
            Debug.Log(updated);



            

            StreamWriter writer = new StreamWriter(playersPath, false);

            writer.Write(updated);

            writer.Close();

        }


        /// <summary>
        /// Gets the name associated with the steamid found in the players file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetName(Steamworks.SteamId id)
        {
            StreamReader reader = new StreamReader(playersPath);

            if (reader.EndOfStream)
            {
                reader.Close();
                return "";
            }

            string[] players = reader.ReadToEnd().Split('\n');

            string name = "";



            // Add all players to the updated string except for the player to remove
            if (players != null && players.Length > 0)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].Length >= 1)
                    {
                        if (players[i] == id.ToString())
                        {
                            name = players[i + 1];
                            break;
                        }

                        i++;

                    }
                }
            }


            reader.Close();


            if (name == "")
            {
                Debug.Log("Couldn't find the name associated with the id " + id + ". " + playersPath + " may be corrupt.");
            }

            return name;
        }


        /// <summary>
        /// 
        /// Attempts to return the steamid associated with the given steam name in the players file
        /// 
        /// Note that players can have duplicate steam names so it might return the wrong player
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Steamworks.SteamId GetId(string name)
        {
            StreamReader reader = new StreamReader(playersPath);

            if (reader.EndOfStream)
            {
                reader.Close();
                return 0;
            }


            string[] players = reader.ReadToEnd().Split('\n');

            reader.Close();

            Steamworks.SteamId id = 0;

            Debug.Log("Finding ID associated with name.");
            Debug.Log("There are " + (players.Length /2) + " players on record:");


            // Add all players to the updated string except for the player to remove
            if (players != null && players.Length > 0)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].Length >= 1)
                    {
                        if (players[i + 1] == name)
                        {
                            id = System.Convert.ToUInt64(players[i]);
                            break;
                        }

                        i++;

                    }
                }
            }


            if (id == 0)
            {
                Debug.Log("Couldn't find the id associated with the name " + name + ". " + playersPath + " may be corrupt.");
            }

            return id;
        }


        /// <summary>
        /// 
        /// Returns true if the steamid was found in  the players file
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IdPresent(Steamworks.SteamId id)
        {
            StreamReader reader = new StreamReader(playersPath);

            if (reader.EndOfStream)
            {
                reader.Close();
                return false;
            }


            string[] players = reader.ReadToEnd().Split('\n');

            reader.Close();


            for (int i = 0; i < players.Length; i++)
            {


                if (players[i] == id.ToString())
                {
                    return true;
                }

                i++;
            }

            return false;

        }
    }


  

    public static class Config
    {



        /// <summary>
        /// Removes a steamid from the file specified
        /// </summary>
        /// <param name="id"></param>
        public static void RemoveId(Steamworks.SteamId id, string path)
        {
            // Get the current list

            StreamReader reader = new StreamReader(path);

            

            if (reader.EndOfStream)
            {
                reader.Close();
                return;
            }

            string[] ids = reader.ReadLine().Split(' ');

            reader.Close();



            // Make a new string to that will exclude this players id

            string updatedIds = "";

            if(ids != null && ids.Length > 0)
            {
                foreach (var idString in ids)
                    if (idString.Length > 3 && System.Convert.ToUInt64(idString) != id)
                        updatedIds += idString + " ";
            }



            // Write the updated id list

            StreamWriter writer = new StreamWriter(path, false);

            writer.Write(updatedIds);

            writer.Close();
        }


        /// <summary>
        /// Checks if the steamid is in the file specified
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IdPresent(Steamworks.SteamId id, string path)
        {
            Debug.Log("Checking if ID is present in file.");

            Debug.Log("ID: " + id.ToString());
            Debug.Log("Path: " + path);

            Debug.Log("Creating stream reader");

            StreamReader reader = new StreamReader(path);

            Debug.Log("Stream reader created");

            bool present = false;



            if (!reader.EndOfStream)
            {
                string[] ids = reader.ReadLine().Split(' ');

                Debug.Log("Read ids");

                

                if (ids != null && ids.Length > 0)
                {

                    foreach (var idString in ids)
                    {

                        if(idString.Length > 1)
                        {
                            Debug.Log("Reading id: " + idString);

                            Debug.Log("Converting to ulong");

                            if (System.Convert.ToUInt64(idString) == id)
                                present = true;
                        }
                    }
                }
            }


            reader.Close();

            return present;
        }



        /// <summary>
        /// Writes the steamid to the file specified
        /// </summary>
        /// <param name="id"></param>
        public static void AddId(Steamworks.SteamId id, string path)
        {
            RemoveId(id, path);

            StreamWriter writer = new StreamWriter(path, true);

            writer.Write(id + " ");

            writer.Close();
        }
    }

    
}
