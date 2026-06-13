using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Runtime-generated blockchain sky: highway blocks, moving hashes, data labels,
/// and a distant civilization network. No external assets are required.
/// </summary>
public class BlockchainSkyManager : MonoBehaviour
{
    const string GeneratedRootName = "BlockchainSky_Runtime";
    const string Hex = "0123456789ABCDEF";

    [Header("Blockchain Highway")]
    [SerializeField] Vector3 highwayCenter = new Vector3(120f, 135f, -100f);
    [SerializeField] Vector3 highwayDirection = new Vector3(1f, 0.08f, -0.62f);
    [SerializeField] int blockCount = 20;
    [SerializeField] float blockSpacing = 23f;
    [SerializeField] Vector3 blockScale = new Vector3(8.5f, 4.2f, 8.5f);

    [Header("Hash Flow")]
    [SerializeField] int hashOrbCount = 34;
    [SerializeField] float hashSpeed = 20f;

    [Header("Floating Hash Text")]
    [SerializeField] int floatingHashCount = 34;
    [SerializeField] float textRadius = 280f;
    [SerializeField] float textMinHeight = 78f;
    [SerializeField] float textMaxHeight = 155f;

    [Header("Civilization Network")]
    [SerializeField] int networkNodeCount = 18;
    [SerializeField] int networkLineCount = 28;
    [SerializeField] Vector3 networkCenter = new Vector3(110f, 110f, 135f);
    [SerializeField] Vector3 networkBounds = new Vector3(390f, 58f, 230f);

    [Header("Motion")]
    [SerializeField] float blockRotationSpeed = 12f;
    [SerializeField] int randomSeed = 928136;

    Transform _root;
    Camera _camera;
    System.Random _rng;

    Material _blockMaterial;
    Material _blockCoreMaterial;
    Material _cyanMaterial;
    Material _blueMaterial;
    Material _purpleMaterial;
    Material _lineMaterial;

    Vector3 _chainStart;
    Vector3 _chainEnd;
    Vector3 _chainDirection;
    float _chainLength;

    readonly List<Transform> _blocks = new List<Transform>();
    readonly List<HashOrb> _hashOrbs = new List<HashOrb>();
    readonly List<FloatingHashText> _hashTexts = new List<FloatingHashText>();
    readonly List<NetworkNode> _networkNodes = new List<NetworkNode>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureSkyExists()
    {
        if (FindFirstObjectByType<BlockchainSkyManager>() != null)
        {
            return;
        }

        new GameObject("BlockchainSkyManager").AddComponent<BlockchainSkyManager>();
    }

    protected virtual void Awake()
    {
        _rng = new System.Random(randomSeed);
        _camera = Camera.main;
        Build();
    }

    protected virtual void Update()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        AnimateBlocks();
        AnimateHashOrbs();
        AnimateFloatingText();
        AnimateNetworkNodes();
    }

    void Build()
    {
        Transform existing = transform.Find(GeneratedRootName);
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        _root = new GameObject(GeneratedRootName).transform;
        _root.SetParent(transform, false);

        CreateMaterials();
        BuildHighway();
        BuildHashOrbs();
        BuildFloatingHashes();
        BuildCivilizationNetwork();
    }

    void CreateMaterials()
    {
        _blockMaterial = CreateGlowMaterial("Blockchain_Block_Blue", new Color(0.15f, 0.58f, 1f, 1f), new Color(0.05f, 0.55f, 1f) * 2.4f, 0.55f);
        _blockCoreMaterial = CreateGlowMaterial("Blockchain_Block_Core", new Color(0.42f, 0.88f, 1f, 1f), new Color(0.2f, 0.9f, 1f) * 3.2f, 0.2f);
        _cyanMaterial = CreateGlowMaterial("Hash_Cyan", new Color(0.35f, 1f, 1f, 1f), new Color(0.15f, 1f, 1f) * 3f, 0.15f);
        _blueMaterial = CreateGlowMaterial("Hash_Blue", new Color(0.28f, 0.58f, 1f, 1f), new Color(0.12f, 0.45f, 1f) * 2.8f, 0.18f);
        _purpleMaterial = CreateGlowMaterial("Hash_Purple", new Color(0.75f, 0.32f, 1f, 1f), new Color(0.68f, 0.15f, 1f) * 2.8f, 0.18f);

        Shader lineShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
        _lineMaterial = new Material(lineShader)
        {
            name = "BlockchainSky_Line"
        };
        SetMaterialColor(_lineMaterial, new Color(0.35f, 0.86f, 1f, 0.7f));
        ConfigureTransparentMaterial(_lineMaterial);
    }

    void BuildHighway()
    {
        _chainDirection = highwayDirection.sqrMagnitude > 0.001f
            ? highwayDirection.normalized
            : new Vector3(1f, 0f, -0.5f).normalized;

        _chainLength = Mathf.Max(200f, (blockCount - 1) * blockSpacing);
        _chainStart = highwayCenter - _chainDirection * (_chainLength * 0.5f);
        _chainEnd = highwayCenter + _chainDirection * (_chainLength * 0.5f);

        for (int i = 0; i < blockCount; i++)
        {
            float t = blockCount <= 1 ? 0f : i / (float)(blockCount - 1);
            Vector3 position = Vector3.Lerp(_chainStart, _chainEnd, t);
            Transform block = CreateBlock(i, position);
            _blocks.Add(block);
        }

        for (int i = 0; i < blockCount - 1; i++)
        {
            CreateLine(
                $"Block_Link_{i:00}",
                _blocks[i].position,
                _blocks[i + 1].position,
                new Color(0.3f, 0.86f, 1f, 0.55f),
                0.8f);
        }
    }

    Transform CreateBlock(int index, Vector3 position)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = $"Block_{index:00}";
        block.transform.SetParent(_root, true);
        block.transform.position = position;
        block.transform.rotation = Quaternion.LookRotation(_chainDirection, Vector3.up);
        block.transform.localScale = blockScale;
        RemoveCollider(block);
        SetSharedMaterial(block, _blockMaterial);

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Cube);
        core.name = "Block_Core";
        core.transform.SetParent(block.transform, false);
        core.transform.localPosition = Vector3.zero;
        core.transform.localRotation = Quaternion.identity;
        core.transform.localScale = new Vector3(0.18f, 1.08f, 1.08f);
        RemoveCollider(core);
        SetSharedMaterial(core, _blockCoreMaterial);

        if (index % 4 == 0)
        {
            Light light = block.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.25f, 0.8f, 1f);
            light.range = 46f;
            light.intensity = 1.35f;
            light.shadows = LightShadows.None;
        }

        return block.transform;
    }

    void BuildHashOrbs()
    {
        for (int i = 0; i < hashOrbCount; i++)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = $"Hash_Orb_{i:00}";
            orb.transform.SetParent(_root, true);
            orb.transform.localScale = Vector3.one * NextFloat(1.4f, 2.6f);
            RemoveCollider(orb);

            Material material = PickHashMaterial(i);
            SetSharedMaterial(orb, material);

            _hashOrbs.Add(new HashOrb
            {
                Transform = orb.transform,
                Offset = i / (float)hashOrbCount,
                Speed = hashSpeed * NextFloat(0.75f, 1.35f),
                LateralPhase = NextFloat(0f, Mathf.PI * 2f),
                LateralAmplitude = NextFloat(2.4f, 6.8f)
            });
        }
    }

    void BuildFloatingHashes()
    {
        for (int i = 0; i < floatingHashCount; i++)
        {
            GameObject textObject = new GameObject($"Floating_Hash_{i:00}");
            textObject.transform.SetParent(_root, true);

            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = NextHashString();
            text.fontSize = NextFloat(9f, 15f);
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.color = PickTextColor(i);
            text.alpha = NextFloat(0.34f, 0.68f);
            text.outlineColor = new Color(0.2f, 0.85f, 1f, 0.45f);
            text.outlineWidth = 0.18f;

            Vector3 position = RandomSkyPosition(textRadius, textMinHeight, textMaxHeight);
            textObject.transform.position = position;
            textObject.transform.localScale = Vector3.one * NextFloat(1.6f, 2.8f);

            _hashTexts.Add(new FloatingHashText
            {
                Transform = textObject.transform,
                Text = text,
                Origin = position,
                Phase = NextFloat(0f, Mathf.PI * 2f),
                DriftSpeed = NextFloat(0.08f, 0.22f),
                DriftAmplitude = NextFloat(4f, 12f),
                DriftAxis = RandomHorizontalAxis()
            });
        }
    }

    void BuildCivilizationNetwork()
    {
        for (int i = 0; i < networkNodeCount; i++)
        {
            Vector3 position = new Vector3(
                networkCenter.x + NextFloat(-networkBounds.x, networkBounds.x) * 0.5f,
                networkCenter.y + NextFloat(-networkBounds.y, networkBounds.y) * 0.5f,
                networkCenter.z + NextFloat(-networkBounds.z, networkBounds.z) * 0.5f);

            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            node.name = $"Civilization_Node_{i:00}";
            node.transform.SetParent(_root, true);
            node.transform.position = position;
            node.transform.localScale = Vector3.one * NextFloat(3.2f, 6.2f);
            RemoveCollider(node);
            SetSharedMaterial(node, i % 3 == 0 ? _purpleMaterial : _cyanMaterial);

            _networkNodes.Add(new NetworkNode
            {
                Transform = node.transform,
                Origin = position,
                Phase = NextFloat(0f, Mathf.PI * 2f),
                Speed = NextFloat(0.06f, 0.18f),
                Amplitude = NextFloat(2f, 7f)
            });
        }

        for (int i = 0; i < networkLineCount; i++)
        {
            int a = NextInt(0, _networkNodes.Count);
            int b = NextInt(0, _networkNodes.Count);
            if (a == b)
            {
                b = (b + 1) % _networkNodes.Count;
            }

            Color color = i % 2 == 0
                ? new Color(0.2f, 0.82f, 1f, 0.48f)
                : new Color(0.72f, 0.25f, 1f, 0.42f);

            LineRenderer line = CreateLine($"Civilization_Link_{i:00}", _networkNodes[a].Transform.position, _networkNodes[b].Transform.position, color, 0.45f);
            _networkNodes[a].Lines.Add(new LinkedLine { Line = line, Other = _networkNodes[b].Transform, IsStart = true });
            _networkNodes[b].Lines.Add(new LinkedLine { Line = line, Other = _networkNodes[a].Transform, IsStart = false });
        }
    }

    LineRenderer CreateLine(string name, Vector3 from, Vector3 to, Color color, float width)
    {
        GameObject lineObject = new GameObject(name);
        lineObject.transform.SetParent(_root, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.startWidth = width;
        line.endWidth = width;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.material = _lineMaterial;
        line.startColor = color;
        line.endColor = new Color(color.r, color.g, Mathf.Min(1f, color.b + 0.1f), color.a * 0.65f);
        return line;
    }

    void AnimateBlocks()
    {
        for (int i = 0; i < _blocks.Count; i++)
        {
            Transform block = _blocks[i];
            if (block == null)
            {
                continue;
            }

            float speed = blockRotationSpeed * (i % 2 == 0 ? 1f : -0.7f);
            block.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
            block.Rotate(_chainDirection, speed * 0.32f * Time.deltaTime, Space.World);
        }
    }

    void AnimateHashOrbs()
    {
        Vector3 lateral = Vector3.Cross(Vector3.up, _chainDirection).normalized;
        if (lateral.sqrMagnitude < 0.001f)
        {
            lateral = Vector3.right;
        }

        for (int i = 0; i < _hashOrbs.Count; i++)
        {
            HashOrb orb = _hashOrbs[i];
            float cycle = Mathf.Repeat(orb.Offset + (Time.time * orb.Speed / Mathf.Max(1f, _chainLength)), 1f);
            Vector3 basePosition = Vector3.Lerp(_chainStart, _chainEnd, cycle);
            float wave = Mathf.Sin(Time.time * 1.4f + orb.LateralPhase) * orb.LateralAmplitude;
            float vertical = Mathf.Cos(Time.time * 1.1f + orb.LateralPhase) * 1.8f;
            orb.Transform.position = basePosition + lateral * wave + Vector3.up * vertical;
        }
    }

    void AnimateFloatingText()
    {
        for (int i = 0; i < _hashTexts.Count; i++)
        {
            FloatingHashText item = _hashTexts[i];
            float wave = Mathf.Sin(Time.time * item.DriftSpeed + item.Phase);
            item.Transform.position = item.Origin + item.DriftAxis * (wave * item.DriftAmplitude) + Vector3.up * (Mathf.Cos(Time.time * item.DriftSpeed * 1.7f + item.Phase) * 2.5f);
            item.Transform.Rotate(Vector3.up, 4f * Time.deltaTime, Space.World);

            if (_camera != null)
            {
                Vector3 toCamera = item.Transform.position - _camera.transform.position;
                if (toCamera.sqrMagnitude > 0.01f)
                {
                    item.Transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
                }
            }
        }
    }

    void AnimateNetworkNodes()
    {
        for (int i = 0; i < _networkNodes.Count; i++)
        {
            NetworkNode node = _networkNodes[i];
            node.Transform.position = node.Origin + Vector3.up * (Mathf.Sin(Time.time * node.Speed + node.Phase) * node.Amplitude);

            for (int j = 0; j < node.Lines.Count; j++)
            {
                LinkedLine linked = node.Lines[j];
                if (linked.Line == null || linked.Other == null)
                {
                    continue;
                }

                if (linked.IsStart)
                {
                    linked.Line.SetPosition(0, node.Transform.position);
                    linked.Line.SetPosition(1, linked.Other.position);
                }
                else
                {
                    linked.Line.SetPosition(0, linked.Other.position);
                    linked.Line.SetPosition(1, node.Transform.position);
                }
            }
        }
    }

    Vector3 RandomSkyPosition(float radius, float minHeight, float maxHeight)
    {
        float angle = NextFloat(0f, Mathf.PI * 2f);
        float distance = NextFloat(radius * 0.22f, radius);
        return new Vector3(
            highwayCenter.x + Mathf.Cos(angle) * distance,
            NextFloat(minHeight, maxHeight),
            highwayCenter.z + Mathf.Sin(angle) * distance);
    }

    Vector3 RandomHorizontalAxis()
    {
        Vector3 axis = new Vector3(NextFloat(-1f, 1f), NextFloat(-0.18f, 0.28f), NextFloat(-1f, 1f));
        return axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.right;
    }

    string NextHashString()
    {
        return $"0x{NextHex(4)}{NextHex(4)}...{NextHex(3)}";
    }

    string NextHex(int count)
    {
        char[] chars = new char[count];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = Hex[NextInt(0, Hex.Length)];
        }

        return new string(chars);
    }

    Material PickHashMaterial(int index)
    {
        switch (index % 3)
        {
            case 0:
                return _cyanMaterial;
            case 1:
                return _blueMaterial;
            default:
                return _purpleMaterial;
        }
    }

    Color PickTextColor(int index)
    {
        switch (index % 3)
        {
            case 0:
                return new Color(0.45f, 1f, 1f, 0.62f);
            case 1:
                return new Color(0.45f, 0.72f, 1f, 0.58f);
            default:
                return new Color(0.78f, 0.38f, 1f, 0.55f);
        }
    }

    static Material CreateGlowMaterial(string name, Color color, Color emission, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        Material material = new Material(shader)
        {
            name = name,
            color = color
        };

        SetMaterialColor(material, color);

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        return material;
    }

    static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    static void ConfigureTransparentMaterial(Material material)
    {
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.renderQueue = 3000;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    }

    static void SetSharedMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    static void RemoveCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    float NextFloat(float min, float max)
    {
        return min + (max - min) * NextFloat();
    }

    float NextFloat()
    {
        return (float)_rng.NextDouble();
    }

    int NextInt(int minInclusive, int maxExclusive)
    {
        return _rng.Next(minInclusive, maxExclusive);
    }

    struct HashOrb
    {
        public Transform Transform;
        public float Offset;
        public float Speed;
        public float LateralPhase;
        public float LateralAmplitude;
    }

    struct FloatingHashText
    {
        public Transform Transform;
        public TextMeshPro Text;
        public Vector3 Origin;
        public Vector3 DriftAxis;
        public float Phase;
        public float DriftSpeed;
        public float DriftAmplitude;
    }

    class NetworkNode
    {
        public Transform Transform;
        public Vector3 Origin;
        public float Phase;
        public float Speed;
        public float Amplitude;
        public readonly List<LinkedLine> Lines = new List<LinkedLine>();
    }

    struct LinkedLine
    {
        public LineRenderer Line;
        public Transform Other;
        public bool IsStart;
    }
}
