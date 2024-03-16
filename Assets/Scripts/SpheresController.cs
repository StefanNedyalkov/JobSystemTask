using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class SpheresController : MonoBehaviour
{
    [SerializeField] protected SphereObject SpherePrefab;
    [SerializeField] protected Vector2Int SpawnCount;
    [SerializeField] protected Vector2 RandomSpeed;

    [Space]
    [SerializeField] protected Transform Playground;
    [SerializeField] protected Vector3 PlaygroundSize;

    protected TransformAccessArray _transformAccessArray;
    protected NativeList<float3> _spheresVelocityList;

    protected readonly List<SphereObject> _spheres = new List<SphereObject>();

    protected const float SPHERE_RADIUS = 0.5f;

    protected virtual void Start()
    {
        Playground.localScale = PlaygroundSize;

        _spheresVelocityList = new NativeList<float3>(Allocator.Persistent);
    }

    protected virtual void OnDestroy()
    {
        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        _spheresVelocityList.Dispose();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var spawnCount = SpawnCount.RandomValue();
            SpawnSpheres(spawnCount);
        }

        MoveSpheres();
    }

    protected virtual void SpawnSpheres(int spawnCount)
    {
        for (var i = 0; i < spawnCount; i++)
        {
            var initialPosition = GetRandomPositionInsideRect(Playground.position, PlaygroundSize, SPHERE_RADIUS);
            var initialVelocity = Random.rotation * Vector3.forward * RandomSpeed.RandomValue();

            _spheresVelocityList.Add(initialVelocity);

            var sphere = Instantiate(SpherePrefab, initialPosition, Quaternion.identity);
            _spheres.Add(sphere);
        }

        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();

        _transformAccessArray = new TransformAccessArray(_spheres.Count);

        for (var i = 0; i < _spheres.Count; i++)
        {
            _transformAccessArray.Add(_spheres[i].transform);
        }
    }

    protected virtual void MoveSpheres()
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

        var spheresMovementHandle = spheresMovementJob.Schedule(_transformAccessArray);
        spheresMovementHandle.Complete();
    }

    private Vector3 GetRandomPositionInsideRect(Vector3 rectPosition, Vector3 rectSize, float offset = 0)
    {
        var minBounds = rectPosition - rectSize / 2f + Vector3.one * offset;
        var maxBounds = rectPosition + rectSize / 2f - Vector3.one * offset;

        var randomX = Random.Range(minBounds.x, maxBounds.x);
        var randomY = Random.Range(minBounds.y, maxBounds.y);
        var randomZ = Random.Range(minBounds.z, maxBounds.z);

        return new Vector3(randomX, randomY, randomZ);
    }
}