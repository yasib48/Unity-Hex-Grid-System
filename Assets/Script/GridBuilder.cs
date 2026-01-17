// GridBuilder.cs
using UnityEngine;
using System.Collections.Generic;
using System;

public class GridBuilder : MonoBehaviour
{
    public enum GridShape { Rectangle, L, T, Cross }

    public int Width = 10;
    public int Height = 10;
    public float cellSize = 1f;
    public Vector2Int startOffset = Vector2Int.zero;

    public GameObject Grid;
    public GridShape shape = GridShape.Rectangle;

    public bool useCustomShape = false;
    public Row[] customShape;

    public bool useCustomSoil = false;
    public Row[] customSoil;

    private GameObject[,] gridObjects;
    public int[,] GridArray;

    private float HexWidth => Mathf.Sqrt(3f) * cellSize;
    private float HexHeight => 2f * cellSize;
    private float HexVerticalSpacing => HexHeight * 0.75f;

    void Awake()
    {
        if (Application.isPlaying)
            Build();
    }

    // GridBuilder.cs - Build() metodundaki deðiþiklik

    public void Build()
    {
        ClearGrid();

        GridArray = new int[Width, Height];
        gridObjects = new GameObject[Width, Height];

        HashSet<Vector2Int> activeCells = GetShapeCells();

        foreach (var pos in activeCells)
        {
            if (pos.x >= Width || pos.y >= Height) continue;

            if (Application.isPlaying)
            {
                Vector3 worldPos = GetWorldPos(pos.x, pos.y);
                GameObject a = Instantiate(Grid, worldPos, Quaternion.identity, transform);
                a.name = $"Hex {pos.x} {pos.y}";
                gridObjects[pos.x, pos.y] = a;

                Grid gridComp = a.GetComponent<Grid>();
                if (gridComp != null)
                {
                    gridComp.gridX = pos.x;
                    gridComp.gridY = pos.y;

                    // Varsayýlan olarak topraksýz
                    bool shouldHaveSoil = false;

                    // Eðer custom soil açýksa ve bu hücre iþaretliyse topraklý yap
                    if (useCustomSoil && customSoil != null &&
                        pos.y < customSoil.Length &&
                        customSoil[pos.y]?.cells != null &&
                        pos.x < customSoil[pos.y].cells.Length)
                    {
                        shouldHaveSoil = customSoil[pos.y].cells[pos.x];
                    }

                    gridComp.SetSoil(shouldHaveSoil);
                }
            }
        }
    }
    void ClearGrid()
    {
        if (gridObjects == null) return;

        foreach (var obj in gridObjects)
        {
            if (obj != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(obj);
#else
                Destroy(obj);
#endif
            }
        }
    }

    public Grid GetGrid(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
        if (gridObjects[x, y] == null) return null;
        return gridObjects[x, y].GetComponent<Grid>();
    }

    public Vector3 GetWorldPos(int x, int y)
    {
        float xPos = x * HexWidth + (y % 2 == 1 ? HexWidth * 0.5f : 0f);
        float yPos = y * HexVerticalSpacing;

        return new Vector3(
            xPos + startOffset.x * HexWidth,
            yPos + startOffset.y * HexVerticalSpacing,
            0
        );
    }

    public void GetXY(Vector3 worldPos, out int x, out int y)
    {
        float px = worldPos.x - startOffset.x * HexWidth;
        float py = worldPos.y - startOffset.y * HexVerticalSpacing;

        float q = (Mathf.Sqrt(3f) / 3f * px - 1f / 3f * py) / cellSize;
        float r = (2f / 3f * py) / cellSize;

        float cubeX = q;
        float cubeZ = r;
        float cubeY = -cubeX - cubeZ;

        int rx = Mathf.RoundToInt(cubeX);
        int ry = Mathf.RoundToInt(cubeY);
        int rz = Mathf.RoundToInt(cubeZ);

        float xDiff = Mathf.Abs(rx - cubeX);
        float yDiff = Mathf.Abs(ry - cubeY);
        float zDiff = Mathf.Abs(rz - cubeZ);

        if (xDiff > yDiff && xDiff > zDiff)
            rx = -ry - rz;
        else if (yDiff > zDiff)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        x = rx + (rz - (rz & 1)) / 2;
        y = rz;
    }

    public List<Vector2Int> GetNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        int[][] oddNeighbors = new int[][]
        {
            new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { -1, -1 },
            new int[] { -1, 0 }, new int[] { -1, 1 }, new int[] { 0, 1 }
        };

        int[][] evenNeighbors = new int[][]
        {
            new int[] { 1, 0 }, new int[] { 1, -1 }, new int[] { 0, -1 },
            new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 1, 1 }
        };

        int[][] directions = (y % 2 == 1) ? oddNeighbors : evenNeighbors;

        foreach (var dir in directions)
        {
            int nx = x + dir[0];
            int ny = y + dir[1];

            if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
            {
                if (gridObjects[nx, ny] != null)
                {
                    neighbors.Add(new Vector2Int(nx, ny));
                }
            }
        }

        return neighbors;
    }

    public void SetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < Width && y < Height)
        {
            if (gridObjects[x, y] != null)
                Debug.Log(gridObjects[x, y].gameObject.name);
        }
    }

    public void SetValue(Vector3 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        SetValue(x, y);
    }

    HashSet<Vector2Int> GetShapeCells()
    {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();

        if (useCustomShape && customShape != null)
        {
            for (int y = 0; y < customShape.Length; y++)
            {
                var row = customShape[y];
                if (row?.cells == null) continue;

                for (int x = 0; x < row.cells.Length; x++)
                {
                    if (row.cells[x])
                    {
                        cells.Add(new Vector2Int(x, y));
                    }
                }
            }
            return cells;
        }

        switch (shape)
        {
            case GridShape.Rectangle:
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        cells.Add(new Vector2Int(x, y));
                break;

            case GridShape.L:
                for (int x = 0; x < Width / 2; x++)
                    cells.Add(new Vector2Int(x, 0));
                for (int y = 0; y < Height; y++)
                    cells.Add(new Vector2Int(0, y));
                break;

            case GridShape.T:
                for (int x = 0; x < Width; x++)
                    cells.Add(new Vector2Int(x, Height - 1));
                for (int y = 0; y < Height; y++)
                    cells.Add(new Vector2Int(Width / 2, y));
                break;

            case GridShape.Cross:
                for (int x = 0; x < Width; x++)
                    cells.Add(new Vector2Int(x, Height / 2));
                for (int y = 0; y < Height; y++)
                    cells.Add(new Vector2Int(Width / 2, y));
                break;
        }

        return cells;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.SceneView.RepaintAll();

            if (customShape == null || customShape.Length != Height)
            {
                customShape = new Row[Height];
                for (int y = 0; y < Height; y++)
                {
                    customShape[y] = new Row { cells = new bool[Width] };
                    for (int x = 0; x < Width; x++)
                    {
                        customShape[y].cells[x] = true;
                    }
                }
            }
            else
            {
                for (int y = 0; y < Height; y++)
                {
                    if (customShape[y].cells == null || customShape[y].cells.Length != Width)
                    {
                        customShape[y].cells = new bool[Width];
                        for (int x = 0; x < Width; x++)
                        {
                            customShape[y].cells[x] = true;
                        }
                    }
                }
            }

            // Custom Soil array'ini de ayarla
            if (customSoil == null || customSoil.Length != Height)
            {
                customSoil = new Row[Height];
                for (int y = 0; y < Height; y++)
                {
                    customSoil[y] = new Row { cells = new bool[Width] };
                }
            }
            else
            {
                for (int y = 0; y < Height; y++)
                {
                    if (customSoil[y].cells == null || customSoil[y].cells.Length != Width)
                    {
                        customSoil[y].cells = new bool[Width];
                    }
                }
            }
        }
    }
#endif

    void OnDrawGizmos()
    {
        HashSet<Vector2Int> cells = GetShapeCells();

        foreach (var pos in cells)
        {
            if (pos.x < Width && pos.y < Height)
            {
                Vector3 center = GetWorldPos(pos.x, pos.y);
                DrawHexagonGizmo(center, cellSize);
            }
        }
    }

    void DrawHexagonGizmo(Vector3 center, float size)
    {
        Gizmos.color = Color.green;

        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = 60f * i - 30f;
            float rad = Mathf.Deg2Rad * angle;
            corners[i] = center + new Vector3(
                size * Mathf.Cos(rad),
                size * Mathf.Sin(rad),
                0
            );
        }

        for (int i = 0; i < 6; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 6]);
        }
    }
}

[Serializable]
public class Row
{
    public bool[] cells;
}