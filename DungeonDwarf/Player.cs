using SFML;
using SFML.Graphics;
using SFML.Window;
using DungeonDwarf.world;

using System;
using System.Text;
using System.Timers;

namespace DungeonDwarf
{
    class Player
    {
        public Vector2f playerPosition, playerSize;
        
        //player stats
        private const int MAX_HEALTH = 100;
        private const int MIN_HEALTH = 0;
        RectangleShape healthBar = new RectangleShape();
        public int health = MAX_HEALTH;     // hm.wulfi~ I changed it to public for Program.EnemyCollision();

        private const float MAX_SHIELD= 100;
        private const float MIN_SHIELD= 0;
        RectangleShape shieldBar = new RectangleShape();
        public float shield = MAX_SHIELD;   // hm.wulfi~ I changed it to public for Program.EnemyCollision();

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
        private Vector2i textureVector = new Vector2i(0, 0);
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
        
        #region DelayUtil
        //confusing stuff -> timer "=>" what is this doing???
        /// <summary>
        /// int delay 1000 = 1 sec
        /// After 1 sec it does any Action
        /// </summary>
        /// <returns></returns>
        private void delayUtil(int delay, Action action)
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
        #endregion
        
        /// <summary>
        /// Update for Player
        /// </summary>
        /// <returns></returns>
        public void Update()
        {
            #region Health
            //health
            healthBar.Size = new Vector2f(health * 1.2f, 20f);
            healthBar.Position = new Vector2f(playerPosition.X, playerPosition.Y - 50f);
            healthBar.FillColor = Color.Red;
            healthBar.OutlineColor = Color.Transparent;
            healthBar.OutlineThickness = 0.01f;

            //Console.WriteLine(health);
            #endregion

            #region Shield
            //shield
           shieldBar.Size = new Vector2f(shield * 1.2f, 20f);
           shieldBar.Position = new Vector2f(playerPosition.X, playerPosition.Y - 80f);
           shieldBar.FillColor = Color.Blue;
           shieldBar.OutlineColor = Color.Transparent;
           shieldBar.OutlineThickness = 0.01f;

            //Console.WriteLine(shield);
            #endregion

            //get offset
            currentOffset = Global.CURRENT_WINDOW_ORIGIN;

            playerSprite.TextureRect = new IntRect(textureVector.X * 59, textureVector.Y * 57, 59, 57);
            playerSize.X = (59) * xScale;
            playerSize.Y = (57) * yScale;

           #region Movement
            //movement !!Now with brilliant stuff added because I tried this THINKING thingy!!
            //xD :D
           if (health > MIN_HEALTH){
               
               //shield gets back up
               if (shield < MAX_SHIELD)
                   shield += 0.25f;

               #region Lava_Death
               //lava
               int[] currentTile = tileMap.GetCurrentTile(new Vector2f(playerPosition.X, playerSize.Y + playerPosition.Y + 30f));
               if (tileMap.GetTileType(currentTile[0], currentTile[1]) == Global.LAVA_TOP_TILE)
               {
                   if (shield > MIN_HEALTH)
                    shield -= 5;
                   else
                    health -= 10;
               }
               #endregion
               
               if (!tileMap.Collides(playerPosition, playerSize)){       
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X -= Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.Y = 0;
                        isLeft = true;
                        isRight = false;

                    }

                #region LeftAnim
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && !isAnim ){
                        isAnim = true;
                        delayUtil(150, () => textureVector.X = 1);
                        delayUtil(300, () => textureVector.X = 2);
                        delayUtil(450, () => textureVector.X = 3);
                        delayUtil(600, () => textureVector.X = 0);
                        delayUtil(600, () => isAnim= false);
                }
                #endregion

                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < (win.Size.X + currentOffset.X) - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X += Global.PLAYER_MOVEMENT_SPEED;
                        textureVector.Y = 1;
                        isRight = true; 
                        isLeft = false;
                    }
                #region RightAnim
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && !isAnim) 
                    {
                        isAnim = true;
                        delayUtil(150, () => textureVector.X = 1);
                        delayUtil(300, () => textureVector.X = 2);
                        delayUtil(450, () => textureVector.X = 3);
                        delayUtil(600, () => textureVector.X = 0);
                        delayUtil(600, () => isAnim = false);
                    }
                #endregion
                
                //debug key: output current player bott vs ground top diff
                if (Keyboard.IsKeyPressed(Keyboard.Key.O))
                {
                    Console.WriteLine(playerPosition.Y);
                }

                /*
                 * Jump Logic
                 */
                JumpLogic();
            
                //logic for resetting player y if he hovers above ground
                CorrectYPosLogic();
                
                //Highly advanced realtime physics calculation
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    playerPosition.Y += Global.GLOBAL_GRAVITY;
            
                //Console.WriteLine("(Player) Tex Vector: "+textureVector.X);
             }
           }
            #endregion

           #region Death
           else
           {
               Texture deathTex = new Texture("textures/player/death.png"); 
               playerSprite = new Sprite(deathTex);
               textureVector.X = 2;
               textureVector.Y = 0;
               if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    playerPosition.Y += Global.GLOBAL_GRAVITY;

           }
           #endregion

           //draw sprite slightly below position to simulate correct sprite cutting
           playerSprite.Position = new Vector2f(playerPosition.X, playerPosition.Y + 5f); ;
        }

        //reset player position if he is just above ground
        #region ResetYPosition
        public void CorrectYPosLogic(){
            //get difference between player left bottom and ground top
            float yDiffLeft = playerPosition.Y + playerSize.Y - tileMap.MinY(playerPosition);

            //calc maximum distance from ground, scale by gravity
            float gravityScale = 9f / Global.GLOBAL_GRAVITY;
            if (yDiffLeft > -10 / gravityScale && yDiffLeft < 60 && yDiffLeft != 0){
                //Console.WriteLine(playerPosition.Y);
                //avoid getting put above the game
                if (playerPosition.Y < -10)
                    playerPosition.Y = 0;
                else{
                    //Console.WriteLine(yDiffLeft);
                    //get current left and right highest positions
                    float leftTopPosition = tileMap.MinY(playerPosition);
                    float rightTopPosition = tileMap.MinY(new Vector2f(playerPosition.X - 1 + playerSize.X, playerPosition.Y));
                    float targetPosition;

                    //use the block higher up 
                    if (leftTopPosition > rightTopPosition)
                        targetPosition = rightTopPosition;
                    else
                        targetPosition = leftTopPosition;
                    playerPosition.Y = targetPosition - playerSize.Y;
                }
                //write changed position
                playerSprite.Position = playerPosition;
            }
        }
        #endregion

        #region JumpLogic
        public void JumpLogic()
        {
            //after pressing space, the further jumping is now no longer user controllable.
            bool abortJump = false;
            if (jumpCount < 10 && hasJumped==true && !abortJump)
            {
                jumpCount++;
                abortJump=JumpIntelligent(Global.PLAYER_JUMP_SPEED - jumpCount * 3f);
            }

            //reset jump boolean on ground touch
            float yDiffLeft = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X);
            float yDiffRight = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X + playerSize.X);
            if ((yDiffLeft == 0 || yDiffRight == 0) )
                hasJumped = false;
            

            //only jump if jump sequence is not already initiated
            if (hasJumped == false){
                //jump at key press
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space)){
                    //jump if there is enough space above
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED))){
                        //set hasJumped to avoid flickering
                        hasJumped = true;
                        //set upwards traveling variable to enable higher jump heights
                        jumpCount = 0;
                        JumpIntelligent(Global.PLAYER_JUMP_SPEED);

                        //change texture
                        if (isRight){
                            textureVector.X = 1;
                            textureVector.Y = 1;
                        }
                        else if (isLeft){
                            textureVector.X = 1;
                            textureVector.Y = 0;
                        }
                        else{
                            textureVector.X = 1;
                            textureVector.Y = 1;
                        }
                        
                    }
                }
               
            }
        }
        #endregion

        #region JumpIntelligent
        /// <summary>
        /// Call this when jumping to avoid jumping into blocks
        /// </summary>
        /// <param name="amount"></param>
        private bool JumpIntelligent(float amount)
        {
            bool abortedJump = false;
            Vector2f testingPosition = playerPosition;
            testingPosition.Y -= amount;
            //decrease jump amount until no collision appears
            while (tileMap.Collides(testingPosition, playerSize)){
                testingPosition.Y += .2f;
                abortedJump = true;
                //Console.WriteLine("decrease jumping to " + testingPosition.Y);
            }
            playerPosition.Y = testingPosition.Y;
            return abortedJump;
        }
        #endregion

        public void Draw(){
            win.Draw(healthBar);
            win.Draw(shieldBar);
            win.Draw(playerSprite);
        }

        private void DrawCollidingRect()
        {
            RectangleShape colliderRect = new RectangleShape();
            colliderRect.Size = new Vector2f(playerSize.X, playerSize.Y);
            colliderRect.Position = new Vector2f(playerPosition.X, playerPosition.Y);
            colliderRect.FillColor = Color.Transparent;
            colliderRect.OutlineColor = Color.Green;
            colliderRect.OutlineThickness = 1f;
            win.Draw(colliderRect);
        }
    }
}
