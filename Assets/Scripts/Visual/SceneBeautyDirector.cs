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
    Material _deepMeadowMat;
    Material _dryMeadowMat;
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
    GameObject _daoHallPrefab;
    GameObject _daoHallAltPrefab;
    GameObject _daoHousePrefab;
    GameObject _daoHouseAltPrefab;
    GameObject _daoTowerPrefab;
    GameObject _stoneGatePrefab;
    GameObject _wallGatePrefab;
    GameObject _wallPrefab;
    GameObject _roadIslandPrefab;
    GameObject _roadIslandAltPrefab;
    GameObject _roadsidePrefab;
    GameObject _mudPatchPrefab;
    GameObject _emaceRockBigPrefab;
    GameObject _emaceRockFlatPrefab;
    GameObject _emaceTreePrefab;
    GameObject _emaceTreeAltPrefab;
    GameObject _emaceBushPrefab;
    GameObject _emaceBushAltPrefab;
    GameObject _emaceGrassPrefab;
    GameObject _emaceGrassAltPrefab;
    GameObject _fieldPrefab;
    GameObject _cratePrefab;
    GameObject _stumpPrefab;
    GameObject _characterPrefab;

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
        DisableLegacyProceduralScenery();
        ApplySkyAndLighting();
        ApplyPlayerVisual();
        StyleGround();
        BuildPaths();
        BuildSafeNaturalEnvironment();
        BuildBoundarySite();
        Debug.Log($"SceneBeautyDirector applied. Asset prefabs loaded: daoHall={_daoHallPrefab != null}, slavicTree={_emaceTreePrefab != null}, rock={_emaceRockBigPrefab != null}, npcReady={Resources.Load<GameObject>("SceneBeauty/Prefabs/Characters/Modular Fantasy Character") != null}.");
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

        _daoHallPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Town_Building_Administrative _01a_PRE");
        _daoHallAltPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Town_Building_Administrative _01c_PRE");
        _daoHousePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Town_House_Comp_01a_PRE");
        _daoHouseAltPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Town_House_Comp_02a_PRE");
        _daoTowerPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Village_Tover_01a_PRE");
        _stoneGatePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Fence_RockGate_01a_PRE");
        _wallGatePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Fence_WallGate_01a_PRE");
        _wallPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Fence_Wall_01b_PRE");
        _roadIslandPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Environment_RoadIsland_08_PRE");
        _roadIslandAltPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Environment_RoadIsland_09_PRE");
        _roadsidePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Environment_RoadSide_PRE");
        _mudPatchPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Env_RoadIslandMud_01a_PRE");
        _emaceRockBigPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Environment_Rock_Big_Head_01a_PRE");
        _emaceRockFlatPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Environment_Rock_Flat_04c_PRE");
        _emaceTreePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Nature_Tree_01b_PRE");
        _emaceTreeAltPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Nature_Tree_03b_PRE");
        _emaceBushPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Nature_Bush_03a_PRE");
        _emaceBushAltPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Nature_Bush_05a_PRE");
        _emaceGrassPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Plant_Grass_01c_PRE");
        _emaceGrassAltPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Plant_Grass_02a_PRE");
        _fieldPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Environment_Field_Comp_01a");
        _cratePrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Prop_Container_Crate_01a_PRE");
        _stumpPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Emace/EA03_Prop_Tool_Stump_02a_PRE");
        _characterPrefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Characters/Modular Fantasy Character");
    }

    void PrepareMaterials()
    {
        _grassMat = CreateMaterial("Civ_Grass_Texture", new Color(0.78f, 0.9f, 0.62f), Color.black, 0.08f, _grassTexture, new Vector2(52f, 52f));
        _meadowMat = CreateMaterial("Civ_Meadow_Texture", new Color(0.48f, 0.72f, 0.42f), Color.black, 0.06f, _grassTexture, new Vector2(22f, 22f));
        _deepMeadowMat = CreateMaterial("Civ_Deep_Meadow_Texture", new Color(0.34f, 0.56f, 0.34f), Color.black, 0.06f, _grassTexture, new Vector2(18f, 18f));
        _dryMeadowMat = CreateMaterial("Civ_Dry_Meadow_Texture", new Color(0.66f, 0.68f, 0.42f), Color.black, 0.05f, _grassTexture, new Vector2(20f, 20f));
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
            SafeDestroy(existing);
        }

        _root = new GameObject(RootName).transform;
    }

    void DisableLegacyProceduralScenery()
    {
        EnvironmentPropSpawner spawner = FindAnyObjectByType<EnvironmentPropSpawner>();
        if (spawner != null)
        {
            spawner.ClearProps();
            spawner.enabled = false;

            Transform oldRoot = spawner.transform.Find("EnvironmentProps");
            if (oldRoot != null)
            {
                SafeDestroy(oldRoot.gameObject);
            }
        }

        DistantLandmarkSystem distant = FindAnyObjectByType<DistantLandmarkSystem>();
        if (distant != null)
        {
            Transform distantRoot = distant.transform.Find("DistantLandmarks");
            if (distantRoot != null)
            {
                SafeDestroy(distantRoot.gameObject);
            }

            distant.enabled = false;
        }

        GameObject looseEnvironmentProps = GameObject.Find("EnvironmentProps");
        if (looseEnvironmentProps != null)
        {
            SafeDestroy(looseEnvironmentProps);
        }

        GameObject looseDistantLandmarks = GameObject.Find("DistantLandmarks");
        if (looseDistantLandmarks != null)
        {
            SafeDestroy(looseDistantLandmarks);
        }

        DestroyLegacyPrototypeObject("BeaconGlow");
        DestroyLegacyPrototypeObject("Beacon_Spire");
        DestroyLegacyPrototypeObject("Pillar_NE");
        DestroyLegacyPrototypeObject("Pillar_SE");
        DestroyLegacyPrototypeObject("Pillar_NW");
        DestroyLegacyPrototypeObject("Pillar_SW");

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == "LandmarkVisual_Procedural")
            {
                transforms[i].gameObject.SetActive(false);
            }
        }
    }

    static void DestroyLegacyPrototypeObject(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            SafeDestroy(obj);
        }
    }

    void ApplyPlayerVisual()
    {
        if (_characterPrefab == null)
        {
            return;
        }

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            return;
        }

        Renderer directRenderer = player.GetComponent<Renderer>();
        if (directRenderer != null)
        {
            directRenderer.enabled = false;
        }

        Transform oldVisual = player.transform.Find("PlayerImportedVisual");
        if (oldVisual != null)
        {
            SafeDestroy(oldVisual.gameObject);
        }

        GameObject visual = InstantiateAsset(_characterPrefab, "PlayerImportedVisual");
        if (visual == null)
        {
            return;
        }

        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = new Vector3(0f, -1f, 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * 1.12f;
        RemoveColliders(visual);
        ApplySafePlayerMaterials(visual);
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
        CreatePatch("DeepMeadow_NorthWest", new Vector3(-168f, -0.031f, 116f), new Vector3(88f, 0.035f, 64f), -24f, _deepMeadowMat);
        CreatePatch("DeepMeadow_RedSouth", new Vector3(118f, -0.03f, 308f), new Vector3(118f, 0.035f, 78f), 14f, _deepMeadowMat);
        CreatePatch("DryMeadow_East", new Vector3(336f, -0.029f, -122f), new Vector3(128f, 0.035f, 74f), 28f, _dryMeadowMat);
        CreatePatch("DryMeadow_SouthWest", new Vector3(-176f, -0.028f, -340f), new Vector3(104f, 0.035f, 66f), -18f, _dryMeadowMat);
    }

    void BuildPaths()
    {
        CreatePath(Spawn, BlueDao, 12f);
        CreatePath(BlueDao, RedDao, 12f);
        CreatePath(RedDao, GreenDao, 12f);
        CreatePath(GreenDao, Boundary, 10f);
        CreatePath(Boundary, CivilizationSeed, 8f);
    }

    void BuildNature()
    {
        CreateCluster(new Vector3(-45f, 0f, 28f), 20f, 22, 100);
        CreateCluster(new Vector3(84f, 0f, -74f), 30f, 36, 200);
        CreateCluster(new Vector3(312f, 0f, -210f), 32f, 42, 300);
        CreateCluster(new Vector3(378f, 0f, -452f), 24f, 28, 400);
        CreateCluster(new Vector3(-238f, 0f, -164f), 30f, 38, 500);
        CreateCluster(new Vector3(172f, 0f, 242f), 30f, 38, 600);
        CreateCluster(new Vector3(-118f, 0f, -260f), 26f, 28, 700);
        CreateCluster(new Vector3(255f, 0f, -68f), 34f, 36, 800);
    }

    void BuildSafeNaturalEnvironment()
    {
        CreateSafeNaturalCluster(new Vector3(-96f, 0f, 88f), 34f, 34, 110);
        CreateSafeNaturalCluster(new Vector3(86f, 0f, -126f), 42f, 42, 210);
        CreateSafeNaturalCluster(new Vector3(-288f, 0f, -88f), 36f, 34, 310);
        CreateSafeNaturalCluster(new Vector3(116f, 0f, 292f), 46f, 44, 410);
        CreateSafeNaturalCluster(new Vector3(362f, 0f, -158f), 42f, 38, 510);
        CreateSafeNaturalCluster(new Vector3(500f, 0f, -316f), 36f, 30, 610);
        CreateSafeNaturalCluster(new Vector3(-142f, 0f, -326f), 32f, 28, 710);
        CreateSafeNaturalCluster(new Vector3(-74f, 0f, -42f), 22f, 22, 820);
        CreateSafeNaturalCluster(new Vector3(-132f, 0f, -98f), 20f, 20, 830);
        CreateSafeNaturalCluster(new Vector3(-24f, 0f, 12f), 22f, 20, 840);
        CreateSafeNaturalCluster(new Vector3(72f, 0f, 104f), 24f, 22, 850);
        CreateSafeNaturalCluster(new Vector3(196f, 0f, 126f), 24f, 20, 860);
        CreateSafeNaturalCluster(new Vector3(320f, 0f, -304f), 20f, 18, 870);

        PlaceSafeField("Asset_Field_QuietMeadow_A", new Vector3(-132f, 0.02f, 132f), Quaternion.Euler(0f, 16f, 0f), 2.2f);
        PlaceSafeField("Asset_Field_QuietMeadow_B", new Vector3(118f, 0.02f, -168f), Quaternion.Euler(0f, -24f, 0f), 2.0f);
        PlaceSafeField("Asset_Field_QuietMeadow_C", new Vector3(330f, 0.02f, -132f), Quaternion.Euler(0f, 34f, 0f), 1.8f);
    }

    void CreateSafeNaturalCluster(Vector3 center, float radius, int count, int seed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (seed + i * 137.5f) * Mathf.Deg2Rad;
            float distance = radius * (0.12f + 0.88f * ((i % 11) / 10f));
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
            if (!IsSafeNaturePosition(pos))
            {
                continue;
            }

            switch ((i + seed) % 7)
            {
                case 0:
                case 1:
                    CreateTree(_root, pos, 1.0f + (i % 4) * 0.16f, seed * 5f + i * 23f);
                    break;
                case 2:
                case 3:
                    CreateShrub(_root, pos, 0.95f + (i % 3) * 0.18f);
                    break;
                case 4:
                    CreateFlower(_root, pos, 0.9f + (i % 3) * 0.12f);
                    break;
                default:
                    CreateGrass(_root, pos, 1.05f + (i % 4) * 0.14f);
                    break;
            }
        }
    }

    void PlaceSafeField(string name, Vector3 position, Quaternion rotation, float scale)
    {
        if (!IsSafeNaturePosition(position))
        {
            return;
        }

        PlaceAsset(_fieldPrefab, _root, name, position, rotation, scale);
    }

    static bool IsSafeNaturePosition(Vector3 position)
    {
        if (DistanceToRoute(position, Spawn, BlueDao) < 22f ||
            DistanceToRoute(position, BlueDao, RedDao) < 24f ||
            DistanceToRoute(position, RedDao, GreenDao) < 24f ||
            DistanceToRoute(position, GreenDao, Boundary) < 22f ||
            DistanceToRoute(position, Boundary, CivilizationSeed) < 18f)
        {
            return false;
        }

        return
            Vector3.Distance(position, BlueDao) > 48f &&
            Vector3.Distance(position, RedDao) > 48f &&
            Vector3.Distance(position, GreenDao) > 48f &&
            Vector3.Distance(position, Boundary) > 42f &&
            Vector3.Distance(position, CivilizationSeed) > 36f;
    }

    static float DistanceToRoute(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector2 p = new Vector2(point.x, point.z);
        Vector2 a = new Vector2(start.x, start.z);
        Vector2 b = new Vector2(end.x, end.z);
        Vector2 segment = b - a;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= Mathf.Epsilon)
        {
            return Vector2.Distance(p, a);
        }

        float t = Mathf.Clamp01(Vector2.Dot(p - a, segment) / lengthSquared);
        return Vector2.Distance(p, a + segment * t);
    }

    void BuildTerrainFeatures()
    {
        PlaceAsset(_fieldPrefab, _root, "Asset_Field_Start", new Vector3(-28f, 0.02f, 48f), Quaternion.Euler(0f, 18f, 0f), 2.8f);
        PlaceAsset(_fieldPrefab, _root, "Asset_Field_RedApproach", new Vector3(146f, 0.02f, 138f), Quaternion.Euler(0f, -28f, 0f), 2.5f);
        PlaceAsset(_fieldPrefab, _root, "Asset_Field_GreenApproach", new Vector3(250f, 0.02f, -226f), Quaternion.Euler(0f, 36f, 0f), 2.3f);
        PlaceAsset(_fieldPrefab, _root, "Asset_Field_Boundary", new Vector3(382f, 0.02f, -368f), Quaternion.Euler(0f, 8f, 0f), 2.0f);

        PlaceAsset(_roadIslandAltPrefab, _root, "Asset_Clearing_BlueRoad", new Vector3(-108f, 0.018f, -118f), Quaternion.Euler(0f, 42f, 0f), 2.1f);
        PlaceAsset(_roadIslandPrefab, _root, "Asset_Clearing_RedRoad", new Vector3(78f, 0.018f, 104f), Quaternion.Euler(0f, -16f, 0f), 2.2f);
        PlaceAsset(_mudPatchPrefab, _root, "Asset_MudPatch_GreenRoad", new Vector3(238f, 0.018f, -90f), Quaternion.Euler(0f, 18f, 0f), 2.4f);
        PlaceAsset(_roadsidePrefab, _root, "Asset_Roadside_Boundary", new Vector3(352f, 0.018f, -346f), Quaternion.Euler(0f, -42f, 0f), 2.0f);

        CreateHill("Hill_StartWest", new Vector3(-74f, 0f, 88f), 24f, 3.2f, 16f);
        CreateHill("Hill_BlueNorth", new Vector3(-238f, 0f, -98f), 30f, 4.6f, -24f);
        CreateHill("Hill_RedSouth", new Vector3(150f, 0f, 282f), 32f, 4.2f, 32f);
        CreateHill("Hill_GreenEast", new Vector3(346f, 0f, -214f), 26f, 3.8f, -8f);
        CreateHill("Hill_BoundaryRidge", new Vector3(466f, 0f, -370f), 38f, 5.4f, 21f);
    }

    void BuildRoadsideDressing()
    {
        PlaceRoadsideDressing(Spawn, BlueDao, 18, 11);
        PlaceRoadsideDressing(BlueDao, RedDao, 24, 23);
        PlaceRoadsideDressing(RedDao, GreenDao, 24, 37);
        PlaceRoadsideDressing(GreenDao, Boundary, 14, 51);
        PlaceRoadsideDressing(Boundary, CivilizationSeed, 6, 67);
    }

    void PlaceRoadsideDressing(Vector3 from, Vector3 to, int count, int seed)
    {
        Vector3 dir = (to - from).normalized;
        Vector3 lateral = new Vector3(-dir.z, 0f, dir.x);

        for (int i = 1; i < count; i++)
        {
            float t = i / (float)count;
            float side = i % 2 == 0 ? -1f : 1f;
            float wave = Mathf.Sin((seed + i) * 1.73f);
            float offset = 20f + Mathf.Abs(wave) * 14f + (i % 4) * 1.8f;
            Vector3 pos = Vector3.Lerp(from, to, t)
                + lateral * side * offset
                + dir * Mathf.Sin((seed + i) * 0.61f) * 4.5f;

            switch ((i + seed) % 9)
            {
                case 0:
                    CreateTree(_root, pos, 0.9f + (i % 3) * 0.16f, seed * 9f + i * 17f);
                    break;
                case 1:
                case 2:
                    CreateShrub(_root, pos, 0.9f + (i % 3) * 0.12f);
                    break;
                case 3:
                case 4:
                    CreateGrass(_root, pos, 1.0f + (i % 4) * 0.18f);
                    break;
                case 5:
                case 6:
                    CreateMushroom(_root, pos, 0.85f + (i % 2) * 0.16f);
                    break;
                default:
                    CreateFlower(_root, pos, 1.0f + (i % 3) * 0.14f);
                    break;
            }
        }
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

        PlaceAssetLocal(_roadIslandPrefab, root, $"{label}_VillagePlaza", new Vector3(0f, 0.015f, 0f), Quaternion.identity, 2.9f);
        PlaceAssetLocal(_mudPatchPrefab, root, $"{label}_PackedEarth", new Vector3(0f, 0.02f, -4.4f), Quaternion.Euler(0f, 8f, 0f), 2.1f);
        PlaceAssetLocal(_daoHallPrefab, root, $"{label}_SanctuaryHall", new Vector3(0f, 0f, 2.7f), Quaternion.Euler(0f, 180f, 0f), 1.05f);
        PlaceAssetLocal(_daoHallAltPrefab, root, $"{label}_CouncilWing", new Vector3(-8.4f, 0f, 1.9f), Quaternion.Euler(0f, 108f, 0f), 0.72f);
        PlaceAssetLocal(_daoHousePrefab, root, $"{label}_ArchivistHouse", new Vector3(8.6f, 0f, 0.8f), Quaternion.Euler(0f, -105f, 0f), 0.82f);
        PlaceAssetLocal(_daoHouseAltPrefab, root, $"{label}_KeeperHouse", new Vector3(5.8f, 0f, 8.5f), Quaternion.Euler(0f, -34f, 0f), 0.72f);
        PlaceAssetLocal(_daoTowerPrefab, root, $"{label}_SignalTower", new Vector3(-6.2f, 0f, 8.7f), Quaternion.Euler(0f, 24f, 0f), 1.15f);

        PlaceAssetLocal(_stoneGatePrefab, root, $"{label}_StoneGate", new Vector3(0f, 0f, -9.2f), Quaternion.identity, 1.75f);
        PlaceAssetLocal(_wallGatePrefab, root, $"{label}_InnerGate", new Vector3(0f, 0f, -5.8f), Quaternion.identity, 1.15f);
        PlaceAssetLocal(_wallPrefab, root, $"{label}_Wall_W", new Vector3(-8.8f, 0f, -4.2f), Quaternion.Euler(0f, 84f, 0f), 1.32f);
        PlaceAssetLocal(_wallPrefab, root, $"{label}_Wall_E", new Vector3(8.8f, 0f, -4.2f), Quaternion.Euler(0f, 96f, 0f), 1.32f);

        PlaceAssetLocal(_menhirPrefab, root, $"{label}_MainMenhir", new Vector3(0f, 0.06f, -1.2f), Quaternion.Euler(0f, 15f, 0f), 2.45f);
        PlaceAssetLocal(_orePrefab, root, $"{label}_EnergyStone", new Vector3(2.2f, 0.12f, -1.1f), Quaternion.Euler(0f, 30f, 0f), 1.35f);
        PlaceAssetLocal(_cratePrefab, root, $"{label}_SupplyCrate", new Vector3(-3.2f, 0f, -3.8f), Quaternion.Euler(0f, 32f, 0f), 1.15f);
        PlaceAssetLocal(_stumpPrefab, root, $"{label}_ToolStump", new Vector3(3.7f, 0f, -3.1f), Quaternion.Euler(0f, -28f, 0f), 1.2f);

        CreateTree(root, new Vector3(-12.2f, 0f, 7.8f), 1.35f, 20f);
        CreateTree(root, new Vector3(12.6f, 0f, 6.8f), 1.18f, -24f);
        CreateTree(root, new Vector3(-10.4f, 0f, -8.5f), 1.08f, 42f);
        CreateShrub(root, new Vector3(-6.2f, 0f, -6.2f), 1.2f);
        CreateShrub(root, new Vector3(6.4f, 0f, -5.8f), 1.1f);
        CreateGrass(root, new Vector3(-3.1f, 0f, -6.9f), 1.25f);
        CreateGrass(root, new Vector3(3.3f, 0f, -6.7f), 1.15f);
        CreateRock(root, new Vector3(1.2f, 0f, 5.8f), 0.95f, 28f);

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

    void CreatePatch(string name, Vector3 position, Vector3 scale, float yaw, Material material = null)
    {
        GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
        patch.name = name;
        patch.transform.SetParent(_root, true);
        patch.transform.position = new Vector3(position.x, 0.004f, position.z);
        patch.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        patch.transform.localScale = new Vector3(scale.x / 10f, 1f, scale.z / 10f);
        RemoveCollider(patch);
        SetMaterial(patch, material != null ? material : _meadowMat);
    }

    void CreatePath(Vector3 from, Vector3 to, float width)
    {
        Vector3 delta = to - from;
        float length = new Vector2(delta.x, delta.z).magnitude;
        float yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;

        GameObject path = GameObject.CreatePrimitive(PrimitiveType.Plane);
        path.name = "Asset_Textured_Path";
        path.transform.SetParent(_root, false);
        path.transform.position = (from + to) * 0.5f + Vector3.up * 0.018f;
        path.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        path.transform.localScale = new Vector3(width / 10f, 1f, length / 10f);
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

    void CreateHill(string name, Vector3 center, float radius, float height, float yaw)
    {
        GameObject hill = new GameObject(name);
        hill.transform.SetParent(_root, false);
        hill.transform.position = center;
        hill.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        MeshFilter filter = hill.AddComponent<MeshFilter>();
        filter.sharedMesh = CreateHillMesh(radius, height, 14);

        MeshRenderer renderer = hill.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = _meadowMat;
    }

    static Mesh CreateHillMesh(float radius, float height, int segments)
    {
        int rings = 3;
        Vector3[] vertices = new Vector3[1 + segments * rings];
        vertices[0] = Vector3.up * height;

        for (int ring = 1; ring <= rings; ring++)
        {
            float ringT = ring / (float)rings;
            float ringRadius = radius * ringT;
            float y = height * Mathf.Pow(1f - ringT, 1.7f);
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.PI * 2f * i / segments;
                float wobble = 0.88f + 0.16f * Mathf.Sin(i * 1.7f + ring * 0.9f);
                vertices[1 + (ring - 1) * segments + i] = new Vector3(
                    Mathf.Cos(angle) * ringRadius * wobble,
                    y,
                    Mathf.Sin(angle) * ringRadius * wobble);
            }
        }

        int[] triangles = new int[segments * 3 + (rings - 1) * segments * 6];
        int t = 0;
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[t++] = 0;
            triangles[t++] = 1 + i;
            triangles[t++] = 1 + next;
        }

        for (int ring = 1; ring < rings; ring++)
        {
            int innerStart = 1 + (ring - 1) * segments;
            int outerStart = 1 + ring * segments;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles[t++] = innerStart + i;
                triangles[t++] = outerStart + i;
                triangles[t++] = innerStart + next;

                triangles[t++] = innerStart + next;
                triangles[t++] = outerStart + i;
                triangles[t++] = outerStart + next;
            }
        }

        Mesh mesh = new Mesh { name = "LowPoly_Hill" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
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
        GameObject prefab = Mathf.Abs(Mathf.Sin(yaw)) > 0.5f && _emaceTreeAltPrefab != null ? _emaceTreeAltPrefab : _emaceTreePrefab;
        if (prefab == null)
        {
            prefab = Mathf.Abs(Mathf.Sin(yaw)) > 0.5f && _fruitTreePrefab != null ? _fruitTreePrefab : _pinePrefab;
        }

        if (PlaceAssetLocal(prefab, parent, "Asset_Tree", localPosition, Quaternion.Euler(0f, yaw, 0f), scale) != null)
        {
            return;
        }
    }

    void CreateShrub(Transform parent, Vector3 localPosition, float scale)
    {
        GameObject prefab = Mathf.Abs(Mathf.Sin(localPosition.x + localPosition.z)) > 0.45f && _emaceBushAltPrefab != null ? _emaceBushAltPrefab : _emaceBushPrefab;
        if (prefab == null)
        {
            prefab = _shrubPrefab;
        }

        if (PlaceAssetLocal(prefab, parent, "Asset_Shrub", localPosition, Quaternion.identity, scale) != null)
        {
            return;
        }
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
        GameObject prefab = Mathf.Abs(Mathf.Sin(localPosition.x)) > 0.5f && _emaceGrassAltPrefab != null ? _emaceGrassAltPrefab : _emaceGrassPrefab;
        PlaceAssetLocal(prefab != null ? prefab : _grassPrefab, parent, "Asset_Grass", localPosition, Quaternion.identity, scale);
    }

    void CreateMushroom(Transform parent, Vector3 localPosition, float scale)
    {
        PlaceAssetLocal(_mushroomPrefab, parent, "Asset_Mushroom", localPosition, Quaternion.identity, scale);
    }

    void CreateRock(Transform parent, Vector3 localPosition, float scale, float yaw)
    {
        GameObject prefab = Mathf.Abs(Mathf.Sin(yaw)) > 0.5f && _emaceRockFlatPrefab != null ? _emaceRockFlatPrefab : _emaceRockBigPrefab;
        if (prefab == null)
        {
            prefab = Mathf.Abs(Mathf.Sin(yaw)) > 0.5f && _riverRockPrefab != null ? _riverRockPrefab : _rockPrefab;
        }

        if (PlaceAssetLocal(prefab, parent, "Asset_Rock", localPosition, Quaternion.Euler(0f, yaw, 0f), scale) != null)
        {
            return;
        }
    }

    Transform PrepareSiteRoot(Transform parent)
    {
        Transform existing = parent.Find(SiteRootName);
        if (existing != null)
        {
            SafeDestroy(existing.gameObject);
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
        FixProblemMaterials(instance);
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
        FixProblemMaterials(instance);
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
            SafeDestroy(collider);
        }
    }

    static void RemoveColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            SafeDestroy(colliders[i]);
        }
    }

    static void SafeDestroy(Object obj)
    {
        if (obj == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            DestroyImmediate(obj);
        }
    }

    static void FixProblemMaterials(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            bool changed = false;
            for (int j = 0; j < materials.Length; j++)
            {
                if (IsProblemMaterial(materials[j]))
                {
                    materials[j] = CreateMaterial(
                        $"Civ_Fallback_{renderers[i].name}",
                        GuessFallbackColor(renderers[i].name + " " + obj.name),
                        Color.black,
                        0.08f);
                    changed = true;
                }
            }

            if (changed)
            {
                renderers[i].sharedMaterials = materials;
            }
        }
    }

    static void ApplySafePlayerMaterials(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            for (int j = 0; j < materials.Length; j++)
            {
                string materialName = materials[j] != null ? materials[j].name : string.Empty;
                string hint = $"{renderers[i].name} {materialName} {obj.name}";
                materials[j] = CreateMaterial(
                    $"Player_Safe_{renderers[i].name}_{j}",
                    GuessPlayerColor(hint),
                    Color.black,
                    0.16f);
            }

            renderers[i].sharedMaterials = materials;
        }
    }

    static Color GuessPlayerColor(string hint)
    {
        string lower = hint.ToLowerInvariant();
        if (lower.Contains("skin") || lower.Contains("face") || lower.Contains("head") || lower.Contains("hand"))
        {
            return new Color(0.82f, 0.62f, 0.46f);
        }

        if (lower.Contains("hair") || lower.Contains("beard") || lower.Contains("brow"))
        {
            return new Color(0.18f, 0.12f, 0.08f);
        }

        if (lower.Contains("boot") || lower.Contains("belt") || lower.Contains("leather"))
        {
            return new Color(0.28f, 0.18f, 0.1f);
        }

        if (lower.Contains("metal") || lower.Contains("armor") || lower.Contains("plate"))
        {
            return new Color(0.48f, 0.52f, 0.52f);
        }

        if (lower.Contains("cloth") || lower.Contains("upper") || lower.Contains("torso") || lower.Contains("body") || lower.Contains("shirt"))
        {
            return new Color(0.18f, 0.34f, 0.42f);
        }

        if (lower.Contains("lower") || lower.Contains("leg") || lower.Contains("pant"))
        {
            return new Color(0.2f, 0.23f, 0.2f);
        }

        return new Color(0.34f, 0.42f, 0.38f);
    }

    static bool IsProblemMaterial(Material material)
    {
        if (material == null || material.shader == null || material.shader.name == "Hidden/InternalErrorShader")
        {
            return true;
        }

        if (material.HasProperty("_BaseColor"))
        {
            Color color = material.GetColor("_BaseColor");
            return color.r > 0.78f && color.b > 0.78f && color.g < 0.28f;
        }

        if (material.HasProperty("_Color"))
        {
            Color color = material.GetColor("_Color");
            return color.r > 0.78f && color.b > 0.78f && color.g < 0.28f;
        }

        return false;
    }

    static Color GuessFallbackColor(string name)
    {
        string lower = name.ToLowerInvariant();
        if (lower.Contains("roof"))
        {
            return new Color(0.42f, 0.17f, 0.12f);
        }

        if (lower.Contains("tree") || lower.Contains("bush") || lower.Contains("grass") || lower.Contains("plant"))
        {
            return new Color(0.34f, 0.56f, 0.28f);
        }

        if (lower.Contains("rock") || lower.Contains("wall") || lower.Contains("gate") || lower.Contains("stone"))
        {
            return new Color(0.55f, 0.54f, 0.48f);
        }

        if (lower.Contains("mud") || lower.Contains("road") || lower.Contains("field") || lower.Contains("ground"))
        {
            return new Color(0.45f, 0.36f, 0.24f);
        }

        if (lower.Contains("crate") || lower.Contains("stump") || lower.Contains("wood") || lower.Contains("house"))
        {
            return new Color(0.48f, 0.32f, 0.2f);
        }

        return new Color(0.62f, 0.58f, 0.5f);
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
