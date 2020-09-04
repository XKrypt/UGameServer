using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;


namespace UGameServer
{
    class CommandPackager
    {
     
            public static string PackCommand(ServerComunication serverComunication)
            {
                string ComandPackage = JsonConvert.SerializeObject(serverComunication);

                return ComandPackage;

            }

            public static ServerComunication UnpackCommand(string value)
            {

                try
                {
                    ServerComunication serverComunication = JsonConvert.DeserializeObject<ServerComunication>(value);
                    return serverComunication;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing command" + e);
                }

            return null;
            
            }


        

     
    }


   public class ServerComunication
    {
        public string command { get; set; }

        public string parameters { get; set; }

    }
}
