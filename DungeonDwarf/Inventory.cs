using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;
using System.Xml.Linq;
using System.Xml;

namespace DungeonDwarf
{
    class Inventory
    {
        private RenderWindow win;
        private RectangleShape backGround;
        private Vector2f absoluteSize, relativeOffset, absoluteOffset, targetIconSize, innerMargins, skillSpace;
        private List<Vector2f> marginList = new List<Vector2f>();
        private List<Sprite> swordList = new List<Sprite>(), arrowList = new List<Sprite>(), armorList = new List<Sprite>();
        private Text descriptionText=new Text();
        public bool isShown = false;
        private bool hovering = false;

        public Inventory(RenderWindow _w, Vector2f menuSize, Vector2f _targetIconSize)
        {
            win = _w;
            targetIconSize = _targetIconSize;
            //only sizes up to 100 percent are permitted
            if (menuSize.X > 100f)
                menuSize.X = 100f;
            if (menuSize.Y > 100f)
                menuSize.Y = 100f;

            //initialize desc text
            descriptionText = new Text("", new Font("inventory/ARIAL.TTF"), 30);
            descriptionText.Color = Color.Black;

            //find final size of menu
            absoluteSize = new Vector2f(((float)win.Size.X * menuSize.X) / 100f, ((float)win.Size.Y * menuSize.Y) / 100f);
            
            //calculate position relative to view
            relativeOffset = new Vector2f((win.Size.X - absoluteSize.X) / 2, (win.Size.Y - absoluteSize.Y) / 2);

            //generate menu background
            backGround = new RectangleShape(absoluteSize);
            backGround.FillColor = Color.White;

            #region XMLLoad
            //load skill xml
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("inventory/skillfile.xml");
            //load all skills children
            XmlNode root = xDoc.SelectSingleNode("/skills");
            //loop through 'em
            foreach (XmlNode singleSkillNode in root.ChildNodes)
            {
                string texBasis = "textures/weapons/";
                Console.WriteLine(singleSkillNode.Name);
                Global.SKILL_COUNT++;
                switch (singleSkillNode.Name)
                {
                    case "sword":
                        Global.TEXPATH_LIST_SWORD = new List<string>();
                        Global.DESCRIPTION_LIST_SWORD = new List<string>();
                        texBasis = "textures/weapons/" + singleSkillNode.FirstChild.InnerText;
                        foreach (XmlNode levelNode in singleSkillNode.ChildNodes)
                        {
                            //check whether we are within the correct child
                            if (levelNode.Name == "level"){
                                string texPath = texBasis + levelNode.FirstChild.InnerText;
                                Global.DESCRIPTION_LIST_SWORD.Add(levelNode.LastChild.InnerText);
                                Global.TEXPATH_LIST_SWORD.Add(texPath);
                            }
                        }
                        break;
                    case "arrow":
                        Global.TEXPATH_LIST_ARROW = new List<string>();
                        Global.DESCRIPTION_LIST_ARROW = new List<string>();
                        texBasis = "textures/weapons/"+ singleSkillNode.FirstChild.InnerText;
                        foreach (XmlNode levelNode in singleSkillNode.ChildNodes)
                        {
                            //check whether we are within the correct child
                            if (levelNode.Name == "level")
                            {
                                string texPath = texBasis + levelNode.FirstChild.InnerText;
                                Global.DESCRIPTION_LIST_ARROW.Add(levelNode.LastChild.InnerText);
                                Global.TEXPATH_LIST_ARROW.Add(texPath);
                            }
                        }
                        break;
                    case "armor":
                        Global.TEXPATH_LIST_ARMOR = new List<string>();
                        Global.DESCRIPTION_LIST_ARMOR = new List<string>();
                        texBasis = "textures/weapons/" + singleSkillNode.FirstChild.InnerText;
                        foreach (XmlNode levelNode in singleSkillNode.ChildNodes)
                        {
                            //check whether we are within the correct child
                            if (levelNode.Name == "level"){
                                string texPath = texBasis + levelNode.FirstChild.InnerText;
                                Global.DESCRIPTION_LIST_ARMOR.Add(levelNode.LastChild.InnerText);
                                Global.TEXPATH_LIST_ARMOR.Add(texPath);
                            }
                        }
                        break;
                }

            }
            #endregion
            Vector2f spacePerSkill;
            
            //calc relative position once
            absoluteOffset = Global.CURRENT_WINDOW_ORIGIN + relativeOffset;
            
            //sword skilling
            //divide available space into sections. this only respects relative size, no positioning
            spacePerSkill = new Vector2f((float)absoluteSize.X / (float)Global.SKILL_COUNT, (float)absoluteSize.Y / (float)Global.TEXPATH_LIST_SWORD.Count);
            skillSpace = spacePerSkill;
            //calculate margins in (!) the section for each section
            //sword
            float listCount=Global.DESCRIPTION_LIST_SWORD.Count;
            marginList.Add(new Vector2f(((float)spacePerSkill.X - targetIconSize.X) / 2f, (float)spacePerSkill.Y - targetIconSize.Y));
            //arrow
            listCount = Global.DESCRIPTION_LIST_ARROW.Count;
            marginList.Add(new Vector2f(((float)spacePerSkill.X - targetIconSize.X) / 2f, (float)spacePerSkill.Y - targetIconSize.Y));
            //armor
            listCount = Global.DESCRIPTION_LIST_ARMOR.Count;
            marginList.Add(new Vector2f(((float)spacePerSkill.X - targetIconSize.X) / 2f, (float)spacePerSkill.Y - targetIconSize.Y));


            innerMargins = marginList.ElementAt(0);
            for(int i=0; i<Global.TEXPATH_LIST_SWORD.Count; i++){
                string tPath = Global.TEXPATH_LIST_SWORD.ElementAt(i);
                Texture newTexture = new Texture(tPath);
                Sprite newSprite = new Sprite(newTexture);
                newSprite.Scale = new Vector2f(targetIconSize.X / newTexture.Size.X, targetIconSize.Y/ newTexture.Size.Y);
                swordList.Add(newSprite);
            }

            //arrow skilling
            innerMargins = marginList[1];
            for(int i=0; i<Global.TEXPATH_LIST_ARROW.Count; i++)
            {
                string tPath = Global.TEXPATH_LIST_ARROW.ElementAt(i);
                Texture newTexture = new Texture(tPath);
                Sprite newSprite = new Sprite(newTexture);
                newSprite.Scale = new Vector2f(targetIconSize.X / newTexture.Size.X, targetIconSize.Y/ newTexture.Size.Y);
                arrowList.Add(newSprite);
            }
            
            //armor skilling
            innerMargins = marginList[2];
            for (int i = 0; i < Global.TEXPATH_LIST_ARMOR.Count; i++)
            {
                string tPath = Global.TEXPATH_LIST_ARMOR.ElementAt(i);
                Texture newTexture = new Texture(tPath);
                Sprite newSprite = new Sprite(newTexture);
                newSprite.Scale = new Vector2f(targetIconSize.X / newTexture.Size.X, targetIconSize.Y/ newTexture.Size.Y);
                armorList.Add(newSprite);
            }

            //show a vertical list of all icons, grey out the ones not unlocked

            //still need a way to figure out where to place the icons
            /*
             * init for background, load inventory xml
             * show method. has while loop, only closes on close click or escape;
             *      parameter and return is money for skilling, and an updated global
             *      uses inventory xml to display elements in a grid, maybe with some kind of <row1> sorting
             */
        }

        private void Update()
        {
            absoluteOffset = Global.CURRENT_WINDOW_ORIGIN + relativeOffset;
            backGround.Position = absoluteOffset;
            //calculte sword positions
            for (int i = 0; i < Global.DESCRIPTION_LIST_SWORD.Count; i++)
            {
                swordList[i].Position = new Vector2f(absoluteOffset.X + marginList[0].X + skillSpace.X * 0, absoluteOffset.Y + marginList[0].Y * (i + 1));
            }
            //calculte arrow positions
            for (int i = 0; i < Global.DESCRIPTION_LIST_ARROW.Count; i++)
            {
                arrowList[i].Position = new Vector2f(absoluteOffset.X + marginList[1].X + skillSpace.X * 1, absoluteOffset.Y + marginList[1].Y * (i + 1));
            }
            //calculte armor positions
            for (int i = 0; i < Global.DESCRIPTION_LIST_ARMOR.Count; i++){
                armorList[i].Position = new Vector2f(absoluteOffset.X + marginList[2].X + skillSpace.X * 2, absoluteOffset.Y + marginList[2].Y * (i + 1));
            }
        }

        private void Draw()
        {
            win.Draw(backGround);
            foreach (Sprite s in swordList)
                win.Draw(s);
            foreach (Sprite s in arrowList)
                win.Draw(s);
            foreach (Sprite s in armorList)
                win.Draw(s);
            if (descriptionText.DisplayedString != "")
            {
                FloatRect descSize = descriptionText.GetGlobalBounds();
                descriptionText.Position = new Vector2f(absoluteOffset.X + (absoluteSize.X - descSize.Width) / 2, absoluteOffset.Y + absoluteSize.Y-60f);
                win.Draw(descriptionText);
            }
            
            win.Display();
        }

        private void HoverDetect()
        {
            hovering = false;
            //show description and price, ungrey icon
            Vector2i mousePosition = win.InternalGetMousePosition();
            mousePosition += new Vector2i((int)Global.CURRENT_WINDOW_ORIGIN.X, (int)Global.CURRENT_WINDOW_ORIGIN.Y);
            //calculate sword positions
            for (int i = 0; i < Global.DESCRIPTION_LIST_SWORD.Count; i++){
                FloatRect collisionDetect = new FloatRect(swordList[i].Position.X, swordList[i].Position.Y, targetIconSize.X, targetIconSize.Y);
                if (collisionDetect.Contains(mousePosition.X, mousePosition.Y))
                {
                    hovering = true;
                    descriptionText.DisplayedString = Global.DESCRIPTION_LIST_SWORD[i];
                    swordList[i].Color = new Color(255, 255, 255, 255);
                }
                else if (i >= Global.LEVEL_SWORD)
                {
                    swordList[i].Color = new Color(255, 255, 255, 100);
                }
            }
            //calculate arrow positions
            for (int i = 0; i < Global.DESCRIPTION_LIST_ARROW.Count; i++){
                FloatRect collisionDetect = new FloatRect(arrowList[i].Position.X, arrowList[i].Position.Y, targetIconSize.X, targetIconSize.Y);
                if (collisionDetect.Contains(mousePosition.X, mousePosition.Y)){
                    hovering = true;
                    descriptionText.DisplayedString = Global.DESCRIPTION_LIST_ARROW[i];
                    arrowList[i].Color = new Color(255, 255, 255, 255);
                }
                else if (i >= Global.LEVEL_ARROW)
                {
                    arrowList[i].Color = new Color(255, 255, 255, 100);
                }
            }
            //calculate armor positions
            for (int i = 0; i < Global.DESCRIPTION_LIST_ARMOR.Count; i++){
                FloatRect collisionDetect = new FloatRect(armorList[i].Position.X, armorList[i].Position.Y, targetIconSize.X, targetIconSize.Y);
                if (collisionDetect.Contains(mousePosition.X, mousePosition.Y)){
                    hovering = true;
                    descriptionText.DisplayedString = Global.DESCRIPTION_LIST_ARMOR[i];
                    armorList[i].Color = new Color(255, 255, 255, 255);
                }
                else if (i >= Global.LEVEL_ARMOR)
                {
                    armorList[i].Color = new Color(255, 255, 255, 100);
                }
            }
            if (!hovering)
                descriptionText.DisplayedString = "";
        }

        public void Click(float x, float y){
            //Console.WriteLine("got it");
            //Vector2i translatedMouse=win.MapCoordsToPixel(new Vector2f(x, y), win.GetView());
            float translatedX = x + Global.CURRENT_WINDOW_ORIGIN.X;
            float translatedY = y + Global.CURRENT_WINDOW_ORIGIN.Y;
            //check collision for each sprite
            for (int i = 0; i < Global.DESCRIPTION_LIST_SWORD.Count; i++){
                FloatRect clickRect = new FloatRect(swordList[i].Position.X, swordList[i].Position.Y, targetIconSize.X, targetIconSize.Y);
                if (clickRect.Contains(translatedX, translatedY))
                    Global.LEVEL_SWORD++;
            }
            for (int i = 0; i < Global.DESCRIPTION_LIST_ARROW.Count; i++){
                FloatRect clickRect = new FloatRect(arrowList[i].Position.X, arrowList[i].Position.Y, targetIconSize.X, targetIconSize.Y);
                if (clickRect.Contains(translatedX, translatedY))
                    Global.LEVEL_ARROW++;
            }
            for (int i = 0; i < Global.DESCRIPTION_LIST_ARMOR.Count; i++){
                FloatRect clickRect = new FloatRect(armorList[i].Position.X, armorList[i].Position.Y, targetIconSize.X, targetIconSize.Y);
                if (clickRect.Contains(translatedX, translatedY))
                    Global.LEVEL_ARMOR++;
            }
        }

        /// <summary>
        /// blocks main thread execution!
        /// </summary>
        public void Show(){
            isShown=true;
            while (!Keyboard.IsKeyPressed(Keyboard.Key.E)){
                Update();
                HoverDetect();
                Draw();
                win.DispatchEvents();
            }
            isShown = false;
        }
    }
}