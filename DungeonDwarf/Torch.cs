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
        private RenderWindow win;

        private world.TileMap tileMap;

        private bool move = false;
        public bool torchBool = true;

        private bool anim = false;
        private Vector2i textureVector = new Vector2i(0, 0);

        public Torch(Vector2f poss, Texture _torchTex, RenderWindow _win, world.TileMap _map)
        {
            tileMap = _map;

            torchSprite = new Sprite(_torchTex);
            torchSprite.Scale = new Vector2f(0.5f, 0.5f);

            torchSize.X = (24) * torchSprite.Scale.X;
            torchSize.Y = (83) * torchSprite.Scale.Y; 
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
            
            torchSprite.TextureRect = new IntRect(textureVector.X * 24, textureVector.Y * 83, 24, 83);

            if (!anim)
            {
                anim = true;
                Player.delayUtil(250, () => textureVector.X = 1);
                Player.delayUtil(500, () => textureVector.X = 2);
                Player.delayUtil(750, () => textureVector.X = 3);
                Player.delayUtil(750, () => anim = false);
            }
        }


        public Vector2f GetCenter()
        {
            return new Vector2f(torchSize.X / 2f + torchPosition.X, torchSize.Y / 2f + torchPosition.Y);
        }
    }
}
