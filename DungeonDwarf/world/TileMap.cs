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
        public Vector2u allTiles, tilesPerView=new Vector2u(20, 10);
        private int[,] tileTypes;
        private Tile[,] tileArray;
        private Texture[] textureList=new Texture[3];

        public TileMap(RenderWindow _w, Vector2u tileAmount, string _levelLocation)
        {
            win = _w;
            allTiles = tileAmount;
            tileTypes = new int[allTiles.X, allTiles.Y];
            tileArray = new Tile[allTiles.X, allTiles.Y];
            fillTileArray(_levelLocation);
            //load all textures
            loadTextures();
            //now get a tile for each of these
            for (int y = 0; y < allTiles.Y; y++){
                for (int x = 0; x < allTiles.X; x++){
                    //add a tile for each array position.
                    tileArray[x, y] = new Tile(win, allTiles, tilesPerView, new Vector2u((uint)x, (uint)y), tileTypes[x, y], textureList[tileTypes[x, y]]);
                }
            }
        }

        /// <summary>
        /// Load all textures here.
        /// </summary>
        private void loadTextures()
        {
            textureList[Tile.EARTH_TILE] = new Texture("textures/world/earthTile.png");
            textureList[Tile.EARTH_TOP_TILE] = new Texture("textures/world/earthTileTop.png");
            textureList[Tile.AIR_TILE] = new Texture("textures/world/air.png");
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
                EARTHTOP = s.Element("earthTop").Value,
                AIR = s.Element("air").Value
            }).FirstOrDefault();
            //get strings from array, removing all linebreaks
            string earth = query.EARTH.Replace("\n", String.Empty);
            string earthtop = query.EARTHTOP.Replace("\n", String.Empty);
            string air = query.AIR.Replace("\n", String.Empty);
            //get byte arrays from strings
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] earthArray = enc.GetBytes(earth);
            byte[] earthTopArray = enc.GetBytes(earthtop);
            byte[] airArray = enc.GetBytes(air);

            //get tiles from byte arrays
            for(int y=0;y<allTiles.Y;y++){
                for (int x = 0; x < allTiles.X; x++){
                    long oneDimensionalArrayPosition=y * allTiles.X + x;
                    if (earthArray[oneDimensionalArrayPosition] == 49)//ASCII one
                        tileTypes[x, y] = Tile.EARTH_TILE;
                    else if (earthTopArray[oneDimensionalArrayPosition] == 49)
                        tileTypes[x, y] = Tile.EARTH_TOP_TILE;
                    else if (airArray[oneDimensionalArrayPosition] == 49)
                        tileTypes[x, y] = Tile.AIR_TILE;
                    else
                        tileTypes[x, y] = Tile.AIR_TILE;
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
                if (aRect.Intersects(t.getRect()) && t.Collidable)
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
            for (int y = 0; y < allTiles.Y; y++)
            {
                Tile t = tileArray[tilePosition[0], y];
                if (t.Collidable)
                    return t.GridPosition.Y;
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
            for (int y = 0; y < allTiles.Y; y++)
            {
                for (int x = 0; x < allTiles.X; x++)
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
            //well the tilemap basically does not get updated at this point anymore...
        }

        public void Draw()
        {
            foreach (Tile t in tileArray)
                t.Draw();
        }
    }
}
