using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct FireInfo
{
    public Gradient gradient;
    public Color lightColour;
    public float lightIntensity;
}

public class FireSettings : ScriptableObject
{
    public const string Path = "Assets/Resources/FireSettings.asset";

    [SerializeField] float fireTickDelaySeconds;
    [SerializeField] FireInfo commonFireSettings;
    [SerializeField] FireInfo liquidFireSettings;
    [SerializeField] FireInfo electricalFireSettings;
    [SerializeField] FireInfo metalFireSettings;
    [SerializeField] FireInfo cookingFireSettings;
    public EnemyAttackInfo electricBackfire;

    public float FireDelay
    {
        get
        {
            return fireTickDelaySeconds;
        }
    }

    public FireInfo GetFireInfo(CombustibleKind fireKind)
    {
        switch (fireKind)
        {
            case CombustibleKind.A_COMMON:
                return commonFireSettings;
            case CombustibleKind.B_LIQUID:
                return liquidFireSettings;
            case CombustibleKind.C_ELECTRICAL:
                return electricalFireSettings;
            case CombustibleKind.D_METAL:
                return metalFireSettings;
            case CombustibleKind.K_COOKING:
                return cookingFireSettings;
            default:
                return commonFireSettings;
        }
    }

    public static FireSettings GetOrCreate()
    {
        #if UNITY_EDITOR
        FireSettings settings = AssetDatabase.LoadAssetAtPath<FireSettings>(Path);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<FireSettings>();
            AssetDatabase.CreateAsset(settings, Path);
            AssetDatabase.SaveAssets();
        }
        #else
        FireSettings settings = Resources.Load<FireSettings>(Path);
        #endif
        return settings;
    }

    #if UNITY_EDITOR
    public static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreate());
    }
    #endif
}