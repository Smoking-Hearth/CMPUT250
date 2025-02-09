using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

// See https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SettingsProvider.html
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

                properties.Add(new PropertyField(settings.FindProperty("fireTickDelaySeconds")));
                properties.Add(new PropertyField(settings.FindProperty("commonFireSettings")));
                properties.Add(new PropertyField(settings.FindProperty("liquidFireSettings")));
                properties.Add(new PropertyField(settings.FindProperty("electricalFireSettings")));
                properties.Add(new PropertyField(settings.FindProperty("metalFireSettings")));
                properties.Add(new PropertyField(settings.FindProperty("cookingFireSettings")));
                properties.Add(new PropertyField(settings.FindProperty("electricBackfire")));

                rootElement.Bind(settings);
            },

            keywords = new HashSet<string>(new[] { "Gradient", "Flame", "Fire", "Color" }),
        };

        return provider;
    }
}
