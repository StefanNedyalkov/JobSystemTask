using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;

public class SpheresControllerWithCollision : SpheresController
{
    private NativeList<float3> _positions;
    private CollisionTree _collisionTree;

    protected override void Start()
    {
        base.Start();

        _positions = new NativeList<float3>(Allocator.Persistent);
        _collisionTree.Initialize(2);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _positions.Dispose();
        _collisionTree.Dispose();
    }

    protected override void SpawnSpheres()
    {
        base.SpawnSpheres();

        for (var i = 0; i < SpawnCount; i++)
        {
            _positions.Add(_spheres[i].transform.position);
        }

        CreateTree(_spheres.Count);
    }

    protected override void MoveSpheres()
    {
        if (!_transformAccessArray.isCreated) return;

        var spheresMovementJob = new SpheresMovementJob
        {
            Speed = 10,
            Radius = SPHERE_RADIUS,
            DeltaTime = Time.deltaTime,
            PlaygroundPosition = Playground.position,
            PlaygroundSize = PlaygroundSize
        };

        var spheresCollisionJob = new SpheresCollisionJob()
        {
            Positions = _positions.AsArray(),
        };

        var spheresMovementHandle = spheresMovementJob.Schedule(_transformAccessArray);
        var spheresCollisionHandle = spheresCollisionJob.Schedule(_transformAccessArray, spheresMovementHandle);

        UpdateTreeData(spheresCollisionHandle);

        for (var i = 0; i < _spheres.Count; i++)
        {
            GetTreeResults(_spheres[i].transform.position);

            _spheres[i].SetColliding(_collisionTree.ResultsCount.Value > 1);
        }
    }

    private void CreateTree(int treeCapacity)
    {
        ref var tree = ref _collisionTree.Tree;
        if (tree.IsCreated) tree.Dispose();

        tree = new KDTree(treeCapacity, Allocator.Persistent);
    }

    private JobHandle UpdateTreeData(JobHandle dependsOn = default(JobHandle))
    {
        ref var tree = ref _collisionTree.Tree;

        var jobHandle = tree.BuildTree(_positions.AsArray(), dependsOn);
        jobHandle.Complete();

        return jobHandle;
    }

    private JobHandle GetTreeResults(Vector3 queryPosition, JobHandle dependsOn = default(JobHandle))
    {
        var searchJob = new GetEntriesInRangeJob
        {
            QueryPosition = queryPosition,
            Range = SPHERE_RADIUS * 2f,
            Tree = _collisionTree.Tree,
            Neighbours = _collisionTree.Neighbours,
            ResultsCount = _collisionTree.ResultsCount,
        };

        var jobHandle = searchJob.Schedule(dependsOn);
        jobHandle.Complete();

        return jobHandle;
    }
}