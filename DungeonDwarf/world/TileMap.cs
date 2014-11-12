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

        //tile specific variables
        private int[,] tileTypes;
        private bool[,] Collidable;
        
        //single variable: how many tiles below the max y value we _know_ about shall we draw
        private uint yInterpolationDuration = 5;

        private Texture textureMap=new Texture("textures/world/tilemap.png");
        //contains target size for quads
        private Vector2f idealQuadSize, currentQuadSize;
        //vertex drawing stuff
        private VertexArray tileMap;
        private RenderStates renderStates = RenderStates.Default;

        public TileMap(RenderWindow _w, Vector2u _tileAmount, string _levelLocation){
            win = _w;
            tileAmount = _tileAmount;
            tileTypes = new int[tileAmount.X, tileAmount.Y];
            Collidable = new bool[tileAmount.X, tileAmount.Y];
            fillTileTypeArray(_levelLocation);

            //load texture for tilemap
            renderStates.Texture = textureMap;

            //create array of all vertices
            tileMap = new VertexArray(PrimitiveType.Quads, tileAmount.X * (tileAmount.Y + yInterpolationDuration) * 4);

            //wanted size is a square, not a rectangle
            //targetQuadSize = new Vector2f((float)win.Size.X / tilesPerView.X, (float)win.Size.Y / tilesPerView.Y);
            //y is too small
            idealQuadSize = new Vector2f((float)win.Size.Y / tilesPerView.Y, (float)win.Size.Y / tilesPerView.Y);
            Global.GLOBAL_SCALE = idealQuadSize.X/textureMap.Size.Y;

            //initialize the current quad size
            currentQuadSize = idealQuadSize;

            //draw vertexicesarrays. y+5 for interpolating ground
            for (uint y = 0; y < tileAmount.Y + yInterpolationDuration; y++){
                for (uint x = 0; x < tileAmount.X; x++){
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * tileAmount.X) + x);
                    //control textures
                    float xOffset = 0;
                    int tileType;
                    if (y < tileAmount.Y)
                        tileType = tileTypes[x, y];
                    else
                        tileType = tileTypes[x, tileAmount.Y-1];
                    switch (tileType)
                    {
                        case Global.EARTH_TILE:
                            if (y < tileAmount.Y)
                                Collidable[x, y] = true;
                            xOffset = 100;
                            break;
                        case Global.EARTH_TOP_TILE:
                            xOffset = 200;
                            if (y < tileAmount.Y)
                                Collidable[x, y] = true;
                            break;
                        default:
                            xOffset = 0;
                            if (y < tileAmount.Y)
                                Collidable[x, y] = false;
                            break;
                    }
                    //map vertex positions
                    tileMap[currentPosition + 0] = new Vertex(new Vector2f(idealQuadSize.X * x, idealQuadSize.Y * y), new Vector2f(xOffset, 0));//top left vertex
                    tileMap[currentPosition + 1] = new Vertex(new Vector2f(idealQuadSize.X * (x + 1), idealQuadSize.Y * y), new Vector2f(xOffset+100, 0));//top right vertex
                    tileMap[currentPosition + 2] = new Vertex(new Vector2f(idealQuadSize.X * (x + 1), idealQuadSize.Y * (y + 1)), new Vector2f(xOffset+100, 100));//bot right vertex
                    tileMap[currentPosition + 3] = new Vertex(new Vector2f(idealQuadSize.X * x, idealQuadSize.Y * (y + 1)), new Vector2f(xOffset+0, 100));//bot left vertex
                    
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
            return new FloatRect(currentQuadSize.X * x, currentQuadSize.Y * y,
                currentQuadSize.X, currentQuadSize.Y);
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
                        tileTypes[x, y] = Global.EARTH_TILE;
                    else if (earthTopArray[oneDimensionalArrayPosition] == 49)
                        tileTypes[x, y] = Global.EARTH_TOP_TILE;
                    else
                        tileTypes[x, y] = Global.AIR_TILE;
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
        public float GetMinYAtX(float xPosition){
            //get highest tile at x position
            int[] tilePosition = GetCurrentTile(new Vector2f(xPosition, 0));
            if (tilePosition[0] < 0)
                return -1;
            for (int y = 0; y < tileAmount.Y; y++){
                if (Collidable[tilePosition[0], y])
                    return GetRectangle(tilePosition[0], y).Top;
            }
            return win.Size.Y;
        }

        /// <summary>
        /// returns the x and y grid position your given center position lies in
        /// </summary>
        /// <param name="center"></param>
        /// <returns></returns>
        public int[] GetCurrentTile(Vector2f center){
            for (int y = 0; y < tileAmount.Y; y++){
                for (int x = 0; x < tileAmount.X; x++){
                    //check each rectangles' position
                    if (GetRectangle(x, y).Contains(center.X, center.Y))
                        return new int[] { x, y };
                }
            }
            return new int[] { -1, -1 };
        }

        public void UpdateWindowResize(){
            //this idea fortunately does work. (reducing x size)
            currentQuadSize.X = idealQuadSize.X * (800f / (float)win.Size.Y);

            //sad implication: we have to scale the position of literally everything when we do this
            Global.GLOBAL_SCALE = currentQuadSize.X / idealQuadSize.X;

            for (uint y = 0; y < tileAmount.Y + yInterpolationDuration; y++){
                for (uint x = 0; x < tileAmount.X; x++){
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * tileAmount.X) + x);
                    //get old texture coordinates
                    Vector2f texCo1 = tileMap[currentPosition + 0].TexCoords;
                    Vector2f texCo2 = tileMap[currentPosition + 1].TexCoords;
                    Vector2f texCo3 = tileMap[currentPosition + 2].TexCoords;
                    Vector2f texCo4 = tileMap[currentPosition + 3].TexCoords;
                    //map vertex positions
                    tileMap[currentPosition + 0] = new Vertex(new Vector2f(currentQuadSize.X * x, currentQuadSize.Y * y), texCo1);//top left vertex
                    tileMap[currentPosition + 1] = new Vertex(new Vector2f(currentQuadSize.X * (x + 1), currentQuadSize.Y * y), texCo2);//top right vertex
                    tileMap[currentPosition + 2] = new Vertex(new Vector2f(currentQuadSize.X * (x + 1), currentQuadSize.Y * (y + 1)), texCo3);//bot right vertex
                    tileMap[currentPosition + 3] = new Vertex(new Vector2f(currentQuadSize.X * x, currentQuadSize.Y * (y + 1)), texCo4);//bot left vertex
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
