#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the complete Dungeon Escape SampleScene from the setup guide.
/// Menu: Tools → Dungeon Escape → Setup Scene Now
/// Also auto-runs when RUN_SETUP.flag is present.
/// </summary>
[InitializeOnLoad]
public static class DungeonEscapeSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";
    private const string SquareSpritePath = "Assets/Art/Square.png";
    private const string FlagPath = "Assets/Scripts/Editor/RUN_SETUP.flag";
    private const string ResultPath = "Assets/Scripts/Editor/setup_result.txt";
    private const string PendingPrefsKey = "DungeonEscape_PendingSceneBuild";

    static DungeonEscapeSceneBuilder()
    {
        EditorApplication.delayCall += TryAutoRun;
    }

    [MenuItem("Tools/Dungeon Escape/Setup Scene Now")]
    public static void SetupSceneMenu()
    {
        BuildScene();
    }

    private static void TryAutoRun()
    {
        bool pending = EditorPrefs.GetBool(PendingPrefsKey, false);
        bool hasFlag = File.Exists(FlagPath);

        if (!pending && !hasFlag)
        {
            return;
        }

        if (hasFlag)
        {
            try { File.Delete(FlagPath); } catch { /* ignore */ }
        }

        // Wait until scripts finished compiling.
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += TryAutoRun;
            return;
        }

        BuildScene();
    }

    private static void BuildScene()
    {
        try
        {
            EnsurePlayerTag();
            EnsureTmpEssentialsOrContinue();
        }
        catch (Exception ex)
        {
            WriteResult("FAILED: " + ex);
            Debug.LogError("[DungeonEscape] Setup failed: " + ex);
        }
    }

    private static void EnsureTmpEssentialsOrContinue()
    {
        string[] settings = AssetDatabase.FindAssets("t:TMP_Settings");
        if (settings.Length == 0)
        {
            EditorPrefs.SetBool(PendingPrefsKey, true);
            string packagePath = FindTmpEssentialPackage();
            if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
            {
                WriteResult("FAILED: TMP Essential Resources package not found.");
                Debug.LogError("[DungeonEscape] Could not find TMP Essential Resources.unitypackage");
                return;
            }

            AssetDatabase.importPackageCompleted -= OnTmpImportCompleted;
            AssetDatabase.importPackageCompleted += OnTmpImportCompleted;
            Debug.Log("[DungeonEscape] Importing TMP Essential Resources...");
            AssetDatabase.ImportPackage(packagePath, false);
            return;
        }

        ContinueBuildAfterTmpReady();
    }

    private static void OnTmpImportCompleted(string packageName)
    {
        AssetDatabase.importPackageCompleted -= OnTmpImportCompleted;
        EditorApplication.delayCall += ContinueBuildAfterTmpReady;
    }

    private static void ContinueBuildAfterTmpReady()
    {
        try
        {
            EditorPrefs.SetBool(PendingPrefsKey, false);

            Sprite square = EnsureSquareSprite();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions == null)
            {
                throw new Exception("Missing Input System actions at " + InputActionsPath);
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            CleanupGeneratedObjects(scene);

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 5f;
                mainCam.transform.position = new Vector3(0f, 0f, -10f);
            }

            GameObject player = CreatePlayer(square, actions);
            CreateWalls(square);
            CreateKey(square);
            CreateEnemy(square);
            CreateExitDoor(square);

            var ui = CreateUi();
            CreateGameManager(ui);
            CreateAudioManager();
            BeginnerEnhancementsSetup.ApplyToCurrentScene();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            WriteResult("SUCCESS: Dungeon Escape scene built at " + DateTime.Now.ToString("u"));
            Debug.Log("[DungeonEscape] Scene setup complete. Press Play to test.");
            EditorUtility.DisplayDialog(
                "Dungeon Escape",
                "Scene setup complete!\n\nSampleScene is ready. Press Play to test.",
                "OK");
        }
        catch (Exception ex)
        {
            WriteResult("FAILED: " + ex);
            Debug.LogError("[DungeonEscape] Setup failed: " + ex);
            EditorPrefs.SetBool(PendingPrefsKey, false);
        }
    }

    private static string FindTmpEssentialPackage()
    {
        string[] guids = AssetDatabase.FindAssets("TMP Essential Resources");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".unitypackage", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(path);
            }
        }

        // Fallback: search PackageCache directly.
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "PackageCache"));
        if (!Directory.Exists(root))
        {
            return null;
        }

        string[] matches = Directory.GetFiles(root, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
        return matches.Length > 0 ? matches[0] : null;
    }

    private static void EnsurePlayerTag()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == "Player")
            {
                return;
            }
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Player";
        tagManager.ApplyModifiedProperties();
    }

    private static Sprite EnsureSquareSprite()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Art"))
        {
            AssetDatabase.CreateFolder("Assets", "Art");
        }

        if (!File.Exists(SquareSpritePath))
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = Enumerable.Repeat(Color.white, 32 * 32).ToArray();
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(SquareSpritePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SquareSpritePath);
        }

        TextureImporter importer = AssetImporter.GetAtPath(SquareSpritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
    }

    private static void CleanupGeneratedObjects(Scene scene)
    {
        string[] removeNames =
        {
            "Player", "Walls", "Key", "EnemyPath", "Enemy", "ExitDoor",
            "GameManager", "AudioManager", "Canvas", "EventSystem"
        };

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (removeNames.Contains(root.name))
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Color color, Vector3 position, Vector3 scale)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position;
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;

        return go;
    }

    private static GameObject CreatePlayer(Sprite square, InputActionAsset actions)
    {
        GameObject player = CreateSpriteObject(
            "Player",
            square,
            new Color(0.2f, 0.5f, 1f, 1f),
            Vector3.zero,
            new Vector3(0.8f, 0.8f, 1f));
        player.tag = "Player";

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        player.AddComponent<BoxCollider2D>();

        PlayerInput playerInput = player.AddComponent<PlayerInput>();
        playerInput.actions = actions;
        playerInput.defaultActionMap = "Player";
        playerInput.notificationBehavior = PlayerNotifications.SendMessages;

        player.AddComponent<PlayerMovement>();
        return player;
    }

    private static void CreateWalls(Sprite square)
    {
        GameObject parent = new GameObject("Walls");
        Color grey = new Color(0.45f, 0.45f, 0.45f, 1f);

        CreateWall(parent.transform, "WallTop", square, grey, new Vector3(0f, 5f, 0f), new Vector3(12f, 0.5f, 1f));
        CreateWall(parent.transform, "WallBottom", square, grey, new Vector3(0f, -5f, 0f), new Vector3(12f, 0.5f, 1f));
        CreateWall(parent.transform, "WallLeft", square, grey, new Vector3(-6f, 0f, 0f), new Vector3(0.5f, 10f, 1f));
        CreateWall(parent.transform, "WallRight", square, grey, new Vector3(6f, 0f, 0f), new Vector3(0.5f, 10f, 1f));
    }

    private static void CreateWall(Transform parent, string name, Sprite square, Color color, Vector3 pos, Vector3 scale)
    {
        GameObject wall = CreateSpriteObject(name, square, color, pos, scale);
        wall.transform.SetParent(parent, true);
        wall.AddComponent<BoxCollider2D>();
    }

    private static void CreateKey(Sprite square)
    {
        GameObject key = CreateSpriteObject(
            "Key",
            square,
            Color.yellow,
            new Vector3(-3f, 2f, 0f),
            new Vector3(0.5f, 0.5f, 1f));

        BoxCollider2D col = key.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        key.AddComponent<KeyCollect>();
    }

    private static void CreateEnemy(Sprite square)
    {
        GameObject path = new GameObject("EnemyPath");
        GameObject pointA = new GameObject("PointA");
        GameObject pointB = new GameObject("PointB");
        pointA.transform.SetParent(path.transform, true);
        pointB.transform.SetParent(path.transform, true);
        pointA.transform.position = new Vector3(2f, -2f, 0f);
        pointB.transform.position = new Vector3(2f, 2f, 0f);

        GameObject enemy = CreateSpriteObject(
            "Enemy",
            square,
            Color.red,
            pointA.transform.position,
            new Vector3(0.7f, 0.7f, 1f));

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        enemy.AddComponent<BoxCollider2D>();

        EnemyMovement movement = enemy.AddComponent<EnemyMovement>();
        SerializedObject so = new SerializedObject(movement);
        so.FindProperty("pointA").objectReferenceValue = pointA.transform;
        so.FindProperty("pointB").objectReferenceValue = pointB.transform;
        so.FindProperty("moveSpeed").floatValue = 2f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateExitDoor(Sprite square)
    {
        GameObject door = CreateSpriteObject(
            "ExitDoor",
            square,
            Color.green,
            new Vector3(4f, 3f, 0f),
            new Vector3(0.8f, 1.2f, 1f));

        BoxCollider2D col = door.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        door.AddComponent<ExitDoor>();
    }

    private struct UiRefs
    {
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI InformationText;
        public GameObject WinPanel;
        public GameObject GameOverPanel;
        public Button RestartButton;
    }

    private static UiRefs CreateUi()
    {
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        TextMeshProUGUI scoreText = CreateTmpLabel(
            canvasGo.transform,
            "ScoreText",
            "Score: 0",
            36,
            TextAlignmentOptions.TopLeft,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -20f),
            new Vector2(400f, 60f));

        TextMeshProUGUI infoText = CreateTmpLabel(
            canvasGo.transform,
            "InformationText",
            "Find the key and escape!",
            32,
            TextAlignmentOptions.Top,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -20f),
            new Vector2(900f, 60f));

        GameObject winPanel = CreateResultPanel(canvasGo.transform, "WinPanel", "YOU WIN!", new Color(0.1f, 0.45f, 0.15f, 0.85f));
        GameObject gameOverPanel = CreateResultPanel(canvasGo.transform, "GameOverPanel", "GAME OVER!", new Color(0.45f, 0.1f, 0.1f, 0.85f));
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        GameObject buttonGo = new GameObject("RestartButton", typeof(RectTransform));
        buttonGo.transform.SetParent(canvasGo.transform, false);
        RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
        buttonRt.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRt.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRt.pivot = new Vector2(0.5f, 0.5f);
        buttonRt.anchoredPosition = new Vector2(0f, -80f);
        buttonRt.sizeDelta = new Vector2(220f, 60f);

        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        Button button = buttonGo.AddComponent<Button>();

        TextMeshProUGUI buttonLabel = CreateTmpLabel(
            buttonGo.transform,
            "Text (TMP)",
            "Restart",
            28,
            TextAlignmentOptions.Center,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            Vector2.zero,
            Vector2.zero);
        RectTransform labelRt = buttonLabel.rectTransform;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        // EventSystem for UI + new Input System
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        return new UiRefs
        {
            ScoreText = scoreText,
            InformationText = infoText,
            WinPanel = winPanel,
            GameOverPanel = gameOverPanel,
            RestartButton = button
        };
    }

    private static GameObject CreateResultPanel(Transform parent, string name, string message, Color bg)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = bg;

        CreateTmpLabel(
            panel.transform,
            "Message",
            message,
            72,
            TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 40f),
            new Vector2(800f, 120f));

        return panel;
    }

    private static TextMeshProUGUI CreateTmpLabel(
        Transform parent,
        string name,
        string text,
        float fontSize,
        TextAlignmentOptions alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPos,
        Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        return tmp;
    }

    private static void CreateGameManager(UiRefs ui)
    {
        GameObject go = new GameObject("GameManager");
        GameManager gm = go.AddComponent<GameManager>();

        SerializedObject so = new SerializedObject(gm);
        so.FindProperty("scoreText").objectReferenceValue = ui.ScoreText;
        so.FindProperty("informationText").objectReferenceValue = ui.InformationText;
        so.FindProperty("winPanel").objectReferenceValue = ui.WinPanel;
        so.FindProperty("gameOverPanel").objectReferenceValue = ui.GameOverPanel;
        so.FindProperty("restartButton").objectReferenceValue = ui.RestartButton;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateAudioManager()
    {
        GameObject go = new GameObject("AudioManager");
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        go.AddComponent<AudioManager>();
    }

    private static void WriteResult(string message)
    {
        try
        {
            File.WriteAllText(ResultPath, message);
        }
        catch
        {
            // ignore IO issues in editor
        }
    }
}
#endif
