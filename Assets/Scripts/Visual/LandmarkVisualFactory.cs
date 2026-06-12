using UnityEngine;

/// <summary>
/// Procedural landmark skins for important gameplay objects.
/// Keeps colliders and interaction scripts on the original object intact.
/// </summary>
public static class LandmarkVisualFactory
{
    const string RootName = "LandmarkVisual_Procedural";

    public static void ApplyDaoSanctuary(GameObject target, string daoName, Color accent, Color stone)
    {
        // DAO visuals are intentionally left to the scene-authored objects.
        // This avoids stacking imported buildings on top of the old gameplay landmarks.
        return;
    }

    public static void ApplyMoonCrystal(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Transform root = PrepareRoot(target);
        HideSupersededVisuals(target);

        Color moon = new Color(0.48f, 0.86f, 1f);
        Color deep = new Color(0.18f, 0.42f, 0.72f);
        CreatePart(PrimitiveType.Cylinder, "Crystal_Base", root, new Vector3(0f, -0.12f, 0f), new Vector3(1.25f, 0.08f, 1.25f), deep * 0.62f, 0.2f);
        CreateCrystal("Main_Crystal", root, Vector3.zero, 0.42f, 1.95f, moon, 1.6f, Quaternion.Euler(0f, 20f, 0f));
        CreateCrystal("Shard_A", root, new Vector3(-0.46f, -0.1f, 0.12f), 0.24f, 1.25f, moon * 0.88f, 1.05f, Quaternion.Euler(8f, -32f, -7f));
        CreateCrystal("Shard_B", root, new Vector3(0.42f, -0.12f, -0.18f), 0.22f, 1.05f, moon * 0.78f, 0.95f, Quaternion.Euler(-6f, 42f, 9f));
        CreateCrystal("Shard_C", root, new Vector3(0.02f, -0.14f, 0.46f), 0.18f, 0.85f, moon * 0.72f, 0.85f, Quaternion.Euler(5f, 8f, 12f));
        CreatePart(PrimitiveType.Cylinder, "Moon_Glow_Ring", root, new Vector3(0f, 0.08f, 0f), new Vector3(1.85f, 0.025f, 1.85f), moon, 0.95f, Quaternion.Euler(90f, 0f, 0f));
    }

    public static void ApplyBoundaryGateway(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Transform root = PrepareRoot(target);
        HideSupersededVisuals(target);

        Color stone = new Color(0.74f, 0.78f, 0.84f);
        Color light = new Color(0.58f, 0.78f, 1f);
        CreatePart(PrimitiveType.Cylinder, "Boundary_Platform", root, new Vector3(0f, 0.16f, 0f), new Vector3(6.2f, 0.12f, 6.2f), stone * 0.76f, 0f);
        CreatePart(PrimitiveType.Cube, "Left_Monolith", root, new Vector3(-1.55f, 2.25f, 0f), new Vector3(0.72f, 4.1f, 0.8f), stone, 0.16f, Quaternion.Euler(0f, 0f, -2.5f));
        CreatePart(PrimitiveType.Cube, "Right_Monolith", root, new Vector3(1.55f, 2.25f, 0f), new Vector3(0.72f, 4.1f, 0.8f), stone, 0.16f, Quaternion.Euler(0f, 0f, 2.5f));
        CreatePart(PrimitiveType.Cube, "Top_Keystone", root, new Vector3(0f, 4.55f, 0f), new Vector3(3.9f, 0.55f, 0.9f), stone * 0.92f, 0.16f);
        CreatePart(PrimitiveType.Cube, "Threshold_Light", root, new Vector3(0f, 2.3f, -0.04f), new Vector3(2.05f, 2.85f, 0.08f), light, 1.4f);
        CreatePart(PrimitiveType.Cylinder, "Boundary_Ring", root, new Vector3(0f, 2.35f, 0f), new Vector3(2.65f, 0.045f, 2.65f), light, 1.1f, Quaternion.Euler(90f, 0f, 0f));
    }

    public static void ApplyCivilizationSeed(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Transform root = PrepareRoot(target);
        HideSupersededVisuals(target);

        Color gold = new Color(1f, 0.84f, 0.32f);
        Color cyan = new Color(0.38f, 0.86f, 1f);
        CreatePart(PrimitiveType.Cylinder, "Seed_Pedestal", root, new Vector3(0f, -0.42f, 0f), new Vector3(1.35f, 0.12f, 1.35f), gold * 0.56f, 0.18f);
        CreateCrystal("Rule_Core", root, new Vector3(0f, 0.02f, 0f), 0.34f, 1.55f, gold, 1.45f, Quaternion.Euler(0f, 45f, 0f));
        CreatePart(PrimitiveType.Cylinder, "Outer_Orbit_A", root, new Vector3(0f, 0.58f, 0f), new Vector3(1.42f, 0.025f, 1.42f), cyan, 1.05f, Quaternion.Euler(68f, 0f, 0f));
        CreatePart(PrimitiveType.Cylinder, "Outer_Orbit_B", root, new Vector3(0f, 0.58f, 0f), new Vector3(1.2f, 0.025f, 1.2f), gold, 0.9f, Quaternion.Euler(0f, 0f, 68f));
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f;
            Vector3 pos = Quaternion.Euler(0f, angle, 0f) * new Vector3(0.72f, 0f, 0f);
            CreatePart(PrimitiveType.Cube, $"Rule_Tablet_{i}", root, pos + Vector3.up * 0.14f, new Vector3(0.12f, 0.72f, 0.36f), Color.Lerp(gold, cyan, 0.35f), 0.45f, Quaternion.Euler(0f, angle, 18f));
        }
    }

    static Transform PrepareRoot(GameObject target)
    {
        Transform existing = target.transform.Find(RootName);
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }

        GameObject root = new GameObject(RootName);
        root.transform.SetParent(target.transform, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root.transform;
    }

    static void HideSupersededVisuals(GameObject target)
    {
        Renderer directRenderer = target.GetComponent<Renderer>();
        if (directRenderer != null)
        {
            directRenderer.enabled = false;
        }

        Transform lowPoly = target.transform.Find("LowPolyVisual");
        if (lowPoly != null)
        {
            lowPoly.gameObject.SetActive(false);
        }
    }

    static GameObject CreatePart(
        PrimitiveType type,
        string name,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color color,
        float emissionStrength,
        Quaternion? localRotation = null)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation ?? Quaternion.identity;
        part.transform.localScale = localScale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = CreateMaterial(color, color * emissionStrength, 0.18f);
        }

        return part;
    }

    static GameObject CreateCrystal(
        string name,
        Transform parent,
        Vector3 localPosition,
        float radius,
        float height,
        Color color,
        float emissionStrength,
        Quaternion localRotation)
    {
        GameObject crystal = new GameObject(name);
        crystal.transform.SetParent(parent, false);
        crystal.transform.localPosition = localPosition;
        crystal.transform.localRotation = localRotation;

        MeshFilter filter = crystal.AddComponent<MeshFilter>();
        filter.sharedMesh = CreateCrystalMesh(radius, height, 6);

        MeshRenderer renderer = crystal.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = CreateMaterial(color, color * emissionStrength, 0.08f);
        return crystal;
    }

    static Mesh CreateCrystalMesh(float radius, float height, int sides)
    {
        int vertexCount = 2 + sides * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        vertices[0] = Vector3.up * height;
        vertices[1] = Vector3.zero;

        float lowerY = height * 0.24f;
        float upperY = height * 0.68f;
        for (int i = 0; i < sides; i++)
        {
            float angle = (Mathf.PI * 2f * i) / sides;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[2 + i] = new Vector3(x, lowerY, z);
            vertices[2 + sides + i] = new Vector3(x * 0.62f, upperY, z * 0.62f);
        }

        int[] triangles = new int[sides * 12];
        int t = 0;
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            int lower = 2 + i;
            int lowerNext = 2 + next;
            int upper = 2 + sides + i;
            int upperNext = 2 + sides + next;

            triangles[t++] = 0;
            triangles[t++] = upper;
            triangles[t++] = upperNext;

            triangles[t++] = upper;
            triangles[t++] = lower;
            triangles[t++] = lowerNext;
            triangles[t++] = upper;
            triangles[t++] = lowerNext;
            triangles[t++] = upperNext;

            triangles[t++] = 1;
            triangles[t++] = lowerNext;
            triangles[t++] = lower;
        }

        Mesh mesh = new Mesh
        {
            name = "Procedural_Crystal"
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static Material CreateMaterial(Color color, Color emission, float smoothness)
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

        if (emission != Color.black && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }

        return material;
    }
}
