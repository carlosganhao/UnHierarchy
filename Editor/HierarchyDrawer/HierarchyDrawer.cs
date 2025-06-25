using System;
using System.Collections.Generic;
using UnHierarchy.Attributes;
using UnHierarchy.Settings;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace UnHierarchy.Editor
{
    [InitializeOnLoad]
    public static class HierarchyDrawer
    {
        private static HierarchySettings settings;
        private static SceneVisibilityManager sceneVisibilityManager;
        private static int currentLine = 0;
        private static int currentIdentLevel = 0;
        private static int checkIdentForFold = -1;
        private static int previousInstanceId = -1;
        private static Dictionary<int, bool> foldoutDict = new Dictionary<int, bool>();
        private static HashSet<int> selectedDict = new HashSet<int>();
        private static Dictionary<int, VisibilityObjectData> visibilityDict = new Dictionary<int, VisibilityObjectData>();
        private static Dictionary<int, HierarchyObjectData> objectDataDict = new Dictionary<int, HierarchyObjectData>();

        static HierarchyDrawer()
        {
            Initialize();
        }

        public static void Initialize()
        {
            settings = HierarchySettings.GetOrFetchSettings();
            sceneVisibilityManager = SceneVisibilityManager.instance;
            if (settings.UseCustomHierarchy)
            {
                SelectionChanged();

                EditorApplication.hierarchyWindowItemOnGUI += HierchyWindowItemOnGUI;
                Selection.selectionChanged += SelectionChanged;
                SceneVisibilityManager.visibilityChanged += RepaintHierarchy;
                SceneVisibilityManager.pickingChanged += RepaintHierarchy;
            }
        }

        public static void Cleanup()
        {
            objectDataDict.Clear();
            visibilityDict.Clear();
            selectedDict.Clear();
            foldoutDict.Clear();

            EditorApplication.hierarchyWindowItemOnGUI -= HierchyWindowItemOnGUI;
            Selection.selectionChanged -= SelectionChanged;
            SceneVisibilityManager.visibilityChanged -= RepaintHierarchy;
            SceneVisibilityManager.pickingChanged -= RepaintHierarchy;
        }

        public static void Restart()
        {
            Cleanup();
            Initialize();
        }

        private static void HierchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var selectionGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (selectionGameObject == null) return;

            var objectData = GetObjectData(instanceID, selectionGameObject);
            GetObjectVisibility(instanceID, selectionGameObject);
            var currentRects = new HierarchyRects(selectionRect, settings);
            DrawBackgroundHierarchy(instanceID, currentRects, objectData);
            DrawDefaultHierarchy(instanceID, currentRects, objectData);
            ListenToEvents(instanceID, currentRects, objectData);

            previousInstanceId = instanceID;
            currentLine += 1;
        }

        private static void SelectionChanged()
        {
            selectedDict.Clear();
            foreach (var selectedInstance in Selection.instanceIDs)
            {
                selectedDict.Add(selectedInstance);
            }
        }

        private static void RepaintHierarchy()
        {
            visibilityDict.Clear();
        }

        private static HierarchyObjectData GetObjectData(int instanceID, GameObject gameObject)
        {
            if (objectDataDict.TryGetValue(instanceID, out HierarchyObjectData currentObject))
            {
                return currentObject;
            }

            HierarchyObjectData newObject = new HierarchyObjectData()
            {
                InstanceId = instanceID,
                GlobalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(instanceID),
                GameObject = gameObject,
                Content = new GUIContent(EditorGUIUtility.ObjectContent(gameObject, null)),
            };

            // Automatic Icon Rules
            if (settings.UseAutomaticIcons)
            {
                var componentCount = gameObject.GetComponentCount();

                if (componentCount == 1) // Single Transform
                {
                    newObject.Content.image = EditorGUIUtility.IconContent("d_Transform Icon").image;
                }
                else if (componentCount == 2) // Tranform and exactly 1 more component
                {
                    newObject.Content.image = EditorGUIUtility.ObjectContent(gameObject.GetComponentAtIndex(1), null).image;
                }
                else if (componentCount > 2 && AllTheSameComponent(gameObject.GetComponents<Component>())) // Transform and equal components
                {
                    newObject.Content.image = EditorGUIUtility.ObjectContent(gameObject.GetComponentAtIndex(1), null).image;
                }
                else if (gameObject.TryGetComponent(out Canvas canvas)) // Canvas
                {
                    newObject.Content.image = EditorGUIUtility.ObjectContent(canvas, null).image;
                }
                else if (gameObject.TryGetComponent(out Camera camera)) // Camera
                {
                    newObject.Content.image = EditorGUIUtility.ObjectContent(camera, null).image;
                }
            }

            // Get Attribute Customizations
            foreach (var component in gameObject.GetComponents<MonoBehaviour>())
            {
                if(component == null) continue;

                var type = component.GetType();
                var colorAttribute = type.GetAttribute<HierarchyColorAttribute>(true);
                if (colorAttribute != null)
                {
                    newObject.BackgroundColor = colorAttribute.hierarchyColor;
                }

                var imageAttribute = type.GetAttribute<HierarchyIconAttribute>(true);
                if (imageAttribute != null)
                {
                    newObject.Content.image = imageAttribute.Icon;
                }
            }

            // Get Persistent Object Customization 
            if (settings.CustomizationDatabase.TryGetValue(newObject.GlobalObjectId, out var customization))
            {
                if (customization.CustomBackgroundColor != default(Color))
                {
                    newObject.BackgroundColor = customization.CustomBackgroundColor;
                }
                if (customization.CustomIconContent?.image != null)
                {
                    newObject.Content.image = customization.CustomIconContent.image;
                }
            }

            objectDataDict.TryAdd(instanceID, newObject);
            return newObject;

            bool AllTheSameComponent(Component[] components)
            {
                if(components[1] == null) return false;

                Type testType = components[1].GetType();
                for (int i = 2; i < components.Length; i++)
                {
                    if (!components[i].GetType().Equals(testType))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private static void GetObjectVisibility(int instanceID, GameObject gameObject)
        {
            if (visibilityDict.TryGetValue(instanceID, out _))
            {
                return;
            }

            var visibilityObject = new VisibilityObjectData()
            {
                Visible = !sceneVisibilityManager.IsHidden(gameObject),
                Pickable = !sceneVisibilityManager.IsPickingDisabled(gameObject),
                AllDescendantsVisible = sceneVisibilityManager.AreAllDescendantsVisible(gameObject),
                AllDescendantsPickable = sceneVisibilityManager.IsPickingEnabledOnAllDescendants(gameObject),
                NoDescendantsVisible = sceneVisibilityManager.AreAllDescendantsHidden(gameObject),
                NoDescendantsPickable = sceneVisibilityManager.IsPickingDisabledOnAllDescendants(gameObject),
            };

            visibilityDict.TryAdd(instanceID, visibilityObject);
        }

        private static void DrawBackgroundHierarchy(int instanceID, HierarchyRects hierarchyRects, HierarchyObjectData objectData)
        {
            // Hide Normal UI
            EditorGUI.DrawRect(hierarchyRects.backgroundRect, GetDefaultBackgroundColor());
            EditorGUI.DrawRect(hierarchyRects.toolbarRect, GetDefaultToolbarColor());

            // Custom Background
            if (objectData.BackgroundColor != default(Color))
            {
                if (settings.UseCustomBackgroundSprite)
                {
                    GUI.backgroundColor = objectData.BackgroundColor;
                    GUI.Box(hierarchyRects.backgroundRect, GUIContent.none, new GUIStyle() { normal = new GUIStyleState() { background = settings.BackgroundSprite } });
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    EditorGUI.DrawRect(hierarchyRects.backgroundRect, objectData.BackgroundColor);
                }
            }

            // Draw Alternating Background
            if (settings.UseAlternatingBackground && currentLine % 2 == 1)
            {
                EditorGUI.DrawRect(hierarchyRects.backgroundRect, settings.AlternatingBackgroundColor);
            }

            // Selection Rect
            if (selectedDict.TryGetValue(instanceID, out int _))
            {
                if (EditorWindow.focusedWindow?.titleContent?.text == "Hierarchy")
                {
                    EditorGUI.DrawRect(hierarchyRects.fullLineRect, settings.SelectionColor);
                }
                else
                {
                    var selectionStyle = new GUIStyle(GUI.skin.GetStyle("TV Selection"));
                    GUI.Box(hierarchyRects.fullLineRect, GUIContent.none, selectionStyle);
                }
            }

            // Hover line
            GUI.Box(hierarchyRects.fullLineRect, GUIContent.none, new GUIStyle(GUI.skin.GetStyle("TV Line")));
        }

        private static void DrawDefaultHierarchy(int instanceID, HierarchyRects hierarchyRects, HierarchyObjectData objectData)
        {
            var isPrefab = objectData.GameObject.IsPrefabInstance();
            var identLevel = GetIdentLevel(hierarchyRects.selectionRect);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));

            // Label - Icon and Text
            var lineStyle = isPrefab ? new GUIStyle(GUI.skin.GetStyle("PR PrefabLabel")) : new GUIStyle(GUI.skin.GetStyle("TV Line"));
            EditorGUI.LabelField(hierarchyRects.selectionRect, objectData.Content, lineStyle);

            // Layer Name
            if (settings.UseLayerNames)
            {
                EditorGUI.LabelField(hierarchyRects.layerNameRect, LayerMask.LayerToName(objectData.GameObject.layer), new GUIStyle(GUI.skin.GetStyle("PR DisabledLabel")));
            }

            // Foldout Toggling
            if (checkIdentForFold != -1)
            {
                foldoutDict[checkIdentForFold] = identLevel > currentIdentLevel;
                checkIdentForFold = -1;
            }

            // Foldout Rendering
            if (objectData.GameObject.transform.childCount > 0)
            {
                foldoutDict.TryAdd(instanceID, false);
                checkIdentForFold = instanceID;

                var foldoutStyle = new GUIStyle(GUI.skin.GetStyle("IN Foldout"));
                if (settings.UseCustomFoldout)
                {
                    foldoutStyle.normal.background = settings.CustomFoldoutClosed;
                    foldoutStyle.active.background = settings.CustomFoldoutClosed;
                    foldoutStyle.focused.background = settings.CustomFoldoutClosed;
                    foldoutStyle.onNormal.background = settings.CustomFoldoutOpen;
                    foldoutStyle.onActive.background = settings.CustomFoldoutOpen;
                    foldoutStyle.onFocused.background = settings.CustomFoldoutOpen;
                }
                EditorGUI.Foldout(hierarchyRects.foldoutRect, foldoutDict[instanceID], GUIContent.none, foldoutStyle);
            }

            // Ident Levels
            if (settings.UseIdentLevel)
            {
                foreach (var identLevelRect in hierarchyRects.identLevelRects)
                {
                    EditorGUI.DrawRect(identLevelRect, settings.IdentColor);
                }
            }

            // Scene Visibility Rendering
            if (visibilityDict.TryGetValue(instanceID, out VisibilityObjectData objectVisibility))
            {
                var sceneToolStyle = new GUIStyle(GUI.skin.GetStyle("SceneVisibility"));
                var sceneVisiblityStyle = new GUIStyle(sceneToolStyle);
                var scenePickabilityStyle = new GUIStyle(sceneToolStyle);

                if (!objectVisibility.Visible)
                {
                    sceneVisiblityStyle.normal.textColor = Color.white;
                    sceneVisiblityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_hidden").image;
                    sceneVisiblityStyle.hover.textColor = Color.white;
                    sceneVisiblityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_hidden_hover").image;

                    if (objectVisibility.MixedDescendantVisibility)
                    {
                        sceneVisiblityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_hidden-mixed").image;
                        sceneVisiblityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_hidden-mixed_hover").image;
                    }

                    GUI.Box(hierarchyRects.visibilityRect, GUIContent.none, sceneVisiblityStyle);
                }
                else if (objectVisibility.MixedDescendantVisibility)
                {
                    sceneVisiblityStyle.normal.textColor = Color.white;
                    sceneVisiblityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_visible-mixed").image;
                    sceneVisiblityStyle.hover.textColor = Color.white;
                    sceneVisiblityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_visible-mixed_hover").image;

                    GUI.Box(hierarchyRects.visibilityRect, GUIContent.none, sceneVisiblityStyle);
                }

                if (!objectVisibility.Pickable)
                {
                    scenePickabilityStyle.normal.textColor = Color.white;
                    scenePickabilityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenepicking_notpickable").image;
                    scenePickabilityStyle.hover.textColor = Color.white;
                    scenePickabilityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenepicking_notpickable_hover").image;

                    if (objectVisibility.MixedDescendantPickability)
                    {
                        scenePickabilityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_notpickable-mixed").image;
                        scenePickabilityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_notpickable-mixed_hover").image;
                    }

                    GUI.Box(hierarchyRects.pickabilityRect, GUIContent.none, scenePickabilityStyle);
                }
                else if (objectVisibility.MixedDescendantPickability)
                {
                    scenePickabilityStyle.normal.textColor = Color.white;
                    scenePickabilityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_pickable-mixed").image;
                    scenePickabilityStyle.hover.textColor = Color.white;
                    scenePickabilityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_pickable_hover").image;

                    GUI.Box(hierarchyRects.pickabilityRect, GUIContent.none, scenePickabilityStyle);
                }

                var mouseY = Event.current.mousePosition.y;
                if (objectVisibility.NoIcon && hierarchyRects.selectionRect.y < mouseY && mouseY < hierarchyRects.selectionRect.y + hierarchyRects.selectionRect.height)
                {
                    sceneVisiblityStyle.normal.textColor = Color.white;
                    sceneVisiblityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_visible").image;
                    sceneVisiblityStyle.hover.textColor = Color.white;
                    sceneVisiblityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenevis_visible_hover").image;

                    scenePickabilityStyle.normal.textColor = Color.white;
                    scenePickabilityStyle.normal.background = (Texture2D)EditorGUIUtility.IconContent("d_scenepicking_pickable").image;
                    scenePickabilityStyle.hover.textColor = Color.white;
                    scenePickabilityStyle.hover.background = (Texture2D)EditorGUIUtility.IconContent("d_scenepicking_pickable_hover").image;

                    GUI.Box(hierarchyRects.visibilityRect, GUIContent.none, sceneVisiblityStyle);
                    GUI.Box(hierarchyRects.pickabilityRect, GUIContent.none, scenePickabilityStyle);
                }
            }

            if (isPrefab)
            {
                // Navigation
                var icon = EditorGUIUtility.IconContent("ArrowNavigationRight");
                var navigationStyle = new GUIStyle(GUI.skin.GetStyle("ArrowNavigationRight"));
                GUI.Box(hierarchyRects.prefabLinkRect, icon, navigationStyle);

                // Prefab Line
                EditorGUI.DrawRect(hierarchyRects.prefabHighlightRect, new Color(0.059f, 0.502f, 0.745f));
            }

            currentIdentLevel = identLevel;
        }

        private static void ListenToEvents(int instanceID, HierarchyRects hierarchyRects, HierarchyObjectData objectData)
        {
            if (Event.current.alt == true && Event.current.button == 0 && Event.current.type == EventType.MouseUp && hierarchyRects.selectionRect.Contains(Event.current.mousePosition))
            {
                PopupWindow.Show(hierarchyRects.selectionRect, new HierarchyModalWindow(instanceID, objectDataDict));
            }
        }

        private static int GetIdentLevel(Rect selectionRect)
        {
            return ((int)selectionRect.x - 60) / 14;
        }

        private static Color GetDefaultBackgroundColor()
        {
            float num = 0.22f;
            return new Color(num, num, num, 1f);
        }

        private static Color GetDefaultToolbarColor()
        {
            float num = 0.19f;
            return new Color(num, num, num, 1f);
        }

        public struct HierarchyRects
        {
            public Rect selectionRect;
            public Rect backgroundRect;
            public Rect fullLineRect;
            public Rect toolbarRect;
            public Rect visibilityRect;
            public Rect pickabilityRect;
            public Rect foldoutRect;
            public Rect prefabLinkRect;
            public Rect prefabHighlightRect;
            public Rect layerNameRect;
            public Rect[] identLevelRects;

            public HierarchyRects(Rect selectionRect, HierarchySettings settings)
            {
                // Default Selection
                this.selectionRect = selectionRect;
                // Background - Line Without Toolbar Section
                backgroundRect = new Rect(selectionRect);
                backgroundRect.width += backgroundRect.x - 32 + 16;
                backgroundRect.x = 32;
                // Full Line
                fullLineRect = new Rect(backgroundRect)
                {
                    x = 0,
                };
                fullLineRect.width += 32;
                // Toolbar Area
                toolbarRect = new Rect(fullLineRect)
                {
                    width = 32,
                };
                // Visibility Icon Area
                visibilityRect = new Rect(selectionRect)
                {
                    x = 0,
                };
                visibilityRect.width = 16;
                // Pickability Icon Area
                pickabilityRect = new Rect(visibilityRect)
                {
                    x = 16,
                };
                // Fouldout Area
                foldoutRect = new Rect(selectionRect)
                {
                    width = 11,
                };
                foldoutRect.x -= 14;
                // Prefab Link Area
                prefabLinkRect = new Rect(selectionRect);
                prefabLinkRect.x += prefabLinkRect.width;
                prefabLinkRect.width = 16;
                // Prefab Highligh
                prefabHighlightRect = new Rect(32, selectionRect.y + 1, 2, 14);
                // Layer Name
                layerNameRect = new Rect();
                if (settings.UseLayerNames)
                {
                    layerNameRect = new Rect(selectionRect);
                    layerNameRect.x += layerNameRect.width - 48;
                    layerNameRect.width = 48;
                    this.selectionRect.width -= layerNameRect.width;
                }
                // Ident Levels
                int identLevels = GetIdentLevel(selectionRect);
                identLevelRects = new Rect[identLevels];
                for (int i = 0; i < identLevels; i++)
                {
                    identLevelRects[i] = new Rect(foldoutRect);
                    identLevelRects[i].x = 46 + (i * 14) + 6;
                    identLevelRects[i].width = 1;
                }
            }
        }
    }
}