using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SFML;
using SFML.Window;
using SFML.Graphics;
using System.Xml.Linq;

namespace DungeonDwarf.world
{
    class TileMap
    {
        public RenderWindow win;
        public Vector2u tiles;
        private int[,] tileTypes;
        private Tile[,] tileArray;
        private Texture[] textureList=new Texture[3];

        //tile type consts
        public const int EARTH = 0, EARTHTOP = 1, AIR = 2;

        public TileMap(RenderWindow _w, Vector2u _t, string _lL)
        {
            win = _w;
            tiles = _t;
            tileTypes = new int[tiles.X, tiles.Y];
            tileArray = new Tile[tiles.X, tiles.Y];
            fillTileArray(_lL);
            //load all textures
            loadTextures();
            //now get a tile for each of these
            for (int y = 0; y < tiles.Y; y++)
            {
                for (int x = 0; x < tiles.X; x++)
                {
                    //add a tile for each array.
                    tileArray[x, y] = new Tile(win, tiles, new Vector2u((uint)x, (uint)y), tileTypes[x, y], textureList[tileTypes[x, y]]);
                }
            }
        }

        private void loadTextures()
        {
            textureList[0] = new Texture("textures/world/earthTile.png");
            textureList[1] = new Texture("textures/world/earthTileTop.png");
            textureList[2] = new Texture("textures/world/air.png");
        }

        /// <summary>
        /// fills tile array using maps created by ogmo
        /// </summary>
        /// <param name="levelLocation"></param>
        private void fillTileArray(string levelLocation)
        {
            //great. get tiles from a f*ckin xml file...
            XDocument xDoc = XDocument.Load(levelLocation);
            var query = xDoc.Descendants("level").Select(s => new
            {
                EARTH = s.Element("earth").Value,
                EARTHTOP = s.Element("earthtop").Value,
                AIR = s.Element("air").Value
            }).FirstOrDefault();
            //get strings from array, removing all linebreaks
            string earth = query.EARTH.Replace("\n", String.Empty);
            string earthtop = query.EARTHTOP.Replace("\n", String.Empty);
            string air = query.AIR.Replace("\n", String.Empty);
            /*Console.Write(earth);
            Console.Write(earthtop);
            Console.Write(air);*/
            //get byte arrays from strings
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] earthArray = enc.GetBytes(earth);
            byte[] earthTopArray = enc.GetBytes(earthtop);
            byte[] airArray = enc.GetBytes(air);

            //get tiles from byte arrays
            for(int y=0;y<tiles.Y;y++){
                for (int x = 0; x < tiles.X; x++){
                    long oneDimensionalArrayPosition=y * tiles.X + x;
                    if (earthArray[oneDimensionalArrayPosition] == 49)
                        tileTypes[x, y] = EARTH;
                    else if (earthTopArray[oneDimensionalArrayPosition] == 49)
                        tileTypes[x, y] = EARTHTOP;
                    else if (airArray[oneDimensionalArrayPosition] == 49)
                        tileTypes[x, y] = AIR;
                    else
                        tileTypes[x, y] = AIR;
                }
            }
            
        }

        /// <summary>
        /// Use this to check for collisions with the tiles that are set to collidable.
        /// It returns true if the Rectangle created using the position and and size you
        /// gave intersects with any tile.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Collides(Vector2f position, Vector2f size)
        {
            FloatRect aRect = new FloatRect(position.X, position.Y, size.X, size.Y);
            foreach (Tile t in tileArray)
                if (aRect.Intersects(t.getRect()) && t.collide)
                    return true;
            return false;
        }

        /// <summary>
        /// This is a convenience function that adds the nextmove on top of the position parameter,
        /// so you can easily check for intersections after the next move.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="nextMove"></param>
        /// <returns></returns>
        public bool CheckNextCollide(Vector2f position, Vector2f size, Vector2f nextMove)
        {
            position+=nextMove;
            return Collides(position, size);
        }

        /// <summary>
        /// You give a float x position, and get the highest colliding y float position.
        /// This function does not yet handle view position.
        /// </summary>
        /// <param name="xPosition"></param>
        /// <returns></returns>
        public float GetMinYAtX(float xPosition)
        {
            //get highest tile at x position
            int[] tilePosition = GetCurrentTile(new Vector2f(xPosition, 0));
            for (int y = 0; y < tiles.Y; y++)
            {
                Tile t = tileArray[tilePosition[0], y];
                if (t.collide)
                    return t.tilePosition.Y;
            }
            return win.Size.Y;
        }

        /// <summary>
        /// returns the x and y grid position your given center position lies in
        /// </summary>
        /// <param name="center"></param>
        /// <returns></returns>
        public int[] GetCurrentTile(Vector2f center)
        {
            for (int y = 0; y < tiles.Y; y++)
            {
                for (int x = 0; x < tiles.X; x++)
                {
                    Tile t = tileArray[x, y];
                    if (t.getRect().Contains(center.X, center.Y))
                        return new int[] { x, y };
                }
            }
            return new int[] { -1, -1 };
        }

        public void Update()
        {
        }

        public void Draw()
        {
            foreach (Tile t in tileArray)
                t.Draw();
        }
    }
}
