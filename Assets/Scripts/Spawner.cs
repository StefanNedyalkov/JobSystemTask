using UnityEngine;
using UnityEngine.Jobs;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject SpherePrefab;
    [SerializeField] private int SpawnCount;
    
    private TransformAccessArray _transformAccessArray;
}