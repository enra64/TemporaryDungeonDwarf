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
        private Vector2f absoluteSize, relativeOffset, absoluteOffset, targetIconSize;
        private List<Sprite> swordList = new List<Sprite>(), arrowList = new List<Sprite>(), armorList = new List<Sprite>();
        public bool isShown = false;

        public Inventory(RenderWindow _w, Vector2f menuSize, Vector2f _targetIconSize)
        {
            win = _w;
            targetIconSize = _targetIconSize;
            //only sizes up to 100 percent are permitted
            if (menuSize.X > 100f)
                menuSize.X = 100f;
            if (menuSize.Y > 100f)
                menuSize.Y = 100f;

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
            Vector2f margins;

            //calc relative position once
            absoluteOffset = Global.CURRENT_WINDOW_ORIGIN + relativeOffset;
            
            //sword skilling
            //divide available space into sections. this only respects relative size, no positioning
            spacePerSkill = new Vector2f((float)absoluteSize.X / (float)Global.SKILL_COUNT, (float)absoluteSize.Y / (float)Global.TEXPATH_LIST_SWORD.Count);
            margins = new Vector2f((spacePerSkill.X - targetIconSize.X) / 2, 20f);
            for(int i=0; i<Global.TEXPATH_LIST_SWORD.Count; i++){
                string tPath = Global.TEXPATH_LIST_SWORD.ElementAt(i);
                Texture newTexture = new Texture(tPath);
                Sprite newSprite = new Sprite(newTexture);
                newSprite.Scale = new Vector2f(targetIconSize.X / newTexture.Size.X, targetIconSize.Y/ newTexture.Size.Y);
                newSprite.Position = new Vector2f(0 + margins.X, absoluteOffset.Y + margins.Y*(i+1));
                swordList.Add(newSprite);
            }

            //arrow skilling
            margins = new Vector2f((spacePerSkill.X - targetIconSize.X) / 2, 
                (spacePerSkill.Y - targetIconSize.Y*Global.TEXPATH_LIST_ARROW.Count) / (Global.TEXPATH_LIST_ARROW.Count+1));//hardcoded, pot dangerous
            for(int i=0; i<Global.TEXPATH_LIST_ARROW.Count; i++)
            {
                string tPath = Global.TEXPATH_LIST_ARROW.ElementAt(i);
                Texture newTexture = new Texture(tPath);
                Sprite newSprite = new Sprite(newTexture);
                newSprite.Scale = new Vector2f(targetIconSize.X / newTexture.Size.X, targetIconSize.Y/ newTexture.Size.Y);
                newSprite.Position = new Vector2f(absoluteOffset.X + margins.X*2, absoluteOffset.Y + margins.Y*(i+1));
                arrowList.Add(newSprite);
            }
            
            //armor skilling
            margins = new Vector2f((spacePerSkill.X - targetIconSize.X) / 2,
                (spacePerSkill.Y - targetIconSize.Y * Global.TEXPATH_LIST_ARMOR.Count) / (Global.TEXPATH_LIST_ARMOR.Count + 1));//hardcoded, pot dangerous
            for (int i = 0; i < Global.TEXPATH_LIST_ARMOR.Count; i++)
            {
                string tPath = Global.TEXPATH_LIST_ARMOR.ElementAt(i);
                Texture newTexture = new Texture(tPath);
                Sprite newSprite = new Sprite(newTexture);
                newSprite.Scale = new Vector2f(targetIconSize.X / newTexture.Size.X, targetIconSize.Y/ newTexture.Size.Y);
                newSprite.Position = new Vector2f(absoluteOffset.X + margins.X*3, absoluteOffset.Y + margins.Y*(i+1));
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
            win.Display();
        }

        /// <summary>
        /// blocks main thread execution!
        /// </summary>
        public void Show(){
            isShown=true;
            while (!Keyboard.IsKeyPressed(Keyboard.Key.E)){
                Update();
                Draw();
                Console.WriteLine("showing");
                win.DispatchEvents();
            }
            isShown = false;
        }
    }
}
