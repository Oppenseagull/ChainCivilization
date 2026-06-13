using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// BoundaryStone trigger zone: multi-stage lore dialog (OnGUI + E key), same pattern as DAO steles.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class BoundaryTrigger : MonoBehaviour
{
    [SerializeField] float triggerRadius = 18f;
    [SerializeField] float proximityFallbackRadius = 20f;
    [SerializeField] float proximityVerticalTolerance = 6f;
    [SerializeField] float groundTriggerCenterHeight = 1f;
    [SerializeField] string titleMessage = "【文明边界】";
    [SerializeField] string loreLine1 = "这里是已知文明的尽头。";
    [SerializeField] string loreLine2 = "远方仍有尚未建立规则的土地。";
    [SerializeField] string continueHint = "按 E 键继续";
    [SerializeField] string closeHint = "按 E 键关闭";
    [SerializeField] string deniedLine1 = "你还没有获得建立文明的资格。";
    [SerializeField] string deniedLine2 = "请先获得DAO认可。";
    [SerializeField] GameObject[] activateOnEnter;

    static readonly string[] DefaultLoreStage2Lines =
    {
        "拥有DAO Pass的旅者。",
        "",
        "未来可以在边界之外建立自己的文明。",
        "",
        "新的规则。",
        "新的货币。",
        "新的共识。",
        "",
        "这一切都将由你定义。"
    };

    [SerializeField] string[] loreStage2Lines = DefaultLoreStage2Lines;

    bool _playerInside;
    bool _uiDismissed;
    int _dialogStage;
    Transform _player;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _bodyStyle;
    GUIStyle _hintStyle;
    GUIStyle _deniedTitleStyle;
    GUIStyle _deniedBodyStyle;
    bool _stylesReady;

    public bool IsPlayerInside => _playerInside;

    void Reset()
    {
        ApplyColliderSettings();
    }

    void Awake()
    {
        SnapTriggerToGround();
        ApplyColliderSettings();
        SetActivatedObjects(false);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
        }

        Transform zone = transform.parent;
        if (zone != null)
        {
            Transform stone = zone.Find("BoundaryStone");
            if (stone != null)
            {
                LandmarkVisualFactory.ApplyBoundaryGateway(stone.gameObject);
                VisualHierarchy.Apply(stone.gameObject, VisualHierarchyTier.Boundary);
                VisualHierarchyOptions boundaryLabel = VisualHierarchyOptions.ForInteractive("BOUNDARY", new Color(0.82f, 0.9f, 1f));
                boundaryLabel.EnableFloat = false;
                boundaryLabel.EnableSpin = false;
                boundaryLabel.LabelHeight = 14f;
                VisualHierarchy.ApplyInteractiveFxOnly(stone.gameObject, boundaryLabel);
                CreateSkyBeam(stone);
            }
        }
    }

    void CreateSkyBeam(Transform parent)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "SkyBeam";
        beam.transform.SetParent(parent, false);
        Destroy(beam.GetComponent<Collider>());

        beam.transform.localPosition = new Vector3(0f, 250f, 0f);
        beam.transform.localScale = new Vector3(8f, 250f, 8f);

        Renderer r = beam.GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material mat = new Material(shader);

        mat.SetInt("_Surface", 1);
        mat.SetInt("_Blend", 0);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");

        Color baseColor = new Color(0.2f, 0.6f, 1f, 0.2f);
        mat.color = baseColor;
        mat.SetColor("_BaseColor", baseColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.1f, 0.4f, 1f) * 2f);

        r.sharedMaterial = mat;
    }

    void SnapTriggerToGround()
    {
        if (GroundSnapUtility.SnapTransform(transform, groundTriggerCenterHeight) && transform.parent != null)
        {
            transform.localPosition = transform.parent.InverseTransformPoint(transform.position);
        }
    }

    void ApplyColliderSettings()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere == null)
        {
            return;
        }

        sphere.isTrigger = true;
        sphere.radius = triggerRadius;
        sphere.center = Vector3.zero;
    }

    void Update()
    {
        SyncPlayerInsideByDistance();

        if (!_playerInside || _uiDismissed || !HasGreenPass())
        {
            return;
        }

        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            AdvanceDialog();
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialog();
        }
#endif
    }

    void LateUpdate()
    {
        SyncPlayerInsideByDistance();

        if (!GameHUDCanvas.IsActive)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!_playerInside)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        float priority = _player != null ? -Vector3.Distance(_player.position, transform.position) : 0f;

        if (!HasGreenPass())
        {
            HUDPromptChannel.Set(this, "需要 Green Pass", null, priority);
            return;
        }

        HUDPromptChannel.Set(this, "你可以创建自己的文明", null, priority);
    }

    void AdvanceDialog()
    {
        if (_dialogStage < 2)
        {
            _dialogStage++;
            return;
        }

        _uiDismissed = true;
        QuestSignals.MarkBoundaryLoreComplete();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || _playerInside)
        {
            return;
        }

        _playerInside = true;
        ResetDialog();
        SetActivatedObjects(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        if (IsPlayerInFallbackRange())
        {
            return;
        }

        _playerInside = false;
        ResetDialog();
        SetActivatedObjects(false);
        HUDPromptChannel.Clear(this);
    }

    void SyncPlayerInsideByDistance()
    {
        if (_player == null)
        {
            return;
        }

        bool isNear = IsPlayerInFallbackRange();
        if (isNear == _playerInside)
        {
            return;
        }

        _playerInside = isNear;
        ResetDialog();
        SetActivatedObjects(isNear);

        if (!isNear)
        {
            HUDPromptChannel.Clear(this);
        }
    }

    bool IsPlayerInFallbackRange()
    {
        if (_player == null)
        {
            return false;
        }

        Vector3 delta = _player.position - transform.position;
        if (Mathf.Abs(delta.y) > proximityVerticalTolerance)
        {
            return false;
        }

        float radius = Mathf.Max(triggerRadius, proximityFallbackRadius);
        Vector2 xz = new Vector2(delta.x, delta.z);
        return xz.sqrMagnitude <= radius * radius;
    }

    void ResetDialog()
    {
        _dialogStage = 0;
        _uiDismissed = false;
    }

    void SetActivatedObjects(bool active)
    {
        if (activateOnEnter == null)
        {
            return;
        }

        for (int i = 0; i < activateOnEnter.Length; i++)
        {
            if (activateOnEnter[i] != null)
            {
                activateOnEnter[i].SetActive(active);
            }
        }
    }

    void OnGUI()
    {
        if (!_playerInside || _uiDismissed)
        {
            return;
        }

        if (GameHUDCanvas.IsActive)
        {
            return;
        }

        EnsureStyles();

        const float width = 640f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.58f;

        if (!HasGreenPass())
        {
            DrawDeniedPanel(x, y, width);
            return;
        }

        switch (_dialogStage)
        {
            case 0:
                DrawPanel(x, y, width, 96f);
                GUI.Label(new Rect(x, y, width, 40f), titleMessage, _titleStyle);
                GUI.Label(new Rect(x, y + 44f, width, 28f), continueHint, _hintStyle);
                break;

            case 1:
                DrawPanel(x, y, width, 168f);
                GUI.Label(new Rect(x, y, width, 40f), titleMessage, _titleStyle);
                GUI.Label(new Rect(x, y + 44f, width, 32f), loreLine1, _bodyStyle);
                GUI.Label(new Rect(x, y + 78f, width, 32f), loreLine2, _bodyStyle);
                GUI.Label(new Rect(x, y + 118f, width, 28f), continueHint, _hintStyle);
                break;

            default:
                DrawStageTwo(x, y, width);
                break;
        }
    }

    void DrawDeniedPanel(float x, float y, float width)
    {
        const float height = 148f;
        DrawPanel(x, y, width, height);
        GUI.Label(new Rect(x, y, width, 40f), titleMessage, _deniedTitleStyle);
        GUI.Label(new Rect(x, y + 44f, width, 32f), deniedLine1, _deniedBodyStyle);
        GUI.Label(new Rect(x, y + 78f, width, 32f), deniedLine2, _deniedBodyStyle);
    }

    void DrawStageTwo(float x, float y, float width)
    {
        string[] lines = loreStage2Lines == null || loreStage2Lines.Length == 0
            ? DefaultLoreStage2Lines
            : loreStage2Lines;

        const float lineHeight = 28f;
        const float hintHeight = 32f;
        const float paddingTop = 16f;
        const float paddingBottom = 20f;
        float bodyHeight = lines.Length * lineHeight;
        float panelHeight = paddingTop + bodyHeight + hintHeight + paddingBottom;

        DrawPanel(x, y, width, panelHeight);

        float lineY = y + paddingTop;
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
            {
                lineY += lineHeight * 0.35f;
                continue;
            }

            GUI.Label(new Rect(x, lineY, width, lineHeight), lines[i], _bodyStyle);
            lineY += lineHeight;
        }

        GUI.Label(new Rect(x, y + panelHeight - hintHeight - 8f, width, hintHeight), closeHint, _hintStyle);
    }

    void DrawPanel(float x, float y, float width, float height)
    {
        GUI.Box(new Rect(x - 20f, y - 18f, width + 40f, height), GUIContent.none, _panelStyle);
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.04f, 0.1f, 0.2f, 0.9f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        _titleStyle.normal.textColor = new Color(0.55f, 0.85f, 1f);

        _bodyStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 22,
            fontStyle = FontStyle.Normal
        };
        _bodyStyle.normal.textColor = new Color(0.78f, 0.9f, 1f);

        _hintStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 18,
            fontStyle = FontStyle.Italic
        };
        _hintStyle.normal.textColor = new Color(0.75f, 0.9f, 1f, 0.85f);

        _deniedTitleStyle = new GUIStyle(_titleStyle);
        _deniedTitleStyle.normal.textColor = new Color(1f, 0.45f, 0.4f);

        _deniedBodyStyle = new GUIStyle(_bodyStyle)
        {
            fontSize = 22,
            fontStyle = FontStyle.Italic
        };
        _deniedBodyStyle.normal.textColor = new Color(0.9f, 0.85f, 0.4f);

        _stylesReady = true;
    }

    static bool HasGreenPass()
    {
        DAOPassManager passes = DAOPassManager.Instance;
        return passes != null && passes.HasGreenPass;
    }

    static Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    static bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return true;
        }

        Transform root = other.transform.root;
        return root.name == "Player" || root.GetComponent<CharacterController>() != null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.35f, 0.7f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(0.9f, 0.95f, 1f, 0.18f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(triggerRadius, proximityFallbackRadius));
    }
}
