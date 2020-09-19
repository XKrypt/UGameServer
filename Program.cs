using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;

namespace UGameServer
{
    class Program
    {
        private static bool isRunning = false;
          

        static void Main(string[] args)
        {

            Console.Title = "Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            ThreadManager threadManager =  new ThreadManager();

            Server server = new Server();
            //server.uDPSendManager = uDPSendManager;
            
            server.Start();
            mainThread.Start();
          
        }
        
        static TimeSpan lastLoopTime;

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_nextLoop > DateTime.Now)
                    {
                        try{

                            Thread.Sleep(_nextLoop - DateTime.Now);

                            lastLoopTime = _nextLoop - DateTime.Now;
                        }
                        catch(Exception e){
                            continue;

                        }
                      
                    }
                }
            }

        }
    }
}
