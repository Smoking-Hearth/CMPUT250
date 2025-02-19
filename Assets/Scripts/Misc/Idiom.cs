using UnityEngine;

public static class Idiom
{
    public static Vector3 WithZ(this Vector2 vec, float z)
    {
        Vector3 vec3 = vec;
        vec3.z = z;
        return vec3;
    }

    public static Vector3 WithZ(this Vector2 vec, Vector3 other)
    {
        Vector3 vec3 = vec;
        vec3.z = other.z;
        return vec3;
    }

    public static Vector3 WithZ(this Vector2 vec, Transform other)
    {
        return vec.WithZ(other.position);
    }

}
