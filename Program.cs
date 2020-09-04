using System;
using System.Diagnostics;
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

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");

            long _nextLoop;
            
            while (isRunning)
            {
                Stopwatch watch = Stopwatch.StartNew();
                bool nextLoop = false;
                while (!nextLoop)
                {
                    GameLogic.Update();

                    

                    if (watch.ElapsedMilliseconds < Constants.TICKS_PER_SEC)
                    {
                        Thread.Sleep((int)(Constants.TICKS_PER_SEC - watch.ElapsedMilliseconds));
                        nextLoop = true;
                    }
                    else
                    {
                        nextLoop = true;
                    }
                }
            }
        }
    }
}
