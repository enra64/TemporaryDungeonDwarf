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
 * TODO: nicer jumping, cleaner spawning, healthbar, a somewhat decent KI for...(see above), WEAAAAAAAPOOOO000oooOOOns,...and whatever there is to do ;)
 * 
 ********/
namespace DungeonDwarf
{
    class Enemy
    {
        public Vector2f enemyPosition, enemySize;
        private Sprite enemySprite;
        private Texture enemyTexture = new Texture("textures/enemies/zeroEnemy.png");
        private RenderWindow win;
        private world.TileMap tileMap;
        private float ENEMY_MOVEMENT_SPEED = Global.PLAYER_MOVEMENT_SPEED / 2f;
        private float ENEMY_JUMP_SPEED = Global.PLAYER_JUMP_SPEED / 1.5f;

        public Enemy(RenderWindow _win, Vector2f _enemyPosition, String enemyType, world.TileMap _tileMap)
        {
            // where the enemy spawns
            enemyPosition.X = _enemyPosition.X;        
            enemyPosition.Y = _enemyPosition.Y-400;

            // following code determines which enemy to spawn
            switch (enemyType)
            {
                case "enemy1":
                    enemyTexture = new Texture("textures/world/earthTileTop.png");
                    enemySprite = new Sprite(enemyTexture);
                    enemySprite.Scale = new Vector2f(0.3f, 0.3f);   // changes the scale of the sprite

                    enemySize.X = enemyTexture.Size.X * enemySprite.Scale.X;      // used for tile colliding in method update();
                    enemySize.Y = enemyTexture.Size.Y * enemySprite.Scale.Y;      // ---- || ----
                    ENEMY_MOVEMENT_SPEED = ENEMY_MOVEMENT_SPEED / 2f;
                    ENEMY_JUMP_SPEED = ENEMY_JUMP_SPEED * enemySprite.Scale.X;
                    
                    break;
                case "enemy2":
                    enemyTexture = new Texture("textures/world/earthTile.png");
                    enemySprite = new Sprite(enemyTexture);
                    enemySprite.Scale = new Vector2f(0.4f, 0.4f);   // changes the scale of the sprite

                    enemySize.X = enemyTexture.Size.X * enemySprite.Scale.X;      // used for tile colliding in method update();
                    enemySize.Y = enemyTexture.Size.Y * enemySprite.Scale.Y;      // ---- || ----
                    ENEMY_MOVEMENT_SPEED = ENEMY_MOVEMENT_SPEED / 4f;
                    ENEMY_JUMP_SPEED = ENEMY_JUMP_SPEED * enemySprite.Scale.X;
                    
                    break;
                default:
                    enemySprite = new Sprite(enemyTexture);
                    enemySprite.Scale = new Vector2f(Global.GLOBAL_SCALE, Global.GLOBAL_SCALE);   // changes the scale of the sprite

                    enemySize.X = enemyTexture.Size.X * enemySprite.Scale.X;      // used for tile colliding in method update();
                    enemySize.Y = enemyTexture.Size.Y * enemySprite.Scale.Y;      // ---- || ----
                    break;
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
            
            // the following code is for the enemy to move in direction of the player (only on the X axis i.e left/right)

            if (!tileMap.Collides(enemyPosition, enemySize))    // check if enemy collides with tiles, if true dont move at all
            { 
                //move to the left in direction of the player
                if (enemyPosition.X > playerPosition.X || enemyPosition.X + enemySize.X > playerPosition.X)     
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(-ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X -= ENEMY_MOVEMENT_SPEED;

                //move to the right in direction of the player
                if (enemyPosition.X < playerPosition.X || enemyPosition.X + enemySize.X < playerPosition.X)      
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X += ENEMY_MOVEMENT_SPEED;

                // Gravity 
                if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    enemyPosition.Y += Global.GLOBAL_GRAVITY;
            
            }

            // enemy jumps if player is unreacheable and if enemy is colliding with tiles to the left or the right of the enemy
            // multiple of ENEMY_MOVEMENT_SPEED for CheckNextCollide so that it doesn't look like that the enemy is crawling up the wall i.e. the enemy is jumping some steps before the wall
            // jumping is endless as of now
            if (tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED / enemySprite.Scale.X * 2f, 0f)) || tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(-ENEMY_MOVEMENT_SPEED / enemySprite.Scale.X * 2f, 0f)))
                enemyPosition.Y -= ENEMY_JUMP_SPEED;
        }

    }
}
