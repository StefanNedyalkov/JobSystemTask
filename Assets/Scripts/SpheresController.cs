using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class SpheresController : MonoBehaviour
{
    [SerializeField] private SphereObject SpherePrefab;
    [SerializeField] [Range(1, 100)] protected int SpawnCount;

    [Space]
    [SerializeField] protected Transform Playground;
    [SerializeField] protected Vector3 PlaygroundSize;

    protected TransformAccessArray _transformAccessArray;

    protected readonly List<SphereObject> _spheres = new List<SphereObject>();

    protected const float SPHERE_RADIUS = 0.5f;

    protected virtual void Start()
    {
        Playground.localScale = PlaygroundSize;
    }

    protected virtual void OnDestroy()
    {
        _transformAccessArray.Dispose();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnSpheres();
        }

        MoveSpheres();
    }

    protected virtual void SpawnSpheres()
    {
        for (var i = 0; i < SpawnCount; i++)
        {
            var initialPosition = GetRandomPositionInsideRect(PlaygroundSize, SPHERE_RADIUS);
            var initialRotation = Random.rotation;

            var sphere = Instantiate(SpherePrefab, initialPosition, initialRotation);
            _spheres.Add(sphere);
        }

        if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();

        var spheresTransformArray = _spheres.Select(sphere => sphere.transform).ToArray();
        _transformAccessArray = new TransformAccessArray(spheresTransformArray);
    }

    protected virtual void MoveSpheres()
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

        var spheresMovementHandle = spheresMovementJob.Schedule(_transformAccessArray);
        spheresMovementHandle.Complete();
    }

    private Vector3 GetRandomPositionInsideRect(Vector3 rectSize, float offset = 0)
    {
        var minBounds = transform.position - rectSize / 2f + Vector3.one * offset;
        var maxBounds = transform.position + rectSize / 2f - Vector3.one * offset;

        var randomX = Random.Range(minBounds.x, maxBounds.x);
        var randomY = Random.Range(minBounds.y, maxBounds.y);
        var randomZ = Random.Range(minBounds.z, maxBounds.z);

        return new Vector3(randomX, randomY, randomZ);
    }
}