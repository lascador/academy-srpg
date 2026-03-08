using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public enum BattleState
    {
        Idle,
        UnitSelected,
        Moving,
        Attacking
    }

    public BattleState currentState = BattleState.Idle;
    public Unit selectedUnit;
    public List<Vector2Int> reachableTiles = new List<Vector2Int>();

    [SerializeField] private GridManager gridManager;
    [SerializeField] private TurnManager turnManager;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Start()
    {
        RegisterUnitsOnGrid();
    }

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged += HandleNewTurn;
            turnManager.OnBattleEnd += HandleBattleEnd;
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.OnTurnChanged -= HandleNewTurn;
            turnManager.OnBattleEnd -= HandleBattleEnd;
        }
    }

    private void Update()
    {
        if (currentState == BattleState.Idle || selectedUnit == null || gridManager == null)
        {
            return;
        }

        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        Vector2Int gridPosition = gridManager.WorldToGrid(worldPosition);

        if (currentState == BattleState.UnitSelected)
        {
            if (reachableTiles.Contains(gridPosition))
            {
                MoveUnit(gridPosition);
            }

            return;
        }

        if (currentState == BattleState.Attacking)
        {
            GridManager.TileData tile = gridManager.GetTile(gridPosition);

            if (tile != null && tile.occupiedUnit != null && tile.occupiedUnit.isPlayerUnit != selectedUnit.isPlayerUnit)
            {
                TryAttack(gridPosition);
            }
            else
            {
                SkipAttack();
            }
        }
    }

    public void HandleNewTurn(Unit unit)
    {
        ResetSelection();

        if (unit == null || !unit.IsAlive())
        {
            currentState = BattleState.Idle;
            return;
        }

        RegisterUnitOnGrid(unit);

        if (unit.isPlayerUnit)
        {
            SelectUnit(unit);
            return;
        }

        if (turnManager != null)
        {
            turnManager.EndCurrentTurn();
        }
    }

    public void SelectUnit(Unit unit)
    {
        if (unit == null || !unit.IsAlive() || gridManager == null)
        {
            currentState = BattleState.Idle;
            return;
        }

        selectedUnit = unit;
        RegisterUnitOnGrid(selectedUnit);
        reachableTiles = GetReachableTiles(selectedUnit.gridPosition, selectedUnit.moveRange);
        currentState = BattleState.UnitSelected;
    }

    public void MoveUnit(Vector2Int targetPos)
    {
        if (selectedUnit == null || gridManager == null || !reachableTiles.Contains(targetPos))
        {
            return;
        }

        currentState = BattleState.Moving;

        GridManager.TileData previousTile = gridManager.GetTile(selectedUnit.gridPosition);

        if (previousTile != null && previousTile.occupiedUnit == selectedUnit)
        {
            previousTile.occupiedUnit = null;
        }

        selectedUnit.gridPosition = targetPos;
        selectedUnit.transform.position = gridManager.GridToWorld(targetPos);

        GridManager.TileData newTile = gridManager.GetTile(targetPos);

        if (newTile != null)
        {
            newTile.occupiedUnit = selectedUnit;
        }

        currentState = BattleState.Attacking;
    }

    public void TryAttack(Vector2Int targetPos)
    {
        if (selectedUnit == null || gridManager == null || turnManager == null)
        {
            return;
        }

        GridManager.TileData tile = gridManager.GetTile(targetPos);

        if (tile == null || tile.occupiedUnit == null)
        {
            return;
        }

        Unit defender = tile.occupiedUnit;

        if (defender == selectedUnit || defender.isPlayerUnit == selectedUnit.isPlayerUnit)
        {
            return;
        }

        if (GetGridDistance(selectedUnit.gridPosition, targetPos) > selectedUnit.attackRange)
        {
            return;
        }

        int damage = Mathf.Max(1, selectedUnit.attack - defender.defense);
        defender.TakeDamage(damage);

        if (!defender.IsAlive())
        {
            tile.occupiedUnit = null;
        }

        selectedUnit.hasActed = true;
        currentState = BattleState.Idle;
        reachableTiles.Clear();
        turnManager.EndCurrentTurn();
    }

    public void SkipAttack()
    {
        if (selectedUnit == null || turnManager == null)
        {
            return;
        }

        selectedUnit.hasActed = true;
        currentState = BattleState.Idle;
        reachableTiles.Clear();
        turnManager.EndCurrentTurn();
    }

    public List<Vector2Int> GetReachableTiles(Vector2Int start, int range)
    {
        List<Vector2Int> results = new List<Vector2Int>();

        if (gridManager == null || range <= 0)
        {
            return results;
        }

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

        frontier.Enqueue(start);
        distances[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            int currentDistance = distances[current];

            if (currentDistance >= range)
            {
                continue;
            }

            for (int index = 0; index < Directions.Length; index++)
            {
                Vector2Int next = current + Directions[index];

                if (distances.ContainsKey(next) || !gridManager.IsValidPosition(next) || !gridManager.IsWalkable(next))
                {
                    continue;
                }

                distances[next] = currentDistance + 1;
                frontier.Enqueue(next);
                results.Add(next);
            }
        }

        return results;
    }

    private void HandleBattleEnd()
    {
        ResetSelection();
        currentState = BattleState.Idle;
    }

    private void RegisterUnitsOnGrid()
    {
        if (gridManager == null)
        {
            return;
        }

        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        for (int index = 0; index < units.Length; index++)
        {
            RegisterUnitOnGrid(units[index]);
        }
    }

    private void RegisterUnitOnGrid(Unit unit)
    {
        if (gridManager == null || unit == null || !unit.IsAlive() || !gridManager.IsValidPosition(unit.gridPosition))
        {
            return;
        }

        GridManager.TileData tile = gridManager.GetTile(unit.gridPosition);

        if (tile != null)
        {
            tile.occupiedUnit = unit;
        }
    }

    private void ResetSelection()
    {
        selectedUnit = null;
        reachableTiles.Clear();
    }

    private int GetGridDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
}