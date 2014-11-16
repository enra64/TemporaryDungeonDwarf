using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace DungeonDwarf
{
    class StartScreen
    {
        private RenderWindow win;
        private Sprite background, startText;
        private int currentAlphaPercent = 100;
        private bool fadeOut = true;
        public StartScreen(RenderWindow _w)
        {
            win = _w;
            Texture bg = new Texture("textures/menu/startBG.png");
            bg.Smooth = true;
            background = new Sprite(bg);
            background.Scale = new Vector2f((float)win.Size.X / (float)bg.Size.X, (float)win.Size.Y / (float)bg.Size.Y);
            Texture text = new Texture("textures/menu/startButton.png");
            startText = new Sprite(text);
            startText.Scale = background.Scale;
            startText.Position = new Vector2f(win.Size.X / 2 - startText.GetGlobalBounds().Left / 2, win.Size.Y / 2 - startText.GetGlobalBounds().Top / 2);
        }

        public void Update(){
            win.DispatchEvents();
            if (currentAlphaPercent == 0)
                fadeOut = false;
            if (currentAlphaPercent == 100)
                fadeOut = true;
            if (fadeOut)
                currentAlphaPercent--;
            else
                currentAlphaPercent++;
            int alphaInt = (255*currentAlphaPercent)/100;
            byte alpha = (byte)alphaInt;
            startText.Color = new Color(255, 255, 255, alpha);
        }

        public void Draw()
        {
            win.Clear();
            win.Draw(background);
            win.Draw(startText);
            win.Display();
        }
    }
}
