﻿using System;
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
        //holds first view position
        static float viewOrigin;
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
            //set view origin
            viewOrigin = currentRenderWindow.GetView().Center.X;
            /*
             * Please write your code after this comment, because rule number one is:
             * dont fuck up the view.
             */
            //init tile map//trying to push new changes -.-
            tileMap = new world.TileMap(currentRenderWindow, new Vector2u(200, 10), "world/levels/longTest1.oel", viewOrigin);
            //instance player
            currentPlayer = new Player(currentRenderWindow, 10f, tileMap);

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
            //currentRenderWindow.SetView(currentView);
            tileMap.Update();

            //move view with player
            moveView();

            //testing view move

            float t = currentRenderWindow.GetView().Center.X;
            Console.WriteLine(t - viewOrigin);

            //moves the player
            currentPlayer.Update();
        }

        private static void moveView()
        {
            //get player center
            Vector2f playerCenter = currentPlayer.GetCenter();
            //player left rectangle
            if (!moveableRectangle.Contains(playerCenter.X, playerCenter.Y)){
                //create offset variable for easier handling
                Vector2f offset = new Vector2f(0, 0);
                //decide below, above, left, right
                //check below
                if (playerCenter.Y > moveableRectangle.Top + moveableRectangle.Height)
                    offset.Y += Player.MOVE_SPEED;
                //check above
                if (playerCenter.Y < moveableRectangle.Top)
                    offset.Y -= Player.MOVE_SPEED;
                //check right of
                if (playerCenter.X > moveableRectangle.Left + moveableRectangle.Width)
                    offset.X += Player.MOVE_SPEED;
                //check left of
                if (playerCenter.X < moveableRectangle.Left)
                    offset.X -= Player.MOVE_SPEED;

                //offset rectangle and view
                moveableRectangle.Left += offset.X;
                moveableRectangle.Top += offset.Y;
                currentView.Move(offset);
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
            //DEBUG
            RectangleShape r = new RectangleShape(new Vector2f(moveableRectangle.Width, moveableRectangle.Height));
            r.Position = new Vector2f(moveableRectangle.Left, moveableRectangle.Top);
            r.FillColor = Color.Transparent;
            r.OutlineColor = Color.Green;
            r.OutlineThickness = 2f;
            currentRenderWindow.Draw(r);
            //DEBUG CALL END
            currentPlayer.Draw();
            /*END
             * Doing last call, do not call anything after this
             */
            currentRenderWindow.Display();
        }
    }
}
