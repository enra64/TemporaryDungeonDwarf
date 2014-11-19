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
 * TODO:    nicer jumping
 *          cleaner spawning
 *          healthbar
 *          a somewhat decent KI for...(see above)
 *          WEAAAAAAAPOOOO000oooOOOns
 *          ...and whatever there is to do ;)
 * 
 * DONE:
 *          load your enemies into a list
 *          fix bug: enemy goes/jumps through ceiling
 *          loop for Enemylist in Program.update(); and Program.draw();
 * ********/
namespace DungeonDwarf
{
    class Enemy
    {
        public Vector2f enemyPosition, enemySize;
        private Sprite enemySprite;
        private Texture enemyTexture;
        private RenderWindow win;
        //private float xScale, yScale;
        private world.TileMap tileMap;
        private float ENEMY_MOVEMENT_SPEED = Global.PLAYER_MOVEMENT_SPEED / 2f;
        private float ENEMY_JUMP_SPEED = Global.PLAYER_JUMP_SPEED / 1.5f;
        private int i = 0;

        public Enemy(String _enemyType, RenderWindow _win, Vector2f _playerPosition, world.TileMap _tileMap)
        {
            float xScale, yScale, _jumpspeed, _movementspeed;
            string texturePath;
            //moved enemy distinguishment here, b/c 7 constructor arguments and somehow the commit was broken
            switch (_enemyType)
            {
                case "zeroEnemy":
                    texturePath="textures/enemies/zeroEnemy.png";
                    xScale=Global.GLOBAL_SCALE;
                    yScale=Global.GLOBAL_SCALE;
                    _jumpspeed = Global.PLAYER_JUMP_SPEED / 1.5f;
                    _movementspeed = Global.PLAYER_MOVEMENT_SPEED / 5f;
                    break;
                case "enemy1":
                    texturePath="textures/world/earthTileTop.png";
                    xScale=.2f;
                    yScale=.2f;
                    _jumpspeed = Global.PLAYER_JUMP_SPEED / 2f;
                    _movementspeed = Global.PLAYER_MOVEMENT_SPEED / 8f;
                    break;
                //do this when case is enemy2 or if no other case fit
                default: case "enemy2":
                    texturePath = "textures/world/earthTile.png";
                    xScale=Global.GLOBAL_SCALE;
                    yScale=Global.GLOBAL_SCALE;
                    _jumpspeed = Global.PLAYER_JUMP_SPEED / 2.5f;
                    _movementspeed = Global.PLAYER_MOVEMENT_SPEED / 10f;
                    break;
            }

            // where the enemy spawns
            enemyPosition.X = _playerPosition.X-200;        
            enemyPosition.Y = _playerPosition.Y-200;

            enemyTexture = new Texture(texturePath);
            enemySprite = new Sprite(enemyTexture);
            enemySprite.Scale = new Vector2f(xScale, yScale);   // changes the scale of the sprite
            enemySize.X = enemyTexture.Size.X * enemySprite.Scale.X;      // used for tile colliding in method update();
            enemySize.Y = enemyTexture.Size.Y * enemySprite.Scale.Y;      // ---- || ----

            //turned these around btw
            ENEMY_JUMP_SPEED = _jumpspeed;
            ENEMY_MOVEMENT_SPEED = _movementspeed;

            tileMap = _tileMap;  // used for tile colliding in method update();

            win = _win;   // used for the draw function
        }

        public void Draw()
        {
            enemySprite.Position = enemyPosition;
            win.Draw(enemySprite);
        }

        public void Update(Vector2f _playerPosition, Vector2f _playerSize)
        {
            // simple movement logic
            if (!tileMap.Collides(enemyPosition, enemySize))    // check if enemy collides with tiles, if true dont move at all
            { 
                //move to the left in direction of the player
                if (enemyPosition.X > _playerPosition.X && enemyPosition.X - _playerSize.X > _playerPosition.X - enemySize.X)
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(-ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X -= ENEMY_MOVEMENT_SPEED;

                //move to the right in direction of the player
                if (enemyPosition.X < _playerPosition.X)
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X += ENEMY_MOVEMENT_SPEED;       
            }

            // Gravity 
            if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                enemyPosition.Y += Global.GLOBAL_GRAVITY;

            // enemy jumps if player is unreacheable and if enemy is colliding with tiles to the left or the right of the enemy
            // multiple of ENEMY_MOVEMENT_SPEED for CheckNextCollide so that it doesn't look like that the enemy is crawling up the wall i.e. the enemy is jumping some steps before the wall
            // jumping is endless as of now
            if (enemyPosition.X != _playerPosition.X && !tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(0f,-ENEMY_JUMP_SPEED)) && 
                                                        (tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED, 0f)) ||
                                                        tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(-ENEMY_MOVEMENT_SPEED, 0f))))
                enemyPosition.Y -= ENEMY_JUMP_SPEED;

            CorrectYPosLogic();
        }

        // borrowed from Player.cs and adjusted for Enemy.cs, thanks Daniel lol ;) pff daniel xD thats my work :D
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
