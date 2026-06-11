using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns floating airdrop coins across the map.
/// </summary>
public class AirdropSpawner : MonoBehaviour
{
    struct ExclusionZone
    {
        public Vector2 Center;
        public float Radius;
    }

    [SerializeField] int coinCount = 20;
    [SerializeField] int randomSeed = 91357;
    [SerializeField] float minSpacing = 12f;
    [SerializeField] Vector2 worldMin = new Vector2(-210f, -420f);
    [SerializeField] Vector2 worldMax = new Vector2(430f, 210f);

    Transform _root;
    readonly List<Vector2> _placed = new List<Vector2>();
    System.Random _rng;

    static readonly ExclusionZone[] Exclusions =
    {
        new ExclusionZone { Center = new Vector2(0f, 0f), Radius = 22f },
        new ExclusionZone { Center = new Vector2(-200f, -200f), Radius = 46f },
        new ExclusionZone { Center = new Vector2(200f, 200f), Radius = 42f },
        new ExclusionZone { Center = new Vector2(280f, -280f), Radius = 46f },
        new ExclusionZone { Center = new Vector2(420f, -420f), Radius = 52f },
        new ExclusionZone { Center = new Vector2(400f, -400f), Radius = 16f },
        new ExclusionZone { Center = new Vector2(200f, 25f), Radius = 24f }
    };

    void Start()
    {
        SpawnAirdrops();
    }

    void SpawnAirdrops()
    {
        if (_root != null)
        {
            Destroy(_root.gameObject);
        }

        _rng = new System.Random(randomSeed);
        _placed.Clear();
        _root = new GameObject("Airdrops").transform;
        _root.SetParent(transform, false);

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = coinCount * 8;

        while (spawned < coinCount && attempts < maxAttempts)
        {
            attempts++;
            if (!TryPickPosition(out Vector2 xz))
            {
                continue;
            }

            GameObject coinObject = new GameObject($"Airdrop_{spawned:00}");
            coinObject.transform.SetParent(_root, false);
            coinObject.transform.position = new Vector3(xz.x, 0f, xz.y);

            AirdropCoin coin = coinObject.AddComponent<AirdropCoin>();
            coin.Initialize($"airdrop_{spawned:00}");

            _placed.Add(xz);
            spawned++;
        }

        Debug.Log($"AirdropSpawner: spawned {spawned} coins.");
    }

    bool TryPickPosition(out Vector2 xz)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            float x = NextFloat(worldMin.x, worldMax.x);
            float z = NextFloat(worldMin.y, worldMax.y);
            xz = new Vector2(x, z);

            if (!IsValid(xz))
            {
                continue;
            }

            return true;
        }

        xz = Vector2.zero;
        return false;
    }

    bool IsValid(Vector2 xz)
    {
        for (int i = 0; i < Exclusions.Length; i++)
        {
            if (Vector2.Distance(xz, Exclusions[i].Center) < Exclusions[i].Radius)
            {
                return false;
            }
        }

        for (int i = 0; i < _placed.Count; i++)
        {
            if (Vector2.Distance(xz, _placed[i]) < minSpacing)
            {
                return false;
            }
        }

        return true;
    }

    float NextFloat(float min, float max)
    {
        return min + (max - min) * (float)_rng.NextDouble();
    }
}
