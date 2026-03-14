// ============================================================================
// ElevatorSceneBuilder.cs — One-click scene setup from the Unity Editor menu
//
// Usage:  Tools ▸ Elevator Sim ▸ Build Scene
//
// Creates the full 2D scene: building, 3 elevator shafts, doors,
// floor call buttons, status panel, camera, and wires all references.
// ============================================================================

#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using ElevatorSimulation;
using ElevatorSimulation.UI;

public static class ElevatorSceneBuilder
{
    // ====================================================================
    // Layout constants
    // ====================================================================

    private const int   TOTAL_FLOORS     = 4;
    private const float FLOOR_HEIGHT     = 3f;
    private const float BUILDING_WIDTH   = 14f;
    private const float SHAFT_WIDTH      = 1.8f;
    private const float SHAFT_GAP        = 0.6f;
    private const float CAR_WIDTH        = 1.6f;
    private const float CAR_HEIGHT       = 2.4f;
    private const float DOOR_WIDTH       = 0.75f;
    private const float DOOR_HEIGHT      = 2.4f;

    // X positions for 3 shafts centered around 0
    private static readonly float[] SHAFT_X = { -2.4f, 0f, 2.4f };

    // Elevator colors
    private static readonly Color[] ELEVATOR_COLORS =
    {
        new Color(0.29f, 0.56f, 0.89f, 1f),   // Blue
        new Color(0.36f, 0.72f, 0.36f, 1f),   // Green
        new Color(0.93f, 0.60f, 0.20f, 1f),   // Orange
    };

    private static readonly string[] ELEVATOR_NAMES = { "Lift A", "Lift B", "Lift C" };
    private static readonly string[] FLOOR_NAMES    = { "G", "1", "2", "3" };

    // ====================================================================
    // Menu entry point
    // ====================================================================

    [MenuItem("Tools/Elevator Sim/Build Scene")]
    public static void BuildScene()
    {
        // Create or locate a white-square sprite for all visuals
        Sprite whiteSprite = CreateAndSaveWhiteSprite();

        // ── Camera ──────────────────────────────────────────────────────
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
        }
        cam.orthographic     = true;
        cam.orthographicSize = 8f;
        cam.transform.position = new Vector3(0f, 5.5f, -10f);
        cam.backgroundColor  = new Color(0.10f, 0.10f, 0.14f); // dark bg
        cam.clearFlags       = CameraClearFlags.SolidColor;

        // ── Building background ─────────────────────────────────────────
        CreateSprite("Building_Background", whiteSprite,
            new Vector3(0f, TOTAL_FLOORS * FLOOR_HEIGHT * 0.5f, 1f),
            new Vector3(BUILDING_WIDTH, TOTAL_FLOORS * FLOOR_HEIGHT + 0.5f, 1f),
            new Color(0.16f, 0.17f, 0.21f));

        // ── Floor dividers & labels ────────────────────────────────────
        GameObject floorsParent = new GameObject("Floors");
        for (int f = 0; f < TOTAL_FLOORS; f++)
        {
            float y = f * FLOOR_HEIGHT;

            // Divider line
            GameObject line = CreateSprite($"FloorLine_{f}", whiteSprite,
                new Vector3(0f, y, 0f),
                new Vector3(BUILDING_WIDTH - 0.4f, 0.04f, 1f),
                new Color(0.30f, 0.32f, 0.38f));
            line.transform.SetParent(floorsParent.transform);

            // Floor label (world-space TextMesh)
            GameObject label = new GameObject($"FloorLabel_{FLOOR_NAMES[f]}");
            label.transform.position = new Vector3(-BUILDING_WIDTH * 0.5f + 0.7f, y + FLOOR_HEIGHT * 0.5f, -1f);
            label.transform.SetParent(floorsParent.transform);
            TextMesh tm        = label.AddComponent<TextMesh>();
            tm.text            = $"Floor {FLOOR_NAMES[f]}";
            tm.fontSize        = 32;
            tm.characterSize   = 0.12f;
            tm.color           = new Color(0.75f, 0.75f, 0.80f);
            tm.anchor          = TextAnchor.MiddleLeft;
            tm.alignment       = TextAlignment.Left;
        }

        // ── Elevator shafts & cars ─────────────────────────────────────
        GameObject elevatorsParent = new GameObject("Elevators");
        Elevator[] elevators = new Elevator[3];

        for (int i = 0; i < 3; i++)
        {
            float sx = SHAFT_X[i];

            // Shaft background
            GameObject shaft = CreateSprite($"Shaft_{i}", whiteSprite,
                new Vector3(sx, TOTAL_FLOORS * FLOOR_HEIGHT * 0.5f, 0.5f),
                new Vector3(SHAFT_WIDTH, TOTAL_FLOORS * FLOOR_HEIGHT, 1f),
                new Color(0.12f, 0.13f, 0.16f));
            shaft.transform.SetParent(elevatorsParent.transform);

            // Shaft rail lines (left & right)
            CreateSprite($"Rail_L_{i}", whiteSprite,
                new Vector3(sx - SHAFT_WIDTH * 0.5f, TOTAL_FLOORS * FLOOR_HEIGHT * 0.5f, 0.3f),
                new Vector3(0.04f, TOTAL_FLOORS * FLOOR_HEIGHT, 1f),
                new Color(0.25f, 0.27f, 0.32f))
                .transform.SetParent(shaft.transform);

            CreateSprite($"Rail_R_{i}", whiteSprite,
                new Vector3(sx + SHAFT_WIDTH * 0.5f, TOTAL_FLOORS * FLOOR_HEIGHT * 0.5f, 0.3f),
                new Vector3(0.04f, TOTAL_FLOORS * FLOOR_HEIGHT, 1f),
                new Color(0.25f, 0.27f, 0.32f))
                .transform.SetParent(shaft.transform);

            // ── Elevator car ────────────────────────────────────────────
            GameObject car = CreateSprite($"Elevator_{ELEVATOR_NAMES[i]}", whiteSprite,
                new Vector3(sx, FLOOR_HEIGHT * 0.5f, -0.2f),
                new Vector3(CAR_WIDTH, CAR_HEIGHT, 1f),
                ELEVATOR_COLORS[i]);
            car.transform.SetParent(elevatorsParent.transform);

            // Elevator component
            Elevator elev   = car.AddComponent<Elevator>();
            elev.moveSpeed   = 3f;
            elev.floorHeight = FLOOR_HEIGHT;
            elev.doorWaitTime = 2f;
            elev.elevatorName = ELEVATOR_NAMES[i];

            // ── Doors (children of car so they move with it) ────────────
            GameObject leftDoor = CreateSprite("Door_L", whiteSprite,
                new Vector3(-DOOR_WIDTH * 0.25f, 0f, -0.3f),
                new Vector3(DOOR_WIDTH, DOOR_HEIGHT, 1f),
                new Color(0.55f, 0.58f, 0.63f));
            leftDoor.transform.SetParent(car.transform, false);

            GameObject rightDoor = CreateSprite("Door_R", whiteSprite,
                new Vector3(DOOR_WIDTH * 0.25f, 0f, -0.3f),
                new Vector3(DOOR_WIDTH, DOOR_HEIGHT, 1f),
                new Color(0.55f, 0.58f, 0.63f));
            rightDoor.transform.SetParent(car.transform, false);

            // ElevatorDoor component
            ElevatorDoor door   = car.AddComponent<ElevatorDoor>();
            door.leftDoor       = leftDoor.transform;
            door.rightDoor      = rightDoor.transform;
            door.openOffset     = 0.5f;
            door.doorSpeed      = 3f;

            elev.door = door;
            elevators[i] = elev;

            // ── Floor indicator on the car (TextMesh) ───────────────────
            GameObject indicator = new GameObject("FloorIndicator");
            indicator.transform.SetParent(car.transform, false);
            indicator.transform.localPosition = new Vector3(0f, CAR_HEIGHT * 0.5f + 0.25f, -1f);
            TextMesh indTm       = indicator.AddComponent<TextMesh>();
            indTm.text           = "G";
            indTm.fontSize       = 36;
            indTm.characterSize  = 0.10f;
            indTm.color          = Color.white;
            indTm.anchor         = TextAnchor.MiddleCenter;
            indTm.alignment      = TextAlignment.Center;
            indTm.fontStyle      = FontStyle.Bold;

            // Add a simple updater script
            var updater = car.AddComponent<ElevatorIndicatorUpdater>();
            updater.label = indTm;
        }

        // ── ElevatorManager ─────────────────────────────────────────────
        GameObject mgrGO = new GameObject("ElevatorManager");
        ElevatorManager mgr = mgrGO.AddComponent<ElevatorManager>();
        mgr.elevators  = elevators;
        mgr.totalFloors = TOTAL_FLOORS;

        // ── UI Canvas ───────────────────────────────────────────────────
        BuildUI(whiteSprite, elevators);

        // ── Mark all created objects dirty for saving ────────────────────
        EditorUtility.SetDirty(cam.gameObject);
        EditorUtility.SetDirty(mgrGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[ElevatorSceneBuilder] ✅ Scene built successfully! Press Play to test.");
    }

    // ====================================================================
    // UI Builder
    // ====================================================================

    private static void BuildUI(Sprite whiteSprite, Elevator[] elevators)
    {
        // ── Canvas ──────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("UI_Canvas");
        Canvas canvas       = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler   = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode    = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Floor call buttons (left side) ──────────────────────────────
        GameObject btnPanel = new GameObject("FloorButtonPanel");
        btnPanel.transform.SetParent(canvasGO.transform, false);

        RectTransform bpRect = btnPanel.AddComponent<RectTransform>();
        bpRect.anchorMin = new Vector2(0f, 0.15f);
        bpRect.anchorMax = new Vector2(0f, 0.95f);
        bpRect.pivot     = new Vector2(0f, 0.5f);
        bpRect.anchoredPosition = new Vector2(20f, 0f);
        bpRect.sizeDelta = new Vector2(110f, 0f);

        VerticalLayoutGroup vlg = btnPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing           = 8f;
        vlg.childAlignment    = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = true;
        vlg.reverseArrangement     = true;    // Ground at bottom

        // Create a button for each floor (bottom to top in UI)
        for (int f = 0; f < TOTAL_FLOORS; f++)
        {
            CreateFloorButton(canvasGO.transform, btnPanel.transform, f, FLOOR_NAMES[f]);
        }

        // ── Title label ─────────────────────────────────────────────────
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(canvasGO.transform, false);
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot     = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);
        titleRect.sizeDelta = new Vector2(500f, 40f);

        Text titleTxt     = titleGO.AddComponent<Text>();
        titleTxt.text     = "ELEVATOR SIMULATION";
        titleTxt.font     = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 26;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color    = new Color(0.85f, 0.85f, 0.90f);
        titleTxt.alignment = TextAnchor.MiddleCenter;

        // ── Status panel (bottom) ───────────────────────────────────────
        GameObject statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(canvasGO.transform, false);

        RectTransform spRect = statusPanel.AddComponent<RectTransform>();
        spRect.anchorMin = new Vector2(0.2f, 0f);
        spRect.anchorMax = new Vector2(0.98f, 0f);
        spRect.pivot     = new Vector2(0.5f, 0f);
        spRect.anchoredPosition = new Vector2(0f, 10f);
        spRect.sizeDelta = new Vector2(0f, 80f);

        // Semi-transparent background
        Image spBG  = statusPanel.AddComponent<Image>();
        spBG.color  = new Color(0.10f, 0.10f, 0.14f, 0.85f);

        HorizontalLayoutGroup hlg = statusPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing            = 20f;
        hlg.childAlignment     = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(15, 15, 5, 5);

        // Status UI component
        ElevatorStatusUI statusUI = statusPanel.AddComponent<ElevatorStatusUI>();
        statusUI.panelParent = statusPanel.transform;
        statusUI.textColor   = Color.white;
        statusUI.fontSize    = 16;

        // ── EventSystem (required for UI interaction) ────────────────────
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ====================================================================
    // Helper: create a floor-call button
    // ====================================================================

    private static void CreateFloorButton(Transform canvasRoot, Transform parent, int floor, string label)
    {
        GameObject btnGO = new GameObject($"Btn_Floor_{label}");
        btnGO.transform.SetParent(parent, false);

        Image img   = btnGO.AddComponent<Image>();
        img.color   = new Color(0.22f, 0.51f, 0.89f, 1f);

        Button btn  = btnGO.AddComponent<Button>();
        ColorBlock cb     = btn.colors;
        cb.highlightedColor = new Color(0.35f, 0.65f, 0.95f);
        cb.pressedColor     = new Color(0.18f, 0.42f, 0.72f);
        btn.colors = cb;

        // Button label
        GameObject txtGO = new GameObject("Label");
        txtGO.transform.SetParent(btnGO.transform, false);

        RectTransform txtRect = txtGO.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        Text txt       = txtGO.AddComponent<Text>();
        txt.text       = $"Floor {label}";
        txt.font       = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize   = 16;
        txt.fontStyle  = FontStyle.Bold;
        txt.color      = Color.white;
        txt.alignment  = TextAnchor.MiddleCenter;

        // FloorCallButton component
        FloorCallButton fcb = btnGO.AddComponent<FloorCallButton>();
        fcb.floor = floor;
    }

    // ====================================================================
    // Helper: create a sprite GameObject
    // ====================================================================

    private static GameObject CreateSprite(string name, Sprite sprite,
        Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.position   = position;
        go.transform.localScale = scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color  = color;

        return go;
    }

    // ====================================================================
    // Helper: create or find a 4×4 white sprite asset
    // ====================================================================

    private static Sprite CreateAndSaveWhiteSprite()
    {
        string dir  = "Assets/Sprites";
        string path = dir + "/WhiteSquare.png";

        // Reuse existing
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        // Create directory
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // Create 4×4 white texture
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();

        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // Configure import settings for pixel-perfect sprite
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null)
        {
            imp.textureType      = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 4;
            imp.filterMode       = FilterMode.Point;
            imp.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}

#endif
