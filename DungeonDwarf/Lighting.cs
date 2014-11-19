using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonDwarf
{
    class Lighting
    {
        private RenderWindow win;
        private RenderTexture lightMap;
        private RenderStates renderStateAdditive = RenderStates.Default, renderStateMult = RenderStates.Default;
        private Texture lightTexture = new Texture("textures/light/lightball.png");
        private List<Vector2f[]> lightList = new List<Vector2f[]>();

        public Lighting(RenderWindow _w)
        {
            win = _w;
            lightMap = new RenderTexture(win.Size.X, win.Size.Y);
            renderStateAdditive.BlendMode = BlendMode.Add;
            renderStateMult.BlendMode = BlendMode.Multiply;
        }

        public void AddLight(Vector2f inputCenter, Vector2f inputSize, Vector2f lightScale)
        {
            lightList.Add(new Vector2f[] { inputCenter, inputSize, lightScale });
        }

        public void Update()
        {
            //add lighting
            lightMap.Clear(Color.Black);
            Sprite newLightSprite = new Sprite(lightTexture);

            foreach(Vector2f[] p in lightList){
                newLightSprite.Scale = p[2];
                newLightSprite.Position = ConvertToLightPos(newLightSprite, p[0], p[1]);
                lightMap.Draw(newLightSprite, renderStateAdditive);
            }
            //clear lightlist, the positions need to be updated anyway
            lightList.Clear();
        }

        public void Draw()
        {
            //draw light
            Sprite lightSprite = new Sprite(lightMap.Texture);
            lightSprite.Position = Global.CURRENT_WINDOW_ORIGIN;
            win.Draw(lightSprite, renderStateMult);
        }

        private Vector2f ConvertToLightPos(Sprite lightSprite, Vector2f inputPosition, Vector2f inputSize)
        {
            float newX = inputPosition.X;
            float newY = lightMap.Size.Y - inputPosition.Y;

            //align with input
            newX -= (float)lightTexture.Size.X * lightSprite.Scale.X / 2f;
            newY -= (float)lightTexture.Size.Y * lightSprite.Scale.Y / 2f;

            //account for moving view
            newX -= Global.CURRENT_WINDOW_ORIGIN.X;
            newY += Global.CURRENT_WINDOW_ORIGIN.Y;

            return new Vector2f(newX, newY);
        }
    }
}
