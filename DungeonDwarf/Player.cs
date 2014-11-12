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
        
        //fenster, sprite, scale, map, viewchange
        private RenderWindow win;
        private Sprite playerSprite;
        private float xScale = 0.8f, yScale = 0.8f;
        private world.TileMap tileMap;
        private Vector2f currentOffset, originalOffset;

        //animated sprite
        Texture playerTextureanim = new Texture("textures/player/player_spritesheet.png");
        enum direction {jump, left, right};
        private Vector2i textureVector;
        IntRect textureRect;
        
        //constructor
        public Player(RenderWindow _w, float _s, world.TileMap _map)
        {
            tileMap = _map;
            //enable offset calculation: get view origin from tilemap
            //originalOffset = tileMap.viewOrigin;
            originalOffset = Global.BEGIN_WINDOW_ORIGIN;

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
            //get offset
            currentOffset = Global.CURRENT_WINDOW_ORIGIN;

            if (!tileMap.Collides(playerPosition, playerSize)){
                //down movement commented out
                //if (Keyboard.IsKeyPressed(Keyboard.Key.S) && playerPosition.Y < win.Size.Y - playerSize.Y)
                //    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Speed)))
                //        playerPosition.Y += Speed;
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X -= Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.Y = (int)direction.left;
                    }
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < (win.Size.X + currentOffset.X) - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(Global.PLAYER_MOVEMENT_SPEED, 0))) 
                    { 
                        playerPosition.X += Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.Y = (int)direction.right;
                    }
                //jump
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && playerPosition.Y > currentOffset.Y)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED)))
                    {
                        playerPosition.Y -= Global.PLAYER_JUMP_SPEED;
                        textureVector.Y=(int)direction.jump;
                    }
                //Gravity stuff
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    playerPosition.Y += Global.GLOBAL_GRAVITY;

                textureRect = new IntRect(textureVector.X * 51, textureVector.Y * 51, 51, 51);
                playerSprite.Position = playerPosition;
            }
        }

        public void Draw()
        {
            win.Draw(playerSprite);
        }
    }
}
