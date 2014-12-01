using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.Window;
using SFML.Audio;
using System.Diagnostics;

namespace DungeonDwarf
{
    class Program
    {
        //fein säuberlich sortiert für daniel ;) Danke :D 
        //player, enemy
        static Player currentPlayer;
        static List<Enemy> EnemyList = new List<Enemy>();
        static Inventory currentInventory;

        //tilemap and view
        static RenderWindow currentRenderWindow;
        static View currentView;
        static FloatRect moveableRectangle;
        static world.TileMap tileMap;
        static Stopwatch tileMapUpdater = new Stopwatch();

        //bullet stuff
        static List<Bullet> BulletList = new List<Bullet>();
        static Texture bulletTexture;
        static bool BulletButton = false, movingRight = true;
        static int bulletLifeTime;

        //benchmark
        static Stopwatch sw = new Stopwatch();
        
        //lighting
        static Lighting lightEngine;
        //torches
        static List<Torch> TorchList = new List<Torch>();
        static Texture torchTexture;
        static bool torchBool = true;
        static uint currentUserTorchCount = 0;

        static SoundBuffer shootS = new SoundBuffer("sound/fireball.wav");
        static Sound shoot = new Sound(shootS);

        static Music background = new Music("sound/background.wav");

        static void Main(string[] args)
        {
            /*
             * These calls are done even before Initialize(), because
             * of a) their importance and b) dont fuck these up.
             * Do window creation
             */
            //ignore at this moment, because we are going to fuck up at first, and doing that in fullscreen is bad
            //creates fullscreen window at your maximum resolution
            //currentRenderWindow = new RenderWindow(VideoMode.FullscreenModes[0], "Dungeon Dwarf", Styles.Fullscreen);

            //create window
            currentRenderWindow = new RenderWindow(new VideoMode(800, 600), "Dungeon Dwarf", Styles.Default);
            //currentRenderWindow = new RenderWindow(VideoMode.FullscreenModes[0], "Dungeon Dwarf", Styles.Fullscreen);
            //sets framerate to a maximum of 45; changing the value will likely result in bad things
            currentRenderWindow.SetFramerateLimit(35);
            //add event handler for klicking the X icon
            currentRenderWindow.Closed += windowClosed;
            //vertical sync is enabled, because master graphics n shit
            currentRenderWindow.SetVerticalSyncEnabled(true);

            //add handler to window resized, because we need to recalculate tile count
            currentRenderWindow.Resized += windowResized;

            //add mouse click handling for getting focus
            currentRenderWindow.MouseButtonPressed += mouseClick;

            //first and only call to load content, not mandatory to use
            LoadContent();
            //first and only call to init, do everything else there
            Initialize();

            //show startscreen

            //Arne, you forgot to push StartScreen.cs and Inventory.cs
            //http://bit.ly/19hKyY7
            StartScreen s = new StartScreen(currentRenderWindow);
            EndScreen e = new EndScreen(currentRenderWindow);

            while (!Keyboard.IsKeyPressed(Keyboard.Key.Space) && currentRenderWindow.IsOpen()){
                s.Update();
                s.Draw();
            }
            background.Play();
            background.Loop = true;
            background.Volume = 8f;

            /*
             * shit be about to get real... starting main loop.
             */
            while (currentRenderWindow.IsOpen() && Global.CURRENT_WINDOW_ORIGIN.Y < currentRenderWindow.Size.Y+40f)
            {
                //mandatory update and draw calls
                Update();
                Draw();
                //dispatch things like "i would like to close this window" and "someone clicked me".
                //only important if you want to close the window. ever.
                currentRenderWindow.DispatchEvents();

                while ((Keyboard.IsKeyPressed(Keyboard.Key.F1) && currentRenderWindow.IsOpen()) || (currentRenderWindow.IsOpen() && Global.CURRENT_WINDOW_ORIGIN.Y > currentRenderWindow.Size.Y && !Keyboard.IsKeyPressed(Keyboard.Key.Escape) ))
                {
                    e.Update();
                    e.Draw();
                }
            }
            
        }

        private static void mouseClick(object sender, MouseButtonEventArgs e){
            Console.WriteLine(e.X + ": x, y: " + e.Y);
            currentInventory.Click(e.X, e.Y);
        }

        private static void windowResized(object sender, SizeEventArgs e){
            //update tile count
            tileMap.UpdateWindowResize();
            currentView.Size = new Vector2f(800, 600);
        }

        private static void windowClosed(object sender, EventArgs e)
        {
            ((RenderWindow)sender).Close();
        }

        /// <summary>
        /// Called immediately after creating the window.
        /// Create or instance anything you think the main function needs here.
        /// </summary>
        private static void Initialize()
        {

            //set beginning view.
            currentView = new View(new FloatRect(0, 0, 800, 600));
            //set view origin and current in static global class
            Global.BEGIN_WINDOW_ORIGIN = currentRenderWindow.GetView().Center;
            Global.CURRENT_WINDOW_ORIGIN = currentRenderWindow.GetView().Center;
            /*
             * Please write your code after this comment, because rule number one is:
             * dont fuck up the view.
             * rule number two, by the way, is: Sometimes I do do mistakes, but please
             * think twice before adding you calls before this comment
             */
            //get lightengine
            lightEngine = new Lighting(currentRenderWindow);
            //get tilemap, add to global
            tileMap = new world.TileMap(currentRenderWindow, lightEngine, new Vector2u(400, 10), "world/levels/lavatest.oel");
            Global.TILE_MAP = tileMap;
            //start tilemap animation stopwatch
            tileMapUpdater.Start();
            
            //get all map defined torches
            foreach (Vector2f t in tileMap.GetAllTorches())
                TorchList.Add(new Torch(t, torchTexture, currentRenderWindow, tileMap));

            //get all map defined enemies, tile x, y, enemytype
            foreach(int[] f in tileMap.spawnPoints){
                //provide valid start point
                string enemyType = "enemy0";
                switch(f[2]){
                    case Global.SPAWNTILE_1:
                        enemyType = "enemy0";
                        break;
                    case Global.SPAWNTILE_2:
                        enemyType = "enemy1";
                        break;
                    case Global.SPAWNTILE_3:
                        enemyType = "enemy2";
                        break;
                }
                //convert xy to coordinates
                Vector2f coords=tileMap.GetXY(f[0], f[1]);
                coords.Y = tileMap.GetMinYAtX(coords.Y);
                EnemyList.Add(new Enemy(enemyType, currentRenderWindow, coords));
            }
                
            //initialize inventory, currently under heavy development (as in probably wont work)
            currentInventory = new Inventory(currentRenderWindow, new Vector2f(70, 70), new Vector2f(50, 50));
            //player and enemy init
            #region playerAndEnemy
            Texture backTex = new Texture("textures/world/background.png");
            //instance player
            currentPlayer = new Player(currentRenderWindow, 10f, tileMap);

            // Konstructor mit Enemy(Gegnername, currenRenderWindow, currentPlayer.playerPosition, tileMap, Texturpath als String, x-Wert Scaling, y-Wert Scaling, Movementspeed, Jumpspeed));
            //EnemyList.Add(new Enemy("enemy0", currentRenderWindow, currentPlayer.playerPosition));
            //EnemyList.Add(new Enemy("enemy1", currentRenderWindow, currentPlayer.playerPosition));
            //EnemyList.Add(new Enemy("enemy2", currentRenderWindow, currentPlayer.playerPosition));
            #endregion

            //rectangle user can move in without changing view
            #region nomoverect
            //create a rectangle the player can move in without changing the view
            Vector2f tempCurrentPlayerCenter = currentPlayer.GetCenter();
            //this is said rectangle
            moveableRectangle = new FloatRect(tempCurrentPlayerCenter.X - currentRenderWindow.Size.X / 10, tempCurrentPlayerCenter.Y - currentRenderWindow.Size.Y / 4, 
                currentRenderWindow.Size.X / 2, currentRenderWindow.Size.Y / 2);
            #endregion

        }

        /// <summary>
        /// Would be nice to load textures you want to use a lot here,
        /// so you can use them in your class' constructors
        /// </summary>
        private static void LoadContent()
        {
            Global.e0 = new Texture("textures/enemies/horror.png");
            Global.e1 = new Texture("textures/enemies/squid.png");
            Global.e2 = new Texture("textures/enemies/crystalenemy.png");
            bulletTexture = new Texture("textures/weapons/arrow/Feuer.png");
            torchTexture = new Texture("textures/light/torch_anim.png");
        }

        /// <summary>
        /// This method handles all mandatory update calls.
        /// It is thus called in each main loop iteration.
        /// </summary>
        private static void Update()
        {
            // stopwatch beginning 
            sw.Start();
            //move view with player
            moveView();
            //update tilemap if due
            if (tileMapUpdater.ElapsedMilliseconds > 500)
            {
                tileMapUpdater.Restart();
                tileMap.AnimationUpdate();
            }
            //store current offset in global class
            Global.CURRENT_WINDOW_ORIGIN = currentView.Center - Global.BEGIN_WINDOW_ORIGIN;
            /*
             * BEGIN YOUR CODE AFTER THIS
             */
            BulletCollision();
            EnemyCollision();
            //moves the player
            currentPlayer.Update();
            //let there be light
            lightEngine.AddLight(currentPlayer.GetCenter(), currentPlayer.playerSize, new Vector2f(4f, 4f));
            //check for key input
            KeyCheck();
            //Moving Bullet
            for (int i = 0; i < BulletList.Count; i++ )
            {
                bulletLifeTime = BulletList[i].BulletLife();        // importiert die bulletlifeTime aus der Bullet
                Bullet b = BulletList[i];
                b.Update();
                //remove on tile collision
                if (tileMap.Collides(b.GetRect()))
                    BulletList.RemoveAt(i);
                else if (bulletLifeTime >= 20)                 // entfernt die Bullet bei Frame 20 falls es sie noch gibt
                    BulletList.RemoveAt(i);
                
                //adds a small light to the bullets; edgar, you can move this to your class (get the Lighting instance (lightEngine, that is) in your constructor,
                //then you can control the color with the fourth parameter, and add a light per bullet with the command below
                lightEngine.AddLight(b.GetCenter(), b.bulletSize, new Vector2f(1f, 1f), new Color(255, 0, 0));
            }
            //hint:
            //torch update, now all torches only get drawn if on screen
            foreach (Torch t in TorchList)
            {
                if (t.torchPosition.X - 400f < Global.CURRENT_WINDOW_ORIGIN.X + currentRenderWindow.Size.X && t.torchPosition.X + 400f > Global.CURRENT_WINDOW_ORIGIN.X){
                    t.Update(false);
                    lightEngine.AddLight(t.GetCenter(), t.torchSize, new Vector2f(5f, 5f), new Color(255, 102, 0));
                }
            }

            foreach (Enemy e in EnemyList)
                e.Update(currentPlayer.playerPosition, currentPlayer.playerSize);

            //tile lighting
            tileMap.Update();
            //calculate lighting. should stay last call
            lightEngine.Update();
        }

        /// <summary>
        /// do bullet collision with enemy health
        /// </summary>
        private static void BulletCollision()
        {
            for (int i = 0; i < BulletList.Count; i++)
            {
                Bullet b = BulletList.ElementAt(i);
                for (int j = 0; j < EnemyList.Count; j++)
                {
                    Enemy e = EnemyList.ElementAt(j);
                    if (!(e.enemyPosition.X > Global.CURRENT_WINDOW_ORIGIN.X + currentRenderWindow.Size.X || e.enemyPosition.X + e.enemySize.X < Global.CURRENT_WINDOW_ORIGIN.X))
                    {
                        //actual collision check
                        FloatRect bulletRect = new FloatRect(b.bulletPosition.X, b.bulletPosition.Y, b.bulletSize.X, b.bulletSize.Y);
                        FloatRect enemyRect = new FloatRect(e.enemyPosition.X, e.enemyPosition.Y, e.enemySize.X, e.enemySize.Y);
                        //remove both on collision
                        if (bulletRect.Intersects(enemyRect))
                        {
                            BulletList.RemoveAt(i);
                            e.subtractHealth();
                            if (e.health <= 0)
                                EnemyList.RemoveAt(j);

                            //avoid wrong indices
                            break;
                        }
                    }
                }
            }
        }

        // method to check for collision of all Enemies of List EnemyList with Player
        private static void EnemyCollision()
        {
            if (currentPlayer.health > 0)  // prevents unnecessary code execution, just thought it might be useful, remove if you think its redundant
            {
                FloatRect playerRect = currentPlayer.GetRect();
                foreach (Enemy enem in EnemyList)
                {
                    if (!(enem.enemyPosition.X > Global.CURRENT_WINDOW_ORIGIN.X + currentRenderWindow.Size.X || enem.enemyPosition.X + enem.enemySize.X < Global.CURRENT_WINDOW_ORIGIN.X))
                    {
                        FloatRect enemyRect = new FloatRect(enem.enemyPosition.X, enem.enemyPosition.Y, enem.enemySize.X, enem.enemySize.Y);

                        // what to do if collision = true (we should discuss what should happen if; for now it only sets the players shield on zero)
                        if (playerRect.Intersects(enemyRect))
                        {

                            if (currentPlayer.shield <= 0)
                            {
                                currentPlayer.health -= 1;
                            }
                            else
                                currentPlayer.shield -= 2.5f;

                        }
                    }
                }
            }

        }

        static void KeyCheck()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                currentRenderWindow.Close();
            //easter egg or something
            if (Keyboard.IsKeyPressed(Keyboard.Key.N))
                Console.WriteLine("batman");
            if (Keyboard.IsKeyPressed(Keyboard.Key.I))
                currentInventory.Show();
            if (Keyboard.IsKeyPressed(Keyboard.Key.T))
            {

                if (currentUserTorchCount >= 10 || !torchBool)
                    torchBool = false;
                else{
                    torchBool = false;
                    currentUserTorchCount++;
                    TorchList.Add(new Torch(currentPlayer.playerPosition, torchTexture, currentRenderWindow, tileMap));
                    DelayUtil.delayUtil(1000, () => torchBool = true);
                }
            }
            //bullet stuff
            if (Keyboard.IsKeyPressed(Keyboard.Key.F) && BulletButton == false)
            {
                BulletList.Add(new Bullet(currentPlayer.playerPosition, bulletTexture, currentRenderWindow, movingRight));
                shoot.Play();
                BulletButton = true;
            }
            if (!Keyboard.IsKeyPressed(Keyboard.Key.F))
                BulletButton = false;
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
                movingRight = false;
            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
                movingRight = true;
            
        }

        /// <summary>
        /// moves the view according to the player position.
        /// If the player only moves a small amount, we do not move.
        /// </summary>

        private static void moveView()
        {
            //get player center
            Vector2f playerCenter = currentPlayer.GetCenter();
            //player is in the rectangle no more
            if (!moveableRectangle.Contains(playerCenter.X, playerCenter.Y))
            {
                //create offset variable for easier handling
                Vector2f offset = new Vector2f(0, 0);
                //check below
                if (playerCenter.Y > moveableRectangle.Top + moveableRectangle.Height)
                    offset.Y += Global.PLAYER_MOVEMENT_SPEED;
                //check above
                if (playerCenter.Y < moveableRectangle.Top)
                    offset.Y -= Global.PLAYER_MOVEMENT_SPEED;
                //check right of
                if (playerCenter.X > moveableRectangle.Left + moveableRectangle.Width)
                    offset.X += Global.PLAYER_MOVEMENT_SPEED;
                //check left of
                if (playerCenter.X < moveableRectangle.Left)
                    offset.X -= Global.PLAYER_MOVEMENT_SPEED;

                //only offset to lvl limits
                if ((currentView.Center.X + offset.X) > 390)
                {
                    //offset rectangle and view
                    currentView.Move(offset);
                    moveableRectangle.Top += offset.Y;
                    moveableRectangle.Left += offset.X;
                }
            }
        }

        /// <summary>
        /// Draws everything. Called each game tick.
        /// </summary>
        private static void Draw()
        {
            /*
             * HE WHO DOES NOT READ THE STARRY COMMENTS (aka oneliners are _mostly_ unimportant) SHALL BE SLAIN
             * TO DEATH
             * AND STUFF
             */
            //clear window and apply view to window
            currentRenderWindow.Clear(Color.Black);
            currentRenderWindow.SetView(currentView);
            //draw tilemap
            tileMap.Draw();

            /*
             * Your drawing calls may begin only now.
             * What is draw-called first, will be most backgroundy, so think about where you place your calls.
             * BEGIN YOUR CALLS AFTER THIS
             */
            /*
             * copy to real web browser for best experience
             http://i.minus.com/ixfHgzTc4VA2Q.gif
             EnemyList[0].draw();
             EnemyList[1].draw();
             EnemyList[2].draw();
             */
            //draw all enemys
            foreach (Enemy e in EnemyList)
                e.Draw();

            //draw all torches
            foreach (Torch t in TorchList)
                t.Draw();
            
            //draw player
            currentPlayer.Draw();

            //draw all current bullets
            foreach (Bullet e in BulletList)
                e.Draw();

            //MovementRectDebug();

            /* END YOUR CALLS HERE
             * Doing last call, do not call anything after this
             */
            lightEngine.Draw();
            //draw gui on top of shadow
            currentPlayer.PriorityDraw();
            currentRenderWindow.Display();
            //fps counter in console, if I am thinking this through correctly it should be accurate to 99%
            //now with more accuracy, tho :D
            //i think rather than writing our own code we should stay with the lock we use now, and maybe lower the locked fps
            sw.Stop();
            double elapsedMicroseconds = (double)sw.ElapsedTicks / 10d;
            //Console.WriteLine("FPS: " + 1f / ((double)sw.ElapsedTicks / (double)Stopwatch.Frequency) + ", took " + (double)sw.ElapsedTicks / ((double)Stopwatch.Frequency / 1000d) + "ms");
            sw.Reset();
        }

        //debugging function
        private static void MovementRectDebug()
        {
            float t = currentRenderWindow.GetView().Center.X;
            Vector2f offset = currentRenderWindow.GetView().Center - Global.BEGIN_WINDOW_ORIGIN;
            //Console.WriteLine("X: " + offset.X + ", Y: " + offset.Y);

            RectangleShape r = new RectangleShape(new Vector2f(moveableRectangle.Width, moveableRectangle.Height));
            r.Position = new Vector2f(moveableRectangle.Left, moveableRectangle.Top);
            r.FillColor = Color.Transparent;
            r.OutlineColor = Color.Green;
            r.OutlineThickness = 2f;
            currentRenderWindow.Draw(r);
        }
    }
}
