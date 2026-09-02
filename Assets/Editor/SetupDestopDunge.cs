#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor helper: สร้าง Scene, Prefab ตัวอย่าง, CharacterData asset และ GameObjects ที่ต้องการ
// เมนู: DestopDunge -> Setup Project (1-4)
public static class SetupDestopDunge
{
    [MenuItem("DestopDunge/Setup Project (1-4)")]
    public static void SetupProject()
    {
        // create folders if missing
        CreateFolderIfMissing("Assets", "Scenes");
        CreateFolderIfMissing("Assets", "Prefabs");
        CreateFolderIfMissing("Assets", "Resources");

        // create placeholder prefabs
        string desktopPrefabPath = "Assets/Prefabs/DesktopPlaceholder.prefab";
        string combatPrefabPath = "Assets/Prefabs/CombatPlaceholder.prefab";

        GameObject desktopPrefab = CreateOrReplacePrimitivePrefab(desktopPrefabPath, PrimitiveType.Sphere, "Desktop_Placeholder", Vector3.one * 0.5f);
        GameObject combatPrefab = CreateOrReplacePrimitivePrefab(combatPrefabPath, PrimitiveType.Cube, "Combat_Placeholder", Vector3.one);

        // create CharacterData asset
        string characterAssetPath = "Assets/Resources/DefaultCharacter.asset";
        CharacterData charAsset = AssetDatabase.LoadAssetAtPath<CharacterData>(characterAssetPath);
        if (charAsset == null)
        {
            charAsset = ScriptableObject.CreateInstance<CharacterData>();
            charAsset.characterName = "DefaultPet";
            charAsset.characterClass = CharacterClass.Monster;
            charAsset.desktopPrefab = desktopPrefab;
            charAsset.desktopIcon = null;
            charAsset.hatchTime = 60f;
            charAsset.growthTime = 300f;
            charAsset.combatPrefab = combatPrefab;
            charAsset.combatAnimatorController = null;
            charAsset.maxHP = 100;
            charAsset.attack = 10;
            charAsset.attackSpeed = 1f;

            AssetDatabase.CreateAsset(charAsset, characterAssetPath);
            Debug.Log("Created CharacterData asset at " + characterAssetPath);
        }
        else
        {
            Debug.Log("CharacterData already exists at " + characterAssetPath + ", skipping creation.");
        }

        AssetDatabase.SaveAssets();

        // create and setup scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // create camera
        GameObject camGO = new GameObject("Main Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(1f, 0f, 1f, 1f); // match default color key (magenta)
        camGO.transform.position = new Vector3(0, 0, -10);

        // create directional light simple
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // create DesktopWindowController object
        GameObject windowControllerGO = new GameObject("DesktopWindowController");
        DesktopWindowController windowController = windowControllerGO.AddComponent<DesktopWindowController>();
        // ensure transparencyColor matches camera
        windowController.transparencyColor = cam.backgroundColor;

        // create DesktopCharacterController object
        GameObject managerGO = new GameObject("DesktopCharacterController");
        DesktopCharacterController charController = managerGO.AddComponent<DesktopCharacterController>();
        charController.characterData = charAsset;

        // create ObjectPool for VFX/damage numbers
        GameObject poolGO = new GameObject("ObjectPool");
        var pool = poolGO.AddComponent<ObjectPool>();
        // setup default pools (prefabs may be null placeholders)
        pool.items = new ObjectPool.PoolItem[2];
        pool.items[0] = new ObjectPool.PoolItem() { id = "damage", prefab = null, initialSize = 16 };
        pool.items[1] = new ObjectPool.PoolItem() { id = "vfx", prefab = null, initialSize = 8 };

        // create ScreenShakeManager
        GameObject shakeGO = new GameObject("ScreenShakeManager");
        shakeGO.AddComponent<ScreenShakeManager>();

        // Save scene
        string scenePath = "Assets/Scenes/DesktopScene.unity";
        bool saved = EditorSceneManager.SaveScene(scene, scenePath);
        if (saved)
        {
            Debug.Log("Saved scene to " + scenePath);
        }
        else
        {
            Debug.LogWarning("Failed to save scene to " + scenePath);
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("DestopDunge Setup", "Setup complete.\n\nCreated:\n- Prefabs: " + desktopPrefabPath + ", " + combatPrefabPath + "\n- CharacterData: " + characterAssetPath + "\n- Scene: " + scenePath, "OK");
    }

    static void CreateFolderIfMissing(string parent, string newFolder)
    {
        string path = parent + "/" + newFolder;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, newFolder);
            Debug.Log("Created folder: " + path);
        }
    }

    static GameObject CreateOrReplacePrimitivePrefab(string prefabPath, PrimitiveType type, string name, Vector3 scale)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.localScale = scale;

        // remove collider for light-weight placeholder
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);

        // create prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        Debug.Log("Created prefab: " + prefabPath);
        return prefab;
    }
}
#endif
