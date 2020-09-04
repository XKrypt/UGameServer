using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UGameServer
{
    class UDPSendManager
    {
        Queue<Action> actions = new Queue<Action>();

        bool isRunning = true;

        float TICK;
  
        public void ExecuteActions()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;
            


          


            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    Queue<Action> _CopyActions;
                    lock (actions)
                    {
                        _CopyActions = new Queue<Action>(actions);
                    }

                    if (_CopyActions.Count > 0) {
                        _CopyActions.Dequeue()();
                    }
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }


        public void ScheduleAction(Action action)
        {
            lock (actions)
            {
                actions.Enqueue(action);
            }
        }
    }
}
