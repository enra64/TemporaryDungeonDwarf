using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.Window;
using System.Diagnostics;

namespace DungeonDwarf
{
    class Program
    {
        static RenderWindow currentRenderWindow;
        static View currentView;
        static world.TileMap tileMap;
        static Player currentPlayer;
        static FloatRect moveableRectangle;
        static Stopwatch tileMapUpdater = new Stopwatch();
        static List<Sprite> backgroundList = new List<Sprite>();
        static List<Enemy> EnemyList = new List<Enemy>();
        static List<Bullet> BulletList = new List<Bullet>();
        static Inventory currentInventory;
        static bool debugInventory = false;
        static Bullet bullet1;
        static bool BulletButton = false;

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
            currentRenderWindow.SetFramerateLimit(45);
            //add event handler for klicking the X icon
            currentRenderWindow.Closed += windowClosed;
            //vertical sync is enabled, because master graphics n shit
            currentRenderWindow.SetVerticalSyncEnabled(true);

            //add handler to window resized, because we need to recalculate tile count
            currentRenderWindow.Resized += windowResized;

            //add mouse click handling for getting focus
            currentRenderWindow.MouseButtonPressed += mouseClick;

            //first and only call to init, do everything else there
            Initialize();
            //first and only call to load content, not mandatory to use
            LoadContent();

            //show startscreen

            //Arne, you forgot to push StartScreen.cs and Inventory.cs
            //http://bit.ly/19hKyY7
            StartScreen s = new StartScreen(currentRenderWindow);
            while (!Keyboard.IsKeyPressed(Keyboard.Key.Space)&&currentRenderWindow.IsOpen())
            {
                s.Update();
                s.Draw();
            } 

            /*
             * shit be about to get real... starting main loop.
             */
            while (currentRenderWindow.IsOpen()){
                //mandatory update and draw calls
                Update();
                Draw();
                //dispatch things like "i would like to close this window" and "someone clicked me".
                //only important if you want to close the window. ever.
                currentRenderWindow.DispatchEvents();
            }
        }

        private static void mouseClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(e.X + ": x, y: " + e.Y);
            currentInventory.Click(e.X, e.Y);
        }

        private static void windowResized(object sender, SizeEventArgs e)
        {
            //update tile count
            tileMap.UpdateWindowResize();
            currentView.Size = new Vector2f(800, 600);
        }

        private static void windowClosed(object sender, EventArgs e){
            ((RenderWindow)sender).Close();
        }

        /// <summary>
        /// Called immediately after creating the window.
        /// Create or instance anything you think the main function needs here.
        /// </summary>
        private static void Initialize(){
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
            /*
             TILEMAP
             */
            //init tile map
            tileMap = new world.TileMap(currentRenderWindow, new Vector2u(400, 10), "world/levels/lavatest.oel");
            //start tilemap update stopwatch
            tileMapUpdater.Start();
            
            //initialize inventory, currently under heavy development (as in probably wont work)
            currentInventory = new Inventory(currentRenderWindow, new Vector2f(70, 70), new Vector2f(50, 50));
            //player and enemy init
            #region playerAndEnemy
            Texture backTex = new Texture("textures/world/background.png");
            //instance player
            currentPlayer = new Player(currentRenderWindow, 10f, tileMap);

            // Konstructor mit Enemy(Gegnername, currenRenderWindow, currentPlayer.playerPosition, tileMap, Texturpath als String, x-Wert Scaling, y-Wert Scaling, Movementspeed, Jumpspeed));
            EnemyList.Add(new Enemy("zeroEnemy", currentRenderWindow, currentPlayer.playerPosition, tileMap));
            EnemyList.Add(new Enemy("enemy1", currentRenderWindow, currentPlayer.playerPosition, tileMap));
            EnemyList.Add(new Enemy("enemy2", currentRenderWindow, currentPlayer.playerPosition, tileMap));
            #endregion

            //Init the rectangle the user can move in without changing view
            #region nomoverect
            //create a rectangle the player can move in without changing the view
            Vector2f tempCurrentPlayerCenter=currentPlayer.GetCenter();
            //this is said rectangle
            moveableRectangle = new FloatRect(tempCurrentPlayerCenter.X - currentRenderWindow.Size.X / 6, tempCurrentPlayerCenter.Y - currentRenderWindow.Size.Y / 4, currentRenderWindow.Size.X / 3, currentRenderWindow.Size.Y / 2);
            #endregion
        }

        /// <summary>
        /// Would be nice to load textures you want to use a lot here,
        /// so you can use them in your class' constructors
        /// </summary>
        private static void LoadContent(){

        }

        /// <summary>
        /// This method handles all mandatory update calls.
        /// It is thus called in each main loop iteration.
        /// </summary>
        private static void Update(){
            //move view with player
            moveView();
            //update tilemap if due
            if (tileMapUpdater.ElapsedMilliseconds > 500){
                tileMapUpdater.Restart();
                tileMap.Update();
            }
            //store current offset in global class
            Global.CURRENT_WINDOW_ORIGIN=currentView.Center-Global.BEGIN_WINDOW_ORIGIN;
            /*
             * BEGIN YOUR CODE AFTER THIS
             */
            BulletCollision();
            EnemyCollision();
            //moves the player
            currentPlayer.Update();
            //check for key input
            KeyCheck();
            //Moving Bullet
            foreach (Bullet e in BulletList)
                e.Update();
            //hint:
            foreach (Enemy e in EnemyList)
                e.Update(currentPlayer.playerPosition);
        }

        private static void BulletCollision()
        {
            for (int i = 0; i < BulletList.Count; i++)
            {
                Bullet b = BulletList.ElementAt(i);
                for (int j = 0; j < EnemyList.Count; j++)
                {
                    Enemy e = EnemyList.ElementAt(j);
                    //actual collision check
                    FloatRect bulletRect = new FloatRect(b.bulletPosition.X, b.bulletPosition.Y, b.bulletSize.X, b.bulletSize.Y);
                    FloatRect enemyRect = new FloatRect(e.enemyPosition.X, e.enemyPosition.Y, e.enemySize.X, e.enemySize.Y);
                    //remove both on collision
                    if (bulletRect.Intersects(enemyRect)){
                        BulletList.RemoveAt(i);
                        EnemyList.RemoveAt(j);
                        //avoid wrong indices
                        break;
                    }

                }
            }
        }

        // method to check for collision of all Enemies of List EnemyList with Player
        private static void EnemyCollision()
        {
            if (currentPlayer.health > 0)  // prevents unnecessary code execution, just thought it might be useful, remove if you think its redundant
            {    
                FloatRect playerRect = new FloatRect(currentPlayer.playerPosition.X, currentPlayer.playerPosition.Y, currentPlayer.playerSize.X, currentPlayer.playerSize.Y);
                foreach (Enemy enem in EnemyList)
                {
                   FloatRect enemyRect = new FloatRect(enem.enemyPosition.X, enem.enemyPosition.Y, enem.enemySize.X, enem.enemySize.Y);
                   
                   // what to do if collision = true (we should discuss what should happen if; for now it only sets the players shield on zero)
                   if (playerRect.Intersects(enemyRect)) 
                   {
                        currentPlayer.shield = 0;
                        //currentPlayer.health = 0;
                    }
                }
            }

        }

        static void KeyCheck()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                currentRenderWindow.Close();
            if (Keyboard.IsKeyPressed(Keyboard.Key.K))
            {
                currentView.Zoom(.99f);
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.L))
            {
                currentView.Zoom(1.01f);
            }
            //easter egg or something
            if (Keyboard.IsKeyPressed(Keyboard.Key.N))
                Console.WriteLine("batman");
            if (Keyboard.IsKeyPressed(Keyboard.Key.I)){
                currentInventory.Show();
            }
            //fire debouncing
            if (Keyboard.IsKeyPressed(Keyboard.Key.F)){
                if (BulletButton == false){
                    BulletList.Add(new Bullet(currentPlayer.playerPosition, "textures/weapons/arrow/Feuer.png", currentRenderWindow));
                    BulletButton = true;
                }
            }
            if (!Keyboard.IsKeyPressed(Keyboard.Key.F)){
                BulletButton = false;
            }
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
            if (!moveableRectangle.Contains(playerCenter.X, playerCenter.Y)){
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
                if ((currentView.Center.X + offset.X) > 390){
                    //offset background
                    //Console.WriteLine("cvc: " + currentView.Center + " offset: " + offset.X);
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
        private static void Draw(){
            Stopwatch sw = new Stopwatch();
            sw.Start();
            /*
             * HE WHO DOES NOT READ THE STARRY COMMENTS (aka oneliners are _mostly_ unimportant) SHALL BE SLAIN
             * TO DEATH
             * AND STUFF
             */
            //clear window
            currentRenderWindow.Clear(new Color(52, 52, 52));
            //draw background
            //currentRenderWindow.Draw(backgroundSprite);
            //apply view to window
            currentRenderWindow.SetView(currentView);
            //draw map/level
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

            //draw player
            currentPlayer.Draw();

            //draw all current bullets
            foreach (Bullet e in BulletList)
                e.Draw();
            
            //MovementRectDebug();
            
            /* END YOUR CALLS HERE
             * Doing last call, do not call anything after this
             */
            currentRenderWindow.Display();
            sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();
        }

        //you can ignore this function
        private static void MovementRectDebug()
        {
            //Debug
            //DEBUG
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
