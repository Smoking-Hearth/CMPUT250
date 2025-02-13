using System.Diagnostics;

/// <summary>
/// Zero cost logging. There is still a penalty in development, but these will be
/// stripped in the actual builds. Helps us avoid the preprocessor nonsense too.
/// </summary>
public static class DevLog
{
    [Conditional("UNITY_EDITOR")]
    public static void Info(object msg, UnityEngine.Object ctx = null)
    {
        UnityEngine.Debug.Log(msg, ctx);
    }

    [Conditional("UNITY_EDITOR")]
    public static void Warn(object msg, UnityEngine.Object ctx = null)
    {
        UnityEngine.Debug.LogWarning(msg, ctx);
    }

    [Conditional("UNITY_EDITOR")]
    public static void Error(object msg, UnityEngine.Object ctx = null)
    {
        UnityEngine.Debug.LogError(msg, ctx);
    }
}