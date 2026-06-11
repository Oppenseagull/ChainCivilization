using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedurally scatters visual-only civilization remnants across the wasteland.
/// No colliders, no gameplay hooks.
/// </summary>
public class EnvironmentPropSpawner : MonoBehaviour
{
    enum PropKind
    {
        CivilizationRuin,
        BlockchainNode,
        StonePillar,
        AncientRuin,
        AirdropStation,
        EnergyTower,
        RandomRock
    }

    struct ExclusionZone
    {
        public Vector2 Center;
        public float Radius;
    }

    struct PathCorridor
    {
        public Vector2 A;
        public Vector2 B;
        public float HalfWidth;
    }

    [SerializeField] int randomSeed = 42857;
    [SerializeField] bool spawnOnStart = true;
    [SerializeField] float minSpacing = 4.5f;

    [Header("Primitive Composition Counts")]
    [SerializeField] int civilizationRuinCount = 12;
    [SerializeField] int blockchainNodeCount = 10;
    [SerializeField] int stonePillarCount = 16;
    [SerializeField] int ancientRuinCount = 12;
    [SerializeField] int airdropStationCount = 8;
    [SerializeField] int energyTowerCount = 10;
    [SerializeField] int randomRockCount = 12;

    [Header("World Bounds (XZ)")]
    [SerializeField] Vector2 worldMin = new Vector2(-210f, -420f);
    [SerializeField] Vector2 worldMax = new Vector2(430f, 210f);

    Transform _propsRoot;
    readonly List<Vector2> _placedPositions = new List<Vector2>();
    System.Random _rng;

    static readonly ExclusionZone[] Exclusions =
    {
        new ExclusionZone { Center = new Vector2(0f, 0f), Radius = 28f },
        new ExclusionZone { Center = new Vector2(-200f, -200f), Radius = 48f },
        new ExclusionZone { Center = new Vector2(200f, 200f), Radius = 44f },
        new ExclusionZone { Center = new Vector2(280f, -280f), Radius = 48f },
        new ExclusionZone { Center = new Vector2(420f, -420f), Radius = 54f },
        new ExclusionZone { Center = new Vector2(400f, -400f), Radius = 18f },
        new ExclusionZone { Center = new Vector2(200f, 25f), Radius = 28f },
        new ExclusionZone { Center = new Vector2(-40f, 40f), Radius = 12f },
        new ExclusionZone { Center = new Vector2(40f, -40f), Radius = 12f },
        new ExclusionZone { Center = new Vector2(40f, 40f), Radius = 12f }
    };

    static readonly PathCorridor[] PathCorridors =
    {
        new PathCorridor { A = new Vector2(0f, 0f), B = new Vector2(-200f, -200f), HalfWidth = 11f },
        new PathCorridor { A = new Vector2(-200f, -200f), B = new Vector2(200f, 200f), HalfWidth = 11f },
        new PathCorridor { A = new Vector2(200f, 200f), B = new Vector2(280f, -280f), HalfWidth = 11f },
        new PathCorridor { A = new Vector2(280f, -280f), B = new Vector2(420f, -420f), HalfWidth = 11f },
        new PathCorridor { A = new Vector2(420f, -420f), B = new Vector2(400f, -400f), HalfWidth = 9f },
        new PathCorridor { A = new Vector2(200f, 200f), B = new Vector2(200f, 25f), HalfWidth = 10f }
    };

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnvironment();
        }
    }

    [ContextMenu("Spawn Environment Props")]
    public void SpawnEnvironment()
    {
        ClearProps();
        _rng = new System.Random(randomSeed);
        _placedPositions.Clear();

        _propsRoot = new GameObject("EnvironmentProps").transform;
        _propsRoot.SetParent(transform, false);

        SpawnKind(PropKind.CivilizationRuin, civilizationRuinCount);
        SpawnKind(PropKind.BlockchainNode, blockchainNodeCount);
        SpawnKind(PropKind.StonePillar, stonePillarCount);
        SpawnKind(PropKind.AncientRuin, ancientRuinCount);
        SpawnKind(PropKind.AirdropStation, airdropStationCount);
        SpawnKind(PropKind.EnergyTower, energyTowerCount);
        SpawnKind(PropKind.RandomRock, randomRockCount);
    }

    [ContextMenu("Clear Environment Props")]
    public void ClearProps()
    {
        if (_propsRoot != null)
        {
            Destroy(_propsRoot.gameObject);
            _propsRoot = null;
        }

        _placedPositions.Clear();
    }

    void SpawnKind(PropKind kind, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!TryPickPosition(out Vector2 xz))
            {
                continue;
            }

            Vector3 world = new Vector3(xz.x, 0f, xz.y);
            float yaw = NextFloat(0f, 360f);
            GameObject prop = BuildProp(kind, world, yaw, i);
            if (prop == null)
            {
                continue;
            }

            VisualHierarchy.ApplyStatic(prop);

            prop.transform.SetParent(_propsRoot, true);
            _placedPositions.Add(xz);
        }
    }

    bool TryPickPosition(out Vector2 xz)
    {
        const int maxAttempts = 80;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = NextFloat(worldMin.x, worldMax.x);
            float z = NextFloat(worldMin.y, worldMax.y);
            xz = new Vector2(x, z);

            if (!IsValidPosition(xz))
            {
                continue;
            }

            return true;
        }

        xz = Vector2.zero;
        return false;
    }

    bool IsValidPosition(Vector2 xz)
    {
        for (int i = 0; i < Exclusions.Length; i++)
        {
            if (Vector2.Distance(xz, Exclusions[i].Center) < Exclusions[i].Radius)
            {
                return false;
            }
        }

        for (int i = 0; i < PathCorridors.Length; i++)
        {
            if (DistanceToSegment(xz, PathCorridors[i].A, PathCorridors[i].B) < PathCorridors[i].HalfWidth)
            {
                return false;
            }
        }

        for (int i = 0; i < _placedPositions.Count; i++)
        {
            if (Vector2.Distance(xz, _placedPositions[i]) < minSpacing)
            {
                return false;
            }
        }

        return true;
    }

    static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float lengthSq = ab.sqrMagnitude;
        if (lengthSq < 0.001f)
        {
            return Vector2.Distance(p, a);
        }

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
        Vector2 projection = a + ab * t;
        return Vector2.Distance(p, projection);
    }

    GameObject BuildProp(PropKind kind, Vector3 world, float yaw, int index)
    {
        switch (kind)
        {
            case PropKind.CivilizationRuin:
                return BuildCivilizationRuin(world, yaw, index);
            case PropKind.BlockchainNode:
                return BuildBlockchainNode(world, yaw, index);
            case PropKind.StonePillar:
                return BuildStonePillar(world, yaw, index);
            case PropKind.AncientRuin:
                return BuildAncientRuin(world, yaw, index);
            case PropKind.AirdropStation:
                return BuildAirdropStation(world, yaw, index);
            case PropKind.EnergyTower:
                return BuildEnergyTower(world, yaw, index);
            case PropKind.RandomRock:
                return BuildRandomRock(world, yaw, index);
            default:
                return null;
        }
    }

    GameObject BuildCivilizationRuin(Vector3 world, float yaw, int index)
    {
        GameObject root = CreateRoot($"Env_CivRuin_{index:00}", world, yaw);
        Color palette = PickDaoPalette(world);
        Color stone = new Color(0.72f, 0.74f, 0.76f);

        GameObject platform = CreatePrimitive(PrimitiveType.Cube, "RuinPlatform", root.transform);
        platform.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        platform.transform.localScale = new Vector3(NextFloat(3.2f, 4.6f), 0.24f, NextFloat(2.8f, 4.2f));
        ApplyColorMaterial(platform, stone * 0.9f, 0.1f);

        int wallCount = NextInt(3, 6);
        for (int i = 0; i < wallCount; i++)
        {
            float angle = i * (360f / wallCount) + NextFloat(-12f, 12f);
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(NextFloat(1.2f, 1.8f), 0f, NextFloat(1.2f, 1.8f));
            GameObject wall = CreatePrimitive(PrimitiveType.Cube, $"Wall_{i}", root.transform);
            wall.transform.localPosition = offset + Vector3.up * NextFloat(0.5f, 1.4f);
            wall.transform.localRotation = Quaternion.Euler(NextFloat(-6f, 6f), angle + NextFloat(-10f, 10f), NextFloat(-14f, 14f));
            wall.transform.localScale = new Vector3(NextFloat(0.9f, 1.8f), NextFloat(0.8f, 2.2f), NextFloat(0.25f, 0.45f));
            ApplyColorMaterial(wall, Color.Lerp(stone, palette, 0.25f), 0.11f);
        }

        int columnCount = NextInt(2, 4);
        for (int i = 0; i < columnCount; i++)
        {
            GameObject column = CreatePrimitive(PrimitiveType.Cylinder, $"Column_{i}", root.transform);
            column.transform.localPosition = new Vector3(NextFloat(-1.4f, 1.4f), NextFloat(0.25f, 0.55f), NextFloat(-1.4f, 1.4f));
            column.transform.localRotation = Quaternion.Euler(NextFloat(-4f, 4f), NextFloat(0f, 360f), NextFloat(75f, 95f));
            column.transform.localScale = new Vector3(NextFloat(0.35f, 0.55f), NextFloat(1.2f, 2f), NextFloat(0.35f, 0.55f));
            ApplyStoneMaterial(column, NextFloat(0.85f, 1f));
        }

        int rubbleCount = NextInt(4, 7);
        for (int i = 0; i < rubbleCount; i++)
        {
            PrimitiveType rubbleType = NextFloat() > 0.5f ? PrimitiveType.Sphere : PrimitiveType.Cube;
            GameObject rubble = CreatePrimitive(rubbleType, $"Rubble_{i}", root.transform);
            rubble.transform.localPosition = new Vector3(NextFloat(-2f, 2f), NextFloat(0.08f, 0.35f), NextFloat(-2f, 2f));
            rubble.transform.localRotation = Quaternion.Euler(NextFloat(0f, 360f), NextFloat(0f, 360f), NextFloat(0f, 360f));
            rubble.transform.localScale = Vector3.one * NextFloat(0.15f, 0.45f);
            ApplyStoneMaterial(rubble, NextFloat(0.8f, 1f));
        }

        GameObject beam = CreatePrimitive(PrimitiveType.Capsule, "FallenBeam", root.transform);
        beam.transform.localPosition = new Vector3(NextFloat(-0.8f, 0.8f), 0.2f, NextFloat(-0.8f, 0.8f));
        beam.transform.localRotation = Quaternion.Euler(0f, NextFloat(0f, 360f), 90f);
        beam.transform.localScale = new Vector3(0.28f, NextFloat(1.4f, 2.2f), 0.28f);
        ApplyColorMaterial(beam, palette * 0.75f, 0.1f);

        SnapToGround(root.transform);
        return root;
    }

    GameObject BuildBlockchainNode(Vector3 world, float yaw, int index)
    {
        GameObject root = CreateRoot($"Env_BlockNode_{index:00}", world, yaw);
        Color node = new Color(0.62f, 0.76f, 0.92f);
        Color block = new Color(0.74f, 0.82f, 0.94f);

        GameObject pedestal = CreatePrimitive(PrimitiveType.Cylinder, "Pedestal", root.transform);
        pedestal.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        pedestal.transform.localScale = new Vector3(1.4f, 0.18f, 1.4f);
        ApplyStoneMaterial(pedestal, 0.92f);

        GameObject hashPlate = CreatePrimitive(PrimitiveType.Cube, "HashPlate", root.transform);
        hashPlate.transform.localPosition = new Vector3(0f, 0.08f, 0.55f);
        hashPlate.transform.localScale = new Vector3(0.9f, 0.06f, 0.35f);
        ApplyColorMaterial(hashPlate, node * 0.85f, 0.12f);

        GameObject core = CreatePrimitive(PrimitiveType.Sphere, "NodeCore", root.transform);
        core.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        core.transform.localScale = Vector3.one * 0.55f;
        ApplyColorMaterial(core, node, 0.16f);

        GameObject ring = CreatePrimitive(PrimitiveType.Cylinder, "OrbitRing", root.transform);
        ring.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.transform.localScale = new Vector3(1.6f, 0.04f, 1.6f);
        ApplyColorMaterial(ring, node * 0.9f, 0.14f);

        int blockCount = NextInt(5, 8);
        for (int i = 0; i < blockCount; i++)
        {
            float angle = i * (360f / blockCount);
            Vector3 orbit = Quaternion.Euler(0f, angle, 0f) * new Vector3(0.85f, 0f, 0f);
            GameObject chainBlock = CreatePrimitive(PrimitiveType.Cube, $"Block_{i}", root.transform);
            chainBlock.transform.localPosition = new Vector3(0f, 1.1f, 0f) + orbit;
            chainBlock.transform.localRotation = Quaternion.Euler(NextFloat(-8f, 8f), angle, NextFloat(-8f, 8f));
            chainBlock.transform.localScale = Vector3.one * NextFloat(0.18f, 0.28f);
            ApplyColorMaterial(chainBlock, block, 0.14f);

            GameObject link = CreatePrimitive(PrimitiveType.Cylinder, $"Link_{i}", root.transform);
            Vector3 mid = (Vector3.up * 1.1f + orbit) * 0.5f;
            link.transform.localPosition = mid;
            link.transform.localScale = new Vector3(0.06f, orbit.magnitude * 0.5f, 0.06f);
            link.transform.rotation = Quaternion.LookRotation(orbit.normalized, Vector3.up);
            ApplyColorMaterial(link, node * 0.7f, 0.1f);
        }

        GameObject antenna = CreatePrimitive(PrimitiveType.Capsule, "Antenna", root.transform);
        antenna.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        antenna.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
        ApplyColorMaterial(antenna, block * 1.05f, 0.12f);

        SnapToGround(root.transform);
        return root;
    }

    GameObject BuildStonePillar(Vector3 world, float yaw, int index)
    {
        GameObject root = CreateRoot($"Env_StonePillar_{index:00}", world, yaw);
        float height = NextFloat(3f, 6.5f);
        float radius = NextFloat(0.28f, 0.45f);
        Color palette = PickDaoPalette(world);

        GameObject basePad = CreatePrimitive(PrimitiveType.Cylinder, "BasePad", root.transform);
        basePad.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        basePad.transform.localScale = new Vector3(radius * 3.2f, 0.1f, radius * 3.2f);
        ApplyStoneMaterial(basePad, 0.88f);

        GameObject shaft = CreatePrimitive(PrimitiveType.Cylinder, "Shaft", root.transform);
        shaft.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
        shaft.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        ApplyStoneMaterial(shaft, NextFloat(0.9f, 1f));

        GameObject band = CreatePrimitive(PrimitiveType.Capsule, "Band", root.transform);
        band.transform.localPosition = new Vector3(0f, height * 0.55f, 0f);
        band.transform.localScale = new Vector3(radius * 2.6f, radius * 0.35f, radius * 2.6f);
        ApplyColorMaterial(band, palette * 0.7f, 0.1f);

        GameObject capital = CreatePrimitive(PrimitiveType.Cube, "Capital", root.transform);
        capital.transform.localPosition = new Vector3(0f, height + 0.2f, 0f);
        capital.transform.localScale = new Vector3(radius * 3f, radius * 1.2f, radius * 3f);
        ApplyStoneMaterial(capital, NextFloat(0.85f, 0.95f));

        if (NextFloat() > 0.4f)
        {
            GameObject orb = CreatePrimitive(PrimitiveType.Sphere, "Orb", root.transform);
            orb.transform.localPosition = new Vector3(0f, height + 0.65f, 0f);
            orb.transform.localScale = Vector3.one * radius * 1.4f;
            ApplyColorMaterial(orb, palette * 0.85f, 0.12f);
        }

        if (NextFloat() > 0.5f)
        {
            GameObject fallen = CreatePrimitive(PrimitiveType.Cube, "FallenCap", root.transform);
            fallen.transform.localPosition = new Vector3(NextFloat(-1f, 1f), 0.25f, NextFloat(-1f, 1f));
            fallen.transform.localRotation = Quaternion.Euler(NextFloat(-20f, 20f), NextFloat(0f, 360f), NextFloat(0f, 35f));
            fallen.transform.localScale = Vector3.one * NextFloat(0.4f, 0.75f);
            ApplyStoneMaterial(fallen, 0.82f);
        }

        SnapToGround(root.transform);
        return root;
    }

    GameObject BuildAncientRuin(Vector3 world, float yaw, int index)
    {
        GameObject root = CreateRoot($"Env_AncientRuin_{index:00}", world, yaw);
        Color palette = PickDaoPalette(world);
        Color stone = new Color(0.7f, 0.72f, 0.76f);
        float gateWidth = NextFloat(2.4f, 3.4f);
        float postHeight = NextFloat(2.8f, 4.2f);

        GameObject floor = CreatePrimitive(PrimitiveType.Cube, "Floor", root.transform);
        floor.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        floor.transform.localScale = new Vector3(gateWidth * 1.8f, 0.2f, gateWidth * 1.4f);
        ApplyColorMaterial(floor, stone, 0.1f);

        GameObject postL = CreatePrimitive(PrimitiveType.Cylinder, "GatePost_L", root.transform);
        postL.transform.localPosition = new Vector3(-gateWidth * 0.5f, postHeight * 0.5f, 0f);
        postL.transform.localScale = new Vector3(0.45f, postHeight * 0.5f, 0.45f);
        ApplyColorMaterial(postL, Color.Lerp(stone, palette, 0.3f), 0.12f);

        GameObject postR = CreatePrimitive(PrimitiveType.Cylinder, "GatePost_R", root.transform);
        postR.transform.localPosition = new Vector3(gateWidth * 0.5f, postHeight * 0.45f, 0f);
        postR.transform.localRotation = Quaternion.Euler(0f, 0f, NextFloat(4f, 10f));
        postR.transform.localScale = new Vector3(0.42f, postHeight * 0.45f, 0.42f);
        ApplyColorMaterial(postR, Color.Lerp(stone, palette, 0.28f), 0.12f);

        GameObject lintel = CreatePrimitive(PrimitiveType.Cube, "Lintel", root.transform);
        lintel.transform.localPosition = new Vector3(0f, postHeight + 0.15f, 0f);
        lintel.transform.localRotation = Quaternion.Euler(0f, 0f, NextFloat(-6f, 6f));
        lintel.transform.localScale = new Vector3(gateWidth * 1.15f, 0.35f, 0.5f);
        ApplyColorMaterial(lintel, palette * 0.8f, 0.12f);

        GameObject altar = CreatePrimitive(PrimitiveType.Sphere, "Altar", root.transform);
        altar.transform.localPosition = new Vector3(0f, 0.45f, -gateWidth * 0.35f);
        altar.transform.localScale = Vector3.one * NextFloat(0.55f, 0.85f);
        ApplyColorMaterial(altar, palette, 0.14f);

        int fragmentCount = NextInt(3, 6);
        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject fragment = CreatePrimitive(PrimitiveType.Cube, $"Fragment_{i}", root.transform);
            fragment.transform.localPosition = new Vector3(NextFloat(-2f, 2f), NextFloat(0.15f, 0.6f), NextFloat(-1.5f, 1.5f));
            fragment.transform.localRotation = Quaternion.Euler(NextFloat(-15f, 15f), NextFloat(0f, 360f), NextFloat(-18f, 18f));
            fragment.transform.localScale = new Vector3(NextFloat(0.3f, 0.9f), NextFloat(0.2f, 0.55f), NextFloat(0.3f, 0.9f));
            ApplyStoneMaterial(fragment, NextFloat(0.82f, 1f));
        }

        GameObject marker = CreatePrimitive(PrimitiveType.Capsule, "SteleMarker", root.transform);
        marker.transform.localPosition = new Vector3(0f, 1.2f, gateWidth * 0.25f);
        marker.transform.localScale = new Vector3(0.35f, 0.9f, 0.35f);
        ApplyColorMaterial(marker, palette * 0.9f, 0.13f);

        SnapToGround(root.transform);
        return root;
    }

    GameObject BuildAirdropStation(Vector3 world, float yaw, int index)
    {
        GameObject root = CreateRoot($"Env_AirdropStation_{index:00}", world, yaw);
        Color pad = new Color(0.68f, 0.7f, 0.74f);
        Color gold = new Color(0.88f, 0.76f, 0.42f);
        Color crate = new Color(0.58f, 0.6f, 0.64f);

        GameObject landingPad = CreatePrimitive(PrimitiveType.Cylinder, "LandingPad", root.transform);
        landingPad.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        landingPad.transform.localScale = new Vector3(3.6f, 0.08f, 3.6f);
        ApplyColorMaterial(landingPad, pad, 0.12f);

        GameObject padMark = CreatePrimitive(PrimitiveType.Cylinder, "PadMark", root.transform);
        padMark.transform.localPosition = new Vector3(0f, 0.14f, 0f);
        padMark.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        padMark.transform.localScale = new Vector3(2.2f, 0.03f, 2.2f);
        ApplyColorMaterial(padMark, gold * 0.85f, 0.1f);

        GameObject beacon = CreatePrimitive(PrimitiveType.Capsule, "DropBeacon", root.transform);
        beacon.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        beacon.transform.localScale = new Vector3(0.45f, 0.75f, 0.45f);
        ApplyColorMaterial(beacon, gold, 0.14f);

        GameObject beaconRing = CreatePrimitive(PrimitiveType.Cylinder, "BeaconRing", root.transform);
        beaconRing.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        beaconRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        beaconRing.transform.localScale = new Vector3(1.1f, 0.05f, 1.1f);
        ApplyColorMaterial(beaconRing, gold * 0.9f, 0.12f);

        GameObject antenna = CreatePrimitive(PrimitiveType.Cylinder, "Antenna", root.transform);
        antenna.transform.localPosition = new Vector3(0f, 2.1f, 0f);
        antenna.transform.localScale = new Vector3(0.08f, 0.55f, 0.08f);
        ApplyColorMaterial(antenna, pad * 1.05f, 0.1f);

        GameObject dish = CreatePrimitive(PrimitiveType.Sphere, "Dish", root.transform);
        dish.transform.localPosition = new Vector3(0f, 2.55f, 0f);
        dish.transform.localScale = new Vector3(0.35f, 0.12f, 0.35f);
        ApplyColorMaterial(dish, gold * 0.95f, 0.12f);

        Vector3[] crateOffsets =
        {
            new Vector3(1.35f, 0.25f, 1.1f),
            new Vector3(-1.25f, 0.25f, 1.15f),
            new Vector3(1.2f, 0.25f, -1.05f),
            new Vector3(-1.15f, 0.25f, -1.1f)
        };

        for (int i = 0; i < crateOffsets.Length; i++)
        {
            GameObject supplyCrate = CreatePrimitive(PrimitiveType.Cube, $"Crate_{i}", root.transform);
            supplyCrate.transform.localPosition = crateOffsets[i];
            supplyCrate.transform.localRotation = Quaternion.Euler(0f, NextFloat(0f, 90f), 0f);
            supplyCrate.transform.localScale = new Vector3(0.55f, 0.5f, 0.55f);
            ApplyColorMaterial(supplyCrate, crate, 0.1f);
        }

        SnapToGround(root.transform);
        return root;
    }

    GameObject BuildEnergyTower(Vector3 world, float yaw, int index)
    {
        GameObject root = CreateRoot($"Env_EnergyTower_{index:00}", world, yaw);
        Color energy = PickEnergyPalette(world);
        Color metal = new Color(0.66f, 0.68f, 0.72f);
        float height = NextFloat(5.5f, 9f);

        GameObject foundation = CreatePrimitive(PrimitiveType.Cube, "Foundation", root.transform);
        foundation.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        foundation.transform.localScale = new Vector3(2.2f, 0.4f, 2.2f);
        ApplyColorMaterial(foundation, metal * 0.9f, 0.1f);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f + 45f;
            Vector3 finOffset = Quaternion.Euler(0f, angle, 0f) * new Vector3(1.1f, 0f, 1.1f);
            GameObject fin = CreatePrimitive(PrimitiveType.Cube, $"Fin_{i}", root.transform);
            fin.transform.localPosition = finOffset + Vector3.up * 0.35f;
            fin.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            fin.transform.localScale = new Vector3(0.25f, 0.7f, 0.55f);
            ApplyColorMaterial(fin, metal, 0.11f);
        }

        GameObject tower = CreatePrimitive(PrimitiveType.Cylinder, "Tower", root.transform);
        tower.transform.localPosition = new Vector3(0f, height * 0.5f + 0.2f, 0f);
        tower.transform.localScale = new Vector3(0.55f, height * 0.5f, 0.55f);
        ApplyColorMaterial(tower, metal, 0.13f);

        int ringCount = NextInt(3, 5);
        for (int i = 0; i < ringCount; i++)
        {
            float y = 1.2f + i * (height / ringCount);
            GameObject coil = CreatePrimitive(PrimitiveType.Capsule, $"Coil_{i}", root.transform);
            coil.transform.localPosition = new Vector3(0f, y, 0f);
            coil.transform.localScale = new Vector3(0.85f, 0.12f, 0.85f);
            ApplyColorMaterial(coil, energy * 0.85f, 0.12f);
        }

        GameObject pipeL = CreatePrimitive(PrimitiveType.Cylinder, "Pipe_L", root.transform);
        pipeL.transform.localPosition = new Vector3(-0.55f, height * 0.35f, 0f);
        pipeL.transform.localScale = new Vector3(0.12f, height * 0.3f, 0.12f);
        ApplyColorMaterial(pipeL, metal * 0.95f, 0.1f);

        GameObject pipeR = CreatePrimitive(PrimitiveType.Cylinder, "Pipe_R", root.transform);
        pipeR.transform.localPosition = new Vector3(0.55f, height * 0.4f, 0f);
        pipeR.transform.localScale = new Vector3(0.12f, height * 0.28f, 0.12f);
        ApplyColorMaterial(pipeR, metal * 0.95f, 0.1f);

        GameObject core = CreatePrimitive(PrimitiveType.Sphere, "EnergyCore", root.transform);
        core.transform.localPosition = new Vector3(0f, height + 0.55f, 0f);
        core.transform.localScale = Vector3.one * 0.65f;
        ApplyColorMaterial(core, energy, 0.16f);

        GameObject crown = CreatePrimitive(PrimitiveType.Capsule, "Crown", root.transform);
        crown.transform.localPosition = new Vector3(0f, height + 1.1f, 0f);
        crown.transform.localScale = new Vector3(0.2f, 0.35f, 0.2f);
        ApplyColorMaterial(crown, energy * 1.05f, 0.14f);

        SnapToGround(root.transform);
        return root;
    }

    GameObject BuildRandomRock(Vector3 world, float yaw, int index)
    {
        GameObject root = CreateRoot($"Env_Rock_{index:00}", world, yaw);
        int chunks = NextInt(1, 3);

        for (int i = 0; i < chunks; i++)
        {
            PrimitiveType type = NextFloat() > 0.35f ? PrimitiveType.Sphere : PrimitiveType.Cube;
            GameObject rock = CreatePrimitive(type, $"Chunk_{i}", root.transform);
            rock.transform.localPosition = new Vector3(NextFloat(-0.25f, 0.25f), NextFloat(0.15f, 0.45f), NextFloat(-0.25f, 0.25f));
            rock.transform.localRotation = Quaternion.Euler(NextFloat(0f, 25f), NextFloat(0f, 360f), NextFloat(0f, 25f));
            float size = NextFloat(0.35f, 1.1f);
            rock.transform.localScale = new Vector3(size, size * NextFloat(0.55f, 0.9f), size);
            ApplyRockMaterial(rock, NextFloat(0.85f, 1f));
        }

        SnapToGround(root.transform);
        return root;
    }

    static GameObject CreateRoot(string name, Vector3 world, float yaw)
    {
        GameObject root = new GameObject(name);
        root.transform.position = world;
        root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        return root;
    }

    static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        RemoveCollider(obj);
        return obj;
    }

    static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }

    static void SnapToGround(Transform target, float heightOffset = 0f)
    {
        GroundSnapUtility.SnapTransform(target, heightOffset);
    }

    Color PickDaoPalette(Vector3 world)
    {
        float blueDist = Vector2.Distance(new Vector2(world.x, world.z), new Vector2(-200f, -200f));
        float redDist = Vector2.Distance(new Vector2(world.x, world.z), new Vector2(200f, 200f));
        float greenDist = Vector2.Distance(new Vector2(world.x, world.z), new Vector2(280f, -280f));

        if (blueDist <= redDist && blueDist <= greenDist)
        {
            return new Color(0.78f, 0.86f, 0.93f);
        }

        if (redDist <= greenDist)
        {
            return new Color(0.9f, 0.6f, 0.4f);
        }

        return new Color(0.48f, 0.72f, 0.46f);
    }

    Color PickEnergyPalette(Vector3 world)
    {
        Color dao = PickDaoPalette(world);
        return Color.Lerp(dao, new Color(0.92f, 0.86f, 0.58f), 0.35f);
    }

    static void ApplyStoneMaterial(GameObject target, float brightness)
    {
        Color color = new Color(0.72f, 0.74f, 0.76f) * brightness;
        ApplyColorMaterial(target, color, 0.12f);
    }

    static void ApplyRockMaterial(GameObject target, float brightness)
    {
        Color color = new Color(0.66f, 0.62f, 0.54f) * brightness;
        ApplyColorMaterial(target, color, 0.08f);
    }

    static void ApplyColorMaterial(GameObject target, Color color, float smoothness)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Material material = CreateLitMaterial(color, Color.black, smoothness);
        renderer.sharedMaterial = material;
    }

    static Material CreateLitMaterial(Color color, Color emission, float smoothness)
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
            material.SetColor("_EmissionColor", emission);
        }

        return material;
    }

    float NextFloat()
    {
        return (float)_rng.NextDouble();
    }

    float NextFloat(float min, float max)
    {
        return min + (max - min) * NextFloat();
    }

    int NextInt(int minInclusive, int maxExclusive)
    {
        return _rng.Next(minInclusive, maxExclusive);
    }
}

/// <summary> Gentle vertical bob for floating props. </summary>
public class EnvironmentFloatMotion : MonoBehaviour
{
    float _baseY;
    float _amplitude;
    float _speed;
    float _phase;

    public void Configure(float amplitude, float speed)
    {
        _amplitude = amplitude;
        _speed = speed;
        _phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Start()
    {
        _baseY = transform.position.y;
    }

    void Update()
    {
        float y = _baseY + Mathf.Sin(Time.time * _speed + _phase) * _amplitude;
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, y, pos.z);
    }
}

/// <summary> Slow yaw rotation for portal rings and chain nodes. </summary>
public class EnvironmentSlowSpin : MonoBehaviour
{
    float _degreesPerSecond;

    public void Configure(float degreesPerSecond)
    {
        _degreesPerSecond = degreesPerSecond;
    }

    void Update()
    {
        transform.Rotate(0f, _degreesPerSecond * Time.deltaTime, 0f, Space.Self);
    }
}
