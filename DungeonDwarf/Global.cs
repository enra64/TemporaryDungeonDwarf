using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDwarf
{
    class Global
    {
        //global = public constant dump
        /* 
         * If you put 3 slashes in front of a variable or function declaration,
         * you can write a summary, which will show up to everyone thinking about
         * using said object.
         */

        /*
         * PLAYER VARIABLES
         */
        /// <summary>
        /// Speed used for movement by user
        /// </summary>
        public const float PLAYER_MOVEMENT_SPEED = 10f;
        /// <summary>
        /// Speed to propel upwards during jumps by player
        /// </summary>
        public const float PLAYER_JUMP_SPEED = 15f;

        /*
         * GLOBAL CONFIGURATION VARIABLES
         */
        /// <summary>
        /// Gravity that is supposed to be used by everyone.
        /// </summary>
        public const float GLOBAL_GRAVITY = 5f;
        /// <summary>
        /// This is an important Variable. On each update and during the Initialization (e.g. dont worry about availability)
        /// this will get updated to contain the current offset of the view. 
        /// ALL POSITIONS _MUST_ BE RELATIVE TO THIS (except if you are sure you want to draw absolute to the window origin),
        /// meaning that if you for example want to position an enemy at top left of the currently seen view, you can not use
        /// position(0,0), but have to use position(CURRENT_WINDOW_ORIGIN.X, CURRENT_WINDOW_ORIGIN.Y)
        /// </summary>
        public Vector2f CURRENT_WINDOW_ORIGIN;
    }
}
