using System;
using Unity.Collections;

/// <summary>
/// Initialize and Dispose the KDTree Data
/// </summary>
public struct KDTreeData : IDisposable
{
    public NativeArray<KDTree.Neighbour> Neighbours;
    public NativeReference<int> ResultsCount;

    public void Initialize(int maxResults)
    {
        Neighbours = new NativeArray<KDTree.Neighbour>(maxResults, Allocator.Persistent);
        ResultsCount = new NativeReference<int>(Allocator.Persistent);
    }

    public void Dispose()
    {
        if (Neighbours.IsCreated) Neighbours.Dispose();
        if (ResultsCount.IsCreated) ResultsCount.Dispose();
    }
}