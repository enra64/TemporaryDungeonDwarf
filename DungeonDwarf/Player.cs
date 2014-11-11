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
        private float xScale = 0.8f, yScale = 0.8f;
        private world.TileMap tileMap;
        private Vector2f currentOffset, originalOffset;
        
        //constructor
        public Player(RenderWindow _w, float _s, world.TileMap _map)
        {
            tileMap = _map;
            //enable offset calculation: get view origin from tilemap
            originalOffset = tileMap.viewOrigin;

            //renderwindow 
            win = _w;
            //add player texture and sprite
            Texture playerTexture = new Texture("textures/world/earthTile.png");
            playerSprite = new Sprite(playerTexture);
            //scale
            playerSprite.Scale = new Vector2f(xScale, yScale);
            playerSize.X = playerTexture.Size.X * xScale;
            playerSize.Y = playerTexture.Size.Y * yScale;
            playerPosition = new Vector2f(270f, 270f);
            playerSprite.Position = playerPosition;
        }

        /// <summary>
        /// Returns the center of the player
        /// </summary>
        /// <returns></returns>
        public Vector2f GetCenter()
        {
            return new Vector2f(playerPosition.X + playerSize.X / 2, playerPosition.Y + playerSize.Y / 2);
        }

        
        public void Update()
        {
            //calculate offset
            currentOffset = win.GetView().Center-originalOffset;

            if (!tileMap.Collides(playerPosition, playerSize)){
                //down movement commented out
                //if (Keyboard.IsKeyPressed(Keyboard.Key.S) && playerPosition.Y < win.Size.Y - playerSize.Y)
                //    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Speed)))
                //        playerPosition.Y += Speed;
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.MOVE_SPEED, 0f)))
                        playerPosition.X -= Global.MOVE_SPEED;
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < (win.Size.X + currentOffset.X) - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(Global.MOVE_SPEED, 0)))
                        playerPosition.X += Global.MOVE_SPEED;
                
                //jump
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && playerPosition.Y > currentOffset.Y)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.JUMP_SPEED)))
                        playerPosition.Y -= Global.JUMP_SPEED;

                //Gravity stuff
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GRAVITY)))
                    playerPosition.Y += Global.GRAVITY;
                
                playerSprite.Position = playerPosition;
            }
            
        }

        public void Draw()
        {
            win.Draw(playerSprite);
        }
    }
}
