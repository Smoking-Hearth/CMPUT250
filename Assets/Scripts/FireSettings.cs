using UnityEngine;
using UnityEditor;

public class FireSettings : ScriptableObject
{
    public const string Path = "Assets/Settings/FireSettings.asset";

    [SerializeField] Gradient commonColor;
    [SerializeField] Gradient liquidColor;
    [SerializeField] Gradient electricalColor;
    [SerializeField] Gradient metalColor;
    [SerializeField] Gradient cookingColor;

    public Gradient CommonColor
    {
        get { return commonColor; }
    }

    public Gradient LiquidColor
    {
        get {return liquidColor; }
    }

    public Gradient ElectricalColor
    {
        get { return electricalColor; }
    }

    public Gradient MetalColor
    {
        get { return metalColor; }
    }

    public Gradient CookingColor 
    {
        get { return cookingColor; }
    }

    public Gradient ColorFor(CombustibleKind fireKind)
    {
        switch (fireKind)
        {
            case CombustibleKind.A_COMMON:
                return commonColor;
            case CombustibleKind.B_LIQUID:
                return liquidColor;
            case CombustibleKind.C_ELECTRICAL:
                return electricalColor;
            case CombustibleKind.D_METAL:
                return metalColor;
            case CombustibleKind.K_COOKING:
                return cookingColor;
            default:
                return commonColor;
        }
    }

    public static FireSettings GetOrCreate()
    {
        FireSettings settings = AssetDatabase.LoadAssetAtPath<FireSettings>(Path);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<FireSettings>();
            AssetDatabase.CreateAsset(settings, Path);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    public static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreate());
    }

}