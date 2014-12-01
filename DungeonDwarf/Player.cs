using SFML;
using SFML.Graphics;
using SFML.Window;
using DungeonDwarf.world;
using SFML.Audio;


using System;
using System.Text;
using System.Timers;

namespace DungeonDwarf
{
    class Player
    {
        public Vector2f playerPosition, playerSize;

        //player stats
        private const int MAX_HEALTH = 500000;
        private const int MIN_HEALTH = 0;
        RectangleShape healthBar = new RectangleShape();
        public int health = MAX_HEALTH;     // hm.wulfi~ I changed it to public for Program.EnemyCollision();

        private const float MAX_SHIELD = 50f;
        private const float MIN_SHIELD = 0f;
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

        static SoundBuffer jumpS = new SoundBuffer("sound/jump.wav");
        static SoundBuffer deathS = new SoundBuffer("sound/fireball.wav");


        //sound
        Sound jump = new Sound(jumpS);
        Sound death = new Sound(deathS);


        //constructor
        public Player(RenderWindow _w, float _s, world.TileMap _map)
        {
            tileMap = _map;
            //enable offset calculation: get view origin from tilemap
            originalOffset = Global.BEGIN_WINDOW_ORIGIN;

            //renderwindow 
            win = _w;

            //add player texture and sprite
            Texture playerTexture = new Texture("textures/player/dwarf.png");
            playerSprite = new Sprite(playerTexture);
            //xScale = yScale = Global.GLOBAL_SCALE;

            //scale
            playerSprite.Scale = new Vector2f(xScale, yScale);
            playerPosition = new Vector2f(270f, 270f);
            playerSprite.Position = playerPosition;

            //health, shield
            healthBar.FillColor = Color.Red;
            healthBar.OutlineColor = Color.Transparent;
            shieldBar.FillColor = Color.Blue;
            shieldBar.OutlineColor = Color.Transparent;


        }

        /// <summary>
        /// Returns the center of the player
        /// </summary>
        /// <returns></returns>
        public Vector2f GetCenter()
        {
            return new Vector2f(playerPosition.X + playerSize.X / 2, playerPosition.Y + playerSize.Y / 2);
        }

        

        /// <summary>
        /// Update for Player
        /// </summary>
        /// <returns></returns>
        public void Update()
        {
            


            //get offset
            currentOffset = Global.CURRENT_WINDOW_ORIGIN;

            #region HealthAndShield
            //health
            healthBar.Size = new Vector2f(health * 2.0f, 17f);
            healthBar.Position = new Vector2f(currentOffset.X + 20f, currentOffset.Y + win.Size.Y - 40f);

            //Console.WriteLine(health);
            //shield
            shieldBar.Size = new Vector2f(shield * 2.0f, 17f);
            shieldBar.Position = new Vector2f(currentOffset.X + 20f, currentOffset.Y + win.Size.Y - 70f);

            //Console.WriteLine(shield);
            #endregion

            playerSprite.TextureRect = new IntRect(textureVector.X * 65, textureVector.Y * 64, 65, 64);
            playerSize.X = (58) * xScale;
            playerSize.Y = (60) * yScale;

            #region Movement
            //movement !!Now with brilliant stuff added because I tried this THINKING thingy!!
            //xD :D
            if (health > MIN_HEALTH)
            {
                //shield gets back up
                if (shield < MAX_SHIELD)
                    shield += 0.1f;

                #region Lava_Death
                //lava
                int[] currentTile = tileMap.GetCurrentTile(new Vector2f(playerPosition.X, playerSize.Y + playerPosition.Y + 30f));
                if (tileMap.GetTileType(currentTile[0], currentTile[1]) == Global.LAVA_TOP_TILE)
                {
                    if (shield > MIN_SHIELD)
                        shield -= 5;
                    else
                        health -= 10;
                }
                #endregion

                if (!tileMap.Collides(playerPosition, playerSize))
                {
                    if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                        if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.PLAYER_MOVEMENT_SPEED, 0f)))
                        {
                            playerPosition.X -= Global.PLAYER_MOVEMENT_SPEED;
                            textureVector.Y = 0;
                            isLeft = true;
                            isRight = false;
                        }

                    #region LeftAnim
                    if (Keyboard.IsKeyPressed(Keyboard.Key.A) && !isAnim)
                    {
                        isAnim = true;
                        DelayUtil.delayUtil(150, () => textureVector.X = 1);
                        DelayUtil.delayUtil(300, () => textureVector.X = 2);
                        DelayUtil.delayUtil(450, () => textureVector.X = 0);
                        DelayUtil.delayUtil(450, () => isAnim = false);
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
                        DelayUtil.delayUtil(150, () => textureVector.X = 0);
                        DelayUtil.delayUtil(300, () => textureVector.X = 1);
                        DelayUtil.delayUtil(450, () => textureVector.X = 2);
                        DelayUtil.delayUtil(450, () => isAnim = false);
                    }
                    #endregion

                    /*
                 * Jump Logic
                 */
                    JumpLogic();

                    //logic for resetting player y if he hovers above ground
                    CorrectYPosLogic();

                    //Highly advanced realtime physics calculation
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                        playerPosition.Y += Global.GLOBAL_GRAVITY;

                    //kill on map leave
                    if (playerPosition.Y > 4000f)
                        health = 0;
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
                //death.Play();
            }
            #endregion

            //draw sprite slightly below position to simulate correct sprite cutting
            playerSprite.Position = new Vector2f(playerPosition.X, playerPosition.Y + 0f); ;
        }

        //reset player position if he is just above ground
        #region ResetYPosition
        public void CorrectYPosLogic()
        {
            //get difference between player left bottom and ground top
            float yDiffLeft = playerPosition.Y + playerSize.Y - tileMap.MinY(playerPosition);

            //calc maximum distance from ground, scale by gravity
            float gravityScale = 9f / Global.GLOBAL_GRAVITY;
            if (yDiffLeft > -10 / gravityScale && yDiffLeft < 60 && yDiffLeft != 0)
            {
                //Console.WriteLine(playerPosition.Y);
                //avoid getting put above the game
                if (playerPosition.Y < -10)
                    playerPosition.Y = 0;
                else
                {
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
            if (jumpCount < 10 && hasJumped == true && !abortJump)
            {
                jumpCount++;
                abortJump = JumpIntelligent(Global.PLAYER_JUMP_SPEED - jumpCount * 3f);
            }

            //reset jump boolean on ground touch
            float yDiffLeft = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X);
            float yDiffRight = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X + playerSize.X);
            if ((yDiffLeft == 0 || yDiffRight == 0))
                hasJumped = false;


            //only jump if jump sequence is not already initiated
            if (hasJumped == false)
            {
                //jump at key press
                if (Keyboard.IsKeyPressed(Keyboard.Key.Space))
                {
                    //jump if there is enough space above
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED)))
                    {
                        jump.Play();
                        //set hasJumped to avoid flickering
                        hasJumped = true;
                        //set upwards traveling variable to enable higher jump heights
                        jumpCount = 0;
                        JumpIntelligent(Global.PLAYER_JUMP_SPEED);

                        //change texture
                        if (isRight)
                        {
                            textureVector.X = 1;
                            textureVector.Y = 1;
                        }
                        else if (isLeft)
                        {
                            textureVector.X = 1;
                            textureVector.Y = 0;
                        }
                        else
                        {
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
            while (tileMap.Collides(testingPosition, playerSize))
            {
                testingPosition.Y += .2f;
                abortedJump = true;
                //Console.WriteLine("decrease jumping to " + testingPosition.Y);
            }
            playerPosition.Y = testingPosition.Y;
            return abortedJump;
        }
        #endregion

        public void Draw()
        {
            win.Draw(playerSprite);
            //DrawCollidingRect();
        }

        public void PriorityDraw()
        {
            //draw on top of shadows
            win.Draw(healthBar);
            win.Draw(shieldBar);
        }

        public FloatRect GetRect()
        {
            return new FloatRect(playerPosition.X, playerPosition.Y, playerSize.X, playerSize.Y);
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