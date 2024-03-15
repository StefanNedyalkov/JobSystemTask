using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct SpheresMovementJob : IJobParallelForTransform
{
    [ReadOnly] public float Speed;
    [ReadOnly] public float Radius;
    [ReadOnly] public float DeltaTime;

    [ReadOnly] public float3 PlaygroundPosition;
    [ReadOnly] public float3 PlaygroundSize;

    public void Execute(int index, TransformAccess transform)
    {
        var direction = math.normalize(math.mul(transform.rotation, new float3(0, 0, 1)));
        var velocity = direction * Speed;

        var nextPosition = transform.position + (Vector3)(velocity * DeltaTime);

        var hitWall = CheckWallCollisions(transform.position, nextPosition, Radius);
        if (hitWall)
        {
            var collisionPoint = Ricochet(nextPosition, direction, out var newDirection, Radius);

            transform.position = collisionPoint;
            transform.rotation = quaternion.LookRotationSafe(newDirection, new float3(0, 1, 0));
        }
        else
        {
            transform.position = nextPosition;
            transform.rotation = quaternion.LookRotationSafe(direction, new float3(0, 1, 0));
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
        var hit = CrossedWall(PlaygroundPosition.x - PlaygroundSize.x / 2f + offset, PlaygroundPosition.x + PlaygroundSize.x / 2f - offset, currentPosition.x, nextPosition.x)
                  || CrossedWall(PlaygroundPosition.y - PlaygroundSize.y / 2f + offset, PlaygroundPosition.y + PlaygroundSize.y / 2f - offset, currentPosition.y, nextPosition.y)
                  || CrossedWall(PlaygroundPosition.z - PlaygroundSize.z / 2f + offset, PlaygroundPosition.z + PlaygroundSize.z / 2f - offset, currentPosition.z, nextPosition.z);

        return hit;
    }

    private float3 Ricochet(float3 position, float3 direction, out float3 reflectedDirection, float offset = 0)
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

        reflectedDirection = math.normalize(math.reflect(direction, collisionNormal));

        // Calculate collision point
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
}