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

            //sets framerate to a maximum of 45; changing the value will likely result in bad things
            currentRenderWindow.SetFramerateLimit(45);
            //add event handler for klicking the X icon
            currentRenderWindow.Closed += windowClosed;
            //vertical sync is enabled, because master graphics n shit
            currentRenderWindow.SetVerticalSyncEnabled(true);

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
            /*
             * Please write your code after this comment, because rule number one is:
             * dont fuck up the view.
             */
            //init tile map
            tileMap = new world.TileMap(currentRenderWindow, new Vector2u(20, 10), "world/levels/v2.oel", 2f);
            //instance player
            currentPlayer = new Player(currentRenderWindow, 10f, tileMap);
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
            //currentRenderWindow.SetView(currentView);
            tileMap.Update();

            //view testing code
            moveView();

            //moves the player
            currentPlayer.Move();
        }

        private static void moveView()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.K))
            {
                currentView.Move(new Vector2f(-10f, 0));
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.L))
            {
                currentView.Move(new Vector2f(10f, 0));
            }
        }

        /// <summary>
        /// Draws everything. This is also called each game tick.
        /// </summary>
        private static void Draw(){
            //clear window
            currentRenderWindow.Clear(new Color(0, 153, 153));
            currentRenderWindow.SetView(currentView);
            tileMap.Draw();
            /*
             * Your drawing calls may begin only now.
             * What is draw-called first, will be at most backgroundy, so think about where you place your calls.
             * BEGIN
             */
            currentPlayer.Draw();
            /*END
             * Doing last call, do not call anything after this
             */
            currentRenderWindow.Display();
        }
    }
}
