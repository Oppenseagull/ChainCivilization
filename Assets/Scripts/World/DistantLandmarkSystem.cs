using UnityEngine;

/// <summary>
/// Spawns giant low-poly glowing landmarks on the horizon for navigation.
/// Visual only — no colliders, no gameplay.
/// </summary>
public class DistantLandmarkSystem : MonoBehaviour
{
    enum LandmarkKind
    {
        BlueDaoTower,
        RedDaoTower,
        GreenDaoTower,
        BoundaryTower,
        BlockchainTree,
        CivilizationCore
    }

    struct LandmarkDef
    {
        public string Name;
        public Vector3 Position;
        public LandmarkKind Kind;
        public float HeightMul;
    }

    [SerializeField] float baseStructureHeight = 28f;
    [SerializeField] float heightMultiplier = 4f;

    Transform _root;

    static readonly LandmarkDef[] Landmarks =
    {
        new LandmarkDef { Name = "Distant_BlueDAO_Tower", Position = new Vector3(-430f, 0f, -430f), Kind = LandmarkKind.BlueDaoTower, HeightMul = 4.2f },
        new LandmarkDef { Name = "Distant_RedDAO_Tower", Position = new Vector3(450f, 0f, 450f), Kind = LandmarkKind.RedDaoTower, HeightMul = 3.8f },
        new LandmarkDef { Name = "Distant_GreenDAO_Tower", Position = new Vector3(470f, 0f, -470f), Kind = LandmarkKind.GreenDaoTower, HeightMul = 4f },
        new LandmarkDef { Name = "Distant_Boundary_Tower", Position = new Vector3(520f, 0f, -520f), Kind = LandmarkKind.BoundaryTower, HeightMul = 4.5f },
        new LandmarkDef { Name = "Distant_Blockchain_Tree", Position = new Vector3(-360f, 0f, 200f), Kind = LandmarkKind.BlockchainTree, HeightMul = 3.5f },
        new LandmarkDef { Name = "Distant_Civilization_Core", Position = new Vector3(0f, 0f, 470f), Kind = LandmarkKind.CivilizationCore, HeightMul = 4f }
    };

    void Start()
    {
        BuildLandmarks();
    }

    void BuildLandmarks()
    {
        _root = new GameObject("DistantLandmarks").transform;
        _root.SetParent(transform, false);

        for (int i = 0; i < Landmarks.Length; i++)
        {
            LandmarkDef def = Landmarks[i];
            float height = baseStructureHeight * def.HeightMul * heightMultiplier / 4f;
            GameObject landmark = new GameObject(def.Name);
            landmark.transform.SetParent(_root, false);
            landmark.transform.position = def.Position;

            switch (def.Kind)
            {
                case LandmarkKind.BlueDaoTower:
                    BuildDaoTower(landmark.transform, height, new Color(0.78f, 0.86f, 0.94f), new Color(0.35f, 0.55f, 0.82f));
                    VisualHierarchy.Apply(landmark, VisualHierarchyTier.DaoBuilding);
                    break;
                case LandmarkKind.RedDaoTower:
                    BuildDaoTower(landmark.transform, height, new Color(0.92f, 0.62f, 0.42f), new Color(0.55f, 0.22f, 0.1f));
                    VisualHierarchy.Apply(landmark, VisualHierarchyTier.DaoBuilding);
                    break;
                case LandmarkKind.GreenDaoTower:
                    BuildDaoTower(landmark.transform, height, new Color(0.48f, 0.72f, 0.46f), new Color(0.16f, 0.42f, 0.14f));
                    VisualHierarchy.Apply(landmark, VisualHierarchyTier.DaoBuilding);
                    break;
                case LandmarkKind.BoundaryTower:
                    BuildBoundaryTower(landmark.transform, height);
                    VisualHierarchy.Apply(landmark, VisualHierarchyTier.Boundary);
                    break;
                case LandmarkKind.BlockchainTree:
                    BuildBlockchainTree(landmark.transform, height);
                    VisualHierarchy.Apply(landmark, VisualHierarchyTier.Static);
                    break;
                case LandmarkKind.CivilizationCore:
                    BuildCivilizationCore(landmark.transform, height);
                    VisualHierarchy.Apply(landmark, VisualHierarchyTier.Static);
                    break;
            }

            GroundSnapUtility.SnapTransform(landmark.transform, 0f);
        }
    }

    void BuildDaoTower(Transform root, float height, Color body, Color glow)
    {
        float baseR = height * 0.14f;

        CreatePart(PrimitiveType.Cylinder, "Base", root, Vector3.up * (height * 0.04f), new Vector3(baseR * 2.4f, height * 0.04f, baseR * 2.4f), body, 0.12f);
        CreatePart(PrimitiveType.Cube, "Tier_1", root, Vector3.up * (height * 0.16f), new Vector3(baseR * 1.8f, height * 0.14f, baseR * 1.8f), body * 0.95f, 0.12f);
        CreatePart(PrimitiveType.Cube, "Tier_2", root, Vector3.up * (height * 0.34f), new Vector3(baseR * 1.4f, height * 0.16f, baseR * 1.4f), body, 0.12f);
        CreatePart(PrimitiveType.Cylinder, "Spire", root, Vector3.up * (height * 0.58f), new Vector3(baseR * 0.7f, height * 0.36f, baseR * 0.7f), body * 1.05f, 0.14f);
        CreatePart(PrimitiveType.Cube, "Cap", root, Vector3.up * (height * 0.82f), new Vector3(baseR * 0.9f, height * 0.08f, baseR * 0.9f), body * 0.9f, 0.12f);

        CreatePart(PrimitiveType.Sphere, "Beacon", root, Vector3.up * (height * 0.94f), Vector3.one * baseR * 0.55f, body, 0.18f);
        CreatePart(PrimitiveType.Cylinder, "GlowRing", root, Vector3.up * (height * 0.72f), new Vector3(baseR * 1.1f, height * 0.015f, baseR * 1.1f), glow, 0.1f);
    }

    void BuildBoundaryTower(Transform root, float height)
    {
        Color silver = new Color(0.82f, 0.84f, 0.88f);
        Color glow = new Color(0.42f, 0.48f, 0.62f);
        float baseR = height * 0.09f;

        CreatePart(PrimitiveType.Cylinder, "Foundation", root, Vector3.up * (height * 0.03f), new Vector3(baseR * 2.6f, height * 0.03f, baseR * 2.6f), silver * 0.9f, 0.14f);
        CreatePart(PrimitiveType.Cube, "Obelisk", root, Vector3.up * (height * 0.48f), new Vector3(baseR * 1.2f, height * 0.82f, baseR * 1.2f), silver, 0.16f);

        for (int i = 0; i < 4; i++)
        {
            float y = height * (0.22f + i * 0.18f);
            CreatePart(PrimitiveType.Cylinder, $"Ring_{i}", root, Vector3.up * y, new Vector3(baseR * 1.5f, height * 0.012f, baseR * 1.5f), glow, 0.12f);
        }

        CreatePart(PrimitiveType.Sphere, "Crown", root, Vector3.up * (height * 0.94f), Vector3.one * baseR * 0.7f, silver, 0.18f);
    }

    void BuildBlockchainTree(Transform root, float height)
    {
        Color trunk = new Color(0.5f, 0.42f, 0.34f);
        Color leaf = new Color(0.52f, 0.76f, 0.5f);
        Color node = new Color(0.62f, 0.78f, 0.92f);
        float baseR = height * 0.08f;

        CreatePart(PrimitiveType.Cylinder, "Trunk", root, Vector3.up * (height * 0.22f), new Vector3(baseR * 0.8f, height * 0.22f, baseR * 0.8f), trunk, 0.1f);

        Vector3[] branchEnds =
        {
            new Vector3(baseR * 1.8f, height * 0.55f, baseR * 0.4f),
            new Vector3(-baseR * 1.6f, height * 0.62f, -baseR * 0.5f),
            new Vector3(baseR * 0.3f, height * 0.72f, baseR * 1.7f),
            new Vector3(-baseR * 0.6f, height * 0.78f, -baseR * 1.5f),
            new Vector3(baseR * 1.2f, height * 0.85f, -baseR * 1.1f)
        };

        Vector3 trunkTop = Vector3.up * (height * 0.42f);
        for (int i = 0; i < branchEnds.Length; i++)
        {
            Vector3 end = branchEnds[i];
            Vector3 mid = (trunkTop + end) * 0.5f;
            Vector3 dir = end - trunkTop;
            float len = dir.magnitude;

            GameObject branch = CreatePart(PrimitiveType.Cube, $"Branch_{i}", root, mid, new Vector3(baseR * 0.25f, baseR * 0.25f, len), trunk * 1.1f, 0.1f);
            if (dir.sqrMagnitude > 0.001f)
            {
                branch.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }

            CreatePart(PrimitiveType.Cube, $"Block_{i}", root, end, Vector3.one * baseR * 0.65f, leaf, 0.12f);

            GameObject link = CreatePart(PrimitiveType.Cylinder, $"Link_{i}", root, (end + branchEnds[(i + 1) % branchEnds.Length]) * 0.5f,
                new Vector3(baseR * 0.9f, baseR * 0.03f, baseR * 0.9f), node, 0.1f);
            link.transform.rotation = Quaternion.Euler(90f, i * 36f, 0f);
        }

        CreatePart(PrimitiveType.Sphere, "TreeCore", root, Vector3.up * (height * 0.92f), Vector3.one * baseR * 0.5f, node, 0.16f);
    }

    void BuildCivilizationCore(Transform root, float height)
    {
        Color gold = new Color(0.94f, 0.88f, 0.62f);
        Color glow = new Color(0.72f, 0.58f, 0.18f);
        float baseR = height * 0.12f;

        CreatePart(PrimitiveType.Cylinder, "Platform", root, Vector3.up * (height * 0.03f), new Vector3(baseR * 2.8f, height * 0.03f, baseR * 2.8f), gold * 0.85f, 0.14f);
        CreatePart(PrimitiveType.Cylinder, "InnerRing", root, Vector3.up * (height * 0.08f), new Vector3(baseR * 2f, height * 0.02f, baseR * 2f), glow, 0.12f);

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * baseR * 1.4f, height * 0.22f, Mathf.Sin(angle) * baseR * 1.4f);
            CreatePart(PrimitiveType.Cube, $"Pillar_{i}", root, pos, new Vector3(baseR * 0.35f, height * 0.22f, baseR * 0.35f), gold * 0.9f, 0.12f);
        }

        CreatePart(PrimitiveType.Sphere, "Core", root, Vector3.up * (height * 0.55f), Vector3.one * baseR * 1.1f, gold, 0.18f);

        GameObject halo = CreatePart(PrimitiveType.Cylinder, "Halo", root, Vector3.up * (height * 0.55f), new Vector3(baseR * 1.8f, height * 0.02f, baseR * 1.8f), glow, 0.1f);
        halo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        CreatePart(PrimitiveType.Cylinder, "Spire", root, Vector3.up * (height * 0.82f), new Vector3(baseR * 0.4f, height * 0.28f, baseR * 0.4f), gold, 0.14f);
        CreatePart(PrimitiveType.Sphere, "Tip", root, Vector3.up * (height * 0.96f), Vector3.one * baseR * 0.45f, gold * 1.1f, 0.2f);
    }

    static GameObject CreatePart(
        PrimitiveType type,
        string partName,
        Transform parent,
        Vector3 localPos,
        Vector3 scale,
        Color color,
        float smoothness)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localScale = scale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = CreateLitMaterial(color, smoothness);
        }

        return part;
    }

    static Material CreateLitMaterial(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        return material;
    }
}
