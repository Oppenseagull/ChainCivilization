using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class LowPolyOverhaulTool : EditorWindow
{
    const string EnvironmentRootName = "LowPolySceneDressing";
    const string SiteRootName = "LowPolySiteDressing";

    const string TerrainMaterialPath = "Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Terrain_mat.mat";
    const string TerrainTexturePath = "Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Ground_Grass_Green_01.png";
    const string GroundGenericTexturePath = "Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Ground_Generic_03.png";
    const string SkyboxMaterialPath = "Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Skybox_mat.mat";
    const string SkyboxTexturePath = "Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Skybox_Texture_01.png";

    const string RockPrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Generic_Rock_01.prefab";
    const string MenhirPrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Menhir_Rock_02.prefab";
    const string OrePrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Ore_Rock_01.prefab";
    const string PinePrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_green.prefab";
    const string ShrubPrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Shrubs/PT_Generic_Shrub_01_green.prefab";
    const string FlowerPrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Flowers/PT_Poppy_02.prefab";
    const string GrassPrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Plants/PT_Grass_02.prefab";
    const string MushroomPrefabPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Mushrooms/PT_Caesars_Mushroom_01.prefab";
    const string FencePrefabPath = "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Fence_Wood_01.prefab";
    const string GatePrefabPath = "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Gate_Wood_01.prefab";
    const string BridgePrefabPath = "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Bridge/PT_Wooden_Bridge_02.prefab";

    static readonly Vector3 Spawn = new Vector3(0f, 0f, 0f);
    static readonly Vector3 BlueDao = new Vector3(-200f, 0f, -200f);
    static readonly Vector3 RedDao = new Vector3(200f, 0f, 200f);
    static readonly Vector3 GreenDao = new Vector3(280f, 0f, -280f);
    static readonly Vector3 Boundary = new Vector3(420f, 0f, -420f);
    static readonly Vector3 CivilizationSeed = new Vector3(400f, 0f, -400f);

    [MenuItem("ChainCivilization/Apply Low Poly Overhaul")]
    public static void ApplyOverhaul()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Stop Play Mode first, then run ChainCivilization/Apply Low Poly Overhaul.", "OK");
            return;
        }

        ForceFixMaterial(TerrainMaterialPath, TerrainTexturePath, true);
        ForceFixMaterial("Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Pine_Tree_Leaves_Mat.mat", "Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Generic_Tree_Leaves_01.png", true);
        ForceFixMaterial("Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Pine_Tree_Trunk_Mat.mat", "Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Tree_Trunk_01.png", true);
        ForceFixMaterial("Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Rocks_mat.mat", "Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Rocks_01.png", true);
        ForceFixMaterial(SkyboxMaterialPath, SkyboxTexturePath, false);

        GameObject rockPrefab = LoadPrefab(RockPrefabPath);
        GameObject menhirPrefab = LoadPrefab(MenhirPrefabPath);
        GameObject orePrefab = LoadPrefab(OrePrefabPath);
        GameObject pinePrefab = LoadPrefab(PinePrefabPath);
        GameObject shrubPrefab = LoadPrefab(ShrubPrefabPath);
        GameObject flowerPrefab = LoadPrefab(FlowerPrefabPath);
        GameObject grassPrefab = LoadPrefab(GrassPrefabPath);
        GameObject mushroomPrefab = LoadPrefab(MushroomPrefabPath);
        GameObject fencePrefab = LoadPrefab(FencePrefabPath);
        GameObject gatePrefab = LoadPrefab(GatePrefabPath);
        GameObject bridgePrefab = LoadPrefab(BridgePrefabPath);

        ConfigureEnvironmentSpawner(rockPrefab, pinePrefab, menhirPrefab, orePrefab);
        ApplyLightingAndSky();
        SetupGroundTerrain();
        BuildWorldDressing(rockPrefab, pinePrefab, shrubPrefab, flowerPrefab, grassPrefab, mushroomPrefab, bridgePrefab);
        UpgradeLandmarks(menhirPrefab, fencePrefab, gatePrefab, pinePrefab, shrubPrefab, flowerPrefab, rockPrefab, mushroomPrefab);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        SceneView.RepaintAll();
        Debug.Log("=== Chain Civilization low-poly scene overhaul applied. Save the scene if the result looks good. ===");
    }

    static void ConfigureEnvironmentSpawner(GameObject rockPrefab, GameObject treePrefab, GameObject ruinPrefab, GameObject towerPrefab)
    {
        EnvironmentPropSpawner spawner = Object.FindFirstObjectByType<EnvironmentPropSpawner>();
        if (spawner == null)
        {
            return;
        }

        spawner.lowPolyRockPrefab = rockPrefab;
        spawner.lowPolyTreePrefab = treePrefab;
        spawner.lowPolyRuinPrefab = ruinPrefab;
        spawner.lowPolyTowerPrefab = towerPrefab;
        EditorUtility.SetDirty(spawner);
    }

    static void ApplyLightingAndSky()
    {
        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.72f, 0.86f);
        RenderSettings.ambientEquatorColor = new Color(0.38f, 0.48f, 0.42f);
        RenderSettings.ambientGroundColor = new Color(0.18f, 0.2f, 0.18f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.5f, 0.64f, 0.7f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 120f;
        RenderSettings.fogEndDistance = 620f;

        Light sun = Object.FindFirstObjectByType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.color = new Color(1f, 0.88f, 0.68f);
            sun.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            EditorUtility.SetDirty(sun);
        }
    }

    static void SetupGroundTerrain()
    {
        Material terrainMat = AssetDatabase.LoadAssetAtPath<Material>(TerrainMaterialPath);
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
        }

        ground.transform.localScale = new Vector3(200f, 1f, 200f);
        ground.transform.position = new Vector3(100f, -0.05f, -100f);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null && terrainMat != null)
        {
            renderer.sharedMaterial = terrainMat;
        }

        EditorUtility.SetDirty(ground);
    }

    static void BuildWorldDressing(
        GameObject rockPrefab,
        GameObject pinePrefab,
        GameObject shrubPrefab,
        GameObject flowerPrefab,
        GameObject grassPrefab,
        GameObject mushroomPrefab,
        GameObject bridgePrefab)
    {
        GameObject existing = GameObject.Find(EnvironmentRootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject root = new GameObject(EnvironmentRootName);
        Material pathMat = CreatePathMaterial();

        CreatePathStrip(root.transform, Spawn, BlueDao, 11f, pathMat);
        CreatePathStrip(root.transform, BlueDao, RedDao, 11f, pathMat);
        CreatePathStrip(root.transform, RedDao, GreenDao, 11f, pathMat);
        CreatePathStrip(root.transform, GreenDao, Boundary, 10f, pathMat);
        CreatePathStrip(root.transform, Boundary, CivilizationSeed, 8f, pathMat);

        PlaceTrailPebbles(root.transform, rockPrefab, Spawn, BlueDao, 12, 1.0f);
        PlaceTrailPebbles(root.transform, rockPrefab, BlueDao, RedDao, 16, 0.9f);
        PlaceTrailPebbles(root.transform, rockPrefab, RedDao, GreenDao, 16, 0.9f);
        PlaceTrailPebbles(root.transform, rockPrefab, GreenDao, Boundary, 10, 1.05f);

        PlacePrefabWorld(bridgePrefab, root.transform, "Bridge_Blue_Approach", new Vector3(-95f, 0.02f, -95f), new Vector3(0f, 45f, 0f), 3.4f);
        PlaceCluster(root.transform, pinePrefab, shrubPrefab, flowerPrefab, grassPrefab, mushroomPrefab, new Vector3(-40f, 0f, 26f), 13f, 10);
        PlaceCluster(root.transform, pinePrefab, shrubPrefab, flowerPrefab, grassPrefab, mushroomPrefab, new Vector3(84f, 0f, -72f), 18f, 14);
        PlaceCluster(root.transform, pinePrefab, shrubPrefab, flowerPrefab, grassPrefab, mushroomPrefab, new Vector3(315f, 0f, -210f), 20f, 16);
        PlaceCluster(root.transform, pinePrefab, shrubPrefab, flowerPrefab, grassPrefab, mushroomPrefab, new Vector3(375f, 0f, -445f), 12f, 8);

        EditorUtility.SetDirty(root);
    }

    static void UpgradeLandmarks(
        GameObject menhirPrefab,
        GameObject fencePrefab,
        GameObject gatePrefab,
        GameObject treePrefab,
        GameObject shrubPrefab,
        GameObject flowerPrefab,
        GameObject rockPrefab,
        GameObject mushroomPrefab)
    {
        BlueDAOSteleInteract blue = Object.FindFirstObjectByType<BlueDAOSteleInteract>();
        RedDAOSteleInteract red = Object.FindFirstObjectByType<RedDAOSteleInteract>();
        GreenDAOSteleInteract green = Object.FindFirstObjectByType<GreenDAOSteleInteract>();

        if (blue != null)
        {
            HideMeshAndAddPrefab(blue.gameObject, menhirPrefab, new Color(0.2f, 0.6f, 1f));
            DecorateDaoSite(blue.transform, "Blue", new Color(0.2f, 0.6f, 1f), fencePrefab, gatePrefab, treePrefab, shrubPrefab, flowerPrefab, rockPrefab, mushroomPrefab);
        }

        if (red != null)
        {
            HideMeshAndAddPrefab(red.gameObject, menhirPrefab, new Color(1f, 0.3f, 0.2f));
            DecorateDaoSite(red.transform, "Red", new Color(1f, 0.3f, 0.2f), fencePrefab, gatePrefab, treePrefab, shrubPrefab, flowerPrefab, rockPrefab, mushroomPrefab);
        }

        if (green != null)
        {
            HideMeshAndAddPrefab(green.gameObject, menhirPrefab, new Color(0.2f, 1f, 0.4f));
            DecorateDaoSite(green.transform, "Green", new Color(0.2f, 1f, 0.4f), fencePrefab, gatePrefab, treePrefab, shrubPrefab, flowerPrefab, rockPrefab, mushroomPrefab);
        }

        BoundaryTrigger boundary = Object.FindFirstObjectByType<BoundaryTrigger>();
        if (boundary != null)
        {
            Transform stone = boundary.transform.parent != null ? boundary.transform.parent.Find("BoundaryStone") : null;
            if (stone != null)
            {
                HideMeshAndAddPrefab(stone.gameObject, menhirPrefab, new Color(0.5f, 0.8f, 1f));
                DecorateBoundary(stone.transform, rockPrefab, treePrefab, shrubPrefab);
            }
        }
    }

    static void DecorateDaoSite(
        Transform site,
        string label,
        Color accent,
        GameObject fencePrefab,
        GameObject gatePrefab,
        GameObject treePrefab,
        GameObject shrubPrefab,
        GameObject flowerPrefab,
        GameObject rockPrefab,
        GameObject mushroomPrefab)
    {
        Transform root = PrepareChildRoot(site, SiteRootName);

        PlacePrefabLocal(gatePrefab, root, $"{label}_Gate", new Vector3(0f, 0f, -5.6f), new Vector3(0f, 0f, 0f), 1.35f);
        PlacePrefabLocal(fencePrefab, root, $"{label}_Fence_NW", new Vector3(-3.8f, 0f, -4.5f), new Vector3(0f, 35f, 0f), 1.4f);
        PlacePrefabLocal(fencePrefab, root, $"{label}_Fence_NE", new Vector3(3.8f, 0f, -4.5f), new Vector3(0f, -35f, 0f), 1.4f);
        PlacePrefabLocal(fencePrefab, root, $"{label}_Fence_W", new Vector3(-5.4f, 0f, 0.3f), new Vector3(0f, 88f, 0f), 1.45f);
        PlacePrefabLocal(fencePrefab, root, $"{label}_Fence_E", new Vector3(5.4f, 0f, 0.3f), new Vector3(0f, 92f, 0f), 1.45f);
        PlacePrefabLocal(fencePrefab, root, $"{label}_Fence_Back", new Vector3(0f, 0f, 5.5f), new Vector3(0f, 0f, 0f), 1.6f);

        PlacePrefabLocal(treePrefab, root, $"{label}_Tree_A", new Vector3(-6.2f, 0f, 5.8f), new Vector3(0f, 22f, 0f), 1.5f);
        PlacePrefabLocal(treePrefab, root, $"{label}_Tree_B", new Vector3(6.4f, 0f, 5.2f), new Vector3(0f, -18f, 0f), 1.35f);
        PlacePrefabLocal(shrubPrefab, root, $"{label}_Shrub_A", new Vector3(-4.8f, 0f, -2.2f), Vector3.zero, 1.25f);
        PlacePrefabLocal(shrubPrefab, root, $"{label}_Shrub_B", new Vector3(4.7f, 0f, -1.7f), Vector3.zero, 1.1f);
        PlacePrefabLocal(flowerPrefab, root, $"{label}_Flowers_A", new Vector3(-2.7f, 0f, -4.0f), Vector3.zero, 1.4f);
        PlacePrefabLocal(flowerPrefab, root, $"{label}_Flowers_B", new Vector3(2.8f, 0f, -3.8f), Vector3.zero, 1.2f);
        PlacePrefabLocal(rockPrefab, root, $"{label}_Rocks", new Vector3(0.9f, 0f, 4.4f), new Vector3(0f, 47f, 0f), 1.1f);
        PlacePrefabLocal(mushroomPrefab, root, $"{label}_Mushrooms", new Vector3(-1.5f, 0f, 4.2f), Vector3.zero, 1.0f);

        Light light = new GameObject($"{label}_SiteLight").AddComponent<Light>();
        light.transform.SetParent(root, false);
        light.transform.localPosition = new Vector3(0f, 2.4f, -1.0f);
        light.type = LightType.Point;
        light.color = accent;
        light.intensity = 2.2f;
        light.range = 14f;
        light.shadows = LightShadows.None;
    }

    static void DecorateBoundary(Transform stone, GameObject rockPrefab, GameObject treePrefab, GameObject shrubPrefab)
    {
        Transform root = PrepareChildRoot(stone, SiteRootName);
        PlacePrefabLocal(rockPrefab, root, "Boundary_Rock_Left", new Vector3(-5.8f, 0f, -1.4f), new Vector3(0f, 12f, 0f), 1.35f);
        PlacePrefabLocal(rockPrefab, root, "Boundary_Rock_Right", new Vector3(5.2f, 0f, 1.1f), new Vector3(0f, -22f, 0f), 1.15f);
        PlacePrefabLocal(treePrefab, root, "Boundary_Tree_Left", new Vector3(-7.5f, 0f, 3.8f), new Vector3(0f, 35f, 0f), 1.45f);
        PlacePrefabLocal(treePrefab, root, "Boundary_Tree_Right", new Vector3(8f, 0f, 3.4f), new Vector3(0f, -20f, 0f), 1.25f);
        PlacePrefabLocal(shrubPrefab, root, "Boundary_Shrubs", new Vector3(0f, 0f, -5.8f), Vector3.zero, 1.6f);
    }

    static Transform PrepareChildRoot(Transform parent, string rootName)
    {
        Transform existing = parent.Find(rootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject root = new GameObject(rootName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root.transform;
    }

    static void CreatePathStrip(Transform parent, Vector3 from, Vector3 to, float width, Material material)
    {
        Vector3 delta = to - from;
        Vector3 midpoint = (from + to) * 0.5f;
        float length = new Vector2(delta.x, delta.z).magnitude;
        float yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;

        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "LowPoly_Path";
        strip.transform.SetParent(parent, false);
        strip.transform.position = midpoint + Vector3.up * 0.012f;
        strip.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        strip.transform.localScale = new Vector3(width, 0.025f, length);
        StripColliders(strip);

        Renderer renderer = strip.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    static void PlaceTrailPebbles(Transform parent, GameObject rockPrefab, Vector3 from, Vector3 to, int count, float baseScale)
    {
        for (int i = 1; i < count; i++)
        {
            float t = i / (float)count;
            Vector3 pos = Vector3.Lerp(from, to, t);
            float side = (i % 2 == 0) ? -1f : 1f;
            Vector3 dir = (to - from).normalized;
            Vector3 lateral = new Vector3(-dir.z, 0f, dir.x);
            pos += lateral * side * (5.6f + (i % 3) * 1.8f);
            PlacePrefabWorld(rockPrefab, parent, $"TrailRock_{i:00}", pos, new Vector3(0f, i * 37f, 0f), baseScale * (0.7f + (i % 4) * 0.12f));
        }
    }

    static void PlaceCluster(
        Transform parent,
        GameObject treePrefab,
        GameObject shrubPrefab,
        GameObject flowerPrefab,
        GameObject grassPrefab,
        GameObject mushroomPrefab,
        Vector3 center,
        float radius,
        int count)
    {
        GameObject[] choices = { treePrefab, shrubPrefab, flowerPrefab, grassPrefab, mushroomPrefab };
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = choices[i % choices.Length];
            float angle = i * 137.5f * Mathf.Deg2Rad;
            float distance = radius * (0.25f + 0.75f * ((i % 7) / 6f));
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            float scale = prefab == treePrefab ? 1.25f + (i % 3) * 0.15f : 0.9f + (i % 4) * 0.12f;
            PlacePrefabWorld(prefab, parent, $"NatureCluster_{i:00}", pos, new Vector3(0f, i * 29f, 0f), scale);
        }
    }

    static GameObject PlacePrefabLocal(GameObject prefab, Transform parent, string name, Vector3 localPosition, Vector3 localEuler, float scale)
    {
        GameObject instance = InstantiatePrefab(prefab, name);
        if (instance == null)
        {
            return null;
        }

        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(localEuler);
        instance.transform.localScale *= scale;
        StripColliders(instance);
        return instance;
    }

    static GameObject PlacePrefabWorld(GameObject prefab, Transform parent, string name, Vector3 position, Vector3 euler, float scale)
    {
        GameObject instance = InstantiatePrefab(prefab, name);
        if (instance == null)
        {
            return null;
        }

        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(euler);
        instance.transform.localScale *= scale;
        StripColliders(instance);
        return instance;
    }

    static GameObject InstantiatePrefab(GameObject prefab, string name)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance != null)
        {
            instance.name = name;
        }

        return instance;
    }

    static void HideMeshAndAddPrefab(GameObject target, GameObject prefab, Color glowColor)
    {
        if (target == null || prefab == null)
        {
            return;
        }

        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in target.transform)
        {
            if (child.name == "LowPolyVisual")
            {
                toDestroy.Add(child.gameObject);
            }
        }

        for (int i = 0; i < toDestroy.Count; i++)
        {
            Object.DestroyImmediate(toDestroy[i]);
        }

        GameObject visual = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (visual == null)
        {
            return;
        }

        visual.name = "LowPolyVisual";
        visual.transform.SetParent(target.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 1.5f;
        StripColliders(visual);

        Light light = visual.GetComponent<Light>();
        if (light == null)
        {
            light = visual.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.color = glowColor;
        light.intensity = 4.2f;
        light.range = 18f;
        light.shadows = LightShadows.None;
    }

    static void StripColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    static Material CreatePathMaterial()
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(GroundGenericTexturePath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader)
        {
            name = "Generated_LowPoly_Path_Mat",
            color = new Color(0.56f, 0.5f, 0.39f)
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", new Color(0.56f, 0.5f, 0.39f));
        }

        if (texture != null && material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }

        return material;
    }

    static GameObject LoadPrefab(string path)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static void ForceFixMaterial(string matPath, string texPath, bool isURPLit)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        if (mat == null)
        {
            return;
        }

        if (isURPLit)
        {
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");
            if (tex != null)
            {
                mat.SetTexture("_BaseMap", tex);
                mat.SetColor("_BaseColor", Color.white);
            }
        }
        else
        {
            mat.shader = Shader.Find("Skybox/Panoramic");
            if (tex != null)
            {
                mat.SetTexture("_MainTex", tex);
                mat.color = Color.white;
            }
        }

        EditorUtility.SetDirty(mat);
    }
}
