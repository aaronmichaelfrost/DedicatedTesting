using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConsoleCommandBase
{


    public enum AuthLevel
    {
        Client,
        Moderator,
        Server
    }


    private string _commandId;
    private string _commandDescription;
    private string _commandFormat;
    

    public string commandId {  get { return _commandId; } }
    public string commandDescription { get { return _commandDescription; } }
    public string commandFormat {  get { return _commandFormat; } }


    public ConsoleCommandBase(string id, string description, string format)
    {
        _commandId = id;
        _commandDescription = description;
        _commandFormat = format;
    }
}




public class ConsoleCommand : ConsoleCommandBase
{

    private Action command;
    public ConsoleCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        this.command = command;
    }


    public void Invoke()
    {
        command.Invoke();
    }
}


public class ConsoleCommand<T> : ConsoleCommandBase
{

    private Action<T> command;
    public ConsoleCommand(string id, string description, string format, Action<T> command) : base(id, description, format)
    {
        this.command = command;
    }


    public void Invoke(T value)
    {
        command.Invoke(value);
    }
}