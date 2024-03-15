using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// Gets the nearest neighbours based on a position and range from the incoming tree
/// </summary>
[BurstCompile]
public struct GetEntriesInRangeJob : IJob
{
    [ReadOnly] public KDTree Tree;
    [ReadOnly] public float3 QueryPosition;
    [ReadOnly] public float Range;
    public NativeArray<KDTree.Neighbour> Neighbours;
    public NativeReference<int> ResultsCount;

    public void Execute()
    {
        ResultsCount.Value = Tree.GetEntriesInRange(QueryPosition, Range, ref Neighbours);
    }
}