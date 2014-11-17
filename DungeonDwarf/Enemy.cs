using SFML.Graphics;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/********
 * Simple Enemy which follows the player, it can jump! WOW!
 * 
 * Already pretty great :D (seriously, cool that it can jump :D) but if you 
 * hide exactly below the enemy he just stops above you :(
 * 
 * Doesn't have a KI yet lol. -wulfihm
 * 
 * TODO:    loop for Enemylist in Program.update(); and Program.draw();
 *          fix bug: enemy goes/jumps through ceiling
 *          nicer jumping
 *          cleaner spawning
 *          healthbar
 *          a somewhat decent KI for...(see above)
 *          WEAAAAAAAPOOOO000oooOOOns
 *          ...and whatever there is to do ;)
 * 
 * DONE:
 *          load your enemies into a list
 * 
 * ********/
namespace DungeonDwarf
{
    class Enemy
    {
        public Vector2f enemyPosition, enemySize;
        private Sprite enemySprite;
        private Texture enemyTexture;
        private RenderWindow win;
        private float xScale, yScale;
        private world.TileMap tileMap;
        private float ENEMY_MOVEMENT_SPEED = Global.PLAYER_MOVEMENT_SPEED / 2f;
        private float ENEMY_JUMP_SPEED = Global.PLAYER_JUMP_SPEED / 1.5f;
        private int i = 0;

        public Enemy(String enemyType, RenderWindow _win, Vector2f _enemyPosition, world.TileMap _tileMap, string texturePath, float xScale, float yScale)
        {
            // where the enemy spawns
            enemyPosition.X = _enemyPosition.X;        
            enemyPosition.Y = _enemyPosition.Y-200;

            /***** OLD STUFF, DON'T REMOVE *********
             * switch (enemyType)
            {
                case "enemy1":

                    ENEMY_MOVEMENT_SPEED = ENEMY_MOVEMENT_SPEED / 2f;
                    ENEMY_JUMP_SPEED = ENEMY_JUMP_SPEED * enemySprite.Scale.X;
                    
                    break;
                case "enemy2":

                    ENEMY_MOVEMENT_SPEED = ENEMY_MOVEMENT_SPEED / 4f;
                    ENEMY_JUMP_SPEED = ENEMY_JUMP_SPEED * enemySprite.Scale.X;
                    
                    break;
                default:

            }**************************************/

            enemyTexture = new Texture(texturePath);
            enemySprite = new Sprite(enemyTexture);
            enemySprite.Scale = new Vector2f(xScale, yScale);   // changes the scale of the sprite

            enemySize.X = enemyTexture.Size.X * enemySprite.Scale.X;      // used for tile colliding in method update();
            enemySize.Y = enemyTexture.Size.Y * enemySprite.Scale.Y;      // ---- || ----

            if (xScale != Global.GLOBAL_SCALE)
            {
                ENEMY_MOVEMENT_SPEED = ENEMY_MOVEMENT_SPEED / (xScale * 10);
                ENEMY_JUMP_SPEED = ENEMY_JUMP_SPEED * enemySprite.Scale.X;
            }

            tileMap = _tileMap;  // used for tile colliding in method update();

            win = _win;   // used for the draw function
        }

        public void draw()
        {
            enemySprite.Position = enemyPosition;
            win.Draw(enemySprite);
        }

        public void update(Vector2f playerPosition)
        {

            // simple movement logic
            if (!tileMap.Collides(enemyPosition, enemySize))    // check if enemy collides with tiles, if true dont move at all
            { 
                //move to the left in direction of the player
                if (enemyPosition.X > playerPosition.X || enemyPosition.X - enemySize.X > playerPosition.X)     
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(-ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X -= ENEMY_MOVEMENT_SPEED;

                //move to the right in direction of the player
                if (enemyPosition.X < playerPosition.X || enemyPosition.X + enemySize.X < playerPosition.X)      
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X += ENEMY_MOVEMENT_SPEED;       
            }

            // Gravity 
            if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                enemyPosition.Y += Global.GLOBAL_GRAVITY;

            // enemy jumps if player is unreacheable and if enemy is colliding with tiles to the left or the right of the enemy
            // multiple of ENEMY_MOVEMENT_SPEED for CheckNextCollide so that it doesn't look like that the enemy is crawling up the wall i.e. the enemy is jumping some steps before the wall
            // jumping is endless as of now
            if (enemyPosition.X != playerPosition.X && !tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(0f,-ENEMY_JUMP_SPEED)) && 
                                                        (tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED, 0f)) ||
                                                        tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(-ENEMY_MOVEMENT_SPEED, 0f))))
                enemyPosition.Y -= ENEMY_JUMP_SPEED;



            CorrectYPosLogic();
        }

        // borrowed from Player.cs and adjusted for Enemy.cs, thanks Daniel lol ;)
        public void CorrectYPosLogic() 
        {
            //get difference between player left bottom and ground top
            float yDiffLeft = enemyPosition.Y + enemySize.Y - tileMap.MinY(enemyPosition);
            //reset player position if he is just above ground
            //even though leaving out yDiffRigh checking seems illogical, it removes the jumping bug
            if (yDiffLeft > -10 && yDiffLeft < 60 && yDiffLeft != 0)
            {
                //avoid getting put above the game
                if (enemyPosition.Y == -50)
                    enemyPosition.Y = 0;
                else
                {
                    //Console.WriteLine(yDiffLeft);
                    //get current left and right highest positions
                    float leftTopPosition = tileMap.MinY(enemyPosition);
                    float rightTopPosition = tileMap.MinY(new Vector2f(enemyPosition.X - 1 + enemySize.X, enemyPosition.Y));
                    float targetPosition;

                    //use the block higher up 
                    if (leftTopPosition > rightTopPosition)
                        targetPosition = rightTopPosition;
                    else
                        targetPosition = leftTopPosition;
                    enemyPosition.Y = targetPosition - enemySize.Y;
                }
                //write changed position
                enemySprite.Position = enemyPosition;
            }
        }
    }
}
