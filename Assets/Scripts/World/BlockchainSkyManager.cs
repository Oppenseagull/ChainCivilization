using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Runtime-generated blockchain sky focused on floating block ruins, migrating
/// data cubes, sparse hash glyphs, and only a few short energy links.
/// </summary>
public class BlockchainSkyManager : MonoBehaviour
{
    const string GeneratedRootName = "BlockchainSky_Runtime";
    const string Hex = "0123456789ABCDEF";

    [Header("Floating Block Ruins")]
    [SerializeField] int floatingBlockCount = 38;
    [SerializeField] Vector3 skyCenter = new Vector3(110f, 0f, -110f);
    [SerializeField] float skyRadius = 360f;
    [SerializeField] float minHeight = 82f;
    [SerializeField] float maxHeight = 198f;

    [Header("Migrating Data Cubes")]
    [SerializeField] int dataCubeCount = 180;
    [SerializeField] float migrationSpeed = 7f;

    [Header("Sparse Hash Text")]
    [SerializeField] int hashTextCount = 12;

    [Header("Sparse Energy Links")]
    [SerializeField] int energyLinkCount = 4;

    [Header("Motion")]
    [SerializeField] int randomSeed = 928136;
    [SerializeField] float blockRotationSpeed = 3.8f;

    Transform _root;
    Camera _camera;
    System.Random _rng;

    Material _cyanMaterial;
    Material _blueMaterial;
    Material _purpleMaterial;
    Material _deepBlueMaterial;
    Material _lineMaterial;

    readonly List<FloatingBlock> _blocks = new List<FloatingBlock>();
    readonly List<DataCube> _dataCubes = new List<DataCube>();
    readonly List<FloatingHashText> _hashTexts = new List<FloatingHashText>();
    readonly List<EnergyLink> _energyLinks = new List<EnergyLink>();

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
        AnimateDataCubes();
        AnimateHashTexts();
        AnimateEnergyLinks();
    }

    void Build()
    {
        Transform existing = transform.Find(GeneratedRootName);
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        _blocks.Clear();
        _dataCubes.Clear();
        _hashTexts.Clear();
        _energyLinks.Clear();

        _root = new GameObject(GeneratedRootName).transform;
        _root.SetParent(transform, false);

        CreateMaterials();
        BuildFloatingBlocks();
        BuildMigratingDataCubes();
        BuildSparseHashText();
        BuildSparseEnergyLinks();
    }

    void CreateMaterials()
    {
        _cyanMaterial = CreateGlowMaterial("Sky_Cyan_Glow", new Color(0.28f, 0.98f, 1f, 1f), new Color(0.08f, 0.9f, 1f) * 2.8f, 0.28f);
        _blueMaterial = CreateGlowMaterial("Sky_Blue_Glow", new Color(0.13f, 0.48f, 1f, 1f), new Color(0.05f, 0.44f, 1f) * 2.6f, 0.38f);
        _purpleMaterial = CreateGlowMaterial("Sky_Purple_Glow", new Color(0.62f, 0.18f, 1f, 1f), new Color(0.58f, 0.06f, 1f) * 2.7f, 0.34f);
        _deepBlueMaterial = CreateGlowMaterial("Sky_Deep_Block", new Color(0.04f, 0.13f, 0.34f, 1f), new Color(0.02f, 0.18f, 0.5f) * 1.6f, 0.72f);

        Shader lineShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
        _lineMaterial = new Material(lineShader) { name = "Sky_Sparse_Energy_Line" };
        SetMaterialColor(_lineMaterial, new Color(0.38f, 0.9f, 1f, 0.45f));
        ConfigureTransparentMaterial(_lineMaterial);
    }

    void BuildFloatingBlocks()
    {
        int guard = 0;
        for (int i = 0; i < floatingBlockCount && guard < floatingBlockCount * 10; guard++)
        {
            Vector3 position = RandomSkyPosition();
            if (!IsFarEnoughFromBlocks(position, 26f))
            {
                continue;
            }

            Vector3 scale = new Vector3(
                NextFloat(3.5f, 15f),
                NextFloat(2.6f, 10f),
                NextFloat(3.5f, 15f));

            Quaternion rotation = Quaternion.Euler(NextFloat(-18f, 18f), NextFloat(0f, 360f), NextFloat(-16f, 16f));
            Transform block = CreateFloatingBlock(i, position, rotation, scale);
            _blocks.Add(new FloatingBlock
            {
                Transform = block,
                Origin = position,
                Axis = RandomAxis(),
                DriftAxis = RandomHorizontalAxis(),
                Phase = NextFloat(0f, Mathf.PI * 2f),
                DriftSpeed = NextFloat(0.12f, 0.34f),
                DriftAmplitude = NextFloat(2.5f, 9.5f),
                RotationSpeed = blockRotationSpeed * NextFloat(0.45f, 1.4f),
                Light = block.GetComponent<Light>()
            });

            i++;
        }
    }

    Transform CreateFloatingBlock(int index, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = $"Sky_Ruin_Block_{index:00}";
        block.transform.SetParent(_root, true);
        block.transform.SetPositionAndRotation(position, rotation);
        block.transform.localScale = scale;
        RemoveCollider(block);
        SetSharedMaterial(block, PickBlockMaterial(index));

        CreateBlockStripe(block.transform, "Data_Stripe_A", new Vector3(0f, 0.51f, 0f), new Vector3(1.04f, 0.035f, 0.18f), PickGlowMaterial(index + 1));
        CreateBlockStripe(block.transform, "Data_Stripe_B", new Vector3(0f, -0.51f, 0f), new Vector3(0.18f, 0.035f, 1.04f), PickGlowMaterial(index + 2));

        if (index % 5 == 0)
        {
            Light light = block.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = index % 2 == 0 ? new Color(0.25f, 0.8f, 1f) : new Color(0.65f, 0.25f, 1f);
            light.range = 48f;
            light.intensity = 1.15f;
            light.shadows = LightShadows.None;
        }

        return block.transform;
    }

    void CreateBlockStripe(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stripe.name = name;
        stripe.transform.SetParent(parent, false);
        stripe.transform.localPosition = localPosition;
        stripe.transform.localRotation = Quaternion.identity;
        stripe.transform.localScale = localScale;
        RemoveCollider(stripe);
        SetSharedMaterial(stripe, material);
    }

    void BuildMigratingDataCubes()
    {
        if (_blocks.Count < 2)
        {
            return;
        }

        for (int i = 0; i < dataCubeCount; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Hash_Data_Cube_{i:000}";
            cube.transform.SetParent(_root, true);
            float baseSize = NextFloat(0.12f, 0.32f);
            cube.transform.localScale = Vector3.one * baseSize;
            RemoveCollider(cube);
            SetSharedMaterial(cube, PickGlowMaterial(i));

            int from = NextInt(0, _blocks.Count);
            int to = NextDifferentBlockIndex(from);
            float mode = NextFloat();
            Vector3 freeOrigin = RandomSkyPosition();

            _dataCubes.Add(new DataCube
            {
                Transform = cube.transform,
                FromIndex = from,
                ToIndex = to,
                FreeOrigin = freeOrigin,
                FreeAxis = RandomHorizontalAxis(),
                Offset = NextFloat(),
                Speed = migrationSpeed * NextFloat(0.35f, 1.18f),
                Phase = NextFloat(0f, Mathf.PI * 2f),
                ArcHeight = NextFloat(7f, 24f),
                DriftAmplitude = NextFloat(2f, 11f),
                Migrates = mode > 0.35f,
                BaseSize = baseSize
            });
        }
    }

    void BuildSparseHashText()
    {
        for (int i = 0; i < hashTextCount; i++)
        {
            GameObject textObject = new GameObject($"Distant_Hash_Glyph_{i:00}");
            textObject.transform.SetParent(_root, true);

            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = NextHashString();
            text.fontSize = NextFloat(7f, 11f);
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;
            text.color = PickTextColor(i);
            text.alpha = NextFloat(0.18f, 0.36f);
            text.outlineColor = new Color(0.18f, 0.78f, 1f, 0.22f);
            text.outlineWidth = 0.08f;

            Vector3 position = RandomSkyPosition();
            textObject.transform.position = position + Vector3.up * NextFloat(8f, 28f);
            textObject.transform.localScale = Vector3.one * NextFloat(2.2f, 4.4f);

            _hashTexts.Add(new FloatingHashText
            {
                Transform = textObject.transform,
                Origin = textObject.transform.position,
                Axis = RandomHorizontalAxis(),
                Phase = NextFloat(0f, Mathf.PI * 2f),
                Speed = NextFloat(0.04f, 0.12f),
                Amplitude = NextFloat(4f, 12f)
            });
        }
    }

    void BuildSparseEnergyLinks()
    {
        if (_blocks.Count < 2)
        {
            return;
        }

        for (int i = 0; i < energyLinkCount; i++)
        {
            int start = NextInt(0, _blocks.Count);
            int end = FindNearbyBlockIndex(start, 95f);

            LineRenderer line = CreateLine(
                $"Rare_Energy_Link_{i:00}",
                _blocks[start].Transform.position,
                _blocks[end].Transform.position,
                i % 2 == 0 ? new Color(0.26f, 0.92f, 1f, 0.32f) : new Color(0.72f, 0.24f, 1f, 0.28f),
                0.18f);

            _energyLinks.Add(new EnergyLink
            {
                Line = line,
                StartIndex = start,
                EndIndex = end,
                Phase = NextFloat(0f, Mathf.PI * 2f)
            });
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
        line.numCapVertices = 3;
        line.material = _lineMaterial;
        line.startColor = color;
        line.endColor = new Color(color.r, color.g, Mathf.Min(1f, color.b + 0.1f), color.a * 0.55f);
        return line;
    }

    void AnimateBlocks()
    {
        for (int i = 0; i < _blocks.Count; i++)
        {
            FloatingBlock block = _blocks[i];
            float wave = Mathf.Sin(Time.time * block.DriftSpeed + block.Phase);
            block.Transform.position = block.Origin + block.DriftAxis * (wave * block.DriftAmplitude) + Vector3.up * (Mathf.Cos(Time.time * block.DriftSpeed * 0.7f + block.Phase) * 2.6f);
            block.Transform.Rotate(block.Axis, block.RotationSpeed * Time.deltaTime, Space.World);

            if (block.Light != null)
            {
                block.Light.intensity = 0.75f + Mathf.Abs(Mathf.Sin(Time.time * 0.9f + block.Phase)) * 0.75f;
            }

            _blocks[i] = block;
        }
    }

    void AnimateDataCubes()
    {
        for (int i = 0; i < _dataCubes.Count; i++)
        {
            DataCube cube = _dataCubes[i];
            if (cube.Migrates)
            {
                float t = Mathf.Repeat(cube.Offset + Time.time * cube.Speed * 0.006f, 1f);
                Vector3 from = _blocks[cube.FromIndex].Transform.position;
                Vector3 to = _blocks[cube.ToIndex].Transform.position;
                Vector3 position = Vector3.Lerp(from, to, SmoothStep(t));
                position += Vector3.up * (Mathf.Sin(t * Mathf.PI) * cube.ArcHeight);
                position += cube.FreeAxis * (Mathf.Sin(Time.time * 0.85f + cube.Phase) * 1.8f);
                cube.Transform.position = position;
            }
            else
            {
                float wave = Mathf.Sin(Time.time * cube.Speed * 0.08f + cube.Phase);
                cube.Transform.position = cube.FreeOrigin + cube.FreeAxis * (wave * cube.DriftAmplitude) + Vector3.up * (Mathf.Cos(Time.time * 0.2f + cube.Phase) * 3f);
            }

            float pulse = 0.75f + Mathf.Abs(Mathf.Sin(Time.time * 1.8f + cube.Phase)) * 0.65f;
            cube.Transform.localScale = Vector3.one * cube.BaseSize * pulse;
            cube.Transform.Rotate(Vector3.up, 18f * Time.deltaTime, Space.World);
        }
    }

    void AnimateHashTexts()
    {
        for (int i = 0; i < _hashTexts.Count; i++)
        {
            FloatingHashText item = _hashTexts[i];
            float wave = Mathf.Sin(Time.time * item.Speed + item.Phase);
            item.Transform.position = item.Origin + item.Axis * (wave * item.Amplitude);

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

    void AnimateEnergyLinks()
    {
        for (int i = 0; i < _energyLinks.Count; i++)
        {
            EnergyLink link = _energyLinks[i];
            Vector3 start = _blocks[link.StartIndex].Transform.position;
            Vector3 end = _blocks[link.EndIndex].Transform.position;
            link.Line.SetPosition(0, start);
            link.Line.SetPosition(1, end);

            Color color = link.Line.startColor;
            float alpha = 0.08f + Mathf.Abs(Mathf.Sin(Time.time * 0.7f + link.Phase)) * 0.22f;
            color.a = alpha;
            link.Line.startColor = color;
            link.Line.endColor = new Color(color.r, color.g, color.b, alpha * 0.45f);
        }
    }

    Vector3 RandomSkyPosition()
    {
        float angle = NextFloat(0f, Mathf.PI * 2f);
        float distance = Mathf.Sqrt(NextFloat()) * skyRadius;
        return new Vector3(
            skyCenter.x + Mathf.Cos(angle) * distance,
            NextFloat(minHeight, maxHeight),
            skyCenter.z + Mathf.Sin(angle) * distance);
    }

    bool IsFarEnoughFromBlocks(Vector3 position, float minDistance)
    {
        float sqr = minDistance * minDistance;
        for (int i = 0; i < _blocks.Count; i++)
        {
            if ((_blocks[i].Origin - position).sqrMagnitude < sqr)
            {
                return false;
            }
        }

        return true;
    }

    int NextDifferentBlockIndex(int index)
    {
        int next = NextInt(0, _blocks.Count);
        if (next == index)
        {
            next = (next + 1) % _blocks.Count;
        }

        return next;
    }

    int FindNearbyBlockIndex(int startIndex, float maxDistance)
    {
        float maxSqr = maxDistance * maxDistance;
        int closest = NextDifferentBlockIndex(startIndex);
        float closestSqr = (_blocks[startIndex].Origin - _blocks[closest].Origin).sqrMagnitude;

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (i == startIndex)
            {
                continue;
            }

            float sqr = (_blocks[startIndex].Origin - _blocks[i].Origin).sqrMagnitude;
            if (sqr < closestSqr)
            {
                closest = i;
                closestSqr = sqr;
            }

            if (sqr <= maxSqr && NextFloat() > 0.45f)
            {
                return i;
            }
        }

        return closest;
    }

    Material PickBlockMaterial(int index)
    {
        switch (index % 4)
        {
            case 0:
                return _deepBlueMaterial;
            case 1:
                return _blueMaterial;
            case 2:
                return _purpleMaterial;
            default:
                return _cyanMaterial;
        }
    }

    Material PickGlowMaterial(int index)
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
                return new Color(0.45f, 1f, 1f, 0.24f);
            case 1:
                return new Color(0.45f, 0.72f, 1f, 0.22f);
            default:
                return new Color(0.78f, 0.38f, 1f, 0.2f);
        }
    }

    Vector3 RandomAxis()
    {
        Vector3 axis = new Vector3(NextFloat(-1f, 1f), NextFloat(-1f, 1f), NextFloat(-1f, 1f));
        return axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
    }

    Vector3 RandomHorizontalAxis()
    {
        Vector3 axis = new Vector3(NextFloat(-1f, 1f), NextFloat(-0.15f, 0.35f), NextFloat(-1f, 1f));
        return axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.right;
    }

    string NextHashString()
    {
        return $"0x{NextHex(4)}...{NextHex(3)}";
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

    static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
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

    struct FloatingBlock
    {
        public Transform Transform;
        public Vector3 Origin;
        public Vector3 Axis;
        public Vector3 DriftAxis;
        public float Phase;
        public float DriftSpeed;
        public float DriftAmplitude;
        public float RotationSpeed;
        public Light Light;
    }

    struct DataCube
    {
        public Transform Transform;
        public int FromIndex;
        public int ToIndex;
        public Vector3 FreeOrigin;
        public Vector3 FreeAxis;
        public float Offset;
        public float Speed;
        public float Phase;
        public float ArcHeight;
        public float DriftAmplitude;
        public bool Migrates;
        public float BaseSize;
    }

    struct FloatingHashText
    {
        public Transform Transform;
        public Vector3 Origin;
        public Vector3 Axis;
        public float Phase;
        public float Speed;
        public float Amplitude;
    }

    struct EnergyLink
    {
        public LineRenderer Line;
        public int StartIndex;
        public int EndIndex;
        public float Phase;
    }
}
