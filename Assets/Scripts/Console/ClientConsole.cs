using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientConsole : MonoBehaviour
{

    public static ClientConsole singleton;


#if !UNITY_SERVER
    bool showHelp = false;
    bool showConsole = false;
#endif

    [HideInInspector]
    [System.NonSerialized]
    public string input;


    public List<object> commandList;


    // ADD MORE COMMANDS HERE

    public static ConsoleCommand HELP;
    public static ConsoleCommand<float> SET_FOV;
    public static ConsoleCommand<string> SERVER_DEBUG_MESSAGE;
    public static ConsoleCommand<string> BAN_BY_NAME;
    public static ConsoleCommand<int> BAN_BY_ID;
    public static ConsoleCommand<int> MOD_BY_ID;
    public static ConsoleCommand<string> MOD_BY_NAME;

    public static ConsoleCommand<ulong> KICK_BY_ID;


    void Start()
    {

#if UNITY_SERVER
        gameObject.AddComponent(typeof(ServerConsole));
        Debug.Log("1");
#endif
    }


    private void Awake()
    {

        if (singleton == null)
            singleton = this;
        else
            Destroy(this);


        Utilities.DontDestroyOnLoad(this.gameObject);



        // ADD MORE COMMANDS HERE

        HELP = new ConsoleCommand("help", "Shows list of console commands", "help", () =>
        {

#if UNITY_SERVER
            Debug.Log("Below is a list of commands you can use. Note some of them are just for clients and they won't do anything.");

            for (int i = 0; i < commandList.Count; i++)
            {
                ConsoleCommandBase command = commandList[i] as ConsoleCommandBase;

                Debug.Log(command.commandId + " - " + command.commandDescription + " - " + command.commandFormat);

            }
#else
            showHelp = !showHelp;
#endif

        });

        SET_FOV = new ConsoleCommand<float>("set_fov", "Sets the fov of Camera.main. use set_fov <any positive number>", "set_fov <float>", (x) =>
        {

            Camera.main.fieldOfView = x;


        });


        BAN_BY_ID = new ConsoleCommand<int>("ban_id", "Bans the user with the given steam id", "ban_id <int>", (x) =>
        {

#if UNITY_SERVER
            SteamManager.singleton.Ban(System.Convert.ToUInt64(x));
#else
            // Ask server to ban this player
            Debug.Log("Asking server to ban player with id: " + x);

#endif


        });

        BAN_BY_NAME = new ConsoleCommand<string>("ban_name", "Bans the first found user with this steam name, be careful using this.", "ban_name <string>", (x) =>
        {

#if UNITY_SERVER
            SteamManager.singleton.Ban(x);
#else

            if (Mirror.NetworkClient.active)
            {

            }
            // Ask server to ban this player
            Debug.Log("Asking server to ban player with name: " + x);

#endif


        });


        MOD_BY_ID = new ConsoleCommand<int>("mod_id", "Adds a server moderator by steamid.", "mod_id <int>", (x) =>
        {

#if UNITY_SERVER

            SteamManager.singleton.AddMod(System.Convert.ToUInt64(x));
#else
            // Ask server to ban this player
            Debug.Log("Asking server to mod player with id: " + x);

#endif


        });

        MOD_BY_NAME = new ConsoleCommand<string>("mod_name", "Adds a server moderator using the first found user with this steam name, be careful using this.", "mod_name <string>", (x) =>
        {

#if UNITY_SERVER
            SteamManager.singleton.AddMod(x);
#else


            // Ask server to ban this player
            Debug.Log("Asking server to mod player with name: " + x);

#endif


        });


        KICK_BY_ID = new ConsoleCommand<ulong>("kick_id", "Kicks the player from the session", "kick <int>", (x) =>
        {

#if UNITY_SERVER
            // SteamManager.singleton.Kick(x);
#else


            // Ask server to ban this player
            Debug.Log("Asking server to kick player with id: " + x);

#endif


        });


        SET_FOV = new ConsoleCommand<float>("set_fov", "Sets the fov of Camera.main. use set_fov <any positive number>", "set_fov <float>", (x) =>
        {

            Camera.main.fieldOfView = x;


        });



        /*      here is an example moderator / server only command
         *      
         *      
        *         BAN = new ConsoleCommand<int>("ban", "Adds steamid to ban list.", "ban steamid", (x) =>
                {

                    // Ask server to ban a player
                    // Server checks to make sure we are a moderator
                    // Server adds steamid to bans.cfg
                       


                });
        * 
        * 
        * 
        * 
        * 
        * 
        * 
        * 
        * 
        * 
        */





        // Prepend server only commands with #if UNITY_SERVER so that they don't get put into client builds at all
#if UNITY_SERVER
        SERVER_DEBUG_MESSAGE = new ConsoleCommand<string>("debug", "Debug.logs a message on the server's client only", "debug <string>", (x) =>
        {
            Debug.Log(x);

        });

#endif

        // ADD MORE COMMANDS HERE






        commandList = new List<object>
        {
            SET_FOV,
            HELP,
            BAN_BY_NAME,
            BAN_BY_ID,
            MOD_BY_ID,
            MOD_BY_NAME,
            KICK_BY_ID,



#if UNITY_SERVER
            SERVER_DEBUG_MESSAGE,
#endif

        };
    }






#if !UNITY_SERVER
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebug();
        }


        if(Input.GetKeyDown(KeyCode.Return) && showConsole)
        {
            HandleInput();
            input = "";
        }
    }


    void ToggleDebug()
    {
        showConsole = !showConsole;
    }




    Vector2 scroll;




    private void OnGUI()
    {
        if(!showConsole)
            return;

        float y = 0f;

        if (showHelp)
        {


            GUI.Box(new Rect(0, y, Screen.width, 100), "");

            Rect viewport = new Rect(0, 0, Screen.width - 30, 20 * commandList.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, 90), scroll, viewport);


            for (int i = 0; i < commandList.Count; i++)
            {
                ConsoleCommandBase command = commandList[i] as ConsoleCommandBase;


                string label = command.commandFormat + " - " + command.commandDescription;

                Rect labelRect = new Rect(5, 20 * i, viewport.width - 100, 20);

                GUI.Label(labelRect, label);

            }

            GUI.EndScrollView();


            y += 100;
        }

        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);

    }


#endif




    // This function handles input from client and server. 

    // The client enters input via text field.

    // The server enters input via the ServerConsole

    // This function just parses the type of action and then invokes the command's action 
    public void HandleInput()
    {
        string[] properties = input.Split(' ');



        for (int i = 0; i < commandList.Count; i++)
        {
            ConsoleCommandBase commandBase = commandList[i] as ConsoleCommandBase;

            if (input.Contains(commandBase.commandId))
            {


                if (commandList[i] as ConsoleCommand != null)
                {
                    (commandList[i] as ConsoleCommand).Invoke();
                }
                else if (commandList[i] as ConsoleCommand<int> != null)
                {
                    int x;
                    if(int.TryParse(properties[1], out x))
                        (commandList[i] as ConsoleCommand<int>).Invoke(x);
                }
                else if (commandList[i] as ConsoleCommand<float> != null)
                {
                    float x;
                    if (float.TryParse(properties[1], out x))
                        (commandList[i] as ConsoleCommand<float>).Invoke(x);
                }
                else if (commandList[i] as ConsoleCommand<ulong> != null)
                {
                    ulong x;
                    if (ulong.TryParse(properties[1], out x))
                        (commandList[i] as ConsoleCommand<ulong>).Invoke(x);
                }
                else if (commandList[i] as ConsoleCommand<string> != null)
                {
                    (commandList[i] as ConsoleCommand<string>).Invoke(properties[1]);
                }

            }
        }
    }


}
