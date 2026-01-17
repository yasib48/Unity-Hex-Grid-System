// BuildSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    public GridBuilder builder;
    public GameObject building, building2, building3;
    public GameObject hologramObj;
    public float alpha = 0.4f;

    public BuildingType currentBuilding;

    void Update()
    {
        HandleInput();
        UpdateHologram();
        HandleBuild();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentBuilding = (BuildingType)(((int)currentBuilding + 1) % 3);

            if (hologramObj != null)
            {
                Destroy(hologramObj);
                hologramObj = null;
            }

            Debug.Log("Þu anki bina: " + currentBuilding);
        }
    }

    void UpdateHologram()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        builder.GetXY(mouseWorldPos, out int x, out int y);

        var (prefab, size) = GetCurrentBuildingData();

        bool canPlace = Input.GetKey(KeyCode.LeftShift) ? CanPlaceBuilding(x, y, size) : CanPlaceSoil(x, y, size);

        if (canPlace)
        {
            CreateHologram(prefab);
            Vector3 snappedPos = builder.GetWorldPos(x, y);
            hologramObj.transform.position = snappedPos;
            SetHologramColor(new Color(0, 1, 0, alpha));
        }
        else if (hologramObj != null)
        {
            SetHologramColor(new Color(1, 0, 0, alpha));
            Vector3 snappedPos = builder.GetWorldPos(x, y);
            hologramObj.transform.position = snappedPos;
        }
    }

    void CreateHologram(GameObject prefab)
    {
        if (hologramObj == null)
        {
            hologramObj = Instantiate(prefab);

            foreach (var comp in hologramObj.GetComponents<Behaviour>())
            {
                comp.enabled = false;
            }

            var sr = hologramObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            foreach (var col in hologramObj.GetComponents<Collider2D>())
            {
                col.enabled = false;
            }
        }
    }

    void SetHologramColor(Color color)
    {
        if (hologramObj != null)
        {
            var sr = hologramObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = color;
            }
        }
    }

    void HandleBuild()
    {
        // Sol týk - Toprak yerleþtir
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            builder.GetXY(mouseWorldPos, out int x, out int y);

            var (prefab, size) = GetCurrentBuildingData();

            if (CanPlaceSoil(x, y, size))
            {
                SetSoil(x, y, size, true);
                Vector3 buildPos = builder.GetWorldPos(x, y);
                Instantiate(prefab, buildPos, Quaternion.identity);
            }
        }

        // Sað týk - Bina inþa et
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            builder.GetXY(mouseWorldPos, out int x, out int y);

            var (prefab, size) = GetCurrentBuildingData();

            if (CanPlaceBuilding(x, y, size))
            {
                SetBuilding(x, y, size, true);
                Vector3 buildPos = builder.GetWorldPos(x, y);
                Instantiate(prefab, buildPos, Quaternion.identity);
            }
        }
    }

    (GameObject prefab, int size) GetCurrentBuildingData()
    {
        return currentBuilding switch
        {
            BuildingType.Small => (building, 1),
            BuildingType.Medium => (building2, 2),
            BuildingType.Large => (building3, 3),
            _ => (building, 1)
        };
    }

    // Toprak yerleþtirilebilir mi? (hasSoil = false olmalý)
    bool CanPlaceSoil(int centerX, int centerY, int size)
    {
        var hexes = GetHexesInRadius(centerX, centerY, size - 1);

        foreach (var hex in hexes)
        {
            var gridCell = builder.GetGrid(hex.x, hex.y);
            if (gridCell == null) return false;
            if (gridCell.hasSoil) return false;
        }
        return true;
    }

    // Bina yerleþtirilebilir mi? (hasSoil = true VE hasBuilding = false olmalý)
    bool CanPlaceBuilding(int centerX, int centerY, int size)
    {
        var hexes = GetHexesInRadius(centerX, centerY, size - 1);
        foreach (var hex in hexes)
        {
            var gridCell = builder.GetGrid(hex.x, hex.y);
            if (gridCell == null) return false;
            if (!gridCell.hasSoil) return false;
            if (gridCell.hasBuilding) return false;
        }
        return true;
    }

    void SetSoil(int centerX, int centerY, int size, bool value)
    {
        var hexes = GetHexesInRadius(centerX, centerY, size - 1);

        foreach (var hex in hexes)
        {
            Grid grid = builder.GetGrid(hex.x, hex.y);
            if (grid != null)
            {
                grid.SetSoil(value);
            }
        }
    }

    void SetBuilding(int centerX, int centerY, int size, bool value)
    {
        var hexes = GetHexesInRadius(centerX, centerY, size - 1);

        foreach (var hex in hexes)
        {
            Grid grid = builder.GetGrid(hex.x, hex.y);
            if (grid != null)
            {
                grid.hasBuilding = value;
            }
        }
    }

    List<Vector2Int> GetHexesInRadius(int centerX, int centerY, int radius)
    {
        List<Vector2Int> results = new List<Vector2Int>();
        results.Add(new Vector2Int(centerX, centerY));

        if (radius <= 0) return results;

        int cz = centerY;
        int cx = centerX - (cz - (cz & 1)) / 2;
        int cy = -cx - cz;

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
            {
                int dz = -dx - dy;

                int nx = cx + dx;
                int ny = cy + dy;
                int nz = cz + dz;

                int col = nx + (nz - (nz & 1)) / 2;
                int row = nz;

                if (col >= 0 && col < builder.Width && row >= 0 && row < builder.Height)
                {
                    var pos = new Vector2Int(col, row);
                    if (!results.Contains(pos))
                        results.Add(pos);
                }
            }
        }

        return results;
    }
}

public enum BuildingType
{
    Small,
    Medium,
    Large
}