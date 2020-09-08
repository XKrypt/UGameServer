using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UGameServer
{
    public static class Interpreter
    {
        public static Command GetCommand(string cmd)
        {
            return (Command)Enum.Parse(typeof(Command),cmd);
        }

        public static string PackComandType(Command cmd)
        {
            return Enum.GetName(typeof(Command), cmd);
        }

        public static string PackEvent(ResultEvent cmd)
        {
            return Enum.GetName(typeof(ResultEvent), cmd);
        }
    }


   public  enum Command
    {
        CreateRoom,
        DestroyRoom,
        UserVariable,
        RoomVariable,
        JoinInRoom,
        ExitRoom,
        Event,
        ConnectionID,
        UpdateClientsInfo,
        UpdateRooms,
        InfoMessage
    }

    public enum ResultEvent
    {
        VariableChange,
        UserEnterInRoom,
        UserExitRoom,
        TrafficVar,
        ErrorJoinRoom,
        ErrorCreateRoom
    }




    



    public struct Room
    {
        public string RoomName { get; set; }

        public List<Client> clientsInRoom { get; set; }

        public int MaxPlayersInRoom { get; set; }



    }


    public struct RoomInfo
    {
        public string RoomName { get; set; }

        public int clientsInRoom { get; set; }

        public int MaxPlayersInRoom { get; set; }



    }


    public struct UpdateRoomsInfo
    {
        public List<RoomInfo> rooms;
    }

    public struct RoomConfig
    {
        public string RoomName { get; set; }

        public int MaxPlayersInRoom { get; set; }



    }


    public struct RoomVariable{
       public string name { get; set; }

       public string value { get; set; }

    }

    public struct UserVariable
    {
        public string name { get; set; }

        public string value { get; set; }

    }


    public struct EventTrigger
    {
        public ResultEvent _event;

        public string eventResultResponse;
    }



    public struct VarChanged
    {
        public UserVariable var;
        public int clientID;
    }


    public struct UDPClientIdentity
    {
        public int id;

        public string content;
    }


    public struct ClientInfo
    {
        public int Id;

        public List<UserVariable> vars;
    }

    public struct Clients
    {
        public List<ClientInfo> clients;
    }
}


public struct MessageInfo
{
    public string message;

    public int id;
}