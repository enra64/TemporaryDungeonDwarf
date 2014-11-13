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
        //jump logic variables
        private bool hasJumped = false;
        private int jumpCount = 0;

        //animated sprite
        private Vector2i textureVector = new Vector2i(0, 1);
        bool isAnim = false;
        bool isRight = false;
        bool isLeft = false;

        //constructor
        public Player(RenderWindow _w, float _s, world.TileMap _map)
        {
            tileMap = _map;
            //enable offset calculation: get view origin from tilemap
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
        /// <summary>
        /// int delay 1000 = 1 sec
        /// After 1 sec it does any Action
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Update for Player
        /// </summary>
        /// <returns></returns>
        public void Update()
        {
            //get offset
            currentOffset = Global.CURRENT_WINDOW_ORIGIN;

            playerSprite.TextureRect = new IntRect(textureVector.X * 60, textureVector.Y * 55, 60, 55);
            playerSize.X = (60) * xScale;
            playerSize.Y = (55) * yScale;


            playerSprite.Position = playerPosition;

            //movement !!Now with brilliant stuff added because I tried this THINKING thingy!!
            //xD :D
            //Sprite gets animated again and again if Key is pressed
            if (!tileMap.Collides(playerPosition, playerSize)){       
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X -= Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.Y = 0;
                        isLeft = true;
                        isRight = false;
                      
                    }
                    if (Keyboard.IsKeyPressed(Keyboard.Key.A) && !isAnim ){
                        isAnim = true;
                        delayedTexture(150, () => textureVector.X = 1);
                        delayedTexture(300, () => textureVector.X = 2);
                        delayedTexture(450, () => textureVector.X = 3);
                        delayedTexture(600, () => textureVector.X = 0);
                        delayedTexture(600, () => isAnim= false);
                    }
                
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < (win.Size.X + currentOffset.X) - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X += Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.Y = 1;
                        isRight = true; 
                        isLeft = false;
                    }
                    
                    if (Keyboard.IsKeyPressed(Keyboard.Key.D) && !isAnim) 
                    {
                        isAnim = true;
                        delayedTexture(150, () => textureVector.X = 1);
                        delayedTexture(300, () => textureVector.X = 2);
                        delayedTexture(450, () => textureVector.X = 3);
                        delayedTexture(600, () => textureVector.X = 0);
                        delayedTexture(600, () => isAnim = false);
                    }
                
                
                //debug key: output current player bott vs ground top diff
                if (Keyboard.IsKeyPressed(Keyboard.Key.O))
                    Console.WriteLine(playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X));

                /*
                 * Jump Logic
                 */
                JumpLogic();

                float yDiffLeft = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X);
                float yDiffRight = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X + playerSize.X);

                //new jump logic: if we have finished jumping, reset our position relative y 0
                if (yDiffLeft > -10 || yDiffRight > -10)
                {
                    //error detected:
                    //if we jump right next to a block, said jump gets killed immediately :/
                    //save both positions
                    float leftTopPosition = tileMap.GetMinYAtX(playerPosition.X);
                    float rightTopPosition = tileMap.GetMinYAtX(playerPosition.X + (playerSize.X - 1f));
                    float targetPosition;
                    
                    //use the one higher up 
                    if (leftTopPosition > rightTopPosition)
                        targetPosition = rightTopPosition;
                    else
                        targetPosition = leftTopPosition;

                    //change position now
                    playerPosition.Y = targetPosition-playerSize.Y;
                    playerSprite.Position = playerPosition;
                }
                
                //Gravity stuff
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    playerPosition.Y += Global.GLOBAL_GRAVITY;

                
                //Console.WriteLine("(Player) Tex Vector: "+textureVector.X);
            }
        }

        public void JumpLogic()
        {
            /* 
             * HOLY HELL DONT DO THREE IFS FOR THIS SHIT COLLAPSE IT INTO ONE -.-
             * seriously though, that shit is shitty for debugging :(
             */
            //after pressing space, the further jumping is now no longer user controllable.
            if (jumpCount < 10)
            {
                jumpCount++;
                playerPosition.Y -= Global.PLAYER_JUMP_SPEED - jumpCount * 3f;
                //dont know if i need these, just remove it if this is unneeded
                textureVector.X = 1;
                textureVector.Y = 1;
                //Console.WriteLine("enhanced jumping");
            }
            float yDiffLeft = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X);
            float yDiffRight = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X + playerSize.X);
            //reset jump boolean on ground touch
            if (yDiffLeft > -5 || yDiffRight > -5)
                hasJumped = false;

            //only jump if jump sequence is not already initiated
            if (hasJumped == false)
            {
                //jump at key press
                //jump right
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && isRight)
                {
                    //Console.WriteLine("jump right call");
                    //jump if there is enough space above
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED)))
                    {
                        //Console.WriteLine("allow jump");
                        //set hasJumped to avoid flickering
                        hasJumped = true;
                        //set upwards traveling variable to enable higher jump heights
                        jumpCount = 0;
                        playerPosition.Y -= Global.PLAYER_JUMP_SPEED;
                        textureVector.X = 1;
                        textureVector.Y = 1;
                    }
                }
                //jump left
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && isLeft)
                {
                    //Console.WriteLine("jump left call");
                    //jump if there is enough space above
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED)))
                    {
                        //Console.WriteLine("allow jump");
                        //set hasJumped to avoid flickering
                        hasJumped = true;
                        //set upwards traveling variable to enable higher jump heights
                        jumpCount = 0;
                        playerPosition.Y -= Global.PLAYER_JUMP_SPEED;
                        textureVector.X = 1;
                        textureVector.Y = 0;
                    }
                }
                //jump in standing
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space) && !isRight && !isLeft)
                {
                    //Console.WriteLine("jump straight call");
                    //jump if there is enough space above
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED)))
                    {
                        //Console.WriteLine("allow jump");
                        //set hasJumped to avoid flickering
                        hasJumped = true;
                        //set upwards traveling variable to enable higher jump heights
                        jumpCount = 0;
                        playerPosition.Y -= Global.PLAYER_JUMP_SPEED;
                        textureVector.X = 1;
                        textureVector.Y = 1;
                    }
                }
            }
        }

        public void Draw()
        {
            //shows playersize
            RectangleShape colliderRect = new RectangleShape();
            colliderRect.Size = new Vector2f(playerSize.X, playerSize.Y);
            colliderRect.Position = new Vector2f(playerPosition.X, playerPosition.Y);
            colliderRect.FillColor = Color.Transparent;
            colliderRect.OutlineColor = Color.Green;
            colliderRect.OutlineThickness = 2f;

            win.Draw(colliderRect);
            win.Draw(playerSprite);
        }
    }
}
