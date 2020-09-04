using System;
using System.Collections.Generic;
using System.Text;

namespace UGameServer
{
    class GameLogic
    {
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }
    }

    class Constants
    {
        public const int TICKS_PER_SEC = 240;
        public const long MS_PER_TICK = 1000 / TICKS_PER_SEC;
    }
}
