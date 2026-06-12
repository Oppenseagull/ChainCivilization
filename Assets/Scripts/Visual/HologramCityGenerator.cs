using UnityEngine;

/// <summary>
/// Generates a procedural Cyber-Hologram city around a center point.
/// </summary>
public static class HologramCityGenerator
{
    public static void GenerateCity(Transform center, Color mainColor, int buildingCount, float radius)
    {
        GameObject cityRoot = new GameObject("HologramCity");
        cityRoot.transform.SetParent(center, false);
        cityRoot.transform.localPosition = Vector3.zero;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material holoMat = new Material(shader);
        
        holoMat.SetInt("_Surface", 1); 
        holoMat.SetInt("_Blend", 0); 
        holoMat.SetInt("_ZWrite", 0);
        holoMat.renderQueue = 3000;
        holoMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        holoMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        
        Color baseColor = new Color(mainColor.r, mainColor.g, mainColor.b, 0.25f);
        holoMat.color = baseColor;
        holoMat.SetColor("_BaseColor", baseColor);
        
        holoMat.EnableKeyword("_EMISSION");
        holoMat.SetColor("_EmissionColor", mainColor * 2.0f);

        for (int i = 0; i < buildingCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(3f, radius);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
            
            float width = Random.Range(1.5f, 5f);
            float depth = Random.Range(1.5f, 5f);
            float height = Random.Range(10f, 40f);
            
            if (dist < radius * 0.4f) height *= 1.5f;

            GameObject bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = "HologramBuilding_" + i;
            bldg.transform.SetParent(cityRoot.transform, false);
            
            bldg.transform.localPosition = new Vector3(pos.x, height / 2f, pos.z);
            bldg.transform.localScale = new Vector3(width, height, depth);
            
            Object.Destroy(bldg.GetComponent<Collider>());
            bldg.GetComponent<Renderer>().sharedMaterial = holoMat;
        }

        GameObject glow = new GameObject("CityGlow");
        glow.transform.SetParent(cityRoot.transform, false);
        glow.transform.localPosition = new Vector3(0, 5f, 0);
        Light l = glow.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = mainColor;
        l.intensity = 10f;
        l.range = radius * 2f;
        l.shadows = LightShadows.None;
    }
}
