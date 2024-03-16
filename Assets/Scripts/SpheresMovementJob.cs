using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public struct SpheresMovementJob : IJobParallelForTransform
{
    public NativeArray<float3> Velocity;

    [ReadOnly] public float Radius;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public uint Seed;

    [ReadOnly] public float3 PlaygroundPosition;
    [ReadOnly] public float3 PlaygroundSize;

    public void Execute(int index, TransformAccess transform)
    {
        var velocity = Velocity[index];
        var speed = math.length(velocity);
        var direction = math.normalize(velocity);

        var nextPosition = transform.position + (Vector3)(velocity * DeltaTime);

        var hitWall = CheckWallCollisions(transform.position, nextPosition, Radius);
        if (hitWall)
        {
            var newDirection = RicochetRandom(nextPosition, direction, out var collisionPoint, Radius);

            transform.position = collisionPoint;
            transform.rotation = quaternion.LookRotationSafe(newDirection, new float3(0, 1, 0));

            Velocity[index] = newDirection * speed;
        }
        else
        {
            transform.position = nextPosition;
            transform.rotation = quaternion.LookRotationSafe(direction, new float3(0, 1, 0));

            Velocity[index] = direction * speed;
        }
    }

    private bool CrossedWall(float min, float max, float previous, float current)
    {
        return (previous > min && current <= min)
               || (previous < max && current >= max)
               || (previous < min && current >= min)
               || (previous > max && current <= max);
    }

    private bool CheckWallCollisions(float3 currentPosition, float3 nextPosition, float offset = 0)
    {
        var hit = CrossedWall(PlaygroundPosition.x - PlaygroundSize.x / 2f + offset,
                      PlaygroundPosition.x + PlaygroundSize.x / 2f - offset,
                      currentPosition.x,
                      nextPosition.x)
                  || CrossedWall(PlaygroundPosition.y - PlaygroundSize.y / 2f + offset,
                      PlaygroundPosition.y + PlaygroundSize.y / 2f - offset,
                      currentPosition.y,
                      nextPosition.y)
                  || CrossedWall(PlaygroundPosition.z - PlaygroundSize.z / 2f + offset,
                      PlaygroundPosition.z + PlaygroundSize.z / 2f - offset,
                      currentPosition.z,
                      nextPosition.z);

        return hit;
    }

    private float3 RicochetRandom(float3 position, float3 direction, out float3 collisionPoint, float offset = 0)
    {
        var randomPositionInsideRect = GetRandomPositionInsideRect(PlaygroundPosition, PlaygroundSize, Radius);
        var randomDirection = math.normalize(randomPositionInsideRect - position);

        collisionPoint = GetCollisionPoint(position, direction, offset);

        return randomDirection;
    }

    private float3 Ricochet(float3 position, float3 direction, out float3 collisionPoint, float offset = 0)
    {
        var collisionNormal = float3.zero;

        if (math.abs(position.x - PlaygroundPosition.x) >= PlaygroundSize.x / 2f - offset)
        {
            collisionNormal += math.sign(position.x) * new float3(1, 0, 0);
        }

        if (math.abs(position.y - PlaygroundPosition.y) >= PlaygroundSize.y / 2f - offset)
        {
            collisionNormal += math.sign(position.y) * new float3(0, 1, 0);
        }

        if (math.abs(position.z - PlaygroundPosition.z) >= PlaygroundSize.z / 2f - offset)
        {
            collisionNormal += math.sign(position.z) * new float3(0, 0, 1);
        }

        var reflectedDirection = math.normalize(math.reflect(direction, collisionNormal));

        collisionPoint = GetCollisionPoint(position, direction, offset);

        return reflectedDirection;
    }

    private float3 GetCollisionPoint(float3 position, float3 direction, float offset = 0)
    {
        var collisionPoint = position;

        // Check which face the ball hit and calculate the collision point accordingly
        if (math.abs(position.x - PlaygroundPosition.x) >= PlaygroundSize.x / 2f - offset)
        {
            var t = (math.sign(position.x) * (PlaygroundSize.x / 2f - offset) - position.x) / direction.x;
            collisionPoint = position + t * direction;
        }
        else if (math.abs(position.y - PlaygroundPosition.y) >= PlaygroundSize.y / 2f - offset)
        {
            var t = (math.sign(position.y) * (PlaygroundSize.y / 2f - offset) - position.y) / direction.y;
            collisionPoint = position + t * direction;
        }
        else if (math.abs(position.z - PlaygroundPosition.z) >= PlaygroundSize.z / 2f - offset)
        {
            var t = (math.sign(position.z) * (PlaygroundSize.z / 2f - offset) - position.z) / direction.z;
            collisionPoint = position + t * direction;
        }

        return collisionPoint;
    }

    private float3 GetRandomPositionInsideRect(float3 rectPosition, float3 rectSize, float offset = 0)
    {
        var random = new Random(Seed);

        var minBounds = rectPosition - rectSize / 2f + new float3(1, 1, 1) * offset;
        var maxBounds = rectPosition + rectSize / 2f - new float3(1, 1, 1) * offset;

        var randomX = random.NextFloat(minBounds.x, maxBounds.x);
        var randomY = random.NextFloat(minBounds.y, maxBounds.y);
        var randomZ = random.NextFloat(minBounds.z, maxBounds.z);

        return new float3(randomX, randomY, randomZ);
    }
}