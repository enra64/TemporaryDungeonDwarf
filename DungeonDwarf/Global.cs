using SFML.Window;
using System;
using System.Collections.Generic;

namespace DungeonDwarf
{
    static class Global
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
        public const float PLAYER_JUMP_SPEED = 35f;

        /*
         * TILE TYPES VS THEIR ID
         */
        public const int
            EARTH_TILE = 0,
            EARTH_TOP_TILE = 1,
            AIR_TILE = 2,
            LAVA_TOP_TILE = 3,
            LAVATILE = 4,
            SPAWNTILE_1 = 5,
            SPAWNTILE_2 = 6,
            SPAWNTILE_3 = 7,
            TORCHTILE=8;

        /*
         * GLOBAL CONFIGURATION VARIABLES
         * Hint: Due to the fact that these Variables are static, you neither can nor need to
         * instance this class to access them.
         */
        /// <summary>
        /// Gravity that is supposed to be used by everyone.
        /// </summary>
        public const float GLOBAL_GRAVITY = 8f;
        /// <summary>
        /// DIS BE OF UTMOST IMPORTANCY. More readable in Global.cs.
        /// On each update and during the Initialization (e.g. dont worry about availability)
        /// this will get updated to contain the current offset of the view. 
        /// ALL POSITIONS _MUST_ BE RELATIVE TO THIS (except if you are sure you want to draw absolute to the window origin),
        /// meaning that if you for example want to position an enemy at top left of the currently seen view, you can not use
        /// position(0,0), but have to use position(CURRENT_WINDOW_ORIGIN.X, CURRENT_WINDOW_ORIGIN.Y)
        /// </summary>
        public static Vector2f CURRENT_WINDOW_ORIGIN;
        /// <summary>
        /// This stores the original window origin, you should be able to use CURRENT_WINDOW_ORIGIN instead to save on
        /// calculations
        /// </summary>
        public static Vector2f BEGIN_WINDOW_ORIGIN;
        /// <summary>
        /// Could possibly be the scalefactor set by the tilemap
        /// </summary>
        public static float GLOBAL_SCALE;

        /*
         * SKILLING
         */
        /// <summary>
        /// .Count contains skilllevel count, strings contain texture position
        /// </summary>
        public static List<string> TEXPATH_LIST_SWORD;
        /// <summary>
        /// contains descriptions
        /// </summary>
        public static List<string> DESCRIPTION_LIST_SWORD;
        /// <summary>
        /// .Count contains skilllevel count, strings contain texture position
        /// </summary>
        public static List<string> TEXPATH_LIST_ARROW;
        /// <summary>
        /// contains descriptions
        /// </summary>
        public static List<string> DESCRIPTION_LIST_ARROW;
        /// <summary>
        /// .Count contains skilllevel count, strings contain texture position
        /// </summary>
        public static List<string> TEXPATH_LIST_ARMOR;
        /// <summary>
        /// contains descriptions
        /// </summary>
        public static List<string> DESCRIPTION_LIST_ARMOR;
        /// <summary>
        /// current skilllevel
        /// </summary>
        public static int LEVEL_SWORD = 0, LEVEL_ARROW = 0, LEVEL_ARMOR = 0;
        public static int SKILL_COUNT = 0;
    }
}
