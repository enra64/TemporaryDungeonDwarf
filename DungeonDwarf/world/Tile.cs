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
    class Tile
    {
        public Vector2u tilePosition;
        public int tileType;
        public bool collide;
        private Vector2u tileCount;
        private RenderWindow win;
        private Sprite mySprite;

        /// <summary>
        /// Constructor. _tP is an int vector containing the grid position,
        /// _tT is a byte representing the tile type.
        /// The given renderwindow will be used to draw the tiles
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <param name="_tileType"></param>
        public Tile(RenderWindow _w, Vector2u _tileCount, Vector2u _tilePerWindowCount, Vector2u gridPosition, int _tileType, Texture _myTexture)
        {
            //decides whether to collide or not
            decideCollide(_tileType);
            win = _w;
            tileCount = _tileCount;
            tilePosition = gridPosition;
            tileType = _tileType;

            //scale sprite
            mySprite = new Sprite(_myTexture);
            //scale sprite for 20x10 (or accordingly, value is variable) grid
            //cast EVERYTHING TO FLOAT BECAUSE BAUM THATS WHY
            mySprite.Scale = new Vector2f(((float)win.Size.X / (float)_tilePerWindowCount.X) / (float)_myTexture.Size.X, ((float)win.Size.Y / (float)_tilePerWindowCount.Y) / (float)_myTexture.Size.Y);
            //set position correctly (hopefully at least)
            //pos calculation: tileposition * texturesize * scaling
            mySprite.Position = new Vector2f((float)tilePosition.X * _myTexture.Size.X * mySprite.Scale.X, (float)tilePosition.Y * _myTexture.Size.Y * mySprite.Scale.Y);
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
                case 0://earth
                    //Console.WriteLine("creating earth");
                    collide = true;
                    break;
                case 1://earth no top
                    collide = true;
                    //Console.WriteLine("creating earthtop");
                    break;
                case 2://air
                    collide = false;
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
            if (!tilePosition.Equals(newPosition)){
                mySprite.Position = new Vector2f(newPosition.X * mySprite.GetLocalBounds().Width, newPosition.Y * mySprite.GetLocalBounds().Height);
                tilePosition = newPosition;
            }
        }

        public void Draw(){
            win.Draw(mySprite);
        }
    }
}
