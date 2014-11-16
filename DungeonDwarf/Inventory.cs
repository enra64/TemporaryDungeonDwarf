using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace DungeonDwarf
{
    class Inventory
    {
        private RenderWindow win;
        private RectangleShape backGround;
        private Vector2f absoluteSize;

        public Inventory(RenderWindow _w, Vector2f menuSize, Vector2f targetIconSize, Vector2f textureIconSize, String spriteListLocation)
        {
            win = _w;
            //only sizes up to 100 percent are permitted
            if (menuSize.X > 100f)
                menuSize.X = 100f;
            if (menuSize.Y > 100f)
                menuSize.Y = 100f;

            //find final size of menu
            absoluteSize = new Vector2f(((float)win.Size.X * menuSize.X) / 100f, ((float)win.Size.Y * menuSize.Y) / 100f);

            //generate menu background
            backGround = new RectangleShape(absoluteSize);
            backGround.FillColor = Color.White;

            //still need a way to figure out where to place the icons
            /*
             * new
             * init for background, load inventory xml
             * show method. has while loop, only closes on close click or escape;
             *      parameter and return is money for skilling and some static global inventory class
             *      uses inventory xml to display elements in a grid, maybe with some kind of <row1> sorting
             */
        }
    }
}
