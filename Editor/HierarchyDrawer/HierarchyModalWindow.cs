using System;
using System.Collections.Generic;
using UnHierarchy.Settings;
using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Editor
{
    public class HierarchyModalWindow : PopupWindowContent
    {
        private ModalStyles styles = new ModalStyles();

        private int selectedTab = 0;
        private string[] tabNames = {
            "Colors",
            "Icons",
        };
        private int buttonsPerRow = 9;
        private Color[] colors;
        private IconEntry[] icons;
        private Texture swatchTexture;

        private Vector2 colorScrollPosition;
        private Vector2 iconScrollPosition;
        private Color currentColor = default(Color);
        private HierarchyObjectData currentObject;
        private CustomizationData previousCustomization;
        private HierarchySettings settings;
        private bool iconChanged = false;
        private bool changesCommited = false;

        public HierarchyModalWindow(int instanceID, Dictionary<int, HierarchyObjectData> dataDict)
        {
            settings = HierarchySettings.GetOrFetchSettings();
            colors = settings.CustomColors;
            icons = settings.CustomIcons;
            swatchTexture = Resources.Load("swatch") as Texture;
            currentObject = dataDict[instanceID];
            previousCustomization = new CustomizationData()
            {
                CustomBackgroundColor = currentObject.BackgroundColor,
                CustomIconContent = new GUIContent(currentObject.Content),
            };
            currentColor = currentObject.BackgroundColor;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(256, 144);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginHorizontal();
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            GUILayout.EndHorizontal();

            switch (selectedTab)
            {
                case 0:
                    RenderColorTab(rect);
                    break;
                case 1:
                    RenderIconTab(rect);
                    break;
            }

            GUILayout.BeginVertical();
            RenderSaveButton();
            GUILayout.EndVertical();
        }

        public override void OnClose()
        {
            if (!changesCommited)
            {
                currentObject.BackgroundColor = previousCustomization.CustomBackgroundColor;
                currentObject.Content.image = previousCustomization.CustomIconContent.image;
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private void RenderColorTab(Rect rect)
        {
            GUILayout.BeginHorizontal();
            currentColor = EditorGUILayout.ColorField(currentColor);
            currentObject.BackgroundColor = currentColor;
            EditorApplication.RepaintHierarchyWindow();
            GUILayout.EndHorizontal();

            colorScrollPosition = GUILayout.BeginScrollView(colorScrollPosition, GUILayout.Width(rect.width));
            for (int i = 0; i < colors.Length; i += buttonsPerRow)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(rect.width));
                for (int j = i; j < Math.Min(i + buttonsPerRow, colors.Length); j++)
                {
                    var prevColor = GUI.color;
                    GUI.contentColor = colors[j];
                    if (GUILayout.Button(swatchTexture, styles.ColorSwatch, GUILayout.Width(rect.width / buttonsPerRow), GUILayout.Height(rect.width / buttonsPerRow)))
                    {
                        currentObject.BackgroundColor = colors[j];
                        currentColor = colors[j];
                        EditorApplication.RepaintHierarchyWindow();
                    }
                    GUI.contentColor = prevColor;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void RenderIconTab(Rect rect)
        {
            iconScrollPosition = GUILayout.BeginScrollView(iconScrollPosition, GUILayout.Width(rect.width));
            for (int i = 0; i < icons.Length; i += buttonsPerRow)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(rect.width));
                for (int j = i; j < Math.Min(i + buttonsPerRow, icons.Length); j++)
                {
                    if (GUILayout.Button(icons[j].getIconContent(), styles.IconButton, GUILayout.Width(rect.width / buttonsPerRow), GUILayout.Height(rect.width / buttonsPerRow)))
                    {
                        currentObject.Content.image = icons[j].getIconContent().image;
                        iconChanged = true;
                        EditorApplication.RepaintHierarchyWindow();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void RenderSaveButton()
        {
            if (GUILayout.Button("Save Changes"))
            {
                var customization = settings.CustomizationDatabase.GetOrFetchPersistentCustomization(currentObject.GlobalObjectId);
                customization.CustomBackgroundColor = currentColor;
                customization.CustomIconContent = iconChanged ? new GUIContent(currentObject.Content?.image) : null;
                EditorUtility.SetDirty(settings);
                changesCommited = true;
            }
        }

        public class ModalStyles
        {
            public GUIStyle ColorSwatch;
            public GUIStyle IconButton;

            public ModalStyles()
            {
                ColorSwatch = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0),
                    imagePosition = ImagePosition.ImageOnly,
                    stretchHeight = false,
                    stretchWidth = false,
                };
                IconButton = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0),
                    imagePosition = ImagePosition.ImageOnly,
                    stretchHeight = false,
                    stretchWidth = false,
                };
            }
        }
    }
}