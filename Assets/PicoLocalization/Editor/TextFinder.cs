using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Plugins.TMPro
{
    public static class TextFinder
    {
        private static readonly List<UsageData> textUsagesData = new List<UsageData>();
        private static readonly Dictionary<string, UsageData> dictString = new Dictionary<string, UsageData>();
        private static readonly List<GameObject> prefabUsages = new List<GameObject>();

        private static readonly string[] targetPrefabFolders = new[] {"Assets"};
        private const string stringTable = "StringTable";

        [MenuItem("Tools/Porting Tool/Localization/1-0 ClearStringTable")]
        public static void ClearStringTable()
        {
            var stringTableCollection = AssetDatabase.LoadAssetAtPath<StringTableCollection>("Assets/PicoLocalization/Tables/StringTable.asset");
            stringTableCollection.ClearAllEntries();
        }
        
        [MenuItem("Tools/Porting Tool/Localization/1-1 FindAllTextKeys")]
        public static void FindAllTextKeys()
        {
            var startTime = Time.time;
            textUsagesData.Clear();
            
            FindAllTextInPrefabs();
            FindAllTextInScene();
            
            if (textUsagesData.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder($"FindAllTextKeys: ");
                foreach (var usage in textUsagesData)
                {
                    stringBuilder.AppendLine(usage.ToString());
                }

                stringBuilder.AppendLine($"Total count = {textUsagesData.Count}");
                Debug.Log(stringBuilder);
                
                dictString.Clear();
                stringBuilder = new StringBuilder($"ToDictionary:");
                foreach (var usage in textUsagesData)
                {
                    if (!dictString.ContainsKey(usage.Text))
                    {
                        dictString.Add(usage.Text, usage);
                        stringBuilder.AppendLine(usage.ToString());
                    }
                }
                stringBuilder.AppendLine($"Total count = {dictString.Count}, During = {Time.time - startTime}");
                Debug.Log(stringBuilder);

                TrimAllTextKeys();
            }
        }

        [MenuItem("Tools/Porting Tool/Localization/1-2 UpdateStringTable")]
        public static void UpdateStringTable()
        {
            var stringTableCollection = AssetDatabase.LoadAssetAtPath<StringTableCollection>("Assets/PicoLocalization/Tables/StringTable.asset");

            foreach (var usage in dictString)
            {
                stringTableCollection.StringTables[1].AddEntry(usage.Key, usage.Key);    
            }
        }

        [MenuItem("Tools/Porting Tool/Localization/2-1 FindTextUsagesInPrefab")]
        public static void FindTextUsagesInPrefab()
        {
            var allPrefabGUIDs = AssetDatabase.FindAssets("t:prefab", targetPrefabFolders);
            if (allPrefabGUIDs == null || allPrefabGUIDs.Length == 0)
            {
                Debug.LogError("There is no prefab under the Assets folder");
                return;
            }

            prefabUsages.Clear();
            
            StringBuilder stringBuilder = new StringBuilder("FindTextUsagesInPrefab:");
            foreach (var guid in allPrefabGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab.GetComponentInChildren<TMP_Text>(true))
                {
                    prefabUsages.Add(prefab);
                    stringBuilder.AppendLine(assetPath);
                }
            }
            Debug.Log(stringBuilder);
        }
        
        [MenuItem("Tools/Porting Tool/Localization/2-2 AddLocalizationToPrefab")]
        public static void AddLocalizationToPrefab()
        {
            if (prefabUsages.Count == 0)
            {
                Debug.LogError("There is no prefab usages, please run FindTextUsagesInPrefab first");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder("AddLocalizationToPrefab:");
            var tmpComponents = new List<TMP_Text>();
            foreach (var prefab in prefabUsages)
            {
                tmpComponents.Clear();
                prefab.transform.GetComponentsInChildren(true, tmpComponents);
                if (tmpComponents.Count == 0)
                {
                    Debug.LogError($"Can not find the TMP_Text component on {prefab}");
                    continue;
                }
                
                foreach (var tmpComponent in tmpComponents)
                {
                    if (!NeedLocalized(tmpComponent.text))
                    {
                        continue;
                    }
                    
                    var localizeStringEvent = tmpComponent.GetComponent<LocalizeStringEvent>();
                    if (localizeStringEvent == null && PrefabUtility.GetPrefabAssetType(tmpComponent.gameObject) != PrefabAssetType.Variant)
                    {
                        localizeStringEvent = tmpComponent.gameObject.AddComponent<LocalizeStringEvent>();
                        stringBuilder.AppendLine($"Prefab = {prefab}, gameObject = {tmpComponent.name}, text = {tmpComponent.text}");
                    }
                    localizeStringEvent.SetTable(stringTable);
                    localizeStringEvent.SetEntry(tmpComponent.text);
                    EditorUtility.SetDirty(localizeStringEvent);
                    EditorUtility.SetDirty(prefab);
                    AssetDatabase.SaveAssetIfDirty(prefab);
                }
            }
            Debug.Log(stringBuilder);
        }

        [MenuItem("Tools/Porting Tool/Localization/2-3 AddLocalizationToScene")]
        public static void AddLocalizationToScene()
        {
            StringBuilder stringBuilder = new StringBuilder("AddLocalizationToScene:");

            var curScenePath = SceneManager.GetActiveScene().path;
            var scenes = EditorBuildSettings.scenes;

            foreach (var item in scenes)
            {
                var scene = EditorSceneManager.OpenScene(item.path);

                var tmpComponents = Resources.FindObjectsOfTypeAll<TMP_Text>();
                foreach (var tmpComponent in tmpComponents)
                {
                    if (!NeedLocalized(tmpComponent.text))
                    {
                        continue;
                    }

                    var localizeStringEvent = tmpComponent.GetComponent<LocalizeStringEvent>();
                    if (localizeStringEvent == null)
                    {
                        localizeStringEvent = tmpComponent.gameObject.AddComponent<LocalizeStringEvent>();
                    }
                    localizeStringEvent.SetTable(stringTable);
                    localizeStringEvent.SetEntry(tmpComponent.text);
                    EditorUtility.SetDirty(localizeStringEvent);
                
                    stringBuilder.AppendLine($"Scene = {item.path}, gameObject = {tmpComponent.name}, text = {tmpComponent.text}");
                }
                
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(curScenePath);
            
            Debug.Log(stringBuilder);
        }

        [MenuItem("Tools/Porting Tool/Localization/3-1 FixedPrefabLocaleStringEvent")]
        public static void FixedPrefabLocaleStringEvent()
        {
            if (prefabUsages.Count == 0)
            {
                Debug.LogError("There is no prefab usages, please run FindTextUsagesInPrefab first");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder("FixedPrefabLocaleStringEvent:");
            var components = new List<LocalizeStringEvent>();
            foreach (var prefab in prefabUsages)
            {
                components.Clear();
                prefab.transform.GetComponentsInChildren(true, components);
                if (components.Count == 0)
                {
                    continue;
                }
                
                foreach (var component in components)
                {
                    var tmpText = component.GetComponent<TMP_Text>();
                    var prefabType = PrefabUtility.GetPrefabAssetType(component.gameObject);
                    if (tmpText != null && prefabType != PrefabAssetType.Variant)
                    {
                        var setStringMethod = tmpText.GetType().GetProperty("text").GetSetMethod();
                        var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), tmpText, setStringMethod) as UnityAction<string>;
                        Events.UnityEventTools.AddPersistentListener(component.OnUpdateString, methodDelegate);
                        component.OnUpdateString.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
                        
                        EditorUtility.SetDirty(component);
                        EditorUtility.SetDirty(prefab);
                        AssetDatabase.SaveAssetIfDirty(prefab);
                        stringBuilder.AppendLine($"Prefab = {prefab}, type = {prefabType}, gameObject = {component.name}");
                    }
                }
            }
            Debug.Log(stringBuilder);
        }
        
        [MenuItem("Tools/Porting Tool/Localization/3-2 FixedSceneLocaleStringEvent")]
        public static void FixedSceneLocaleStringEvent()
        {
            StringBuilder stringBuilder = new StringBuilder("AddLocalizationToScene:");

            var curScenePath = SceneManager.GetActiveScene().path;
            var scenes = EditorBuildSettings.scenes;

            foreach (var item in scenes)
            {
                var scene = EditorSceneManager.OpenScene(item.path);

                var components = Resources.FindObjectsOfTypeAll<LocalizeStringEvent>();
                foreach (var component in components)
                {
                    var tmpText = component.GetComponent<TMP_Text>();
                    var prefabType = PrefabUtility.GetPrefabAssetType(component.gameObject);
                    if (tmpText != null && prefabType != PrefabAssetType.Variant)
                    {
                        if (component.OnUpdateString.GetPersistentEventCount() == 0)
                        {
                            var setStringMethod = tmpText.GetType().GetProperty("text").GetSetMethod();
                            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), tmpText, setStringMethod) as UnityAction<string>;
                            Events.UnityEventTools.AddPersistentListener(component.OnUpdateString, methodDelegate);
                            component.OnUpdateString.SetPersistentListenerState(0, UnityEventCallState.EditorAndRuntime);
                        
                            EditorUtility.SetDirty(component);
                            stringBuilder.AppendLine($"Scene = {scene.path}, type = {prefabType}, gameObject = {component.name}");   
                        }
                    }
                }
                
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(curScenePath);
            
            Debug.Log(stringBuilder);
        }
        
        [MenuItem("Tools/Porting Tool/Localization/3-3 RemoveDuplicatedStringEventInPrefabs")]
        public static void RemoveDuplicatedStringEventInPrefabs()
        {
            if (prefabUsages.Count == 0)
            {
                Debug.LogError("There is no prefab usages, please run FindTextUsagesInPrefab first");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder("RemoveDuplicatedStringEvent:");
            var tmpComponents = new List<LocalizeStringEvent>();
            var children = new List<Transform>();
            foreach (var prefab in prefabUsages)
            {
                if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant)
                {
                    continue;
                }
                
                children.Clear();
                prefab.GetComponentsInChildren<Transform>(true, children);
                foreach (var child in children)
                {
                    if (PrefabUtility.GetPrefabAssetType(child) == PrefabAssetType.Variant)
                    {
                        continue;
                    }
                    
                    tmpComponents.Clear();
                    child.GetComponents<LocalizeStringEvent>(tmpComponents);
                    if (tmpComponents.Count <= 1)
                    {
                        continue;
                    }

                    for (int i = 1; i < tmpComponents.Count; i++)
                    {
                        var tmpComponent = tmpComponents[i];
                        Object.DestroyImmediate(tmpComponent, true);
                        EditorUtility.SetDirty(child);
                        EditorUtility.SetDirty(prefab);
                        AssetDatabase.SaveAssetIfDirty(prefab);
                        
                        stringBuilder.AppendLine($"Prefab = {prefab}, child = {child}");
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log(stringBuilder);
        }
        
        [MenuItem("Tools/Porting Tool/Localization/3-4 RemoveDuplicatedStringEventInScenes")]
        public static void RemoveDuplicatedStringEventInScenes()
        {
            StringBuilder stringBuilder = new StringBuilder("RemoveDuplicatedStringEventInScenes:");
            var tmpComponents = new List<LocalizeStringEvent>();

            var curScenePath = SceneManager.GetActiveScene().path;
            var scenes = EditorBuildSettings.scenes;
            
            foreach (var item in scenes)
            {
                var scene = EditorSceneManager.OpenScene(item.path);

                var children = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (var child in children)
                {
                    if (PrefabUtility.GetPrefabAssetType(child) == PrefabAssetType.Variant)
                    {
                        continue;
                    }
                    
                    tmpComponents.Clear();
                    child.GetComponents<LocalizeStringEvent>(tmpComponents);
                    if (tmpComponents.Count <= 1)
                    {
                        continue;
                    }

                    for (int i = 1; i < tmpComponents.Count; i++)
                    {
                        var tmpComponent = tmpComponents[i];
                        Object.DestroyImmediate(tmpComponent, true);

                        stringBuilder.AppendLine($"Scene = {scene}, child = {child}");
                    }
                }
                EditorSceneManager.SaveScene(scene);
            }
            EditorSceneManager.OpenScene(curScenePath);

            Debug.Log(stringBuilder);
        }
        
        [MenuItem("Tools/Porting Tool/Localization/3-5 FindMissingLocaleStringInPrefab")]
        public static void FindMissingLocaleStringInPrefab()
        {
            if (prefabUsages.Count == 0)
            {
                Debug.LogError("There is no prefab usages, please run FindTextUsagesInPrefab first");
                return;
            }

            LocalizationSettings.StringDatabase.MissingTranslationState = MissingTranslationBehavior.PrintWarning;
            
            StringBuilder stringBuilder = new StringBuilder("FindMissingLocaleStringInPrefab:");
            var components = new List<LocalizeStringEvent>();
            foreach (var prefab in prefabUsages)
            {
                components.Clear();
                prefab.transform.GetComponentsInChildren(true, components);
                if (components.Count == 0)
                {
                    continue;
                }
                
                foreach (var component in components)
                {
                    if (component.StringReference == null)
                    {
                        stringBuilder.AppendLine($"Prefab = {prefab}, gameObject = {component.name}");    
                    }
                    else
                    {

                        var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(component.StringReference.TableReference, component.StringReference.TableEntryReference);

                        if (localizedString == LocalizationSettings.StringDatabase.NoTranslationFoundMessage)
                        {
                            stringBuilder.AppendLine($"Prefab = {prefab}, gameObject = {component.name}");    
                        }
                    }
                }
            }
            Debug.Log(stringBuilder);
        }
        
        [MenuItem("Tools/Porting Tool/Localization/3-6 FindMissingLocaleStringInScene")]
        public static void FindMissingLocaleStringInScene()
        {
            StringBuilder stringBuilder = new StringBuilder("FindMissingLocaleStringInScene:");

            var curScenePath = SceneManager.GetActiveScene().path;
            var scenes = EditorBuildSettings.scenes;

            foreach (var item in scenes)
            {
                var scene = EditorSceneManager.OpenScene(item.path);

                var tmpComponents = Resources.FindObjectsOfTypeAll<LocalizeStringEvent>();
                foreach (var component in tmpComponents)
                {
                    if (component.StringReference == null)
                    {
                        stringBuilder.AppendLine($"Scene = {item}, gameObject = {component.name}");    
                    }
                    else
                    {
                        var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(component.StringReference.TableReference, component.StringReference.TableEntryReference);

                        if (localizedString == LocalizationSettings.StringDatabase.NoTranslationFoundMessage)
                        {
                            stringBuilder.AppendLine($"Scene = {item}, gameObject = {component.name}");    
                        }
                    }
                }
            }
            EditorSceneManager.OpenScene(curScenePath);
            
            Debug.Log(stringBuilder);
        }

        #region Inner Helper

        private static bool NeedLocalized(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.Log($"The text is null, maybe it was changed in runtime.");
                return false;
            }

            if (System.Text.Encoding.Default.GetByteCount(text) <= 1)
            {
                Debug.Log($"The text is one alphabet, maybe it should not be replaced.");
                return false;
            }
            
            if (text == "New Text" || text == "New Label")
            {
                Debug.Log($"The text is New Text, maybe it was changed in runtime.");
                return false;
            }

            text = Regex.Replace(text, @"<(\S*?)[^>]*>.*?|<.*? />", "");

            if (text.Contains("RSS") || text.Contains("Placeholder") || text.Contains("YYYY HH:MM:SS"))
            {
                Debug.Log($"The text {text} is RSS or Placeholder, maybe it was set in runtime.");
                return false;
            }

            if (!Regex.IsMatch(text, "[\u4e00-\u9fa5a-zA-Z]+"))
            {
                Debug.Log($"The text {text} is not contain word.");
                return false;
            }

            return true;
        }

        private static void FindAllTextInPrefabs()
        {
            var allPrefabGUIDs = AssetDatabase.FindAssets("t:prefab", targetPrefabFolders);
            if (allPrefabGUIDs == null || allPrefabGUIDs.Length == 0)
            {
                Debug.LogError("There is no prefab under the Assets folder");
                return;
            }
            
            var textMeshProsUGUI = new List<TMP_Text>();
            
            foreach (var guid in allPrefabGUIDs)
            {
                textMeshProsUGUI.Clear();
                
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                prefab.GetComponentsInChildren(true, textMeshProsUGUI);

                foreach (var tmpText in textMeshProsUGUI)
                {
                    if (string.IsNullOrEmpty(tmpText.text))
                    {
                        continue;
                    }
                    
                    UsageData usageData = new UsageData {Path = assetPath, Name = tmpText.name, Text = tmpText.text};
                    textUsagesData.Add(usageData);
                }
            }
        }
        
        private static void FindAllTextInScene()
        {
            StringBuilder stringBuilder = new StringBuilder("FindAllTextInScene:");

            var curScenePath = SceneManager.GetActiveScene().path;
            var scenes = EditorBuildSettings.scenes;

            foreach (var item in scenes)
            {
                var scene = EditorSceneManager.OpenScene(item.path);

                var tmpComponents = Resources.FindObjectsOfTypeAll<TMP_Text>();
                foreach (var tmpComponent in tmpComponents)
                {
                    if (string.IsNullOrEmpty(tmpComponent.text))
                    {
                        continue;
                    }
                    
                    UsageData usageData = new UsageData {Path = item.path, Name = tmpComponent.name, Text = tmpComponent.text};
                    textUsagesData.Add(usageData);

                    stringBuilder.AppendLine($"Scene = {item.path}, gameObject = {tmpComponent.name}, text = {tmpComponent.text}");
                }
            }
            EditorSceneManager.OpenScene(curScenePath);
            
            Debug.Log(stringBuilder);
        }

        private static void TrimAllTextKeys()
        {
            List<UsageData> needTrimKeys = new List<UsageData>();
            foreach (var usage in dictString)
            {
                if (!NeedLocalized(usage.Key))
                {
                    needTrimKeys.Add(usage.Value);
                }
            }

            StringBuilder stringBuilder = new StringBuilder($"TrimAllTextKeys: ");
            foreach (var usage in needTrimKeys)
            {
                dictString.Remove(usage.Text);
                stringBuilder.AppendLine(usage.ToString());
            }
            stringBuilder.AppendLine($"Total count = {needTrimKeys.Count}");
            Debug.Log(stringBuilder);
        }

        #endregion

        private class UsageData
        {
            public string Path;
            public string Name;
            public long Line;
            public string Text;

            public override string ToString()
            {
                return $"Path = {Path}, Line = {Line}, Text = {Text}";
            }
        }
    }
}


