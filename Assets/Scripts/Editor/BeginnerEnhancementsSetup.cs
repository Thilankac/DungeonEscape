#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Adds beginner features without rebuilding or removing the existing artwork.</summary>
public static class BeginnerEnhancementsSetup
{
    [MenuItem("Tools/Dungeon Escape/Add Beginner Enhancements")]
    public static void ApplyToSampleScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        ApplyToCurrentScene();
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Beginner enhancements ready: five coins, three lives and a 60-second timer.");
    }

    public static void ApplyToCurrentScene()
    {
        var manager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        var scoreObject = GameObject.Find("ScoreText");
        if (manager == null || scoreObject == null)
            throw new InvalidOperationException("Set up the Dungeon Escape scene first.");

        var score = scoreObject.GetComponent<TextMeshProUGUI>();
        var lives = EnsureLabel(score, "LivesText", "Lives: 3", new Vector2(20f, -85f));
        var timer = EnsureLabel(score, "TimerText", "Time: 60s", new Vector2(20f, -150f));
        var settings = new SerializedObject(manager);
        settings.FindProperty("livesText").objectReferenceValue = lives;
        settings.FindProperty("timerText").objectReferenceValue = timer;
        settings.ApplyModifiedPropertiesWithoutUndo();

        // Reuse the group if this menu is run again.
        var coins = GameObject.Find("Coins");
        if (coins == null) coins = new GameObject("Coins");
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Square.png");
        if (sprite == null) throw new InvalidOperationException("Square sprite is missing.");
        Vector2[] positions =
        {
            new Vector2(-4f, -2f), new Vector2(-3f, 0f), new Vector2(-1f, 3f),
            new Vector2(1f, -3f), new Vector2(4f, 1f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            if (coins.transform.Find("Coin" + (i + 1)) != null) continue;
            var coin = new GameObject("Coin" + (i + 1));
            coin.transform.SetParent(coins.transform, false);
            coin.transform.position = positions[i];
            coin.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            coin.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            var renderer = coin.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(1f, 0.65f, 0.1f);
            renderer.sortingOrder = 3;
            coin.AddComponent<BoxCollider2D>().isTrigger = true;
            coin.AddComponent<CoinCollect>();
        }
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
    }

    private static TextMeshProUGUI EnsureLabel(TextMeshProUGUI template, string name,
        string text, Vector2 position)
    {
        var existing = template.transform.parent.Find(name);
        var label = existing != null ? existing.GetComponent<TextMeshProUGUI>() :
            UnityEngine.Object.Instantiate(template, template.transform.parent);
        label.name = name;
        label.text = text;
        label.fontSize = 32f;
        label.color = Color.white;
        label.raycastTarget = false;
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0f, 1f);
        label.rectTransform.pivot = new Vector2(0f, 1f);
        label.rectTransform.anchoredPosition = position;
        label.rectTransform.sizeDelta = new Vector2(350f, 55f);
        return label;
    }
}
#endif
