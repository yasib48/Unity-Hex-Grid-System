using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Tile Durumu")]
    public bool hasSoil = false;        // Toprak var mý? (Bina yapýlabilir mi)
    public bool hasBuilding = false;    // Üzerinde bina var mý?

    [Header("Referanslar")]
    


    public SpriteRenderer spriteRenderer;
    public Sprite soilSprite;    // Topraklý görünüm (açýk/yapýlabilir)


    BuildingData currentBuilding;
    [Header("Koordinatlar")]
    public int gridX;
    public int gridY;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateVisual();
    }

    public void SetSoil(bool value)
    {
        if (hasSoil == value) return;

        hasSoil = value;
        UpdateVisual();

    }

    public void PlaceBuilding(BuildingData building, GameObject instance)
    {
        currentBuilding = building;
        hasBuilding = true;
        UpdateVisual();
        Debug.Log($"Bina yerleþtirildi: {building.buildingName} at ({gridX}, {gridY})");
    }

    /*public void RemoveBuilding()
    {
        if (buildingInstance != null)
        {
            Destroy(buildingInstance);
        }
        currentBuilding = null;
        buildingInstance = null;
        hasBuilding = false;
        UpdateVisual();
    }*/

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        if (!hasSoil)
        {
            // Topraksýz - koyu/kilitli görünüm
            spriteRenderer.sprite = null;
            spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
        else if (hasBuilding)
        {
            // Bina var - tile gizlenebilir veya farklý renk
            spriteRenderer.sprite = soilSprite;
            spriteRenderer.color = new Color(0.5f, 0.8f, 0.5f, 0.3f);
        }
        else
        {
            // Topraklý ve boþ - yapýlabilir
            spriteRenderer.sprite = soilSprite;
            spriteRenderer.color = Color.white;
        }
    }



    /// <summary>
    /// Bu tile'a bina yapýlabilir mi?
    /// </summary>
    public bool CanBuild()
    {
        return hasSoil && !hasBuilding;
    }
    public bool CanPlace()
    {
        return !hasSoil;
    }
}

[CreateAssetMenu(fileName = "New Building", menuName = "Hexonia/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string buildingName;
    public Sprite icon;
    public GameObject prefab;

    [Header("Yerleþim")]
    [Tooltip("Kaç hex kaplar (1 = sadece merkez, 2 = merkez + komþular)")]
    [Range(1, 3)]
    public int hexSize = 1;

    [Header("Geniþleme")]
    [Tooltip("Bu bina yapýldýðýnda kaç halka komþu tile topraklý olur")]
    [Range(0, 3)]
    public int expandRadius = 1;

    [Header("Maliyet")]
    public int goldCost = 100;
    public int woodCost = 0;
    public int stoneCost = 0;
}