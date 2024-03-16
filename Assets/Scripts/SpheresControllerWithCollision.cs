using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class SpheresControllerWithCollision : SpheresController
{
    private KDTree _kdTree;
    private List<KDTreeData> _treeDataList;
    private NativeList<float3> _spheresPositionList;

    protected override void Start()
    {
        base.Start();

        _treeDataList = new List<KDTreeData>();
        _spheresPositionList = new NativeList<float3>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_kdTree.IsCreated) _kdTree.Dispose();

        for (var i = 0; i < _treeDataList.Count; i++)
        {
            _treeDataList[i].Dispose();
        }

        _spheresPositionList.Dispose();
    }

    protected override void SpawnSpheres(int spawnCount)
    {
        base.SpawnSpheres(spawnCount);

        for (var i = 0; i < spawnCount; i++)
        {
            _spheresPositionList.Add(float3.zero);

            var treeData = new KDTreeData();
            treeData.Initialize(2);
            _treeDataList.Add(treeData);
        }

        CreateTree(_spheres.Count);
    }

    protected override void MoveSpheres()
    {
        if (!_transformAccessArray.isCreated) return;

        var spheresMovementJob = new SpheresMovementJob
        {
            Velocity = _spheresVelocityList.AsArray(),
            Radius = SPHERE_RADIUS,
            DeltaTime = Time.deltaTime,
            Seed = (uint)(Random.value * uint.MaxValue),
            PlaygroundPosition = Playground.position,
            PlaygroundSize = PlaygroundSize
        };

        var spheresPositionJob = new SpheresPositionJob()
        {
            Positions = _spheresPositionList.AsArray(),
        };

        var spheresMovementHandle = spheresMovementJob.Schedule(_transformAccessArray);
        var spheresPositionHandle = spheresPositionJob.Schedule(_transformAccessArray, spheresMovementHandle);
        var updateTreeHandle = UpdateTreeData(spheresPositionHandle);

        var jobList = new NativeList<JobHandle>(Allocator.TempJob);
        for (var i = 0; i < _spheres.Count; i++)
        {
            var treeResultsHandle = GetTreeResults(i, _spheres[i].transform.position, updateTreeHandle);
            jobList.Add(treeResultsHandle);
        }

        JobHandle.CompleteAll(jobList.AsArray());
        jobList.Dispose();

        for (var i = 0; i < _spheres.Count; i++)
        {
            _spheres[i].SetColliding(_treeDataList[i].ResultsCount.Value > 1);
        }
    }

    private void CreateTree(int treeCapacity)
    {
        if (_kdTree.IsCreated) _kdTree.Dispose();

        _kdTree = new KDTree(treeCapacity, Allocator.Persistent);
    }

    private JobHandle UpdateTreeData(JobHandle dependsOn = default)
    {
        var jobHandle = _kdTree.BuildTree(_spheresPositionList.AsArray(), dependsOn);

        return jobHandle;
    }

    private JobHandle GetTreeResults(int index, Vector3 queryPosition, JobHandle dependsOn = default)
    {
        var searchJob = new GetEntriesInRangeJob
        {
            QueryPosition = queryPosition,
            Range = SPHERE_RADIUS * 2f,
            Tree = _kdTree,
            Neighbours = _treeDataList[index].Neighbours,
            ResultsCount = _treeDataList[index].ResultsCount,
        };

        var jobHandle = searchJob.Schedule(dependsOn);

        return jobHandle;
    }
}