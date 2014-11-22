using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DungeonDwarf
{
    class DelayUtil
    {
        //confusing stuff -> timer "=>" what is this doing???
        /// <summary>
        /// int delay 1000 = 1 sec
        /// After 1 sec it does any Action
        /// </summary>
        /// <returns></returns>
        public static void delayUtil(int delay, Action action)
        {
            Timer timer = new Timer();
            timer.Interval = delay;
            timer.Elapsed += (s, e) =>
            {
                action();
                timer.Stop();
            };
            timer.Start();
        }
    
    }
}
