using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML;
using SFML.Window;

namespace DungeonDwarf
{
    class EndScreen
    {
        
        private RenderWindow win;
        private Sprite background;

        public EndScreen(RenderWindow _w)
        {
            win = _w;
            Texture bg = new Texture("textures/menu/endBG.png");
            bg.Smooth = true;
            background = new Sprite(bg);
            background.Scale = new Vector2f((float)win.Size.X / (float)bg.Size.X, (float)win.Size.Y / (float)bg.Size.Y);
        }

        public void Update(){
            win.DispatchEvents();
        }

        public void Draw()
        {
            win.Clear();
            win.Draw(background);
            win.Display();
        }
    }
}

