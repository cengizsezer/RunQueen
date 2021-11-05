/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace PluginMaster
{
    public class PlayModeSave : EditorWindow
    {
        #region CONTEXT
        private const string TOOL_NAME = "Play Mode Save";
        [MenuItem("CONTEXT/Component/Save Now", true, 1201)]
        private static bool ValidateSaveNowMenu(MenuCommand command)
            => !PrefabUtility.IsPartOfPrefabAsset(command.context) && Application.IsPlaying(command.context);
        [MenuItem("CONTEXT/Component/Save Now", false, 1201)]
        private static void SaveNowMenu(MenuCommand command)
            => Add(command.context as Component, SaveCommand.SAVE_NOW, false, true);

        [MenuItem("CONTEXT/Component/Save When Exiting Play Mode", true, 1202)]
        private static bool ValidateSaveOnExtiMenu(MenuCommand command) => ValidateSaveNowMenu(command);
        [MenuItem("CONTEXT/Component/Save When Exiting Play Mode", false, 1202)]
        private static void SaveOnExitMenu(MenuCommand command)
            => Add(command.context as Component, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, false, true);

        [MenuItem("CONTEXT/Component/Always Save When Exiting Play Mode", true, 1203)]
        private static bool ValidateAlwaysSaveOnExitMenu(MenuCommand command)
            => PrefabUtility.IsPartOfPrefabAsset(command.context) ? false
            : !PMSData.Contains(command.context as Component);
        [MenuItem("CONTEXT/Component/Always Save When Exiting Play Mode", false, 1203)]
        private static void AlwaysSaveOnExitMenu(MenuCommand command)
            => Add(command.context as Component, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, true, true);

        [MenuItem("CONTEXT/Component/Remove From Save List", true, 1204)]
        private static bool ValidateRemoveFromSaveList(MenuCommand command)
            => PrefabUtility.IsPartOfPrefabAsset(command.context) ? false
            : PMSData.Contains(command.context as Component);
        [MenuItem("CONTEXT/Component/Remove From Save List", false, 1204)]
        private static void RemoveFromSaveList(MenuCommand command)
        {
            var component = command.context as Component;
            var key = new ComponentSaveDataKey(component);
            PMSData.Remove(key);
            _compData.Remove(key);
        }

        [MenuItem("CONTEXT/Component/Apply Play Mode Changes", true, 1205)]
        private static bool ValidateApplyMenu(MenuCommand command)
            => !Application.isPlaying && _compData.ContainsKey(GetKey(command.context));
        [MenuItem("CONTEXT/Component/Apply Play Mode Changes", false, 1205)]
        private static void ApplyMenu(MenuCommand command) => Apply(GetKey(command.context));

        [MenuItem("CONTEXT/Component/", false, 1300)]
        private static void Separator(MenuCommand command) { }

        [MenuItem("CONTEXT/ScriptableObject/Save Now", false, 1206)]
        private static void SaveScriptableObject(MenuCommand command)
        {
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(command.context);
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region WINDOW
        [MenuItem("Tools/Plugin Master/" + TOOL_NAME, false, int.MaxValue)]
        private static void ShowWindow() => GetWindow<PlayModeSave>(TOOL_NAME);

        private const string AUTO_SAVE_PREF = "PLAY_MODE_SAVE_autoSave";
        private const string AUTO_SAVE_PERIOD_PREF = "PLAY_MODE_SAVE_autoSavePeriod";
        private const string AUTO_APPLY_PREF = "PLAY_MODE_SAVE_autoApply";

        private static void LoadPrefs()
        {
            _autoSave = EditorPrefs.GetBool(AUTO_SAVE_PREF, false);
            _autoSavePeriod = EditorPrefs.GetInt(AUTO_SAVE_PERIOD_PREF, 1);
            _autoApply = EditorPrefs.GetBool(AUTO_APPLY_PREF, true);
        }

        private void OnEnable() => LoadPrefs();

        private void OnDisable()
        {
            EditorPrefs.SetBool(AUTO_SAVE_PREF, _autoSave);
            EditorPrefs.SetInt(AUTO_SAVE_PERIOD_PREF, _autoSavePeriod);
            EditorPrefs.SetBool(AUTO_APPLY_PREF, _autoApply);
        }

        private void OnSelectionChange() => Repaint();

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUIUtility.labelWidth = 70;
                    _autoSave = EditorGUILayout.ToggleLeft("Auto-Save Every:", _autoSave);
                    if (check.changed)
                    {
                        EditorPrefs.SetBool(AUTO_SAVE_PREF, _autoSave);
                        if (_autoSave) AutoSave();
                    }
                }
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    _autoSavePeriod = EditorGUILayout.IntSlider(_autoSavePeriod, 1, 10);
                    if (check.changed) EditorPrefs.SetInt(AUTO_SAVE_PERIOD_PREF, _autoSavePeriod);
                }
                GUILayout.Label("minutes");
                GUILayout.FlexibleSpace();
            }
            if (!Application.isPlaying)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var label = "Auto-Apply All Changes When Exiting Play Mode";
                        _autoApply = EditorGUILayout.ToggleLeft(label, _autoApply);
                        if (check.changed) EditorPrefs.SetBool(AUTO_APPLY_PREF, _autoApply);
                    }
                    if (!_autoApply)
                    {
                        if (_compData.Count == 0) EditorGUILayout.LabelField("Nothing to apply");
                        else
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Apply All Changes")) ApplyAll();
                                if (_objData.Count > 0)
                                    if (GUILayout.Button("Instatiate Objects Created In Play Mode")) Create();
                                GUILayout.FlexibleSpace();
                            }
                        }
                    }
                }
            }
            var selection = Selection.GetFiltered<GameObject>(SelectionMode.Editable
                | SelectionMode.ExcludePrefab | SelectionMode.TopLevel);
            if (selection.Length > 0)
            {
                void SaveSelection(SaveCommand cmd, bool always)
                {
                    foreach (var obj in selection)
                    {
                        var components = obj.GetComponentsInChildren<Component>();
                        foreach (var comp in components) Add(comp, cmd, always, true);
                    }
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("Execute for all selected objects and their children: ");
                    if (Application.isPlaying)
                    {
                        maxSize = minSize = new Vector2(376, 134);
                        if (GUILayout.Button("Save all components now"))
                            SaveSelection(SaveCommand.SAVE_NOW, true);
                        if (GUILayout.Button("Save all components when exiting play mode"))
                            SaveSelection(SaveCommand.SAVE_ON_EXITING_PLAY_MODE, false);
                    }
                    else maxSize = minSize = new Vector2(376, _autoApply ? 116 : 136);
                    if (GUILayout.Button("Always save all components when exiting play mode"))
                        SaveSelection(SaveCommand.SAVE_ON_EXITING_PLAY_MODE, true);
                    if (GUILayout.Button("Remove all components from save list"))
                    {
                        foreach (var obj in selection)
                        {
                            var components = obj.GetComponentsInChildren<Component>();
                            foreach (var comp in components)
                            {
                                var key = new ComponentSaveDataKey(comp);
                                PMSData.Remove(key);
                                _compData.Remove(key);
                            }
                        }
                    }
                }
            }
            else maxSize = minSize = new Vector2(376, _autoApply ? 50 : 70);
        }
        #endregion

        #region SAVE
        private static bool _autoApply = true;
        private static bool _autoSave = false;
        private static int _autoSavePeriod = 1;
        private static List<string> _scenesToOpen = new List<string>();
        [Serializable]
        private class PMSData
        {
            private const string FILE_NAME = "PMSData";
            private const string RELATIVE_PATH = "/PluginMaster/PlayModeSave/Editor/Resources/";
            [SerializeField] private string _rootDirectory = null;
            [SerializeField] private List<ComponentSaveDataKey> _alwaysSave = new List<ComponentSaveDataKey>();
            public static ComponentSaveDataKey[] alwaysSaveArray => instance._alwaysSave.ToArray();
            public static void AlwaysSave(ComponentSaveDataKey key)
            {
                if (instance._alwaysSave.Contains(key)) return;
                instance._alwaysSave.Add(key);
                Save();
            }
            public static bool saveAfterLoading { get; set; }
            public static void Save()
            {
                if (string.IsNullOrEmpty(instance._rootDirectory)) instance._rootDirectory = Application.dataPath;
                var fullDirectoryPath = instance._rootDirectory + RELATIVE_PATH;
                var fullFilePath = fullDirectoryPath + FILE_NAME + ".txt";
                if (!File.Exists(fullFilePath))
                {
                    var directories = Directory.GetDirectories(Application.dataPath,
                        "PluginMaster", SearchOption.AllDirectories);
                    if (directories.Length == 0) Directory.CreateDirectory(fullDirectoryPath);
                    else
                    {
                        instance._rootDirectory = Directory.GetParent(directories[0]).FullName;
                        fullDirectoryPath = instance._rootDirectory + RELATIVE_PATH;
                        fullFilePath = fullDirectoryPath + FILE_NAME + ".txt";
                    }
                    if (!Directory.Exists(fullDirectoryPath)) Directory.CreateDirectory(fullDirectoryPath);
                }
                var jsonString = JsonUtility.ToJson(instance);
                File.WriteAllText(fullFilePath, jsonString);
                AssetDatabase.Refresh();
            }
            public static void Remove(ComponentSaveDataKey item)
            {
                if (!Contains(item)) return;
                instance._alwaysSave.Remove(item);
                Save();
            }

            public static bool Load()
            {
                var jsonTextAsset = Resources.Load<TextAsset>(FILE_NAME);
                if (jsonTextAsset == null) return false;
                _instance = JsonUtility.FromJson<PMSData>(jsonTextAsset.text);
                _loaded = true;
                if (saveAfterLoading) Save();
                return true;
            }

            public static bool Contains(ComponentSaveDataKey key)
            {
                if (!_loaded) Load();
                return instance._alwaysSave.Contains(key);
            }

            private static PMSData _instance = null;
            private static bool _loaded = false;
            private PMSData() { }
            private static PMSData instance
            {
                get
                {
                    if (_instance == null) _instance = new PMSData();
                    return _instance;
                }
            }
        }

        private enum SaveCommand { SAVE_NOW, SAVE_ON_EXITING_PLAY_MODE }

        [Serializable]
        private struct ComponentSaveDataKey : ISerializationCallbackReceiver
        {
            [SerializeField] private int _objId;
            [SerializeField] private int _compId;
            [SerializeField] private string _globalObjId;
            [SerializeField] private string _globalCompId;
            [SerializeField] private string _scenePath;

            public int objId => _objId;
            public int compId => _compId;
            public string globalObjId => _globalObjId;
            public string globalCompId => _globalCompId;
            public string scenePath => _scenePath;
            
            public ComponentSaveDataKey(Component component)
            {
                _objId = component.gameObject.GetInstanceID();
                _compId = component.GetInstanceID();
                _globalObjId = GlobalObjectId.GetGlobalObjectIdSlow(component.gameObject).ToString();
                _globalCompId = GlobalObjectId.GetGlobalObjectIdSlow(component).ToString();
                _scenePath = component.gameObject.scene.path;
            }

            public static implicit operator ComponentSaveDataKey(Component component)
                => new ComponentSaveDataKey(component);

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize()
            {
                if (GlobalObjectId.TryParse(_globalObjId, out GlobalObjectId id))
                {
                    var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                    if (obj == null) return;
                    _objId = obj.GetInstanceID();
                }
                if (GlobalObjectId.TryParse(_globalCompId, out GlobalObjectId compId))
                {
                    var comp = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(compId) as Component;
                    if (comp == null) return;
                    _compId = comp.GetInstanceID();
                }
                PMSData.saveAfterLoading = true;
            }
        }

        private class SaveDataValue
        {
            public SerializedObject serializedObj;
            public SaveCommand saveCmd;
            public Type componentType;
            public SaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd, Type componentType)
                => (this.serializedObj, this.saveCmd, this.componentType) = (serializedObj, saveCmd, componentType);
            public virtual void Update(int componentId) => serializedObj.Update();
        }
        private class SpriteRendererSaveDataValue : SaveDataValue
        {
            public int sortingOrder;
            public int sortingLayerID;
            public SpriteRendererSaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd,
                int sortingOrder, int sortingLayerID) : base(serializedObj, saveCmd, typeof(SpriteRenderer))
                => (this.sortingOrder, this.sortingLayerID) = (sortingOrder, sortingLayerID);

            public override void Update(int componentId)
            {
                base.Update(componentId);
                var spriteRenderer = EditorUtility.InstanceIDToObject(componentId) as SpriteRenderer;
                sortingOrder = spriteRenderer.sortingOrder;
                sortingLayerID = spriteRenderer.sortingLayerID;
            }
        }

        private struct ObjectDataKey
        {
            [SerializeField] private int _objId;
            [SerializeField] private string _globalObjId;
            [SerializeField] private string _scenePath;
            public int objId => _objId;
            public string globalObjId => _globalObjId;
            public string scenePath => _scenePath;
            public ObjectDataKey(GameObject gameObject)
            {
                if (gameObject == null)
                {
                    _objId = -1;
                    _globalObjId = null;
                    _scenePath = null;
                    return;
                }
                _objId = gameObject.GetInstanceID();
                _globalObjId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();
                _scenePath = gameObject.scene.path;
            }
        }
        private class SaveObject
        {
            public SaveCommand saveCmd;
            public string name = null;
            public string tag = null;
            public int layer = 0;
            public bool isStatic = false;
            public bool always = false;
            public List<SaveObject> saveChildren = null;
            public Dictionary<Type, List<SaveDataValue>> compDataDictionary = new Dictionary<Type, List<SaveDataValue>>();
            public bool isRoot = true;
            public ObjectDataKey parentKey;
            public string prefabPath = null;
            public int siblingIndex = -1;
            public SaveObject(Transform transform, SaveCommand cmd)
            {
                var obj = transform.gameObject;
                var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                if(prefabRoot == obj)
                    prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                name = obj.name;
                tag = obj.tag;
                layer = obj.layer;
                isStatic = obj.isStatic;
                isRoot = transform.parent == null;
                var parent = isRoot ? null : transform.parent.gameObject;
                parentKey = new ObjectDataKey(parent);
                siblingIndex = obj.transform.GetSiblingIndex();
                saveCmd = cmd;
                var components = obj.GetComponents<Component>();
                foreach (var comp in components)
                {
                    var data = new SerializedObject(comp);
                    var type = comp.GetType();
                    if (!compDataDictionary.ContainsKey(type)) compDataDictionary.Add(type, new List<SaveDataValue>());
                    compDataDictionary[type].Add(new SaveDataValue(data, saveCmd, comp.GetType()));
                    var prop = new SerializedObject(comp).GetIterator();
                    while (prop.NextVisible(true)) data.CopyFromSerializedProperty(prop);
                }
                var children = obj.GetComponentsInChildren<Transform>();
                if (children.Length == 1) return;
                saveChildren = new List<SaveObject>();
                foreach(var child in children)
                {
                    if (child == transform) continue;
                    var saveChild = new SaveObject(child, cmd);
                    saveChildren.Add(saveChild);
                }
            }

            public Type[] types => compDataDictionary.Keys.ToArray();

            public void Update()
            {
                void UpdateComponents(SaveObject saveObj)
                {
                    foreach(var dataList in saveObj.compDataDictionary.Values)
                        foreach (var data in dataList) data.serializedObj.Update();
                    if (saveObj.saveChildren == null) return;
                    foreach (var child in saveObj.saveChildren) UpdateComponents(child);
                }
                UpdateComponents(this);
            }
        }
        private static Dictionary<ObjectDataKey, SaveObject> _objData
            = new Dictionary<ObjectDataKey, SaveObject>();


        private static Dictionary<ComponentSaveDataKey, SaveDataValue> _compData
            = new Dictionary<ComponentSaveDataKey, SaveDataValue>();
        private static ComponentSaveDataKey GetKey(UnityEngine.Object comp)
            => new ComponentSaveDataKey(comp as Component);

        private static void Add(Component component, SaveCommand cmd, bool always, bool serialize)
        {
            var scenePath = component.gameObject.scene.path;
            if (!_scenesToOpen.Contains(scenePath)) _scenesToOpen.Add(scenePath);
            var key = new ComponentSaveDataKey(component);
            if (always) PMSData.AlwaysSave(key);
            var data = new SerializedObject(component);
            SaveDataValue GetValue()
            {
                if (!serialize) return new SaveDataValue(null, cmd, typeof(Component));
                if (component is SpriteRenderer)
                {
                    var spriteRenderer = component as SpriteRenderer;
                    return new SpriteRendererSaveDataValue(data, cmd, spriteRenderer.sortingOrder,
                        spriteRenderer.sortingLayerID);
                }
                else return new SaveDataValue(data, cmd, component.GetType());
            }
            if (_compData.ContainsKey(key)) _compData[key] = GetValue();
            else _compData.Add(key, GetValue());
            var prop = new SerializedObject(component).GetIterator();
            while (prop.NextVisible(true)) data.CopyFromSerializedProperty(prop);

            var objKey = new ObjectDataKey(component.gameObject);
            var saveObj = new SaveObject(component.transform, cmd);
            if (_objData.ContainsKey(objKey)) _objData[objKey] = saveObj;
            else _objData.Add(objKey, saveObj);
            EditorApplication.RepaintHierarchyWindow();
        }

        async static void AutoSave()
        {
            if (!EditorApplication.isPlaying) return;
            if (!_autoSave) return;
            foreach (var data in _compData) data.Value.Update(data.Key.compId);
            await Task.Delay(_autoSavePeriod * 60000);
            AutoSave();
        }

        private static void Apply(ComponentSaveDataKey key)
        {
            var obj = EditorUtility.InstanceIDToObject(key.objId) as GameObject;
            if (obj == null)
            {
                if (GlobalObjectId.TryParse(key.globalObjId, out GlobalObjectId id))
                    obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
            }
            if (obj == null) return;
            var comp = EditorUtility.InstanceIDToObject(key.compId) as Component;
            if (comp == null)
            {
                if (GlobalObjectId.TryParse(key.globalCompId, out GlobalObjectId id))
                    comp = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as Component;
            }
            if (comp == null) return;
            var data = _compData[key].serializedObj;
            var serializedObj = new SerializedObject(comp);
            var prop = data.GetIterator();
            while (prop.NextVisible(true)) serializedObj.CopyFromSerializedProperty(prop);
            serializedObj.ApplyModifiedProperties();
            if (_compData[key] is SpriteRendererSaveDataValue)
            {
                var spriteRendererData = _compData[key] as SpriteRendererSaveDataValue;
                var spriteRenderer = EditorUtility.InstanceIDToObject(key.compId) as SpriteRenderer;
                spriteRenderer.sortingOrder = spriteRendererData.sortingOrder;
                spriteRenderer.sortingLayerID = spriteRendererData.sortingLayerID;
            }
            if (!PMSData.Contains(key)) _compData.Remove(key);
        }

        private static void ApplyAll()
        {
            var comIds = _compData.Keys.ToArray();
            foreach (var id in comIds) Apply(id);
            Create();
        }

        private static void Create()
        {
            GameObject FindObject(ObjectDataKey key)
            {
                var obj = EditorUtility.InstanceIDToObject(key.objId) as GameObject;
                if (obj == null)
                {
                    if (GlobalObjectId.TryParse(key.globalObjId, out GlobalObjectId id))
                        obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                }
                return obj;
            }
            void CreateObj(SaveObject saveObj, Transform parent, string scenePath)
            {
                var scene = EditorSceneManager.GetSceneByPath(scenePath);
                if (scene.IsValid()) EditorSceneManager.SetActiveScene(scene);
                else return;
                GameObject obj = null;
                var isPrefabInstance = saveObj.prefabPath != null;
                if (isPrefabInstance)
                {
                    var prefab = AssetDatabase.LoadMainAssetAtPath(saveObj.prefabPath);
                    obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                }
                else
                {
                    if(parent != null && saveObj.siblingIndex >= 0 && parent.childCount > saveObj.siblingIndex)
                    {
                        var t = parent.GetChild(saveObj.siblingIndex);
                        if (t != null && t.name == saveObj.name) obj = t.gameObject;
                    }
                    if(obj == null) obj = new GameObject(saveObj.name);
                }
                Undo.RegisterCreatedObjectUndo(obj, "Save Object Created In Play Mode");
                obj.transform.parent = parent;
                obj.isStatic = saveObj.isStatic;
                obj.tag = saveObj.tag;
                obj.layer = saveObj.layer;
                foreach (var type in saveObj.compDataDictionary.Keys)
                {
                    var compDataList = saveObj.compDataDictionary[type];
                    var components = obj.GetComponents(type);
                    for (int i = 0; i < compDataList.Count; ++i)
                    {
                        var compData = compDataList[i];
                        var comp = obj.GetComponent(type);
                        if(comp == null || components.Length < i + 1) comp = obj.AddComponent(type);
                        if (components.Length > i) comp = components[i];
                        var serializedObj = new SerializedObject(comp);
                        var prop = compData.serializedObj.GetIterator();
                        while (prop.NextVisible(true)) serializedObj.CopyFromSerializedProperty(prop);
                        serializedObj.ApplyModifiedProperties();
                    }
                }
                if (saveObj.saveChildren == null) return;
                foreach (var child in saveObj.saveChildren) CreateObj(child, obj.transform, scenePath);
            }
            foreach (var key in _objData.Keys)
            {
                var root = FindObject(key);
                if (root != null) continue;
                var data = _objData[key];
                Transform rootParent = null;
                if(!data.isRoot)
                {
                    var rootParentObj = FindObject(data.parentKey);
                    if (rootParentObj == null) continue;
                    rootParent = rootParentObj.transform;
                }
                CreateObj(data, rootParent, key.scenePath);
            }
            _objData.Clear();
        }

        [InitializeOnLoad]
        private static class ApplicationEventHandler
        {
            private static GameObject autoApplyFlag = null;
            private const string AUTO_APPLY_OBJECT_NAME = "PlayModeSave_AutoApply";
            private static Texture2D _icon = Resources.Load<Texture2D>("Save");
            private static string _currentScenePath = null;
            private static bool _loadData = true;
            static ApplicationEventHandler()
            {
                EditorApplication.playModeStateChanged += OnStateChanged;
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCallback;
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                LoadPrefs();
                EditorSceneManager.sceneOpened += OnSceneOpened;
            }

            static void OnHierarchyChanged()
            {
                var activeScene = EditorSceneManager.GetActiveScene();
                string activeScenePath = null;
                if (activeScene != null) activeScenePath = activeScene.path;
                if (!_loadData && _currentScenePath == activeScenePath) return;
                if (_currentScenePath != activeScenePath) _currentScenePath = activeScenePath;
                LoadData();
            }

            static private void OnSceneOpened(Scene scene, OpenSceneMode mode)
            {
                _scenesToOpen.Remove(scene.path);
                if (_scenesToOpen.Count > 0) return;
                PlayModeSave.ApplyAll();
                EditorSceneManager.SaveOpenScenes();
                
            }

            private static void LoadData()
            {
                if (!PMSData.Load()) return;
                foreach (var key in PMSData.alwaysSaveArray)
                {
                    var obj = EditorUtility.InstanceIDToObject(key.objId) as GameObject;
                    var comp = EditorUtility.InstanceIDToObject(key.compId) as Component;

                    if (obj == null || comp == null)
                    {
                        if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(key.scenePath)))
                            PMSData.Remove(key);
                        else
                        {
                            var activeScene = EditorSceneManager.GetActiveScene();
                            if (activeScene.path == key.scenePath) PMSData.Remove(key);
                        }
                        continue;
                    }
                    Add(comp, SaveCommand.SAVE_ON_EXITING_PLAY_MODE, false, Application.isPlaying);
                }
                EditorApplication.RepaintHierarchyWindow();
            }

            private static void OnStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingEditMode && _autoApply)
                {
                    autoApplyFlag = new GameObject(AUTO_APPLY_OBJECT_NAME);
                    autoApplyFlag.hideFlags = HideFlags.HideAndDontSave;
                    return;
                }
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    foreach (var data in _compData)
                    {
                        if (data.Value.saveCmd == SaveCommand.SAVE_NOW) continue;
                        data.Value.Update(data.Key.compId);
                    }
                    foreach(var data in _objData.Values)
                    {
                        if (data.saveCmd == SaveCommand.SAVE_NOW) continue;
                        data.Update();
                    }
                    return;
                }
                if (state == PlayModeStateChange.EnteredPlayMode)
                {
                    _scenesToOpen.Clear();
                    if (_autoSave) AutoSave();
                    return;
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    var scenesToOpen = _scenesToOpen.ToArray();
                    var openedSceneCount = EditorSceneManager.sceneCount;
                    var openedScenes = new List<string>();
                    for (int i = 0; i < openedSceneCount; ++i) openedScenes.Add(SceneManager.GetSceneAt(i).path);
                    bool applyAll = true;
                    foreach(var scenePath in scenesToOpen)
                    {
                        if(openedScenes.Contains(scenePath))
                        {
                            _scenesToOpen.Remove(scenePath);
                            continue;
                        }
                        applyAll = false;
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    }
                    _loadData = true;
                    autoApplyFlag = GameObject.Find(AUTO_APPLY_OBJECT_NAME);
                    _autoApply = autoApplyFlag != null;
                    if (_autoApply)
                    {
                        DestroyImmediate(autoApplyFlag);
                        if(applyAll) PlayModeSave.ApplyAll();
                    }
                }
            }

            private static void HierarchyItemCallback(int instanceID, Rect selectionRect)
            {
                var data = _compData;
                var keys = _compData.Keys.Where(k => k.objId == instanceID).ToArray();
                if (keys.Length == 0) return;
                if (_icon == null) _icon = Resources.Load<Texture2D>("Save");
                var rect = new Rect(selectionRect.xMax - 10, selectionRect.y + 2, 11, 11);
                GUI.Box(rect, _icon, GUIStyle.none);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0
                    && rect.Contains(Event.current.mousePosition))
                {
                    var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                    var compNames = obj.name + " components to save: ";
                    foreach (var key in keys)
                    {
                        if (key.objId != instanceID) continue;
                        var comp = EditorUtility.InstanceIDToObject(key.compId) as Component;
                        if (comp == null) continue;
                        compNames += comp.GetType().Name + ", ";
                    }
                    compNames = compNames.Remove(compNames.Length - 2);
                    Debug.Log(compNames);
                }
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        #endregion
    }
}