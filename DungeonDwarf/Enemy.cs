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
 ********/
namespace DungeonDwarf
{
    class Enemy
    {
        public Vector2f enemyPosition, enemySize;
        private Sprite enemySprite;
        private RenderWindow win;
        private world.TileMap tileMap;
        private float ENEMY_MOVEMENT_SPEED = Global.PLAYER_MOVEMENT_SPEED / 2f;

        public Enemy(RenderWindow _win, Vector2f _enemyPosition, world.TileMap _tileMap)
        {
            enemyPosition.X = _enemyPosition.X+400f;
            enemyPosition.Y = _enemyPosition.Y-50;

            tileMap = _tileMap;
            win = _win;

            Texture enemyTexture = new Texture("textures/enemies/zeroEnemy.png");
            enemySprite = new Sprite(enemyTexture);
            enemySize.X = enemyTexture.Size.X;
            enemySize.Y = enemyTexture.Size.Y;
        }

        public void draw()
        {
            enemySprite.Position = enemyPosition;
            win.Draw(enemySprite);
        }

        public void update(Vector2f playerPosition)
        {
            // the following code is for the enemy to move in direction of the player (only on the X axis i.e left/right)
            
            if (!tileMap.Collides(enemyPosition, enemySize))    // check if enemy collides with tiles, if true dont do anything
            {
                if (enemyPosition.X > playerPosition.X)      //move to the left in direction of the player
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(-ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X -= (ENEMY_MOVEMENT_SPEED);


                if (enemyPosition.X < playerPosition.X)      //move to the right in direction of the player
                    if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED, 0f)))
                        enemyPosition.X += (ENEMY_MOVEMENT_SPEED);

                // Gravity 
                if (!tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(0f, Global.GLOBAL_GRAVITY)))
                    enemyPosition.Y += Global.GLOBAL_GRAVITY;

            }

            // enemy jumps if player is unreacheable and if enemy is colliding with tiles
            // multiple of ENEMY_MOVEMENT_SPEED so that it doesn't look like that the enemy is crawling up the wall 
            // i.e. the enemy is jumping some steps before the wall
            if (tileMap.CheckNextCollide(enemyPosition, enemySize, new Vector2f(ENEMY_MOVEMENT_SPEED * 4, 0f)))      
                    enemyPosition.Y -= Global.PLAYER_JUMP_SPEED / 1.5f;
        }

    }
}
