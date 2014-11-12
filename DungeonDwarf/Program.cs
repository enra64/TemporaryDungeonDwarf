using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.Window;

namespace DungeonDwarf
{
    class Program
    {
        static RenderWindow currentRenderWindow;
        static View currentView;
        static world.TileMap tileMap;
        static Player currentPlayer;
        static Enemy zeroEnemy;
        static FloatRect moveableRectangle;

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

            //first and only call to init, do everything else there
            Initialize();
            //first and only call to load content, not mandatory to use
            LoadContent();

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

        private static void windowResized(object sender, SizeEventArgs e)
        {
            //update tile count
            tileMap.Update();
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
            //init tile map//trying to push new changes -.-
            tileMap = new world.TileMap(currentRenderWindow, new Vector2u(400, 10), "world/levels/longTest2.oel");
            //instance player
            currentPlayer = new Player(currentRenderWindow, 10f, tileMap);
            zeroEnemy = new Enemy(currentRenderWindow, currentPlayer.playerPosition, tileMap);

            //create a rectangle the player can move in without changing the view
            Vector2f tempCurrentPlayerCenter=currentPlayer.GetCenter();
            //this is said rectangle
            moveableRectangle = new FloatRect(tempCurrentPlayerCenter.X - currentRenderWindow.Size.X / 6, tempCurrentPlayerCenter.Y - currentRenderWindow.Size.Y / 4, currentRenderWindow.Size.X / 3, currentRenderWindow.Size.Y / 2);
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
            //store current offset in global class
            Global.CURRENT_WINDOW_ORIGIN=currentView.Center-Global.BEGIN_WINDOW_ORIGIN;
            //moves the player
            currentPlayer.Update();
            //check for key input
            KeyCheck();
            zeroEnemy.update(currentPlayer.playerPosition);
        }

        static void KeyCheck()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                currentRenderWindow.Close();
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
                    //offset rectangle and view
                    currentView.Move(offset);
                    moveableRectangle.Top += offset.Y;
                    moveableRectangle.Left += offset.X;

                }
            }
        }

        /// <summary>
        /// Draws everything. This is also called each game tick.
        /// </summary>
        private static void Draw(){
            /*
             * HE WHO DOES NOT READ THE STARRY COMMENTS (aka oneliners are _mostly_ unimportant) SHALL BE SLAIN
             * TO DEATH
             * AND STUFF
             */
            //clear window
            currentRenderWindow.Clear(new Color(0, 153, 153));
            //apply view to window
            currentRenderWindow.SetView(currentView);
            //draw map/level
            tileMap.Draw();
            /*
             * Your drawing calls may begin only now.
             * What is draw-called first, will be most backgroundy, so think about where you place your calls.
             * BEGIN YOUR CALLS AFTER THIS
             */
            zeroEnemy.draw();
            currentPlayer.Draw();
            /* END YOUR CALLS HERE
             * Doing last call, do not call anything after this
             */
            currentRenderWindow.Display();
        }

        //you can ignore this function
        private void MovementRectDebug()
        {
            //Debug
            //DEBUG
            float t = currentRenderWindow.GetView().Center.X;
            Vector2f offset = currentRenderWindow.GetView().Center - Global.BEGIN_WINDOW_ORIGIN;
            Console.WriteLine("X: " + offset.X + ", Y: " + offset.Y);

            RectangleShape r = new RectangleShape(new Vector2f(moveableRectangle.Width, moveableRectangle.Height));
            r.Position = new Vector2f(moveableRectangle.Left, moveableRectangle.Top);
            r.FillColor = Color.Transparent;
            r.OutlineColor = Color.Green;
            r.OutlineThickness = 2f;
            currentRenderWindow.Draw(r);
        }
    }
}
