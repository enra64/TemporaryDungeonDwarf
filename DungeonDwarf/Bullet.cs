using System;
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
        //private float xScale, yScale;
        //private world.TileMap tileMap;
        int bulletLifeTime = 0;
        bool spielerbewegung = true;

        public Bullet(Vector2f poss, Texture _tex, RenderWindow fenster1,bool bewegung)
        {
            bulletTexture = _tex;
            bulletSprite = new Sprite(bulletTexture);
            bulletSprite.Scale = new Vector2f(0.5f,0.5f);   // changes the scale of the sprite

            bulletSize.X = bulletTexture.Size.X * bulletSprite.Scale.X;      // used for tile colliding in method update();
            bulletSize.Y = bulletTexture.Size.Y * bulletSprite.Scale.Y;    // ---- || ----
            bulletPosition = poss;

            win = fenster1;
            spielerbewegung = bewegung;
        }

        public void Draw()
        {
            bulletSprite.Position = bulletPosition;
            win.Draw(bulletSprite);
            //Console.WriteLine(zaehler);
        }

        public FloatRect GetRect()
        {
            return new FloatRect(bulletPosition.X, bulletPosition.Y, bulletSize.X, bulletSize.Y);
        }

        public void Update()
        {
            if (spielerbewegung== true){
            bulletPosition.X += 15f;
            bulletSprite.Position = bulletPosition;
            }
            else{
              

                bulletPosition.X -= 15f;
            bulletSprite.Position = bulletPosition;
            }

            bulletLifeTime++;
           
        }

        /// <summary>
        /// convenience method used by lighting
        /// </summary>
        /// <returns></returns>
        public Vector2f GetCenter()
        {
            return new Vector2f(bulletSize.X / 2f + bulletSprite.Position.X, bulletSize.Y / 2f + bulletSprite.Position.Y);
        }
        public int BulletLife()
        {
            return bulletLifeTime;
        }
        
    }

}
