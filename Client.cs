using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;

namespace UGameServer
{
    public class Client
    {
        private int id; //client ID
        

        public TcpClient clientSock;
        public IPEndPoint udpClient;

        NetworkStream stream;


        private static int dataBufferSize = 4096; //BufferSize
        private byte[] receiveBuffer; //bufferReceived;
        private  List<UserVariable> userVariables = new List<UserVariable>();


        //the room of player is in.
        public Room room = new Room();
        
        public bool isUsed;
        public Client(int _id)
        {
            id = _id;
        }

        public int getID()
        {
            return id;
        }

        public List<UserVariable> GetUserVars()
        {
            return userVariables;
        }


        public void Connect(TcpClient _socket)
        {
            //Connect client
            clientSock = _socket;
            //Buffer Configs
            clientSock.ReceiveBufferSize = dataBufferSize;
            clientSock.SendBufferSize = dataBufferSize;

            //save stream socket in the class
            stream = clientSock.GetStream();

            //receiveBufferSize;
            receiveBuffer = new byte[dataBufferSize];
            //callBack to read messages from clients.
           stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            Send(CommandPackager.PackCommand(new ServerComunication()
            {
                command = Interpreter.PackComandType(Command.ConnectionID),
                parameters = id.ToString()
            }));


        }

        //Receive callback.
        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                //Lenght of message;
                int _byteLength = stream.EndRead(_result);

               
                if (_byteLength <= 0)
                {
                    ClientDisconnect();
                    // TODO: disconnect
                    return;
                }
                byte[] copyBuffer = new byte[dataBufferSize];

                Array.Copy(receiveBuffer, copyBuffer, _byteLength);
                

                
                //Decode message
                
            
                /*string datareceived = Encoding.UTF8.GetString(copyBuffer);

                Console.WriteLine("\n  Data Received From cliente id: " + id);
                Console.WriteLine("\n"+ datareceived + "\n");
                Console.WriteLine("-----------------\n");
                Console.WriteLine("Parsing.... \n ");*/
                Packet pacote =  new Packet(copyBuffer);
                //Packet pacote2 =  new Packet(datareceived);
                byte[] TT = new byte[pacote.PackageLenght()];
                Array.Copy(copyBuffer, 4, TT,0, pacote.PackageLenght());
                string datareceived = Encoding.UTF8.GetString(TT);

                //Unpack message to parse
                ServerComunication serverComunication = CommandPackager.UnpackCommand(datareceived);
                receiveBuffer = null;
                receiveBuffer = new byte[dataBufferSize];
                //Register Calback Again.

                //if ocurred an error parsing.
                if (serverComunication != null)
                {
                    Console.WriteLine("Parsed, executing Command \n ");
                    CommandExecuter(serverComunication);
                }
            }
            catch ( Exception e)
            {
                Console.WriteLine("Error receiving data: " + e);
            }
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }



        private void AddUserVariable(string variable)
        {
            UserVariable userVariable;
            try
            {
                //parse json info to variable
                userVariable = JsonConvert.DeserializeObject<UserVariable>(variable);
            }
            catch
            {
                Console.WriteLine("Error Parsing user Variable");
                return;
            }
            
            //if variable exists, will be changed
            for (int i = 0; i < userVariables.Count; i++)
            {
                if (userVariables[i].name == userVariable.name)
                {
                    userVariables[i] = new UserVariable()
                    {
                        name = userVariable.name,
                        value = userVariable.value
                    };
                    Console.WriteLine("Added UserVar for user ID : " + id + "\n");
                    Console.WriteLine("User var Name :" + userVariable.name +"\n" + " Value :" + userVariable.value +  "\n");
                    if (room.RoomName != "" || room.RoomName != null)
                    {
                        updateClients?.Invoke(room);
                        onClientChangeVar?.Invoke(room, id, userVariable);
                    }
                    return;
                }
            }
            //if not exist, add to user variables.
            userVariables.Add(userVariable);

            Console.WriteLine("Added UserVar for user ID : " + id + "\n");
            Console.WriteLine("User var Name :" + userVariable.name + "\n" + " Value :" + userVariable.value + "\n");


            onClientChangeVar?.Invoke(room, id, userVariable);
            if (room.RoomName != "" || room.RoomName != null) {
                updateClients?.Invoke(room);
            }
            
        }
        
       
        //Send message to client.
        public void Send(string msg)
        {

            if (stream == null) return;
            Packet package = new Packet(msg);

            Packet test = new Packet(package.writeData());
                Console.WriteLine("Sending Message to user " + id + ", Message : " + test.GetData() + "\n");

                stream.BeginWrite(package.writeData(), 0, package.writeData().Length, null, null);
                //Console.WriteLine("Sending Message to user " + id + ", Message : " + msg + "\n");
       
            
           
        }


        public delegate void OnClientDisconnect(int ID);

        public OnClientDisconnect onClientDisconnect;

        public delegate void OnClientNeedToJoinRoom(int ID, string roomName);

        public  OnClientNeedToJoinRoom onClientNeedToJoinRoom;

        public delegate void OnClientNeedToExitRoom(int ID, string roomName);

        public  OnClientNeedToExitRoom onClientNeedToExitRoom;

        public delegate void OnClientNeedToCreateRoom(int id,string Name, int _maxPlayers);

        public  OnClientNeedToCreateRoom onClientNeedToCreateRoom;

        public delegate void OnClientChangeVar(Room room, int clientID, UserVariable varName);

        public OnClientChangeVar onClientChangeVar;

        public delegate void UpdateClientsInfo(Room room);

        public UpdateClientsInfo updateClients;


        private void CommandExecuter(ServerComunication serverComunication)
        {
            Command command = Interpreter.GetCommand(serverComunication.command);


            if (command == Command.UserVariable)
            {

                AddUserVariable(serverComunication.parameters);

            }
            else if (command == Command.CreateRoom)
            {

                RoomConfig room = JsonConvert.DeserializeObject<RoomConfig>(serverComunication.parameters);
                onClientNeedToCreateRoom?.Invoke(id,room.RoomName,room.MaxPlayersInRoom);

            }
            else if (command == Command.JoinInRoom)
            {

                onClientNeedToJoinRoom?.Invoke(id, serverComunication.parameters);
                updateClients?.Invoke(room);

            }
            else if (command == Command.ExitRoom)
            {
                onClientNeedToExitRoom?.Invoke(id,room.RoomName);
                updateClients?.Invoke(room);
            }

        }
        private void ClientDisconnect()
        {
            clientSock.GetStream().Close();
            clientSock.Close();
            clientSock.Dispose();
            stream.Close();
            stream.Dispose();
            receiveBuffer = null;
            stream = null;
            clientSock = null;
            onClientDisconnect?.Invoke(id);
            udpClient = null;
            updateClients?.Invoke(room);
        }


        public void SendEventResult(EventTrigger eventTrigger)
        {
            
            string package = CommandPackager.PackCommand(

                new ServerComunication()
                {
                    command = Interpreter.PackComandType(Command.Event),
                    parameters = JsonConvert.SerializeObject(eventTrigger)
                }

                );

            Send(package);
        }


       
        }



    }

