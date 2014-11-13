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

            //movement !!Now with stupid stuff I added, because I don't start THINKING before I code!!
            //xD :D
            //Sprite gets animated again and again if Key is pressed
            if (!tileMap.Collides(playerPosition, playerSize)){       
                if (Keyboard.IsKeyPressed(Keyboard.Key.A) && playerPosition.X > currentOffset.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(-Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X -= Global.PLAYER_MOVEMENT_SPEED;
                        //textureVector.X = 0;
                        textureVector.Y = 0;
                        delayedTexture(250, ()=> textureVector.X = 1);
                        delayedTexture(500, ()=> textureVector.X = 2);
                        delayedTexture(750, ()=> textureVector.X = 3);
                        delayedTexture(1000, ()=> textureVector.X = 0);
                      
                    }
                
                if (Keyboard.IsKeyPressed(Keyboard.Key.D) && playerPosition.X < (win.Size.X + currentOffset.X) - playerSize.X)
                    if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(Global.PLAYER_MOVEMENT_SPEED, 0f))) 
                    { 
                        playerPosition.X += Global.PLAYER_MOVEMENT_SPEED;
                        //textureVector.X = 0;
                        textureVector.Y = 1;
                        delayedTexture(250, ()=> textureVector.X = 1);
                        delayedTexture(500, ()=> textureVector.X = 2);
                        delayedTexture(750, ()=> textureVector.X = 3);
                        delayedTexture(1000, ()=> textureVector.X = 0);
                    }
                
                //debug key: output current player bott vs ground top diff
                if (Keyboard.IsKeyPressed(Keyboard.Key.O))
                {
                    Console.WriteLine(playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X));
                }

                /*
                 * Jump Logic
                 */
                //after pressing space, the further jumping is now no longer user controllable.
                //maybe reduce jumpspeed by height, does however seemingly clash with the collision
                //detection :/
                if (jumpCount<10){
                    jumpCount++;
                    playerPosition.Y -= Global.PLAYER_JUMP_SPEED-jumpCount*3f;
                    //dont know if i need these, just remove it if this is unneeded
                    textureVector.X = 1;
                    textureVector.Y = 1;
                    //Console.WriteLine("enhanced jumping");
                }
                float yDiffLeft = playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X);
                float yDiffRight =playerPosition.Y + playerSize.Y - tileMap.GetMinYAtX(playerPosition.X+playerSize.X);
                //reset jump boolean on ground touch
                if (yDiffLeft > -5  || yDiffRight > -5)
                    hasJumped = false;

                //only jump if jump sequence is not already initiated
                if (hasJumped == false){
                    //jump at key press
                    if (Keyboard.IsKeyPressed(Keyboard.Key.Space)){
                        //jump if there is enough space above
                        if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, -Global.PLAYER_JUMP_SPEED)))
                        {
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
                
                //Gravity stuff
                if (!tileMap.CheckNextCollide(playerPosition, playerSize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    playerPosition.Y += Global.GLOBAL_GRAVITY;

                
                //Console.WriteLine("(Player) Tex Vector: "+textureVector.X);
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
