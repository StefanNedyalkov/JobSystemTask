using UnityEngine;
using Random = UnityEngine.Random;

public static class ExtensionMethods
{
    /// <summary>
    /// <para>Returns a random float value within x(inclusive) and y(inclusive)</para>
    /// </summary>
    public static float RandomValue(this Vector2 vector)
    {
        return Random.Range(vector.x, vector.y);
    }

    /// <summary>
    /// <para>Returns a random int value within x(inclusive) and y(inclusive)</para>
    /// </summary>
    public static int RandomValue(this Vector2Int vector)
    {
        return Random.Range(vector.x, vector.y + 1);
    }
}