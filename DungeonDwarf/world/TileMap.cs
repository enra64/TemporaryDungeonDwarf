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
        public List<int[]> spawnPoints=new List<int[]>();
        public List<Vector2f> torchPositions = new List<Vector2f>();

        //tile specific variables
        private int[,] tileTypes;
        private bool[,] Collidable;
        
        //single variable: how many tiles below the max y value we _know_ about shall we draw
        private uint yInterpolationDuration = 5;

        private Texture textureMap=new Texture("textures/world/spritemap.png");
        //contains target size for quads
        private Vector2f idealQuadSize, currentQuadSize;
        //vertex drawing stuff
        private VertexArray tileMap;
        private RenderStates renderStates = RenderStates.Default;
        private const int EARTHTILEOFFSET=100, EARTHTOPTILEOFFSET=200, AIROFFSET=0, LAVA1OFFSET=300, LAVA2OFFSET=500, LAVA1TOPOFFSET=400, LAVA2TOPOFFSET=600;
        private Lighting lightEngine;

        public TileMap(RenderWindow _w, Lighting _l, Vector2u _tileAmount, string _levelLocation){
            win = _w;
            lightEngine = _l;
            tileAmount = _tileAmount;
            tileTypes = new int[tileAmount.X, tileAmount.Y];
            Collidable = new bool[tileAmount.X, tileAmount.Y];

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

            //fill the tile types
            fillTileTypeArray(_levelLocation);

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
                    switch (tileType){
                        case Global.EARTH_TILE:
                            if (y < tileAmount.Y)
                                Collidable[x, y] = true;
                            xOffset = EARTHTILEOFFSET;
                            break;
                        case Global.LAVA_TOP_TILE:
                            if (y < tileAmount.Y)
                                Collidable[x, y] = true;
                            xOffset = LAVA1TOPOFFSET;
                            break;
                        case Global.LAVATILE:
                            if (y < tileAmount.Y)
                                Collidable[x, y] = true;
                            xOffset = LAVA1OFFSET;
                            break;
                        case Global.EARTH_TOP_TILE:
                            xOffset = EARTHTOPTILEOFFSET;
                            if (y < tileAmount.Y)
                                Collidable[x, y] = true;
                            break;
                        case Global.SPAWNTILE_1:
                            xOffset = AIROFFSET;
                            spawnPoints.Add(new int[]{(int)x, (int)y, Global.SPAWNTILE_1});
                            if (y < tileAmount.Y)
                                Collidable[x, y] = false;
                            break;
                        case Global.SPAWNTILE_2:
                            xOffset = AIROFFSET;
                            spawnPoints.Add(new int[] { (int)x, (int)y, Global.SPAWNTILE_2 });
                            if (y < tileAmount.Y)
                                Collidable[x, y] = false;
                            break;
                        case Global.SPAWNTILE_3:
                            xOffset = AIROFFSET;
                            spawnPoints.Add(new int[] { (int)x, (int)y, Global.SPAWNTILE_3 });
                            if (y < tileAmount.Y)
                                Collidable[x, y] = false;
                            break;
                        default:
                            xOffset = AIROFFSET;
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

        public List<Vector2f> TilesOfType(int type)
        {
            List<Vector2f> returnList=new List<Vector2f>();
            //draw vertexicesarrays. y+5 for interpolating ground
            for (uint y = 0; y < tileAmount.Y + yInterpolationDuration; y++)
            {
                for (uint x = 0; x < tileAmount.X; x++)
                {
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * tileAmount.X) + x);
                    if (tileTypes[x, y] == type)
                        returnList.Add(new Vector2f(currentQuadSize.X * x + currentQuadSize.X / 2f , currentQuadSize.Y * y + currentQuadSize.Y / 2f));
                }
            }
            return returnList;
        }

        /// <summary>
        /// Returns the tile type defined in global (see GetCurrentTile for
        /// getting tile at x, y position)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetTileType(int x, int y){
            if(x!=-1&&y!=-1)
                return tileTypes[x, y];
            else return Global.AIR_TILE;
        }

        /// <summary>
        /// Returns whether the given tile collides (see GetCurrentTile)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool GetTileCollidable(int x, int y)
        {
            return Collidable[x, y];
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

        private Vector2f GetXY(int x, int y)
        {
            return new Vector2f(currentQuadSize.X * x, currentQuadSize.Y * y);
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
                LAVATOP = s.Element("lavaTop").Value,
                LAVA = s.Element("lava").Value,
                S1 = s.Element("spawn1").Value,
                S2 = s.Element("spawn2").Value,
                S3 = s.Element("spawn3").Value,
                TORCH = s.Element("torch").Value,
                AIR = s.Element("air").Value
            }).FirstOrDefault();
            //get strings from array, removing all linebreaks
            string earth = query.EARTH.Replace("\n", String.Empty);
            string earthtop = query.EARTHTOP.Replace("\n", String.Empty);
            string air = query.AIR.Replace("\n", String.Empty);
            string lava = query.LAVA.Replace("\n", String.Empty);
            string lavatop = query.LAVATOP.Replace("\n", String.Empty);
            string spawn1 = query.S1.Replace("\n", String.Empty);
            string spawn2 = query.S2.Replace("\n", String.Empty);
            string spawn3 = query.S3.Replace("\n", String.Empty);
            string torch = query.TORCH.Replace("\n", String.Empty);
            //get byte arrays from strings
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] earthArray = enc.GetBytes(earth);
            byte[] earthTopArray = enc.GetBytes(earthtop);
            byte[] airArray = enc.GetBytes(air);
            byte[] lavaArray = enc.GetBytes(lava);
            byte[] lavaTopArray = enc.GetBytes(lavatop);
            byte[] spawn1Array = enc.GetBytes(spawn1);
            byte[] spawn2Array = enc.GetBytes(spawn2);
            byte[] spawn3Array = enc.GetBytes(spawn3);
            byte[] torchArray = enc.GetBytes(torch);
            //get tiles from byte arrays
            //somewhat dirty implementation, we can only define tile priority
            for(int y=0;y<tileAmount.Y;y++){
                for (int x = 0; x < tileAmount.X; x++){
                    long oneDimensionalArrayPosition=y * tileAmount.X + x;
                    //default to air tile
                    tileTypes[x, y] = Global.AIR_TILE;
                    //decide on tiles with higher priority, low to high
                    if (earthArray[oneDimensionalArrayPosition] == 49)//ASCII one: earth
                        tileTypes[x, y] = Global.EARTH_TILE;
                    if (earthTopArray[oneDimensionalArrayPosition] == 49)//earth top
                        tileTypes[x, y] = Global.EARTH_TOP_TILE;
                    if (lavaArray[oneDimensionalArrayPosition] == 49)//ASCII one: lava
                        tileTypes[x, y] = Global.LAVATILE;
                    if (lavaTopArray[oneDimensionalArrayPosition] == 49)//ASCII one: lavatop
                        tileTypes[x, y] = Global.LAVA_TOP_TILE;
                    //spawns
                    if (spawn1Array[oneDimensionalArrayPosition] == 49)//ASCII one: spawn 1
                        tileTypes[x, y] = Global.SPAWNTILE_1;
                    if (spawn2Array[oneDimensionalArrayPosition] == 49)//ASCII one: spawn 2
                        tileTypes[x, y] = Global.SPAWNTILE_2;
                    if (spawn3Array[oneDimensionalArrayPosition] == 49)//ASCII one: spawn 3
                        tileTypes[x, y] = Global.SPAWNTILE_3;
                    //torches, handled seperatly
                    if (torchArray[oneDimensionalArrayPosition] == 49){//ASCII one: spawn 3
                        torchPositions.Add(GetXY(x, y));
                    }
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

        public float MinY(Vector2f position)
        {
            int[] tilePosition = GetCurrentTile(new Vector2f(position.X, position.Y));
            if (tilePosition[0] < 0)
                return -1;
            for (int y = tilePosition[1]; y < tileAmount.Y; y++)
            {
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
            Global.GLOBAL_SCALE = currentQuadSize.X / idealQuadSize.X;//returns a smaller value
        }

        public void Update()
        {
            //get x we are at
            uint xOffset = (uint)(Global.CURRENT_WINDOW_ORIGIN.X / currentQuadSize.X);
            Color lavaColor=new Color(207, 128, 16);
            for (uint y = 0; y < tileAmount.Y; y++)
            {
                for (uint x = xOffset; x < tileAmount.X; x++)
                {
                    if(tileTypes[x, y]==Global.LAVA_TOP_TILE || tileTypes[x, y]==Global.LAVATILE)
                        lightEngine.AddLight(new Vector2f(Global.CURRENT_WINDOW_ORIGIN.X + ((x - xOffset) * currentQuadSize.X), y * currentQuadSize.Y), currentQuadSize, new Vector2f(1.5f, 1.5f), lavaColor);
                }
            }
        }

        public List<Vector2f> GetCurrentTorches()
        {
            List<Vector2f> returnList=new List<Vector2f>();
            uint xOffset = (uint)(Global.CURRENT_WINDOW_ORIGIN.X / currentQuadSize.X);
            foreach(Vector2f v in torchPositions)
                if (v.X < Global.CURRENT_WINDOW_ORIGIN.X + win.Size.X && v.X > Global.CURRENT_WINDOW_ORIGIN.X){
                    returnList.Add(v);
                }
            return returnList;
        }

        public void AnimationUpdate()
        {
            for (uint y = 0; y < tileAmount.Y + yInterpolationDuration; y++)
            {
                for (uint x = 0; x < tileAmount.X; x++)
                {
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * tileAmount.X) + x);
                    //initialize updated texture coords
                    Vector2f texCo1, texCo2, texCo3, texCo4;
                    //enable interpolation:
                    int tileType;
                    if (y < tileAmount.Y)
                        tileType = tileTypes[x, y];
                    else
                        tileType = tileTypes[x, tileAmount.Y - 1];
                    if (tileType == Global.LAVATILE)
                    {
                        //+0 should contain pure x offset
                        texCo1 = tileMap[currentPosition + 0].TexCoords;
                        Vector2f newOffset=new Vector2f(0,0);
                        //swap offset
                        if (texCo1.X == LAVA1OFFSET)
                            newOffset.X = LAVA2OFFSET-LAVA1OFFSET;
                        else
                            newOffset.X = LAVA1OFFSET-LAVA2OFFSET;
                        texCo1 = tileMap[currentPosition + 0].TexCoords + newOffset;
                        texCo2 = tileMap[currentPosition + 1].TexCoords + newOffset;
                        texCo3 = tileMap[currentPosition + 2].TexCoords + newOffset;
                        texCo4 = tileMap[currentPosition + 3].TexCoords + newOffset;
                    }
                    else if (tileType == Global.LAVA_TOP_TILE)
                    {
                        //+0 should contain pure x offset
                        texCo1 = tileMap[currentPosition + 0].TexCoords;
                        Vector2f newOffset = new Vector2f(0, 0);
                        //swap offset
                        if (texCo1.X == LAVA1TOPOFFSET)
                            newOffset.X = LAVA2TOPOFFSET - LAVA1TOPOFFSET;
                        else
                            newOffset.X = LAVA1TOPOFFSET - LAVA2TOPOFFSET;
                        texCo1 = tileMap[currentPosition + 0].TexCoords + newOffset;
                        texCo2 = tileMap[currentPosition + 1].TexCoords + newOffset;
                        texCo3 = tileMap[currentPosition + 2].TexCoords + newOffset;
                        texCo4 = tileMap[currentPosition + 3].TexCoords + newOffset;
                    }
                    else{
                        texCo1 = tileMap[currentPosition + 0].TexCoords;
                        texCo2 = tileMap[currentPosition + 1].TexCoords;
                        texCo3 = tileMap[currentPosition + 2].TexCoords;
                        texCo4 = tileMap[currentPosition + 3].TexCoords;
                    }
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
