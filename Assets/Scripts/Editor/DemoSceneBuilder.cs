// DemoSceneBuilder.cs
// Construye una escena demo completa (managers + canvas + hub/victoria/derrota) y la guarda.
// Menú: Tools ▸ Juice ▸ Build Demo Scene.
// Cablea todas las referencias [SerializeField] vía SerializedObject (robusto, sin GUIDs a mano).
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Game.Core;
using Game.Juice;
using Game.UI;

public static class DemoSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/JuiceDemo.unity";

    [MenuItem("Tools/Juice/Build Demo Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Cámara ──────────────────────────────────────────────────────────
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.11f, 0.15f);
        cam.orthographic = true;
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0, 0, -10);

        // ── Managers ────────────────────────────────────────────────────────
        var managers = new GameObject("_Managers");
        var gameManager = managers.AddComponent<GameManager>();
        var juiceManager = managers.AddComponent<JuiceManager>();
        var audioManager = managers.AddComponent<AudioManager>();
        SetRef(juiceManager, "mainCamera", cam);

        // ── EventSystem ─────────────────────────────────────────────────────
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();
        var screenManager = canvasGo.AddComponent<ScreenManager>();

        // ── Pantallas ───────────────────────────────────────────────────────
        var hub = BuildHubScreen(canvasGo.transform);
        var win = BuildLevelCompleteScreen(canvasGo.transform);
        var defeat = BuildDefeatScreen(canvasGo.transform);
        var hud = BuildHud(canvasGo.transform);

        SetRef(screenManager, "hubScreen", hub.screen);
        SetRef(screenManager, "levelCompleteScreen", win.screen);
        SetRef(screenManager, "defeatScreen", defeat.screen);
        SetRef(screenManager, "gameplayHud", hud);

        EditorSceneManager.MarkSceneDirty(scene);
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);

        Debug.Log($"[DemoSceneBuilder] Escena creada en {ScenePath}. " +
                  "Falta: asignar clips en AudioManager (tone/prueba) y añadir overrides al Volume URP.");
        EditorUtility.DisplayDialog("Juice Demo",
            "Escena 'JuiceDemo' creada y abierta.\n\nPendiente manual:\n" +
            "1. AudioManager: arrastra tone*.wav (id 'tone') y prueba.wav (id 'prueba').\n" +
            "2. JuiceManager: asigna el Volume de post-proceso (opcional).\n\n" +
            "Pulsa Play para probar el flujo Hub→Victoria→Derrota.", "OK");
    }

    // ── Prefabs base ─────────────────────────────────────────────────────────
    [MenuItem("Tools/Juice/Build Base Prefabs")]
    public static void BuildPrefabs()
    {
        System.IO.Directory.CreateDirectory("Assets/Prefabs");

        // Prefab 1: botón de UI con juice (punch + flash de color TMP).
        var btnGo = new GameObject("JuicyButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.85f, 1f);
        ((RectTransform)btnGo.transform).sizeDelta = new Vector2(200, 90);

        var label = new GameObject("Text", typeof(RectTransform));
        label.transform.SetParent(btnGo.transform, false);
        var tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = "Button"; tmp.fontSize = 40;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        Stretch((RectTransform)label.transform);

        var juice = btnGo.AddComponent<UIJuice>();
        SetRef(juice, "tmpText", tmp);

        SavePrefab(btnGo, "Assets/Prefabs/JuicyButton.prefab");

        // Prefab 2: _Managers (GameManager + JuiceManager + AudioManager).
        var mgr = new GameObject("_Managers");
        mgr.AddComponent<GameManager>();
        mgr.AddComponent<JuiceManager>();
        mgr.AddComponent<AudioManager>();
        SavePrefab(mgr, "Assets/Prefabs/Managers.prefab");

        AssetDatabase.SaveAssets();
        Debug.Log("[DemoSceneBuilder] Prefabs creados en Assets/Prefabs/ (JuicyButton, Managers).");
        EditorUtility.DisplayDialog("Juice Prefabs",
            "Prefabs creados en Assets/Prefabs/:\n• JuicyButton.prefab\n• Managers.prefab", "OK");
    }

    private static void SavePrefab(GameObject go, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    // ── Hub ─────────────────────────────────────────────────────────────────
    private static (HubScreen screen, GameObject go) BuildHubScreen(Transform parent)
    {
        var go = CreatePanel("HubScreen", parent, new Color(0.12f, 0.13f, 0.18f, 1f));
        var screen = go.AddComponent<HubScreen>();

        var title = CreateText("Title", go.transform, "JUICE DEMO", 72,
            new Vector2(0, 380), new Vector2(900, 120));
        var score = CreateText("Score", go.transform, "Puntaje: 0", 40,
            new Vector2(0, 290), new Vector2(900, 80));

        // Grid 3x3 de botones de nivel
        var grid = new GameObject("LevelGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(go.transform, false);
        var gridRt = (RectTransform)grid.transform;
        gridRt.anchoredPosition = new Vector2(0, -40);
        gridRt.sizeDelta = new Vector2(560, 560);
        var gl = grid.GetComponent<GridLayoutGroup>();
        gl.cellSize = new Vector2(160, 160);
        gl.spacing = new Vector2(20, 20);
        gl.childAlignment = TextAnchor.MiddleCenter;
        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = 3;

        var buttons = new Button[9];
        for (int i = 0; i < 9; i++)
            buttons[i] = CreateButton($"Level{i + 1}", grid.transform, (i + 1).ToString(), 48);

        SetRef(screen, "titleText", GetTMP(title));
        SetRef(screen, "scoreText", GetTMP(score));
        SetArrayRef(screen, "levelButtons", buttons);

        go.SetActive(true);
        return (screen, go);
    }

    // ── LevelComplete (victoria / progreso) ──────────────────────────────────
    private static (LevelCompleteScreen screen, GameObject go) BuildLevelCompleteScreen(Transform parent)
    {
        var go = CreatePanel("LevelCompleteScreen", parent, new Color(0.10f, 0.20f, 0.13f, 1f));
        var screen = go.AddComponent<LevelCompleteScreen>();

        var head = CreateText("Headline", go.transform, "¡Nivel superado!", 80,
            new Vector2(0, 250), new Vector2(1200, 140));
        var score = CreateText("Score", go.transform, "0", 110,
            new Vector2(0, 60), new Vector2(600, 160));
        var scoreJuice = score.AddComponent<UIJuice>();
        SetRef(scoreJuice, "tmpText", GetTMP(score));

        var next = CreateButton("NextButton", go.transform, "Siguiente", 44);
        Place(next, new Vector2(-180, -200), new Vector2(280, 100));
        var hub = CreateButton("HubButton", go.transform, "Hub", 44);
        Place(hub, new Vector2(180, -200), new Vector2(280, 100));

        SetRef(screen, "headlineText", GetTMP(head));
        SetRef(screen, "scoreText", GetTMP(score));
        SetRef(screen, "scoreJuice", scoreJuice);
        SetRef(screen, "nextButton", next);
        SetRef(screen, "hubButton", hub);

        go.SetActive(false);
        return (screen, go);
    }

    // ── Defeat (derrota) ─────────────────────────────────────────────────────
    private static (DefeatScreen screen, GameObject go) BuildDefeatScreen(Transform parent)
    {
        var go = CreatePanel("DefeatScreen", parent, new Color(0.22f, 0.10f, 0.11f, 1f));
        var screen = go.AddComponent<DefeatScreen>();

        var head = CreateText("Headline", go.transform, "Derrota", 96,
            new Vector2(0, 200), new Vector2(1200, 180));
        var headJuice = head.AddComponent<UIJuice>();
        SetRef(headJuice, "tmpText", GetTMP(head));

        var retry = CreateButton("RetryButton", go.transform, "Reintentar", 44);
        Place(retry, new Vector2(-180, -160), new Vector2(300, 100));
        var hub = CreateButton("HubButton", go.transform, "Hub", 44);
        Place(hub, new Vector2(180, -160), new Vector2(300, 100));

        SetRef(screen, "headlineText", GetTMP(head));
        SetRef(screen, "headlineJuice", headJuice);
        SetRef(screen, "retryButton", retry);
        SetRef(screen, "hubButton", hub);

        go.SetActive(false);
        return (screen, go);
    }

    // ── HUD de gameplay (con botones de prueba Win/Fail) ─────────────────────
    private static GameObject BuildHud(Transform parent)
    {
        var go = new GameObject("GameplayHud", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Stretch((RectTransform)go.transform);

        var info = CreateText("Info", go.transform, "EN PARTIDA — botones de prueba:", 36,
            new Vector2(0, 300), new Vector2(1200, 80));

        // LevelController real en escena: target serializable para los listeners persistentes.
        var lc = new GameObject("LevelController").AddComponent<LevelController>();

        // Botón de prueba: ganar
        var win = CreateButton("TestWin", go.transform, "WIN (pasar nivel)", 36);
        Place(win, new Vector2(-220, 0), new Vector2(380, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(win.onClick, lc.Win);

        // Botón de prueba: fallar
        var fail = CreateButton("TestFail", go.transform, "FAIL (perder vida)", 36);
        Place(fail, new Vector2(220, 0), new Vector2(380, 100));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(fail.onClick, lc.Fail);

        go.SetActive(false);
        return go;
    }

    // ── Helpers de UI ────────────────────────────────────────────────────────
    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        go.transform.SetParent(parent, false);
        Stretch((RectTransform)go.transform);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static GameObject CreateText(string name, Transform parent, string text, float size,
        Vector2 pos, Vector2 sizeDelta)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        return go;
    }

    private static Button CreateButton(string name, Transform parent, string label, float size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.85f, 1f);
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(160, 160);

        var txt = new GameObject("Text", typeof(RectTransform));
        txt.transform.SetParent(go.transform, false);
        var tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        Stretch((RectTransform)txt.transform);

        return go.GetComponent<Button>();
    }

    private static TMP_Text GetTMP(GameObject go) => go.GetComponent<TMP_Text>();

    private static void Place(Button b, Vector2 pos, Vector2 size)
    {
        var rt = b.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ── Cableado de referencias privadas vía SerializedObject ────────────────
    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"[Builder] Campo '{field}' no encontrado en {target.GetType().Name}"); return; }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetArrayRef(Object target, string field, Object[] values)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"[Builder] Array '{field}' no encontrado"); return; }
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
