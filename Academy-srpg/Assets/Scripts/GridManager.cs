using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;

    private readonly Dictionary<Vector2Int, TileData> tiles = new Dictionary<Vector2Int, TileData>();

    public class TileData
    {
        public bool isWalkable;
        public Unit occupiedUnit;
    }

    private void Start()
    {
        InitializeTiles();
    }

    public TileData GetTile(Vector2Int pos)
    {
        EnsureTilesInitialized();

        if (!IsValidPosition(pos))
        {
            return null;
        }

        tiles.TryGetValue(pos, out TileData tileData);
        return tileData;
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        TileData tileData = GetTile(pos);
        return tileData != null && tileData.isWalkable && tileData.occupiedUnit == null;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);
    }

    private void InitializeTiles()
    {
        tiles.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                tiles[position] = new TileData
                {
                    isWalkable = true,
                    occupiedUnit = null
                };
            }
        }
    }

    private void EnsureTilesInitialized()
    {
        if (tiles.Count == width * height)
        {
            return;
        }

        InitializeTiles();
    }
}