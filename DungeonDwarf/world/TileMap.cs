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
using System.Diagnostics;

namespace DungeonDwarf.world
{
    class TileMap
    {
        public RenderWindow win;
        public Vector2u allTiles, tilesPerView=new Vector2u(20, 10);
        private int[,] tileTypes;
        private Tile[,] tileArray;
        private bool[,] Collidable;
        private Texture[] textureList=new Texture[3];
        private Texture textureMap=new Texture("textures/world/tilemap.png");
        private VertexArray tileMap;

        public TileMap(RenderWindow _w, Vector2u tileAmount, string _levelLocation){
            win = _w;
            allTiles = tileAmount;
            tileTypes = new int[allTiles.X, allTiles.Y];
            tileArray = new Tile[allTiles.X, allTiles.Y];
            Collidable = new bool[allTiles.X, allTiles.Y];
            fillTileArray(_levelLocation);
            //load all textures
            loadTextures();

            //do the same in vertex
            //create array of all vertices
            tileMap = new VertexArray(PrimitiveType.Quads, allTiles.X*allTiles.Y*4);

            //wanted size
            Vector2f targetQuadSize = new Vector2f((float)win.Size.X / tilesPerView.X, (float)win.Size.Y / tilesPerView.Y);

            //set vertices
            //vertexes
            //whatever
            for (uint y = 0; y < allTiles.Y; y++)
            {
                for (uint x = 0; x < allTiles.X; x++)
                {
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * allTiles.X) + x);
                    //control textures
                    float xOffset = 0;
                    switch (tileTypes[x, y])
                    {
                        case Global.EARTH_TILE:
                            Collidable[x, y] = true;
                            xOffset = 100;
                            break;
                        case Global.EARTH_TOP_TILE:
                            xOffset = 200;
                            Collidable[x, y] = true;
                            break;
                        default:
                            xOffset = 0;
                            Collidable[x, y] = false;
                            break;
                    }

                    //map vertex positions
                    tileMap[currentPosition + 0] = new Vertex(new Vector2f(targetQuadSize.X * x, targetQuadSize.Y * y), new Vector2f(xOffset, 0));//top left vertex
                    tileMap[currentPosition + 1] = new Vertex(new Vector2f(targetQuadSize.X * (x + 1), targetQuadSize.Y * y), new Vector2f(xOffset+100, 0));//top right vertex
                    tileMap[currentPosition + 2] = new Vertex(new Vector2f(targetQuadSize.X * (x + 1), targetQuadSize.Y * (y + 1)), new Vector2f(xOffset+100, 100));//bot right vertex
                    tileMap[currentPosition + 3] = new Vertex(new Vector2f(targetQuadSize.X * x, targetQuadSize.Y * (y + 1)), new Vector2f(xOffset+0, 100));//bot left vertex
                }
            }
            
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

        public void Update(){
            //well the tilemap basically does not get updated at this point anymore...
        }

        public void Draw()
        {
            //foreach (Tile t in tileArray)
            //    t.Draw();
            RenderStates s = RenderStates.Default;
            s.Texture = textureMap;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            tileMap.Draw(win, s);
            sw.Stop();
            Console.WriteLine("tilemap rendering took " + sw.ElapsedMilliseconds+" ms");

        }
    }
}
