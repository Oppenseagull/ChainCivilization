using UnityEngine;

/// <summary>
/// Lightweight procedural NPC skins used when no character asset pack is present.
/// </summary>
public static class NpcVisualFactory
{
    const string RootName = "NPCVisual_Procedural";

    public static void ApplyHighPriest(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Transform root = PrepareRoot(target);
        HideOriginalRenderer(target);

        Color robe = new Color(0.12f, 0.16f, 0.28f);
        Color robeAccent = new Color(0.35f, 0.72f, 1f);
        Color gold = new Color(1f, 0.78f, 0.26f);
        Color skin = new Color(0.82f, 0.66f, 0.5f);
        Color shadow = new Color(0.06f, 0.08f, 0.14f);

        CreatePart(PrimitiveType.Cylinder, "Robe", root, new Vector3(0f, 0.95f, 0f), new Vector3(0.72f, 0.95f, 0.72f), robe, 0.03f);
        CreatePart(PrimitiveType.Cylinder, "Mantle", root, new Vector3(0f, 1.72f, -0.03f), new Vector3(0.82f, 0.22f, 0.82f), Color.Lerp(robe, robeAccent, 0.22f), 0.08f);
        CreatePart(PrimitiveType.Sphere, "Head", root, new Vector3(0f, 2.15f, 0f), new Vector3(0.42f, 0.46f, 0.42f), skin, 0f);
        CreatePart(PrimitiveType.Cylinder, "Hood", root, new Vector3(0f, 2.2f, -0.04f), new Vector3(0.54f, 0.34f, 0.54f), shadow, 0.02f);
        CreatePart(PrimitiveType.Cube, "Chest_Sigil", root, new Vector3(0f, 1.55f, -0.38f), new Vector3(0.18f, 0.44f, 0.04f), robeAccent, 0.65f);

        CreatePart(PrimitiveType.Cylinder, "Staff", root, new Vector3(0.72f, 1.28f, -0.08f), new Vector3(0.05f, 1.35f, 0.05f), gold * 0.72f, 0.12f, Quaternion.Euler(0f, 0f, -4f));
        CreatePart(PrimitiveType.Sphere, "Staff_Core", root, new Vector3(0.82f, 2.68f, -0.08f), new Vector3(0.22f, 0.22f, 0.22f), robeAccent, 1.4f);
        CreatePart(PrimitiveType.Cylinder, "Halo", root, new Vector3(0f, 2.62f, -0.03f), new Vector3(0.72f, 0.03f, 0.72f), gold, 0.7f, Quaternion.Euler(90f, 0f, 0f));

        AddAura(root, robeAccent);
    }

    static bool TryApplyImportedCharacter(Transform root)
    {
        GameObject prefab = Resources.Load<GameObject>("SceneBeauty/Prefabs/Characters/Modular Fantasy Character");
        if (prefab == null)
        {
            return false;
        }

        GameObject character = Object.Instantiate(prefab);
        character.name = "Priest_ZAI_Character";
        character.transform.SetParent(root, false);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        character.transform.localScale = Vector3.one * 1.18f;
        RemoveColliders(character);
        FixProblemMaterials(character);
        return true;
    }

    static void AddAura(Transform root, Color color)
    {
        Light aura = new GameObject("Priest_Aura").AddComponent<Light>();
        aura.transform.SetParent(root, false);
        aura.transform.localPosition = new Vector3(0f, 1.9f, -0.2f);
        aura.type = LightType.Point;
        aura.color = color;
        aura.intensity = 1.8f;
        aura.range = 5f;
        aura.shadows = LightShadows.None;
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

    static void HideOriginalRenderer(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    static void RemoveColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.Destroy(colliders[i]);
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
                if (materials[j] == null || materials[j].shader == null || materials[j].shader.name == "Hidden/InternalErrorShader")
                {
                    materials[j] = CreateMaterial(new Color(0.16f, 0.18f, 0.28f), Color.black);
                    changed = true;
                }
            }

            if (changed)
            {
                renderers[i].sharedMaterials = materials;
            }
        }
    }

    static void CreatePart(
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
            renderer.sharedMaterial = CreateMaterial(color, color * emissionStrength);
        }
    }

    static Material CreateMaterial(Color color, Color emission)
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

        if (emission != Color.black && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }

        return material;
    }
}
