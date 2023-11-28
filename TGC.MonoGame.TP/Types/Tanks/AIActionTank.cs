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
    private const float VELOCIDAD_MAX = 0.021f;
    private int PathIndex = 0;
    public float BotNum;
    public Map PlaneMap;
    public Tank targetEnemy;
    public bool hasObjective = false;
    public List<Tank> enemies;
    private float angle = 0f;

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

        // AI logic
        if (enemies.Count > 0)
        {
            // Get the closest enemy
            Vector3 targetEnemy = GetClosestEnemy(tank);

            // Move towards the enemy
            MoveTowards(targetEnemy, tank);

            // Check and avoid map props
            AvoidProps(tank);
        }
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
        tank.Angle = (float)Math.Atan2(-direction.X, direction.Z);

        // Apply acceleration
        tank.Velocidad += tank.Acceleration;

        // Limit speed
        tank.Velocidad = MathHelper.Clamp(tank.Velocidad, 0, tank.MaxSpeed);

        // Update tank position
        tank.Position += direction * tank.Velocidad;

        // Apply friction
        tank.Velocidad *= (1 - tank.Friction);

        // Update translation matrix
        tank.Translation = Matrix.CreateTranslation(tank.Position);
    }

    private void AvoidProps(Tank tank)
    {
        // Check and avoid map props
        foreach (StaticProp prop in PlaneMap.Props)
        {
            float distanceToProp = Vector3.Distance(tank.Position, prop.Position);

            if (distanceToProp < 5)
            {
                // Adjust tank position to avoid the prop
                Vector3 awayFromProp = Vector3.Normalize(tank.Position - prop.Position);
                tank.Position += awayFromProp * 2; // Move 2 units away from the prop
            }
        }
    }

    public override void Respawn(Tank tank)
    {
        PathIndex = 0;
        perseguir = false;
    }
}