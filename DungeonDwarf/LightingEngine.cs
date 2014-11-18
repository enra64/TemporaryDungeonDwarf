using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

/*
 * What could go wrong, anyway
 */
namespace DungeonDwarf
{
    class LightingEngine
    {
        private RenderWindow win;
        private VertexArray lightMap;
        private Vector2f quadSize, quadCount;
        private RenderStates renderStates = RenderStates.Default;
        private Color dark = new Color(50, 50, 50, 50);
        private List<float[]> lightList=new List<float[]>();

        public LightingEngine(RenderWindow _w)
        {
            win = _w;
            quadCount = new Vector2f(100, 100);
            /*
             * Idea: create a vertex array containing only colors with a low alpha, and lower the alpha for each source of light.
             * might look kinda shitty.
             */
            //define quad size
            quadSize = new Vector2f((float)win.Size.X / quadCount.X, (float)win.Size.Y / quadCount.Y);
            //begin creating the vertex array
            //vertex array with quads, a thousand for starting
            lightMap = new VertexArray(PrimitiveType.Quads, 100 * 100 * 4);
            for (uint y = 0; y < quadCount.Y; y++)
            {
                for (uint x = 0; x < quadCount.X; x++){
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * (uint)quadCount.X) + x);
                    lightMap[currentPosition + 0] = new Vertex(new Vector2f(quadSize.X * x, quadSize.Y * y), dark);//top left vertex
                    lightMap[currentPosition + 1] = new Vertex(new Vector2f(quadSize.X * (x + 1), quadSize.Y * y), dark);//top right vertex
                    lightMap[currentPosition + 2] = new Vertex(new Vector2f(quadSize.X * (x + 1), quadSize.Y * (y + 1)), dark);//bot right vertex
                    lightMap[currentPosition + 3] = new Vertex(new Vector2f(quadSize.X * x, quadSize.Y * (y + 1)), dark);//bot left vertex
                }
            }
        }

        //only add a lightsource to the list, we need to calculate the circle later
        /// <summary>
        /// everything that wants to emit light must call this for each update, because the list gets consumed
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        public void addLightSource(float x, float y, float size)
        {
            lightList.Add(new float[] { x, y, size });
        }

        private void calculateLightCircles(Vector2f center, float size)
        {
            /*
             * scale radius with size
             * calculate tiles from x,y
             * increase darking by distance from xy
             * need to rotate vector
             */
            CircleShape c=new CircleShape(size);
            c.Position=center;
            Transform rotation=new Transform();
            Vector2f directionalCheck=new Vector2f(1,1);
            for(int _size=1;_size<size;_size++){
                directionalCheck*=_size;
                for (int rot = 1; rot < 361; rot+=3 ){
                    rotation.Rotate(rot, center);
                    Vector2f currentCheck = directionalCheck.;
                    Console.WriteLine(currentCheck);
                }
            }
        }

        public void Update()
        {
            for (int i = 0; i < lightList.Count;i++ )
            {
                float[] ls = lightList.ElementAt(i);
                calculateLightCircles(new Vector2f(ls[0], ls[1]), ls[2]);
                lightList.RemoveAt(i);
            }
            return;
            for (uint y = 0; y < quadCount.Y; y++)
            {
                for (uint x = 0; x < quadCount.X; x++)
                {
                    //because: 4 vertexes/quad * (current y times how many x per view) * x
                    uint currentPosition = 4 * ((y * (uint)quadCount.X) + x);
                    lightMap[currentPosition + 0] = new Vertex(new Vector2f(quadSize.X * x, quadSize.Y * y), dark);//top left vertex
                    lightMap[currentPosition + 1] = new Vertex(new Vector2f(quadSize.X * (x + 1), quadSize.Y * y), dark);//top right vertex
                    lightMap[currentPosition + 2] = new Vertex(new Vector2f(quadSize.X * (x + 1), quadSize.Y * (y + 1)), dark);//bot right vertex
                    lightMap[currentPosition + 3] = new Vertex(new Vector2f(quadSize.X * x, quadSize.Y * (y + 1)), dark);//bot left vertex
                }
            }
        }

        /// <summary>
        /// draw for you imaginationless people
        /// </summary>
        public void LetThereBeLight()
        {
            lightMap.Draw(win, renderStates);
        }
    }
}
