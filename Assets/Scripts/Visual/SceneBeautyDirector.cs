using System.Collections;
using UnityEngine;

/// <summary>
/// Runtime scene dressing that uses the imported low-poly asset pack.
/// Procedural meshes are only fallbacks when a packaged prefab is missing.
/// </summary>
public class SceneBeautyDirector : MonoBehaviour
{
    const string RootName = "SceneBeauty_Runtime";
    const string SiteRootName = "SceneBeauty_Site";

    static readonly Vector3 Spawn = new Vector3(0f, 0f, 0f);
    static readonly Vector3 BlueDao = new Vector3(-200f, 0f, -200f);
    static readonly Vector3 RedDao = new Vector3(200f, 0f, 200f);
    static readonly Vector3 GreenDao = new Vector3(280f, 0f, -280f);
    static readonly Vector3 Boundary = new Vector3(420f, 0f, -420f);
    static readonly Vector3 CivilizationSeed = new Vector3(400f, 0f, -400f);

    Transform _root;
    Material _grassMat;
    Material _meadowMat;
    Material _pathMat;
    Material _stoneMat;
    Material _blueMat;
    Material _redMat;
    Material _greenMat;
    Material _goldMat;

    Texture2D _grassTexture;
    Texture2D _pathTexture;
    Texture2D _skyTexture;

    GameObject _pinePrefab;
    GameObject _fruitTreePrefab;
    GameObject _shrubPrefab;
    GameObject _grassPrefab;
    GameObject _flowerPrefab;
    GameObject _mushroomPrefab;
    GameObject _rockPrefab;
    GameObject _riverRockPrefab;
    GameObject _menhirPrefab;
    GameObject _orePrefab;
    GameObject _fencePrefab;
    GameObject _fenceAltPrefab;
    GameObject _gatePrefab;
    GameObject _bridgePrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindAnyObjectByType<SceneBeautyDirector>() != null)
        {
            return;
        }

        GameObject obj = new GameObject("SceneBeautyDirector");
        obj.AddComponent<SceneBeautyDirector>();
    }

    IEnumerator Start()
    {
        yield return null;
        Apply();
    }

    [ContextMenu("Apply Scene Beauty")]
    public void Apply()
    {
        LoadResources();
        PrepareMaterials();
        PrepareRoot();
        ApplySkyAndLighting();
        StyleGround();
        BuildPaths();
        BuildNature();
        BuildDaoSites();
        BuildBoundarySite();
        Debug.Log($"SceneBeautyDirector applied. Asset prefabs loaded: tree={_pinePrefab != null}, grass={_grassPrefab != null}, menhir={_menhirPrefab != null}, fence={_fencePrefab != null}.");
    }

    void LoadResources()
    {
        _grassTexture = Resources.Load<Texture2D>("SceneBeauty/Textures/PT_Ground_Grass_Green_01");
        _pathTexture = Resources.Load<Texture2D>("SceneBeauty/Textures/PT_Ground_Generic_03");
        _skyTexture = Resources.Load<Texture2D>("SceneBeauty/Textures/PT_Skybox_Texture_01");

        _pinePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Pine_Tree_03_green");
        _fruitTreePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Fruit_Tree_01_green");
        _shrubPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Generic_Shrub_01_green");
        _grassPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Grass_02");
        _flowerPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Poppy_02");
        _mushroomPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Caesars_Mushroom_01");
        _rockPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Generic_Rock_01");
        _riverRockPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_River_Rock_Pile_02");
        _menhirPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Menhir_Rock_02");
        _orePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Ore_Rock_01");
        _fencePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Modular_Fence_Wood_01");
        _fenceAltPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Modular_Fence_Wood_02");
        _gatePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Modular_Gate_Wood_01");
        _bridgePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/PT_Wooden_Bridge_02");
    }

    void PrepareMaterials()
    {
        _grassMat = CreateMaterial("Civ_Grass_Texture", new Color(0.78f, 0.9f, 0.62f), Color.black, 0.08f, _grassTexture, new Vector2(52f, 52f));
        _meadowMat = CreateMaterial("Civ_Meadow_Texture", new Color(0.48f, 0.72f, 0.42f), Color.black, 0.06f, _grassTexture, new Vector2(22f, 22f));
        _pathMat = CreateMaterial("Civ_Path_Texture", new Color(0.68f, 0.58f, 0.42f), Color.black, 0.04f, _pathTexture, new Vector2(18f, 18f));
        _stoneMat = CreateMaterial("Civ_Stone", new Color(0.58f, 0.62f, 0.58f), Color.black, 0.1f);
        _blueMat = CreateMaterial("Civ_Blue_Rune", new Color(0.28f, 0.62f, 1f), new Color(0.18f, 0.55f, 1f) * 1.15f, 0.2f);
        _redMat = CreateMaterial("Civ_Red_Rune", new Color(1f, 0.38f, 0.24f), new Color(1f, 0.2f, 0.14f) * 1.05f, 0.18f);
        _greenMat = CreateMaterial("Civ_Green_Rune", new Color(0.26f, 0.9f, 0.42f), new Color(0.16f, 0.82f, 0.32f) * 1.05f, 0.16f);
        _goldMat = CreateMaterial("Civ_Gold", new Color(1f, 0.76f, 0.28f), new Color(1f, 0.56f, 0.16f) * 0.65f, 0.22f);
    }

    void PrepareRoot()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            Destroy(existing);
        }

        _root = new GameObject(RootName).transform;
    }

    void ApplySkyAndLighting()
    {
        Material sky = CreateSkybox();
        if (sky != null)
        {
            RenderSettings.skybox = sky;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.78f, 0.9f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.52f, 0.66f, 0.52f);
        RenderSettings.ambientGroundColor = new Color(0.28f, 0.33f, 0.24f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.72f, 0.86f, 0.95f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 320f;
        RenderSettings.fogEndDistance = 980f;

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.62f, 0.82f, 1f);
        }

        Light sun = FindAnyObjectByType<Light>();
        if (sun != null && sun.type == LightType.Directional)
        {
            sun.color = new Color(1f, 0.9f, 0.68f);
            sun.intensity = 1.45f;
            sun.transform.rotation = Quaternion.Euler(48f, -34f, 0f);
        }
    }

    void StyleGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
        }

        ground.transform.position = new Vector3(100f, -0.055f, -100f);
        ground.transform.localScale = new Vector3(205f, 1f, 205f);
        SetMaterial(ground, _grassMat);

        CreatePatch("Meadow_A", new Vector3(-86f, -0.035f, 38f), new Vector3(110f, 0.035f, 76f), 18f);
        CreatePatch("Meadow_B", new Vector3(108f, -0.034f, -72f), new Vector3(132f, 0.035f, 92f), -12f);
        CreatePatch("GreenDao_Meadow", new Vector3(286f, -0.033f, -270f), new Vector3(96f, 0.035f, 82f), 24f);
        CreatePatch("Boundary_Meadow", new Vector3(410f, -0.032f, -425f), new Vector3(74f, 0.035f, 58f), -8f);
    }

    void BuildPaths()
    {
        CreatePath(Spawn, BlueDao, 12f);
        CreatePath(BlueDao, RedDao, 12f);
        CreatePath(RedDao, GreenDao, 12f);
        CreatePath(GreenDao, Boundary, 10f);
        CreatePath(Boundary, CivilizationSeed, 8f);

        PlaceTrailStones(Spawn, BlueDao, 16);
        PlaceTrailStones(BlueDao, RedDao, 22);
        PlaceTrailStones(RedDao, GreenDao, 22);
        PlaceTrailStones(GreenDao, Boundary, 14);
        PlaceAsset(_bridgePrefab, _root, "Asset_Bridge_BlueApproach", new Vector3(-92f, 0f, -92f), Quaternion.Euler(0f, 45f, 0f), 3.4f);
    }

    void BuildNature()
    {
        CreateCluster(new Vector3(-45f, 0f, 28f), 20f, 22, 100);
        CreateCluster(new Vector3(84f, 0f, -74f), 26f, 28, 200);
        CreateCluster(new Vector3(312f, 0f, -210f), 28f, 30, 300);
        CreateCluster(new Vector3(378f, 0f, -452f), 18f, 18, 400);
        CreateCluster(new Vector3(-238f, 0f, -164f), 24f, 24, 500);
        CreateCluster(new Vector3(172f, 0f, 242f), 24f, 24, 600);
    }

    void BuildDaoSites()
    {
        BlueDAOSteleInteract blue = FindAnyObjectByType<BlueDAOSteleInteract>();
        RedDAOSteleInteract red = FindAnyObjectByType<RedDAOSteleInteract>();
        GreenDAOSteleInteract green = FindAnyObjectByType<GreenDAOSteleInteract>();

        if (blue != null)
        {
            BuildDaoSite(blue.transform, "Blue", _blueMat, new Color(0.32f, 0.66f, 1f), 0f);
        }

        if (red != null)
        {
            BuildDaoSite(red.transform, "Red", _redMat, new Color(1f, 0.42f, 0.24f), 18f);
        }

        if (green != null)
        {
            BuildDaoSite(green.transform, "Green", _greenMat, new Color(0.28f, 0.9f, 0.38f), -16f);
        }
    }

    void BuildDaoSite(Transform site, string label, Material accentMat, Color lightColor, float yawOffset)
    {
        HideOldLandmark(site);
        Transform root = PrepareSiteRoot(site);
        root.localRotation = Quaternion.Euler(0f, yawOffset, 0f);

        CreateLocalPart(root, $"{label}_StonePlaza", PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(7.8f, 0.08f, 7.8f), _stoneMat);
        CreateLocalPart(root, $"{label}_RuneRing", PrimitiveType.Cylinder, new Vector3(0f, 0.14f, 0f), new Vector3(5.6f, 0.04f, 5.6f), accentMat);
        PlaceAssetLocal(_menhirPrefab, root, $"{label}_MainMenhir", new Vector3(0f, 0.06f, 0f), Quaternion.Euler(0f, 15f, 0f), 3.2f);
        PlaceAssetLocal(_orePrefab, root, $"{label}_EnergyStone", new Vector3(0f, 0.12f, 3.35f), Quaternion.Euler(0f, 30f, 0f), 2.0f);

        PlaceAssetLocal(_gatePrefab, root, $"{label}_Gate", new Vector3(0f, 0f, -5.8f), Quaternion.identity, 1.75f);
        PlaceAssetLocal(_fencePrefab, root, $"{label}_Fence_NW", new Vector3(-3.8f, 0f, -4.7f), Quaternion.Euler(0f, 35f, 0f), 1.55f);
        PlaceAssetLocal(_fencePrefab, root, $"{label}_Fence_NE", new Vector3(3.8f, 0f, -4.7f), Quaternion.Euler(0f, -35f, 0f), 1.55f);
        PlaceAssetLocal(_fenceAltPrefab, root, $"{label}_Fence_W", new Vector3(-5.8f, 0f, 0.2f), Quaternion.Euler(0f, 90f, 0f), 1.65f);
        PlaceAssetLocal(_fenceAltPrefab, root, $"{label}_Fence_E", new Vector3(5.8f, 0f, 0.2f), Quaternion.Euler(0f, 90f, 0f), 1.65f);
        PlaceAssetLocal(_fencePrefab, root, $"{label}_Fence_Back", new Vector3(0f, 0f, 5.8f), Quaternion.identity, 1.75f);

        CreateTree(root, new Vector3(-6.8f, 0f, 5.8f), 1.35f, 20f);
        CreateTree(root, new Vector3(6.6f, 0f, 5.2f), 1.18f, -24f);
        CreateShrub(root, new Vector3(-4.8f, 0f, -2.5f), 1.2f);
        CreateShrub(root, new Vector3(4.8f, 0f, -2.0f), 1.1f);
        CreateFlower(root, new Vector3(-2.8f, 0f, -4.7f), 1.2f);
        CreateFlower(root, new Vector3(2.8f, 0f, -4.6f), 1.1f);
        CreateRock(root, new Vector3(1.2f, 0f, 4.5f), 0.95f, 28f);

        Light light = new GameObject($"{label}_DAO_Light").AddComponent<Light>();
        light.transform.SetParent(root, false);
        light.transform.localPosition = new Vector3(0f, 3.1f, -0.6f);
        light.type = LightType.Point;
        light.color = lightColor;
        light.intensity = 3.2f;
        light.range = 18f;
        light.shadows = LightShadows.None;
    }

    void BuildBoundarySite()
    {
        BoundaryTrigger boundary = FindAnyObjectByType<BoundaryTrigger>();
        if (boundary == null)
        {
            return;
        }

        Transform parent = boundary.transform.parent != null ? boundary.transform.parent : boundary.transform;
        HideOldLandmark(parent);
        Transform root = PrepareSiteRoot(parent);
        CreateLocalPart(root, "Boundary_StoneWalk", PrimitiveType.Cylinder, new Vector3(0f, 0.04f, 0f), new Vector3(9f, 0.08f, 9f), _stoneMat);
        PlaceAssetLocal(_menhirPrefab, root, "Boundary_LeftMenhir", new Vector3(-3.1f, 0f, 0f), Quaternion.Euler(0f, -8f, 0f), 2.9f);
        PlaceAssetLocal(_menhirPrefab, root, "Boundary_RightMenhir", new Vector3(3.1f, 0f, 0f), Quaternion.Euler(0f, 8f, 0f), 2.9f);
        PlaceAssetLocal(_orePrefab, root, "Boundary_CoreOre", new Vector3(0f, 0.12f, 0f), Quaternion.Euler(0f, 32f, 0f), 2.25f);
        CreateTree(root, new Vector3(-7.2f, 0f, 4.0f), 1.35f, 14f);
        CreateTree(root, new Vector3(7.5f, 0f, 3.5f), 1.2f, -22f);
        CreateShrub(root, new Vector3(0f, 0f, -6.0f), 1.65f);
    }

    void CreatePatch(string name, Vector3 position, Vector3 scale, float yaw)
    {
        GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        patch.name = name;
        patch.transform.SetParent(_root, true);
        patch.transform.position = position;
        patch.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        patch.transform.localScale = scale;
        RemoveCollider(patch);
        SetMaterial(patch, _meadowMat);
    }

    void CreatePath(Vector3 from, Vector3 to, float width)
    {
        Vector3 delta = to - from;
        float length = new Vector2(delta.x, delta.z).magnitude;
        float yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;

        GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "Asset_Textured_Path";
        path.transform.SetParent(_root, false);
        path.transform.position = (from + to) * 0.5f + Vector3.up * 0.016f;
        path.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        path.transform.localScale = new Vector3(width, 0.035f, length);
        RemoveCollider(path);
        SetMaterial(path, _pathMat);
    }

    void PlaceTrailStones(Vector3 from, Vector3 to, int count)
    {
        Vector3 dir = (to - from).normalized;
        Vector3 lateral = new Vector3(-dir.z, 0f, dir.x);
        for (int i = 1; i < count; i++)
        {
            float t = i / (float)count;
            float side = i % 2 == 0 ? -1f : 1f;
            Vector3 pos = Vector3.Lerp(from, to, t) + lateral * side * (6.6f + (i % 3) * 1.5f);
            CreateRock(_root, pos, 0.85f + (i % 4) * 0.16f, i * 31f);
        }
    }

    void CreateCluster(Vector3 center, float radius, int count, int seed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (seed + i * 137.5f) * Mathf.Deg2Rad;
            float distance = radius * (0.16f + 0.84f * ((i % 9) / 8f));
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

            switch (i % 6)
            {
                case 0:
                    CreateTree(_root, pos, 1.2f + (i % 4) * 0.18f, i * 19f);
                    break;
                case 1:
                    CreateShrub(_root, pos, 1.0f + (i % 3) * 0.16f);
                    break;
                case 2:
                    CreateFlower(_root, pos, 1.0f + (i % 3) * 0.12f);
                    break;
                case 3:
                    CreateGrass(_root, pos, 1.0f + (i % 3) * 0.12f);
                    break;
                case 4:
                    CreateMushroom(_root, pos, 0.9f + (i % 3) * 0.12f);
                    break;
                default:
                    CreateRock(_root, pos, 0.75f + (i % 3) * 0.15f, i * 41f);
                    break;
            }
        }
    }

    void CreateTree(Transform parent, Vector3 localPosition, float scale, float yaw)
    {
        GameObject prefab = Mathf.Abs(Mathf.Sin(yaw)) > 0.5f && _fruitTreePrefab != null ? _fruitTreePrefab : _pinePrefab;
        if (PlaceAssetLocal(prefab, parent, "Asset_Tree", localPosition, Quaternion.Euler(0f, yaw, 0f), scale) != null)
        {
            return;
        }

        Transform root = CreateEmpty(parent, "Fallback_Tree", localPosition, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale);
        CreateLocalPart(root, "Trunk", PrimitiveType.Cylinder, new Vector3(0f, 0.85f, 0f), new Vector3(0.22f, 0.85f, 0.22f), _stoneMat);
        CreateLocalPart(root, "Canopy", PrimitiveType.Sphere, new Vector3(0f, 1.85f, 0f), new Vector3(1.05f, 0.68f, 1.05f), _meadowMat);
    }

    void CreateShrub(Transform parent, Vector3 localPosition, float scale)
    {
        if (PlaceAssetLocal(_shrubPrefab, parent, "Asset_Shrub", localPosition, Quaternion.identity, scale) != null)
        {
            return;
        }

        CreateLocalPart(CreateEmpty(parent, "Fallback_Shrub", localPosition, Quaternion.identity, Vector3.one * scale), "Shrub", PrimitiveType.Sphere, Vector3.up * 0.35f, new Vector3(0.9f, 0.5f, 0.8f), _meadowMat);
    }

    void CreateFlower(Transform parent, Vector3 localPosition, float scale)
    {
        if (PlaceAssetLocal(_flowerPrefab, parent, "Asset_Flowers", localPosition, Quaternion.identity, scale) != null)
        {
            return;
        }

        CreateLocalPart(CreateEmpty(parent, "Fallback_Flower", localPosition, Quaternion.identity, Vector3.one * scale), "Bloom", PrimitiveType.Sphere, Vector3.up * 0.35f, Vector3.one * 0.2f, _goldMat);
    }

    void CreateGrass(Transform parent, Vector3 localPosition, float scale)
    {
        PlaceAssetLocal(_grassPrefab, parent, "Asset_Grass", localPosition, Quaternion.identity, scale);
    }

    void CreateMushroom(Transform parent, Vector3 localPosition, float scale)
    {
        PlaceAssetLocal(_mushroomPrefab, parent, "Asset_Mushroom", localPosition, Quaternion.identity, scale);
    }

    void CreateRock(Transform parent, Vector3 localPosition, float scale, float yaw)
    {
        GameObject prefab = Mathf.Abs(Mathf.Sin(yaw)) > 0.5f && _riverRockPrefab != null ? _riverRockPrefab : _rockPrefab;
        if (PlaceAssetLocal(prefab, parent, "Asset_Rock", localPosition, Quaternion.Euler(0f, yaw, 0f), scale) != null)
        {
            return;
        }

        CreateLocalPart(CreateEmpty(parent, "Fallback_Rock", localPosition, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale), "Rock", PrimitiveType.Cube, Vector3.up * 0.25f, new Vector3(0.9f, 0.5f, 0.7f), _stoneMat, Quaternion.Euler(0f, 25f, 7f));
    }

    Transform PrepareSiteRoot(Transform parent)
    {
        Transform existing = parent.Find(SiteRootName);
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        GameObject root = new GameObject(SiteRootName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root.transform;
    }

    static void HideOldLandmark(Transform parent)
    {
        Transform oldVisual = parent.Find("LandmarkVisual_Procedural");
        if (oldVisual != null)
        {
            oldVisual.gameObject.SetActive(false);
        }
    }

    Transform CreateEmpty(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = localRotation;
        obj.transform.localScale = localScale;
        return obj.transform;
    }

    GameObject CreateLocalPart(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Material material, Quaternion? localRotation = null)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation ?? Quaternion.identity;
        part.transform.localScale = localScale;
        RemoveCollider(part);
        SetMaterial(part, material);
        return part;
    }

    GameObject PlaceAsset(GameObject prefab, Transform parent, string name, Vector3 position, Quaternion rotation, float scale)
    {
        GameObject instance = InstantiateAsset(prefab, name);
        if (instance == null)
        {
            return null;
        }

        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale *= scale;
        RemoveColliders(instance);
        return instance;
    }

    GameObject PlaceAssetLocal(GameObject prefab, Transform parent, string name, Vector3 localPosition, Quaternion localRotation, float scale)
    {
        GameObject instance = InstantiateAsset(prefab, name);
        if (instance == null)
        {
            return null;
        }

        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = localRotation;
        instance.transform.localScale *= scale;
        RemoveColliders(instance);
        return instance;
    }

    static GameObject InstantiateAsset(GameObject prefab, string name)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject instance = Instantiate(prefab);
        instance.name = name;
        return instance;
    }

    static void RemoveCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    static void RemoveColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }
    }

    static void SetMaterial(GameObject obj, Material material)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    static Material CreateMaterial(string name, Color color, Color emission, float smoothness, Texture2D texture = null, Vector2? textureScale = null)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader)
        {
            name = name,
            color = color
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (texture != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", textureScale ?? Vector2.one);
            }
            else if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
                material.SetTextureScale("_MainTex", textureScale ?? Vector2.one);
            }
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        if (emission.maxColorComponent > 0.001f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }

        return material;
    }

    Material CreateSkybox()
    {
        Shader panoramic = Shader.Find("Skybox/Panoramic");
        if (panoramic != null && _skyTexture != null)
        {
            Material sky = new Material(panoramic);
            sky.SetTexture("_MainTex", _skyTexture);
            sky.SetColor("_Tint", new Color(0.86f, 0.95f, 1f));
            sky.SetFloat("_Exposure", 1.28f);
            sky.SetFloat("_Rotation", 0f);
            return sky;
        }

        Shader procedural = Shader.Find("Skybox/Procedural");
        if (procedural == null)
        {
            return null;
        }

        Material material = new Material(procedural);
        material.SetColor("_SkyTint", new Color(0.48f, 0.72f, 1f));
        material.SetColor("_GroundColor", new Color(0.42f, 0.58f, 0.36f));
        material.SetFloat("_AtmosphereThickness", 0.72f);
        material.SetFloat("_Exposure", 1.32f);
        return material;
    }
}
