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
        public Vector2u tileAmount, tilesPerView=new Vector2u(20, 10);
        private int[,] tileTypes;
        private Tile[,] tileArray;
        private bool[,] Collidable;
        private Texture[] textureList=new Texture[3];
        private Texture textureMap=new Texture("textures/world/tilemap.png");
        private VertexArray tileMap;
        //contains target sie for quads
        private Vector2f targetQuadSize;
        private RenderStates renderStates = RenderStates.Default;

        public TileMap(RenderWindow _w, Vector2u _tileAmount, string _levelLocation){
            win = _w;
            tileAmount = _tileAmount;
            tileTypes = new int[tileAmount.X, tileAmount.Y];
            tileArray = new Tile[tileAmount.X, tileAmount.Y];
            Collidable = new bool[tileAmount.X, tileAmount.Y];
            fillTileTypeArray(_levelLocation);

            //load texture for tilemap
            renderStates.Texture = textureMap;

            //create array of all vertices
            tileMap = new VertexArray(PrimitiveType.Quads, tileAmount.X*tileAmount.Y*4);

            //wanted size is a square, not a rectangle
            //targetQuadSize = new Vector2f((float)win.Size.X / tilesPerView.X, (float)win.Size.Y / tilesPerView.Y);
            //y is too small
            targetQuadSize = new Vector2f((float)win.Size.Y / tilesPerView.Y, (float)win.Size.Y / tilesPerView.Y);
            Global.GLOBAL_SCALE = targetQuadSize.X/textureMap.Size.Y;

            //set vertices
            //vertexes
            //whatever
            for (uint y = 0; y < tileAmount.Y; y++){
                for (uint x = 0; x < tileAmount.X; x++){
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * tileAmount.X) + x);
                    //control textures
                    float xOffset = 0;
                    switch (tileTypes[x, y]){
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
        }

        /// <summary>
        /// returns the rectangle of the vertex at position x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private FloatRect GetRectangle(int x, int y){
            return new FloatRect(targetQuadSize.X * x, targetQuadSize.Y * y,
                targetQuadSize.X, targetQuadSize.Y);
        }

        /// <summary>
        /// fills tile type array using maps created by ogmo
        /// </summary>
        /// <param name="levelLocation"></param>
        private void fillTileTypeArray(string levelLocation){
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
            for(int y=0;y<tileAmount.Y;y++){
                for (int x = 0; x < tileAmount.X; x++){
                    long oneDimensionalArrayPosition=y * tileAmount.X + x;
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
        public bool Collides(Vector2f position, Vector2f size){
            //create floatrect from input
            FloatRect aRect = new FloatRect(position.X, position.Y, size.X, size.Y);
            //check intersection for each tile
            for (int y = 0; y < tileAmount.Y; y++)
                for (int x = 0; x < tileAmount.X; x++)
                    //check each rectangles' position
                    if (aRect.Intersects(GetRectangle(x, y)) && Collidable[x, y])
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
        public bool CheckNextCollide(Vector2f position, Vector2f size, Vector2f nextMove){
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
            for (int y = 0; y < tileAmount.Y; y++){
                if (Collidable[tilePosition[0], y])
                    return 2;
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
            for (int y = 0; y < tileAmount.Y; y++){
                for (int x = 0; x < tileAmount.X; x++){
                    //check each rectangles' position
                    if (GetRectangle(x, y).Contains(center.X, center.Y)) ;
                    return new int[] { x, y };
                }
            }
            return new int[] { -1, -1 };
        }

        public void Update(){
            //draw more x tiles if the screen has been resized...
            //calculate how many tiles in x direction should be drawn
            tilesPerView=new Vector2u(win.Size.X/60, 10);

            for (uint y = 0; y < tileAmount.Y; y++)
            {
                for (uint x = 0; x < tileAmount.X; x++)
                {
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * tileAmount.X) + x);
                    //get old texture coordinates
                    Vector2f texCo1 = tileMap[currentPosition + 0].TexCoords;
                    Vector2f texCo2 = tileMap[currentPosition + 1].TexCoords;
                    Vector2f texCo3 = tileMap[currentPosition + 2].TexCoords;
                    Vector2f texCo4 = tileMap[currentPosition + 3].TexCoords;
                    //map vertex positions
                    tileMap[currentPosition + 0] = new Vertex(new Vector2f(targetQuadSize.X * x, targetQuadSize.Y * y), texCo1);//top left vertex
                    tileMap[currentPosition + 1] = new Vertex(new Vector2f(targetQuadSize.X * (x + 1), targetQuadSize.Y * y), texCo2);//top right vertex
                    tileMap[currentPosition + 2] = new Vertex(new Vector2f(targetQuadSize.X * (x + 1), targetQuadSize.Y * (y + 1)), texCo3);//bot right vertex
                    tileMap[currentPosition + 3] = new Vertex(new Vector2f(targetQuadSize.X * x, targetQuadSize.Y * (y + 1)), texCo4);//bot left vertex
                }
            }
        }

        public void Draw()
        {
            //timekeeping is commented out
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            tileMap.Draw(win, renderStates);
            //sw.Stop();
            //Console.WriteLine("tilemap rendering took " + sw.Elapsed.Duration()+" ms");
        }
    }
}
