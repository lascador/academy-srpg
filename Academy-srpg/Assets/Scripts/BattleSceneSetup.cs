using System.Collections.Generic;
using UnityEngine;

public class BattleSceneSetup : MonoBehaviour
{
    private Sprite defaultSprite;
    private Transform tileRoot;
    private Transform moveHighlightRoot;
    private Transform attackHighlightRoot;
    private GridManager gridManager;
    private BattleManager battleManager;
    private readonly List<GameObject> moveHighlightedTiles = new List<GameObject>();
    private readonly List<GameObject> attackHighlightedTiles = new List<GameObject>();
    private BattleManager.BattleState previousState = BattleManager.BattleState.Idle;
    private int previousReachableTileCount = -1;
    private Unit previousSelectedUnit;

    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        battleManager = FindFirstObjectByType<BattleManager>();

        if (gridManager == null)
        {
            Debug.LogWarning("BattleSceneSetup could not find a GridManager in the scene.");
            return;
        }

        defaultSprite = CreateDefaultSprite();
        CreateTileVisuals(gridManager);
        SetupUnits(gridManager);
    }

    private void Update()
    {
        if (battleManager == null)
        {
            return;
        }

        bool shouldRefreshHighlights = battleManager.currentState != previousState
            || battleManager.selectedUnit != previousSelectedUnit
            || battleManager.reachableTiles.Count != previousReachableTileCount;

        if (!shouldRefreshHighlights)
        {
            return;
        }

        if (battleManager.currentState == BattleManager.BattleState.UnitSelected)
        {
            RefreshReachableTileHighlights();
            ClearAttackTileHighlights();
        }
        else if (battleManager.currentState == BattleManager.BattleState.Attacking)
        {
            ClearReachableTileHighlights();
            RefreshAttackTileHighlights();
        }
        else
        {
            ClearReachableTileHighlights();
            ClearAttackTileHighlights();
        }

        previousState = battleManager.currentState;
        previousSelectedUnit = battleManager.selectedUnit;
        previousReachableTileCount = battleManager.reachableTiles.Count;
    }

    private void SetupUnits(GridManager gridManager)
    {
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        for (int index = 0; index < units.Length; index++)
        {
            Unit unit = units[index];

            if (unit == null)
            {
                continue;
            }

            SpriteRenderer spriteRenderer = unit.GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                spriteRenderer = unit.gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = defaultSprite;
            spriteRenderer.color = unit.isPlayerUnit ? Color.blue : Color.red;
            spriteRenderer.sortingOrder = 1;

            unit.transform.position = gridManager.GridToWorld(unit.gridPosition);
        }
    }

    private void CreateTileVisuals(GridManager gridManager)
    {
        tileRoot = new GameObject("GridTiles").transform;
        tileRoot.SetParent(transform, false);

        Color tileColor = new Color(0.8f, 0.8f, 0.8f, 0.35f);

        for (int y = 0; y < gridManager.height; y++)
        {
            for (int x = 0; x < gridManager.width; x++)
            {
                GameObject tileObject = new GameObject($"Tile_{x}_{y}");
                tileObject.transform.SetParent(tileRoot, false);
                tileObject.transform.position = gridManager.GridToWorld(new Vector2Int(x, y));

                SpriteRenderer spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = defaultSprite;
                spriteRenderer.color = tileColor;
                spriteRenderer.sortingOrder = 0;
            }
        }
    }

    private void RefreshReachableTileHighlights()
    {
        ClearReachableTileHighlights();

        if (gridManager == null || battleManager == null)
        {
            return;
        }

        if (moveHighlightRoot == null)
        {
            moveHighlightRoot = new GameObject("ReachableTileHighlights").transform;
            moveHighlightRoot.SetParent(transform, false);
        }

        Color highlightColor = new Color(0f, 0.5f, 1f, 0.3f);

        for (int index = 0; index < battleManager.reachableTiles.Count; index++)
        {
            Vector2Int tilePosition = battleManager.reachableTiles[index];
            GameObject highlightObject = new GameObject($"ReachableTile_{tilePosition.x}_{tilePosition.y}");
            highlightObject.transform.SetParent(moveHighlightRoot, false);
            highlightObject.transform.position = gridManager.GridToWorld(tilePosition);

            SpriteRenderer spriteRenderer = highlightObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = defaultSprite;
            spriteRenderer.color = highlightColor;
            spriteRenderer.sortingOrder = 2;

            moveHighlightedTiles.Add(highlightObject);
        }
    }

    private void ClearReachableTileHighlights()
    {
        for (int index = 0; index < moveHighlightedTiles.Count; index++)
        {
            if (moveHighlightedTiles[index] != null)
            {
                Destroy(moveHighlightedTiles[index]);
            }
        }

        moveHighlightedTiles.Clear();
    }

    private void RefreshAttackTileHighlights()
    {
        ClearAttackTileHighlights();

        if (gridManager == null || battleManager == null || battleManager.selectedUnit == null)
        {
            return;
        }

        if (attackHighlightRoot == null)
        {
            attackHighlightRoot = new GameObject("AttackTileHighlights").transform;
            attackHighlightRoot.SetParent(transform, false);
        }

        Color highlightColor = new Color(1f, 0.5f, 0f, 0.4f);
        Vector2Int origin = battleManager.selectedUnit.gridPosition;
        int attackRange = battleManager.selectedUnit.attackRange;

        for (int y = 0; y < gridManager.height; y++)
        {
            for (int x = 0; x < gridManager.width; x++)
            {
                Vector2Int tilePosition = new Vector2Int(x, y);

                if (GetGridDistance(origin, tilePosition) > attackRange)
                {
                    continue;
                }

                GridManager.TileData tile = gridManager.GetTile(tilePosition);

                if (tile == null || tile.occupiedUnit == null || tile.occupiedUnit.isPlayerUnit == battleManager.selectedUnit.isPlayerUnit)
                {
                    continue;
                }

                GameObject highlightObject = new GameObject($"AttackTile_{tilePosition.x}_{tilePosition.y}");
                highlightObject.transform.SetParent(attackHighlightRoot, false);
                highlightObject.transform.position = gridManager.GridToWorld(tilePosition);

                SpriteRenderer spriteRenderer = highlightObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = defaultSprite;
                spriteRenderer.color = highlightColor;
                spriteRenderer.sortingOrder = 2;

                attackHighlightedTiles.Add(highlightObject);
            }
        }
    }

    private void ClearAttackTileHighlights()
    {
        for (int index = 0; index < attackHighlightedTiles.Count; index++)
        {
            if (attackHighlightedTiles[index] != null)
            {
                Destroy(attackHighlightedTiles[index]);
            }
        }

        attackHighlightedTiles.Clear();
    }

    private Sprite CreateDefaultSprite()
    {
        return Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
    }

    private int GetGridDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}