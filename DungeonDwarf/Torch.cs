using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML;
using SFML.Graphics;
using SFML.Window;

namespace DungeonDwarf
{
    class Torch
    {
        public Vector2f torchPosition, torchSize;
        private Sprite torchSprite;
        private Texture torchTexture;
        private RenderWindow win;

        private float xScale, yScale;
        private world.TileMap tileMap;

        private bool move = false;


        public bool torchBool = true;

        public Torch(Vector2f poss, string texturePath, RenderWindow _win, world.TileMap _map)
        {
            tileMap = _map;

            torchTexture = new Texture(texturePath);
            torchSprite = new Sprite(torchTexture);
            torchSprite.Scale = new Vector2f(0.5f, 0.5f);   

            torchSize.X = torchTexture.Size.X * torchSprite.Scale.X;
            torchSize.Y = torchTexture.Size.Y * torchSprite.Scale.Y;    
            torchPosition = poss;

            win = _win;
        }

        public void Draw()
        {
            torchSprite.Position = torchPosition;
            win.Draw(torchSprite);
        }

        public void Update()
        {
            if (!tileMap.CheckNextCollide(torchPosition, torchSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                torchPosition.Y += Global.GLOBAL_GRAVITY;
        }

        public void Update(bool _move)
        {
            move = _move;
            if(move)
                if (!tileMap.CheckNextCollide(torchPosition, torchSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    torchPosition.Y += Global.GLOBAL_GRAVITY;
        }


        public Vector2f GetCenter()
        {
            return new Vector2f(torchSize.X / 2f + torchPosition.X, torchSize.Y / 2f + torchPosition.Y);
        }
    }
}
