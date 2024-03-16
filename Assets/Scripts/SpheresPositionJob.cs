using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct SpheresPositionJob : IJobParallelForTransform
{
    [WriteOnly] public NativeArray<float3> Positions;

    public void Execute(int index, TransformAccess transform)
    {
        Positions[index] = transform.position;
    }
}