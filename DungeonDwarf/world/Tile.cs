using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Window;
using SFML.Graphics;

namespace DungeonDwarf.world
{
    /// <summary>
    /// You should be able to mostly ignore this class, and only change it if you are sure you know what
    /// you are doing
    /// </summary>
    class Tile
    {
        public Vector2u GridPosition;
        public int Type;
        public bool Collidable;
        private Vector2u tileCount;
        private RenderWindow win;
        private Sprite mySprite;
        //following are the tile types VS ID
        public const int AIR_TILE = 2, EARTH_TILE = 0, EARTH_TOP_TILE = 1;


        /// <summary>
        /// Constructor. _tP is an int vector containing the grid position,
        /// _tT is a byte representing the tile type.
        /// The given renderwindow will be used to draw the tiles
        /// </summary>
        /// <param name="_gridPosition"></param>
        /// <param name="_tileType"></param>
        public Tile(RenderWindow _w, Vector2u _tileCount, Vector2u _tilePerWindowCount, Vector2u _gridPosition, int _tileType, Texture _myTexture)
        {
            //transfer parameters into class variables
            decideCollide(_tileType);
            win = _w;
            tileCount = _tileCount;
            GridPosition = _gridPosition;
            Type = _tileType;

            //scale sprite
            mySprite = new Sprite(_myTexture);
            //scale sprite for 20x10 (or accordingly, value is variable) grid
            //and cast EVERYTHING TO FLOAT BECAUSE BAUM THATS WHY
            mySprite.Scale = new Vector2f(((float)win.Size.X / (float)_tilePerWindowCount.X) / (float)_myTexture.Size.X, ((float)win.Size.Y / (float)_tilePerWindowCount.Y) / (float)_myTexture.Size.Y);
            //set position correctly (hopefully at least)
            //pos calculation: tileposition * texturesize * scaling
            mySprite.Position = new Vector2f((float)GridPosition.X * _myTexture.Size.X * mySprite.Scale.X, (float)GridPosition.Y * _myTexture.Size.Y * mySprite.Scale.Y);
            //debugging
            //mySprite.Position = new Vector2f(20f, 20f);
        }

        public FloatRect getRect(){
            FloatRect globalBounds = mySprite.GetGlobalBounds();
            globalBounds.Left = mySprite.Position.X;
            globalBounds.Top = mySprite.Position.Y;
            return globalBounds;
        }

        /// <summary>
        /// ugly collision definition
        /// </summary>
        private void decideCollide(int type)
        {
            switch(type){
                case EARTH_TILE://earth
                    //Console.WriteLine("creating earth");
                    Collidable = true;
                    break;
                case EARTH_TOP_TILE://earth no top
                    Collidable = true;
                    //Console.WriteLine("creating earthtop");
                    break;
                case AIR_TILE://air
                    Collidable = false;
                    //Console.WriteLine("creating air");
                    break;
            }
        }

        /// <summary>
        /// update position if tile position has changed.
        /// </summary>
        /// <param name="newPosition"></param>
        public void Update(Vector2u newPosition)
        {
            //commit test
            if (!GridPosition.Equals(newPosition)){
                mySprite.Position = new Vector2f(newPosition.X * mySprite.GetLocalBounds().Width, newPosition.Y * mySprite.GetLocalBounds().Height);
                GridPosition = newPosition;
            }
        }

        public void Draw(){
            win.Draw(mySprite);
        }
    }
}
