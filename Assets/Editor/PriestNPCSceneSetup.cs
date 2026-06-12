using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class PriestNPCSceneSetup
{
    [MenuItem("Tools/Chain Civilization/Setup Priest NPC")]
    public static void SetupPriestNPC()
    {
        EnsureTMPResources();

        var oldPriest = GameObject.Find("Priest_NPC");
        if (oldPriest != null)
            Object.DestroyImmediate(oldPriest);

        var oldCanvas = GameObject.Find("AgentChatCanvas");
        if (oldCanvas != null)
            Object.DestroyImmediate(oldCanvas);

        var matBody = MakeMat("M_PriestBody", new Color(0.12f, 0.08f, 0.22f), new Color(0.2f, 0.1f, 0.5f), 2f);
        var matStaff = MakeMat("M_PriestStaff", new Color(0.9f, 0.75f, 0.3f), new Color(0.5f, 0.35f, 0.1f), 1.5f);
        var matGem = MakeMat("M_PriestGem", new Color(0.4f, 0.2f, 0.9f), new Color(0.3f, 0.15f, 0.7f), 3f);

        var priest = new GameObject("Priest_NPC");
        priest.transform.position = new Vector3(0f, 0f, 30f);
        var agentNPC = priest.AddComponent<AgentNPC>();
        var glm = priest.AddComponent<GLMAgent>();

        var npcSO = new SerializedObject(agentNPC);
        npcSO.FindProperty("interactRadius").floatValue = 6f;
        npcSO.FindProperty("npcName").stringValue = "\u5927\u796d\u53f8";
        npcSO.ApplyModifiedPropertiesWithoutUndo();

        var visual = new GameObject("Visual");
        visual.transform.SetParent(priest.transform, false);
        visual.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        visual.AddComponent<PriestFloat>();

        MakePrim(PrimitiveType.Capsule, "Body", Vector3.zero, new Vector3(0.6f, 1f, 0.6f), matBody, visual.transform);
        MakePrim(PrimitiveType.Sphere, "Head", new Vector3(0f, 1.1f, 0f), new Vector3(0.45f, 0.45f, 0.45f), matBody, visual.transform);

        var staff = MakePrim(PrimitiveType.Cylinder, "Staff", new Vector3(0.45f, 0.2f, 0f), new Vector3(0.05f, 1.2f, 0.05f), matStaff, visual.transform);
        staff.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
        MakePrim(PrimitiveType.Sphere, "Gem", new Vector3(0f, 1.1f, 0f), new Vector3(0.6f, 0.6f, 0.6f), matGem, staff.transform);

        var particles = new GameObject("AuraParticles");
        particles.transform.SetParent(visual.transform, false);
        particles.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        var ps = particles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 2f;
        main.startSpeed = 0.3f;
        main.startSize = 0.15f;
        main.startColor = new Color(0.4f, 0.2f, 1f, 0.6f);
        main.gravityModifier = -0.05f;
        var emission = ps.emission;
        emission.rateOverTime = 25f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.8f;

        var lightGo = new GameObject("Glow");
        lightGo.transform.SetParent(visual.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.4f, 0.2f, 0.9f);
        light.intensity = 3f;
        light.range = 8f;
        light.shadows = LightShadows.None;

        var canvasGo = new GameObject("AgentChatCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var agentUI = canvasGo.AddComponent<AgentUI>();

        var panelRoot = CreatePanel(canvasGo.transform, "PanelRoot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 480), new Color(0.06f, 0.08f, 0.15f, 0.95f));

        var titleBar = CreatePanel(panelRoot.transform, "TitleBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0, 40), new Color(0.1f, 0.12f, 0.25f, 1f));
        var titleText = CreateTMP(titleBar.transform, "TitleText", "\u4e0e\u5927\u796d\u53f8\u5bf9\u8bdd", 20, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
        Stretch(titleText.rectTransform, 0, 0, 0, 0);

        var chatLogGo = new GameObject("ChatLog", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        chatLogGo.transform.SetParent(panelRoot.transform, false);
        Stretch(chatLogGo.GetComponent<RectTransform>(), 16, 48, 16, 60);
        var chatLog = chatLogGo.GetComponent<TextMeshProUGUI>();
        ApplyDefaultFont(chatLog);
        chatLog.fontSize = 16;
        chatLog.alignment = TextAlignmentOptions.TopLeft;
        chatLog.color = new Color(0.9f, 0.92f, 0.98f);
        chatLog.textWrappingMode = TextWrappingModes.Normal;

        var inputBar = CreatePanel(panelRoot.transform, "InputBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0, 50), new Color(0.08f, 0.1f, 0.18f, 1f));

        var inputGo = new GameObject("InputField", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
        inputGo.transform.SetParent(inputBar.transform, false);
        var inputRt = inputGo.GetComponent<RectTransform>();
        inputRt.anchorMin = new Vector2(0f, 0.5f);
        inputRt.anchorMax = new Vector2(0f, 0.5f);
        inputRt.pivot = new Vector2(0f, 0.5f);
        inputRt.anchoredPosition = new Vector2(10, 0);
        inputRt.sizeDelta = new Vector2(590, 40);
        inputGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 1f);

        var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(inputGo.transform, false);
        Stretch(textArea.GetComponent<RectTransform>(), 8, 4, 8, 4);

        var placeholder = CreateTMP(textArea.transform, "Placeholder", "\u5411\u5927\u796d\u53f8\u63d0\u95ee...", 18, TextAlignmentOptions.Left, new Color(0.5f, 0.55f, 0.65f), FontStyles.Italic);
        Stretch(placeholder.rectTransform, 0, 0, 0, 0);
        var inputText = CreateTMP(textArea.transform, "Text", "", 18, TextAlignmentOptions.Left, Color.white);
        Stretch(inputText.rectTransform, 0, 0, 0, 0);

        var inputField = inputGo.GetComponent<TMP_InputField>();
        inputField.textViewport = textArea.GetComponent<RectTransform>();
        inputField.textComponent = inputText;
        inputField.placeholder = placeholder;
        inputField.lineType = TMP_InputField.LineType.SingleLine;

        var sendBtnGo = new GameObject("SendBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        sendBtnGo.transform.SetParent(inputBar.transform, false);
        var sendRt = sendBtnGo.GetComponent<RectTransform>();
        sendRt.anchorMin = new Vector2(1f, 0.5f);
        sendRt.anchorMax = new Vector2(1f, 0.5f);
        sendRt.pivot = new Vector2(1f, 0.5f);
        sendRt.anchoredPosition = new Vector2(-10, 0);
        sendRt.sizeDelta = new Vector2(80, 36);
        sendBtnGo.GetComponent<Image>().color = new Color(0.25f, 0.35f, 0.65f, 1f);
        var sendBtn = sendBtnGo.GetComponent<Button>();
        var sendLabel = CreateTMP(sendBtnGo.transform, "Text", "\u53d1\u9001", 16, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
        Stretch(sendLabel.rectTransform, 0, 0, 0, 0);

        var closeHint = CreateTMP(panelRoot.transform, "CloseHint", "ESC \u5173\u95ed", 12, TextAlignmentOptions.TopRight, new Color(0.65f, 0.7f, 0.8f));
        var closeRt = closeHint.rectTransform;
        closeRt.anchorMin = new Vector2(1f, 1f);
        closeRt.anchorMax = new Vector2(1f, 1f);
        closeRt.pivot = new Vector2(1f, 1f);
        closeRt.anchoredPosition = new Vector2(-10, -8);
        closeRt.sizeDelta = new Vector2(120, 20);

        panelRoot.SetActive(false);

        var uiSO = new SerializedObject(agentUI);
        SetRef(uiSO, "chatLog", chatLog);
        SetRef(uiSO, "inputField", inputField);
        SetRef(uiSO, "sendButton", sendBtn);
        SetRef(uiSO, "panelRoot", panelRoot);
        SetRef(uiSO, "agent", glm);
        uiSO.ApplyModifiedPropertiesWithoutUndo();

        EnsureEventSystem();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = priest;
        Debug.Log("[PriestNPCSceneSetup] Priest_NPC + AgentChatCanvas created at (0,0,30).");
    }

    static void SetRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
            prop.objectReferenceValue = value;
        else
            Debug.LogWarning("[PriestNPCSceneSetup] Missing property: " + propName);
    }

    static void EnsureEventSystem()
    {
        var eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemGo.GetComponent<EventSystem>();
            Debug.Log("[PriestNPCSceneSetup] Created EventSystem for UI input.");
        }

#if ENABLE_INPUT_SYSTEM
        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
        }

        var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
        {
            Object.DestroyImmediate(legacyModule);
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    static void EnsureTMPResources()
    {
        if (System.IO.File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
            return;

        TMP_PackageResourceImporter.ImportResources(true, false, false);
        AssetDatabase.Refresh();
        Debug.Log("[PriestNPCSceneSetup] Imported TMP Essential Resources.");
    }

    static Material MakeMat(string name, Color baseColor, Color emission, float emissionIntensity)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.name = name;
        mat.color = baseColor;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emission * emissionIntensity);
        return mat;
    }

    static GameObject MakePrim(PrimitiveType type, string name, Vector3 localPos, Vector3 localScale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, int fontSize, TextAlignmentOptions align, Color color, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.fontStyle = style;
        ApplyDefaultFont(tmp);
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    static void ApplyDefaultFont(TextMeshProUGUI tmp)
    {
        if (tmp == null)
            return;

        var font = LoadDefaultFont();
        if (font != null)
            tmp.font = font;
    }

    static TMP_FontAsset LoadDefaultFont()
    {
        const string fontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (font != null)
            return font;

        var settings = Resources.Load<TMP_Settings>("TMP Settings");
        if (settings == null)
            return null;

        var so = new SerializedObject(settings);
        return so.FindProperty("m_defaultFontAsset").objectReferenceValue as TMP_FontAsset;
    }

    static void Stretch(RectTransform rt, float left, float top, float right, float bottom)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }
}
