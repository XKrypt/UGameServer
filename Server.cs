using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;


namespace UGameServer
{



    class Server
    {
        public static int MaxPlayers = 64;
        public static int Port = 26950;
        public static int UDPPort = 8006;


        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        static List<Room> rooms = new List<Room>();

        private static TcpListener tcpListener;

        public ThreadManager uDPSendManager;

        private static UdpClient udpListener;

        public void Start()
        {

            Console.WriteLine("Starting server...");
            InitializeServerData();

            rooms.Add(new Room()
            {
                RoomName = "Room 1",
                MaxPlayersInRoom = 32,
                clientsInRoom = new List<Client>()

            });

            //Start Server
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();

            //register client accept callback
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(UDPPort);
            udpListener.BeginReceive(new AsyncCallback(UDPCallBack), null);

            Console.WriteLine($"Server started on port {Port}.");
        }


        private void TCPConnectCallback(IAsyncResult _result)
        {

            //accept conection
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}... \n");



            //Find a empty client space to register
            for (int i = 1; i <= MaxPlayers; i++)
            {

                if (clients[i].clientSock == null)
                {


                    //Events
                    clients[i].onClientDisconnect += DisconnectClient;
                    clients[i].onClientNeedToCreateRoom += CreateRoom;
                    clients[i].onClientNeedToExitRoom += RemovePlayerFromRoom;
                    clients[i].onClientChangeVar += TriggerVariableChange;
                    clients[i].onClientNeedToJoinRoom += AddPlayerToRoom;
                    clients[i].updateClients += UpdateInfoFromUserInRoom;
                    clients[i].onMessageInfo += OnMessageInfo;
                    clients[i].isUsed = true;
                    clients[i].Connect(_client);



                    UpdateRooms();
                    return;
                }
            }
            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");



        }



        private void UDPCallBack(IAsyncResult result)
        {
           
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] _data = udpListener.EndReceive(result, ref _clientEndPoint);
            
            string msg = Encoding.UTF8.GetString(_data);
            Packet package = new Packet(_data);
            Console.WriteLine("UDP Received");
            try
            {
               //ThreadManager.ExecuteOnMainThread(() => {
                    UDPClientIdentity uDPClientIdentity = JsonConvert.DeserializeObject<UDPClientIdentity>(package.GetData());



                if (!clients[uDPClientIdentity.id].isUsed)
                {

                    return;
                }

                if (clients[uDPClientIdentity.id].udpClient == null)
                {
                    Console.WriteLine("added" + _clientEndPoint.Address.ToString());
                    clients[uDPClientIdentity.id].udpClient = _clientEndPoint;

                }
                if (uDPClientIdentity.content == "Connect")
                {

                    string confirm = CommandPackager.PackCommand(

                   new ServerComunication()
                   {
                       command = Interpreter.PackComandType(Command.ConnectionID),
                       parameters = "Conected"
                   });

                    Packet confirmPackege = new Packet(confirm);
                    udpListener.BeginSend(confirmPackege.writeData(), confirmPackege.writeData().Length, _clientEndPoint.Address.ToString(), _clientEndPoint.Port, null, null);
                    
                }
                    if (uDPClientIdentity.content != "Connect") {
                        SendUDPTrafficVar(package.GetData(), uDPClientIdentity.id);
                    }
                    udpListener.BeginReceive(UDPCallBack, null);
               //});

            }
            catch (Exception e)
            {
                Console.WriteLine("error \n " + e  );
            }
          
                

           





        }

        public void SendUDPTrafficVar(string value, int id)
        {
            Console.WriteLine("Sending");
            string data = CommandPackager.PackCommand(

                new ServerComunication()
                {
                    command = Interpreter.PackComandType(Command.Event),
                    parameters = JsonConvert.SerializeObject(new EventTrigger()
                    {
                        _event = ResultEvent.TrafficVar,
                        eventResultResponse = value
                    })
                });
            Packet package = new Packet(data);

            if (clients[id].room.RoomName == null || clients[id].room.RoomName == "")
            {
                Console.WriteLine("The player is not in a room");
                return;
            }


            Room room = clients[id].room;

            foreach (var item in clients[id].room.clientsInRoom)
            {

                if (item.udpClient == null)
                {
                    continue;
                }
                Console.WriteLine("Sending To id :" + id);
               
                udpListener.BeginSend(package.writeData(), package.writeData().Length, item.udpClient.Address.ToString(), item.udpClient.Port, null,null);
            }






        }

        //Initialize clients Data
        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i)
                {
                    isUsed = false
                });
            }


            Console.WriteLine("Initialized packets.");
        }

        private void AddPlayerToRoom(int ID, string roomName)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].RoomName == roomName)
                {
                    rooms[i].clientsInRoom.Add(clients[ID]);
                    clients[ID].room = rooms[i];
                    Console.WriteLine("Client ID" + ID + " joined in room " + roomName);



                    Console.WriteLine("User , " + ID + " joined in room " + roomName);

                        for (int x = 0; x < rooms.Count; x++)
                        {
                            if (rooms[x].RoomName == roomName)
                            {
                                foreach (Client player in rooms[x].clientsInRoom)
                                {
                                    if (player.isUsed)
                                    {
                                    Clients _clients = new Clients();
                                    _clients.clients = GetAllUsersFromRoom(rooms[x]);
                                    //TriggerEvent and send id of player
                                    player.SendEventResult(new EventTrigger()
                                        {

                                            _event = ResultEvent.UserEnterInRoom,
                                            eventResultResponse = JsonConvert.SerializeObject(_clients)

                                        });;
                                    }
                                    else
                                    {
                                        Console.WriteLine("error on add player" + player.getID());
                                    }

                                }
                                break;
                            }
                        }
                    

                    UpdateRooms();

                    return;
                }
            }

            clients[ID].SendEventResult(new EventTrigger()
            {
                _event = ResultEvent.ErrorCreateRoom,
                eventResultResponse = "Room not find"
            });

           

           
        }


        private void RemovePlayerFromRoom(int ID, string roomName)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].RoomName == roomName)
                {
                    rooms[i].clientsInRoom.Remove(clients[ID]);
                    clients[ID].room = new Room();
                    Console.WriteLine("Client ID" + ID + " removed from room " + roomName);
                    break;
                }
            }


            //Send event order to all players in room
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].RoomName == roomName)
                {
                    foreach (Client player in rooms[i].clientsInRoom)
                    {
                        if (player.isUsed)
                        {
                            //TriggerEvent and send id of player
                            player.SendEventResult(new EventTrigger()
                            {

                                _event = ResultEvent.UserExitRoom,
                                eventResultResponse = ID.ToString()

                            });
                        }
                        else
                        {
                            Console.WriteLine("Error removing player from room. player Id :" + player.getID());
                        }

                    }

                    break;
                }

            }

            UpdateRooms();

        }

        private void CreateRoom(int id,string Name, int _maxPlayers)
        {

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].RoomName == Name)
                {
                    Console.WriteLine("The room "+Name+"already exist");
                    clients[id].SendEventResult(new EventTrigger()
                    {
                        _event = ResultEvent.ErrorCreateRoom,
                        eventResultResponse = "This room "+ Name +"already exists"
                    });
                    return;
                }
            }


            rooms.Add(new Room()
            {
                RoomName = Name,
                MaxPlayersInRoom = _maxPlayers,
                clientsInRoom = new List<Client>()

            });
            UpdateRooms();
            Console.WriteLine("Created new room , " + Name + " MaxPlayers : " + _maxPlayers);
        }




        void Remove(string roomName)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].RoomName == roomName)
                {
                    rooms.Remove(rooms[i]);
                }
            }

            UpdateRooms();

        }

        private List<ClientInfo> GetAllUsersFromRoom(Room room)
        {
            if (room.RoomName == null || room.RoomName == "")
            {
                return null;
            }
            List<ClientInfo> clientInfos = new List<ClientInfo>();

            foreach (var user in room.clientsInRoom)
            {
                clientInfos.Add(new ClientInfo()
                {
                    Id = user.getID(),
                    vars = user.GetUserVars()
                });
            }

            return clientInfos;
        }


        private void UpdateRooms()
        {
            List<RoomInfo> roomsInfo = new List<RoomInfo>();


            foreach (var _room in rooms)
            {
                roomsInfo.Add(new RoomInfo()
                {
                    RoomName = _room.RoomName,
                    MaxPlayersInRoom = _room.MaxPlayersInRoom,
                    clientsInRoom = _room.clientsInRoom.Count
                });
            }

            string content = JsonConvert.SerializeObject(new UpdateRoomsInfo()
            {
                rooms = roomsInfo
            });

            

            foreach (var _client in clients)
            {
                if (_client.Value.isUsed)
                {
                    _client.Value.Send(CommandPackager.PackCommand(new ServerComunication() { 

                        command = Enum.GetName(typeof(Command), Command.UpdateRooms),
                        parameters = content
                    }));

                }
            }
        }


        private void UpdateInfoFromUserInRoom(Room room)
        {
            if (room.RoomName == null || room.RoomName == "")
            {
                return;
            }

            Clients _clients = new Clients();
            _clients.clients = GetAllUsersFromRoom(room);
            ServerComunication serverComunication = new ServerComunication()
            {
                command = Enum.GetName(typeof(Command), Command.UpdateClientsInfo),
                parameters = JsonConvert.SerializeObject(_clients)
                
            };


            
            foreach (var user in room.clientsInRoom)
            {
                user.Send(CommandPackager.PackCommand(serverComunication));
            }
        }


        private void TriggerVariableChange(Room room, int _clientID, UserVariable _varName)
        {
            if (room.RoomName == null || room.RoomName == "")
            {
                return;
            }
            EventTrigger eventTrigger = new EventTrigger()
            {
                _event = ResultEvent.VariableChange,
                eventResultResponse = JsonConvert.SerializeObject(new VarChanged()
                {
                    var = _varName,
                    clientID = _clientID
                })

            };
           
                foreach (Client player in room.clientsInRoom)
                {
                    player.SendEventResult(eventTrigger);
                }
        }


        private void OnMessageInfo(MessageInfo message)
        {
            foreach (var _client in  clients)
            {
                if (_client.Value.getID() == message.id) {
                    _client.Value.SendMessageInfo(message.message, message.id);
                }
            }
        }

        private void DisconnectClient(int id)
        {
           /* clients[id].clientSock.Dispose();
            clients[id].clientSock.Close();*/
            
            
            UpdateRooms();
            if (clients[id].room.RoomName != null || clients[id].room.RoomName != "")
            {
                RemovePlayerFromRoom(id, clients[id].room.RoomName);
            }
            clients[id] = new Client(id)
            {
                isUsed = false
            };
            Console.WriteLine("Client id :" + id + " Disconnected");
        }

        
    }
        
}
