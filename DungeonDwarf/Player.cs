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
    class Player
    {
        public Vector2f playerPosition, playerSize;
        private RenderWindow win;
        private Sprite playerSprite;
        private const float SPEED = 10f;
        private const float GRAVITY = 5f;
        private const float JUMP_SPEED = 15f;
        private float xScale = 0.8f, yScale = 0.8f;
        private world.TileMap tileMap;
        //constructor
        public Player(RenderWindow _w, float _s, world.TileMap _map)
        {
            tileMap = _map;
            //renderwindow 
            win = _w;
            //add player texture and sprite
            Texture playerTexture = new Texture("textures/world/earthTile.png");
            playerSprite = new Sprite(playerTexture);
            //scale
            playerSprite.Scale = new Vector2f(xScale, yScale);
            playerSize.X = playerTexture.Size.X * xScale;
            playerSize.Y = playerTexture.Size.Y * yScale;
            playerPosition = new Vector2f(10f, 270f);
            playerSprite.Position = playerPosition;
        }

        // !!! x offset by view change new 0 implementing
        public void Move()
        {
            if (!tileMap.Collides(playerPosition, playerSize))
            {   
                //down commented out
                //if (Keyboard.IsKeyPressed(Keyboard.Key.S) && playerPosition.Y < win.Size.Y - playerSize.Y)
                //    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Speed)))
                //        playerPosition.Y += Speed;
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > 0)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-SPEED, 0f)))
                        playerPosition.X -= SPEED;
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < win.Size.X - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(SPEED, 0)))
                        playerPosition.X += SPEED;
                
                //jump
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && playerPosition.Y > 0)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -JUMP_SPEED)))
                        playerPosition.Y -= JUMP_SPEED;

                //Gravity stuff
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, GRAVITY)))
                        playerPosition.Y += GRAVITY;
                
                playerSprite.Position = playerPosition;
            }
            
        }

        public void Draw()
        {
            win.Draw(playerSprite);
        }
    }
}
