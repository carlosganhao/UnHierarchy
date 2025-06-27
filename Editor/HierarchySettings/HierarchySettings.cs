using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Settings
{
    public class HierarchySettings : ScriptableObject
    {
        public static readonly string SettingsPath = "Assets/Editor/HierarchySettings.asset";

        [SerializeField]
        private bool _useCustomHierarchy = true;
        public bool UseCustomHierarchy
        {
            get => _useCustomHierarchy;
        }

        [SerializeField]
        private Color _selectionColor = new Color(0.239f, 0.502f, 0.875f, 0.424f);
        public Color SelectionColor
        {
            get => _selectionColor;
        }

        [SerializeField]
        private bool _useAlternatingBackground = false;
        public bool UseAlternatingBackground
        {
            get => _useAlternatingBackground;
        }

        [SerializeField]
        private Color _alternatingBackgroundColor = new Color(1, 1, 1, 0.05f);
        public Color AlternatingBackgroundColor
        {
            get => _alternatingBackgroundColor;
        }

        [SerializeField]
        private bool _useCustomBackgroundSprite = false;
        public bool UseCustomBackgroundSprite
        {
            get => _useCustomBackgroundSprite;
        }

        [SerializeField]
        private Texture2D _backgroundSprite;
        public Texture2D BackgroundSprite
        {
            get => _backgroundSprite;
        }

        [SerializeField]
        private bool _useAutomaticIcons = false;
        public bool UseAutomaticIcons
        {
            get => _useAutomaticIcons;
        }

        [SerializeField]
        private bool _useLayerNames = false;
        public bool UseLayerNames
        {
            get => _useLayerNames;
        }

        [SerializeField]
        private bool _useIdentLevel = false;
        public bool UseIdentLevel
        {
            get => _useIdentLevel;
        }

        [SerializeField]
        private Color _identColor = new Color(0.698f, 0.698f, 0.698f, 0.5f);
        public Color IdentColor
        {
            get => _identColor;
        }

        [SerializeField]
        private bool _useCustomFoldout = false;
        public bool UseCustomFoldout
        {
            get => _useCustomFoldout;
        }

        [SerializeField]
        private Texture2D _customFoldoutClosed;
        public Texture2D CustomFoldoutClosed
        {
            get => _customFoldoutClosed;
        }

        [SerializeField]
        private Texture2D _customFoldoutOpen;
        public Texture2D CustomFoldoutOpen
        {
            get => _customFoldoutOpen;
        }

        [SerializeField]
        private Color[] _customColors = {
            new Color(0.5450981f, 0, 0),
            new Color(0.5450981f, 0.4078431f, 0),
            new Color(0.3686275f, 0.5450981f, 0),
            new Color(0, 0.5450981f, 0.1372549f),
            new Color(0, 0.545098f, 0.545098f),
            new Color(0, 0.13725f, 0.5450981f),
            new Color(0.270588f, 0, 0.5450981f),
            new Color(0.5450981f, 0, 0.4117647f),
        };

        public Color[] CustomColors
        {
            get => _customColors;
        }

        [SerializeField]
        private IconEntry[] _customIcons = {
            new IconEntry("d_Package Manager"),
            new IconEntry("d_Settings"),
            new IconEntry("d_Folder Icon"),
            new IconEntry("d_cs Script Icon"),
            new IconEntry("d_Favorite Icon"),
            new IconEntry("d_Spotlight Icon"),
            new IconEntry("d_AudioImporter Icon"),
            new IconEntry("d_ParticleSystem Icon"),
            new IconEntry("d_Terrain Icon"),
        };

        public IconEntry[] CustomIcons
        {
            get => _customIcons;
        }

        [SerializeField]
        private CustomizationDatabaseDictionary _customizationDatabase = new CustomizationDatabaseDictionary();
        public CustomizationDatabaseDictionary CustomizationDatabase
        {
            get => _customizationDatabase;
        }

        internal static HierarchySettings GetOrFetchSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<HierarchySettings>(SettingsPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<HierarchySettings>();
                settings._backgroundSprite = Resources.Load("background") as Texture2D;
                settings._customFoldoutClosed = Resources.Load("plus") as Texture2D;
                settings._customFoldoutOpen = Resources.Load("minus") as Texture2D;
                if(!Directory.Exists(Path.GetDirectoryName(SettingsPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                    AssetDatabase.Refresh();
                }
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrFetchSettings());
        }

    }
}