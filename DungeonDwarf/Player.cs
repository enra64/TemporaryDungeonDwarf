using SFML;
using SFML.Graphics;
using SFML.Window;

using System;
using System.Text;
using System.Timers;


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


        //animated sprite
        enum direction {jump, left, right};
        private Vector2i textureVector = new Vector2i(1, 1);
        private static Timer animTimer = new Timer(3000);
        

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
            Texture playerTexture = new Texture("textures/player/player_spritesheet.png");
            playerSprite = new Sprite(playerTexture);
            //xScale = yScale = Global.GLOBAL_SCALE;
            
            //scale
            playerSprite.Scale = new Vector2f(xScale, yScale);
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

        //confusing stuff -> timer "=>" what is this doing???
        private void delayedTexture(int delay, Action action)
        {
            Timer timer = new Timer();
            timer.Interval = delay;
            timer.Elapsed += (s, e) =>
            {
                action();
                timer.Stop();
            };
            timer.Start();
        }
        
        public void Update()
        {
            
            //get offset
            currentOffset = Global.CURRENT_WINDOW_ORIGIN;

            playerSprite.TextureRect = new IntRect(textureVector.X * 60, textureVector.Y * 55, 60, 55);
            playerSize.X = (60) * xScale;
            playerSize.Y = (55) * yScale;


            playerSprite.Position = playerPosition;
            
            //Timer for anim
            animTimer.AutoReset = true;
            animTimer.Enabled = true;
            animTimer.Start();
            
            //movement
            if (!tileMap.Collides(playerPosition, playerSize)){       
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X -= Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.X = 0;
                        textureVector.Y = 0;
                        delayedTexture(1000, ()=> textureVector.X = 1);
                        delayedTexture(2000, ()=> textureVector.X = 2);
                        delayedTexture(3000, ()=> textureVector.X = 3);
                      
                    }
                
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < (win.Size.X + currentOffset.X) - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X += Global.PLAYER_MOVEMENT_SPEED;
                        //textureVector.X = 0;
                        textureVector.Y = 1;
                        delayedTexture(1000, ()=> textureVector.X = 1);
                        delayedTexture(2000, ()=> textureVector.X = 2);
                        delayedTexture(3000, ()=> textureVector.X = 3);
                    }
               
                //jump
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && (playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X))>-55f){
                    Console.WriteLine(playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X));
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED))){
                        playerPosition.Y -= Global.PLAYER_JUMP_SPEED;
                        textureVector.X = 1;
                        textureVector.Y = 1;
                    }
                
                }
                //Gravity stuff
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    playerPosition.Y += Global.GLOBAL_GRAVITY;


                Console.WriteLine(textureVector.X);
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
