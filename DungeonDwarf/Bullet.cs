﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;
using SFML.Graphics;

namespace DungeonDwarf
{
    class Bullet
    {
        public Vector2f bulletPosition, bulletSize;
        private Sprite bulletSprite;
        private Texture bulletTexture;
        private RenderWindow win;
        private float xScale, yScale;
        private world.TileMap tileMap;
        int zaehler = 0;

        public Bullet(Vector2f poss, string texturePath, RenderWindow fenster1)
        {
            bulletTexture = new Texture(texturePath);
            bulletSprite = new Sprite(bulletTexture);
            bulletSprite.Scale = new Vector2f(0.5f,0.5f);   // changes the scale of the sprite

            bulletSize.X = bulletTexture.Size.X * bulletSprite.Scale.X;      // used for tile colliding in method update();
            bulletSize.Y = bulletTexture.Size.Y * bulletSprite.Scale.Y;    // ---- || ----
            bulletPosition = poss;

            win = fenster1;
        }

        public void draw()
        {
            bulletSprite.Position = bulletPosition;
            win.Draw(bulletSprite);
            Console.WriteLine(zaehler);
        }

        public void update()
        {
            bulletPosition.X += 1;
            bulletSprite.Position = bulletPosition;
        }


    }

}