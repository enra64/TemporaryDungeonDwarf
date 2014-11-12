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
        private float xScale = 1f, yScale = 1f;
        private world.TileMap tileMap;
        private Vector2f currentOffset, originalOffset;

        private bool canJump = true;

        //animated sprite
        enum direction {jump, left, right};
        private Vector2i textureVector = new Vector2i(1, 1);
        
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
            //Texture playerTexture = new Texture("textures/world/earthTile.png");
            Texture playerTexture = new Texture("textures/player/player_spritesheet.png");
            playerSprite = new Sprite(playerTexture);
            //xScale = yScale = Global.GLOBAL_SCALE;
            
            //scale
            playerSprite.Scale = new Vector2f(xScale, yScale);
            //playerSize.X = playerTexture.Size.X * xScale;
            //playerSize.Y = playerTexture.Size.Y * yScale;
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

            playerSprite.TextureRect = new IntRect(textureVector.X * 60, textureVector.Y * 55, 60, 55);
            playerSize.X = (textureVector.X * 35) * xScale;
            playerSize.Y = (textureVector.Y * 55) * yScale;


            playerSprite.Position = playerPosition;

            if (!tileMap.Collides(playerPosition, playerSize)){
                //down movement commented out
                //if (Keyboard.IsKeyPressed(Keyboard.Key.S) && playerPosition.Y < win.Size.Y - playerSize.Y)
                //    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Speed)))
                //        playerPosition.Y += Speed;
               
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X -= Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.X = (int)direction.left;
                    }
                
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < (win.Size.X + currentOffset.X) - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X += Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.X = (int)direction.right;
                    }
               
                //jump
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && (playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X))>-55f){
                    Console.WriteLine(playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X));
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED))){
                        playerPosition.Y -= Global.PLAYER_JUMP_SPEED;
                        textureVector.X = (int)direction.jump + 1;
                    }
                
                }
                //Gravity stuff
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    playerPosition.Y += Global.GLOBAL_GRAVITY;

                //playerSprite.TextureRect = new IntRect(textureVector.X * 62, textureVector.Y * 55, 62, 55);
                //playerSize.X = (textureVector.X * 62)* xScale;
                //playerSize.Y = (textureVector.Y * 55)* yScale;
                
                
                //playerSprite.Position = playerPosition;
            }
        }

        public void Draw()
        {
            RectangleShape tester = new RectangleShape();
            tester.Size = new Vector2f(playerSize.X, playerSize.Y);
            tester.Position = new Vector2f(playerPosition.X, playerPosition.Y);
            tester.FillColor = Color.Transparent;
            tester.OutlineColor = Color.Green;
            tester.OutlineThickness = 2f;
            win.Draw(tester);
            win.Draw(playerSprite);
        }
    }
}
