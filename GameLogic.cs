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

    public static class Constants
    {
        public const int TICKS_PER_SEC = 144;
        public const long MS_PER_TICK = 1000 / TICKS_PER_SEC;

        public  const float TICK30 = 33.33333333333333f;
        public  const float TICK60 = 16.66666666666667f;
        public  const float TICK120 = 8.333333333333333f;
        public  const float TICK240 = 4.166666666666667f;
    }


   
}
