using UnityEngine;

public class HexTile : MonoBehaviour
{
    [Header("Tile Durumu")]
    public bool hasSoil = false;        // Toprak var mý? (Bina yapýlabilir mi)
    public bool hasBuilding = false;    // Üzerinde bina var mý?

    [Header("Referanslar")]
    

  
    public SpriteRenderer spriteRenderer;
    public Sprite soilSprite;     s      // Topraklý görünüm (açýk/yapýlabilir)

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

        if (value)
        {
            // Toprak kazanýldýðýnda animasyon/efekt
            PlayUnlockEffect();
        }
    }

    public void PlaceBuilding(BuildingData building, GameObject instance)
    {
        currentBuilding = building;
        buildingInstance = instance;
        hasBuilding = true;
        UpdateVisual();
    }

    public void RemoveBuilding()
    {
        if (buildingInstance != null)
        {
            Destroy(buildingInstance);
        }
        currentBuilding = null;
        buildingInstance = null;
        hasBuilding = false;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        if (!hasSoil)
        {
            // Topraksýz - koyu/kilitli görünüm
            spriteRenderer.sprite = emptySprite;
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

    void PlayUnlockEffect()
    {
        // Basit scale animasyonu
        StartCoroutine(UnlockAnimation());
    }

    System.Collections.IEnumerator UnlockAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease out back
            t = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);
            transform.localScale = originalScale * t;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// Bu tile'a bina yapýlabilir mi?
    /// </summary>
    public bool CanBuild()
    {
        return hasSoil && !hasBuilding;
    }
}