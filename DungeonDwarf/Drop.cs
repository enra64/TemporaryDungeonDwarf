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
    class Drop
    {
        public Vector2f position, size;

        public string dropType;

        private Sprite sprite;
        private RenderWindow win;

        private world.TileMap tileMap;

        private bool anim = false;
        private Vector2i textureVector = new Vector2i(0, 0);

        public Drop(Vector2f startPosition, Texture _tex, string _dropType, RenderWindow _win, world.TileMap _map)
        {
            tileMap = _map;
            dropType = _dropType;

            sprite = new Sprite(_tex);
            sprite.Scale = new Vector2f(0.5f, 0.5f);

            size.X = (24) * sprite.Scale.X;
            size.Y = (83) * sprite.Scale.Y; 
            position = startPosition;

            win = _win;
        }

        public void Draw()
        {
            sprite.Position = position;
            win.Draw(sprite);
        }

        public void Update()
        {
            if (!tileMap.CheckNextCollide(position, size, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                position.Y += Global.GLOBAL_GRAVITY;
            
            sprite.TextureRect = new IntRect(textureVector.X * 20, textureVector.Y * 80, 20, 80);

            if (!anim){
                anim = true;
                DelayUtil.delayUtil(250, () => textureVector.X = 1);
                DelayUtil.delayUtil(500, () => textureVector.X = 2);
                DelayUtil.delayUtil(750, () => textureVector.X = 0);
                DelayUtil.delayUtil(750, () => anim = false);
            }
        }

        public Vector2f GetCenter()
        {
            return new Vector2f(size.X / 2f + position.X, size.Y / 2f + position.Y);
        }
    }
}