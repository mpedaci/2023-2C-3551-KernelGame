using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Maps;
using TGC.MonoGame.TP.Types.Props;

namespace TGC.MonoGame.TP.Types.Tanks;

public class AIActionTank : ActionTank
{
    public bool perseguir = false;
    private const float VELOCIDAD_MAX = 0.04f;
    private int PathIndex = 0;
    public float BotNum;
    public Map PlaneMap;
    public Tank targetEnemy;
    public bool hasObjective = false;
    public List<Tank> enemies;
    private float angle = 0f;
    private Random random = new Random();
    private float timeout = 0f;

    public AIActionTank(bool isAEnemy, int Index, Map plane)
    {
        PlaneMap = plane;
        BotNum = Index;
        isEnemy = isAEnemy;
    }

    public override void Update(GameTime gameTime, Tank tank)
    {
        var elapsedTime = (float)gameTime.ElapsedGameTime.Milliseconds;
        if (enemies is null)
        {
            if (isEnemy)
            {
                enemies = PlaneMap.Tanks.Where(t => !t.Action.isEnemy).ToList();
            }
            else
            {
                enemies = PlaneMap.Tanks.Where(t => t.Action.isEnemy).ToList();
            }
        }

        if (timeout > 0)
        {
            timeout -= elapsedTime;
            tank.Velocidad = -0.0075f;
            return;
        }
        // AI logic
        if (enemies.Count > 0)
        {
                // Get the closest enemy
                Vector3 targetEnemy = GetClosestEnemy(tank);

                // Move towards the enemy
                MoveTowards(targetEnemy, tank);

                // Check and avoid map props
                // AvoidProps(tank);
                if (Collided)
                {
                    timeout += 2500f;
                    Collided = false;
                }
        }
        
        if(random.Next(100) < 15)
            Shoot(tank);
    }

    private Vector3 GetClosestEnemy(Tank tank)
    {
        Vector3 closestEnemy = Vector3.Zero;
        float closestDistance = float.MaxValue;

        foreach (Tank enemy in enemies)
        {
            float distance = Vector3.Distance(tank.Position, enemy.Position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.Position;
            }
        }

        return closestEnemy;
    }

    private void MoveTowards(Vector3 target, Tank tank)
    {
        // Calculate direction vector
        Vector3 direction = Vector3.Normalize(target - tank.Position);

        // Update tank angle to face the target
        tank.Angle = (float)Math.Atan2(direction.X, direction.Z) + MathHelper.Pi;

        // Apply acceleration
        tank.Velocidad += tank.Acceleration;

        // Limit speed
        tank.Velocidad = MathHelper.Clamp(tank.Velocidad, 0, tank.MaxSpeed);
    }
    
    public void Shoot(Tank tank)
    {
        if (!tank.hasShot)
        {
            var degree = random.Next(40) * 0.2;
            var bulletPosition = tank.Position;
            var yawRadians = MathHelper.ToRadians(0f);
            var pitchRadians = MathHelper.ToRadians((float)degree);
            Vector3 bulletDirection;


            bulletDirection = Vector3.Transform(
                    Vector3.Transform(
                        tank.cannonBone.Transform.Forward,
                        Matrix.CreateFromYawPitchRoll(yawRadians, pitchRadians, 0f)
                    ),
                    Matrix.CreateRotationY(tank.Angle));

            var bullet = new Bullet(
                tank.BulletModel,
                tank.BulletEffect,
                tank.BulletReference,
                Matrix.CreateFromYawPitchRoll(yawRadians, -pitchRadians, 0f),
                Matrix.CreateRotationY(tank.Angle),
                bulletPosition,
                bulletDirection);
            tank.Bullets.Add(bullet);
            tank.hasShot = true;
            tank.shootTime = 1.25f;
        }
    }

    public override void Respawn(Tank tank)
    {
        PathIndex = 0;
        perseguir = false;
    }
}