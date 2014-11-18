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
        private Color dark = new Color(0, 0, 0, 200);
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
            c.Position = new Vector2f(center.X + c.Radius, center.Y + c.Radius);
            long counter=0;
            for (uint y = 0; y < quadCount.Y; y++){
                for (uint x = 0; x < quadCount.X; x++){
                    uint currentPosition = 4 * ((y * (uint)quadCount.X) + x);
                    for(uint i=0; i<c.GetPointCount();i+=2){
                        FloatRect quadCheck=new FloatRect(quadSize.X * x, quadSize.Y * y, quadSize.X * (x + 1), quadSize.Y * (y + 1));
                        Vector2f pointCheck=c.GetPoint(i);
                        if(quadCheck.Contains(pointCheck.X, pointCheck.Y))
                            SetQuadBrightness(currentPosition, 0);
                    }
                }
            }
        }

        private void SetQuadBrightness(uint currentPosition, byte brightness)
        {
            //create new color
            Color newColor = new Color(0, 0, 0, brightness);
            //save old positions
            Vector2f texCo1, texCo2, texCo3, texCo4;
            texCo1 = lightMap[currentPosition + 0].TexCoords;
            texCo2 = lightMap[currentPosition + 1].TexCoords;
            texCo3 = lightMap[currentPosition + 2].TexCoords;
            texCo4 = lightMap[currentPosition + 3].TexCoords;
            //write new
            lightMap[currentPosition + 0] = new Vertex(texCo1, newColor);//top left vertex
            lightMap[currentPosition + 1] = new Vertex(texCo2, newColor);//top right vertex
            lightMap[currentPosition + 2] = new Vertex(texCo3, newColor);//bot right vertex
            lightMap[currentPosition + 3] = new Vertex(texCo4, newColor);//bot left vertex
        }

        public void Update()
        {
            for (int i = 0; i < lightList.Count;i++ )
            {
                float[] ls = lightList.ElementAt(i);
                calculateLightCircles(new Vector2f(ls[0], ls[1]), ls[2]);
                lightList.RemoveAt(i);
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
