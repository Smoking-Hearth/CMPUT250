using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

// See https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SettingsProvider.html


public class FireSettings : ScriptableObject
{
    public const string Path = "Assets/Settings/FireSettings.asset";

    [SerializeField] Gradient commonColor;
    [SerializeField] Gradient liquidColor;
    [SerializeField] Gradient electricalColor;
    [SerializeField] Gradient metalColor;
    [SerializeField] Gradient cookingColor;

    internal static FireSettings GetOrCreate()
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

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreate());
    }

}

static class FireSettingsUIElementsRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateFireSettingsProvider()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
        var provider = new SettingsProvider("Project/FireSettings", SettingsScope.Project)
        {
            label = "Fire",
            // activateHandler is called when the user clicks on the Settings item in the Settings window.
            activateHandler = (searchContext, rootElement) =>
            {
                var settings = FireSettings.GetSerializedSettings();

                // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                // isn't called because the SettingsProvider uses the UIElements drawing framework.
                // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/settings_ui.uss");
                // rootElement.styleSheets.Add(styleSheet);
                var title = new Label()
                {
                    text = "Fire Settings"
                };
                title.AddToClassList("title");
                rootElement.Add(title);

                var properties = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                };
                properties.AddToClassList("property-list");
                rootElement.Add(properties);

                properties.Add(new PropertyField(settings.FindProperty("commonColor")));
                properties.Add(new PropertyField(settings.FindProperty("liquidColor")));
                properties.Add(new PropertyField(settings.FindProperty("electricalColor")));
                properties.Add(new PropertyField(settings.FindProperty("metalColor")));
                properties.Add(new PropertyField(settings.FindProperty("cookingColor")));

                rootElement.Bind(settings);
            },

            keywords = new HashSet<string>(new[] { "Gradient", "Flame", "Fire", "Color" }),
        };

        return provider;
    }
}
