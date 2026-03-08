using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum BattleResult
    {
        None,
        Victory,
        Defeat
    }

    public List<Unit> allUnits = new List<Unit>();
    public Unit currentUnit;
    public int turnCount = 1;
    public BattleResult CurrentBattleResult { get; private set; }

    public event Action<Unit> OnTurnChanged;
    public event Action OnBattleEnd;

    public Dictionary<Unit, float> cooldowns = new Dictionary<Unit, float>();

    private bool battleEnded;

    public void InitBattle()
    {
        Unit[] foundUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        allUnits.Clear();
        cooldowns.Clear();
        currentUnit = null;
        turnCount = 1;
        battleEnded = false;
        CurrentBattleResult = BattleResult.None;

        for (int index = 0; index < foundUnits.Length; index++)
        {
            Unit unit = foundUnits[index];

            if (unit == null)
            {
                continue;
            }

            allUnits.Add(unit);
            cooldowns[unit] = 0f;
            unit.ResetTurn();
        }

        if (CheckBattleEnd())
        {
            return;
        }

        NextTurn();
    }

    public void NextTurn()
    {
        if (battleEnded || CheckBattleEnd())
        {
            currentUnit = null;
            return;
        }

        Unit nextUnit = GetReadyUnit();

        while (nextUnit == null)
        {
            float smallestCooldown = GetSmallestCooldown();

            if (smallestCooldown == float.MaxValue)
            {
                currentUnit = null;
                return;
            }

            ReduceAllCooldowns(smallestCooldown);
            nextUnit = GetReadyUnit();
        }

        currentUnit = nextUnit;
        currentUnit.hasActed = false;
        OnTurnChanged?.Invoke(currentUnit);
    }

    public void EndCurrentTurn()
    {
        if (currentUnit == null || battleEnded)
        {
            return;
        }

        if (currentUnit.IsAlive())
        {
            cooldowns[currentUnit] = 100f / Mathf.Max(1, currentUnit.speed);
            currentUnit.hasActed = true;
        }

        if (CheckBattleEnd())
        {
            currentUnit = null;
            return;
        }

        turnCount += 1;
        NextTurn();
    }

    public bool CheckBattleEnd()
    {
        bool hasAlivePlayerUnit = false;
        bool hasAliveEnemyUnit = false;

        for (int index = 0; index < allUnits.Count; index++)
        {
            Unit unit = allUnits[index];

            if (unit == null || !unit.IsAlive())
            {
                continue;
            }

            if (unit.isPlayerUnit)
            {
                hasAlivePlayerUnit = true;
            }
            else
            {
                hasAliveEnemyUnit = true;
            }
        }

        if (hasAlivePlayerUnit && hasAliveEnemyUnit)
        {
            return false;
        }

        if (!battleEnded)
        {
            CurrentBattleResult = hasAlivePlayerUnit && !hasAliveEnemyUnit
                ? BattleResult.Victory
                : BattleResult.Defeat;
            battleEnded = true;
            OnBattleEnd?.Invoke();
        }

        return true;
    }

    private Unit GetReadyUnit()
    {
        Unit selectedUnit = null;

        for (int index = 0; index < allUnits.Count; index++)
        {
            Unit unit = allUnits[index];

            if (unit == null || !unit.IsAlive())
            {
                continue;
            }

            if (!cooldowns.TryGetValue(unit, out float cooldown))
            {
                cooldown = 0f;
                cooldowns[unit] = cooldown;
            }

            if (cooldown > 0f)
            {
                continue;
            }

            if (selectedUnit == null || unit.speed > selectedUnit.speed)
            {
                selectedUnit = unit;
            }
        }

        return selectedUnit;
    }

    private float GetSmallestCooldown()
    {
        float smallestCooldown = float.MaxValue;

        for (int index = 0; index < allUnits.Count; index++)
        {
            Unit unit = allUnits[index];

            if (unit == null || !unit.IsAlive())
            {
                continue;
            }

            if (!cooldowns.TryGetValue(unit, out float cooldown))
            {
                cooldown = 0f;
                cooldowns[unit] = cooldown;
            }

            if (cooldown < smallestCooldown)
            {
                smallestCooldown = cooldown;
            }
        }

        return smallestCooldown;
    }

    private void ReduceAllCooldowns(float amount)
    {
        for (int index = 0; index < allUnits.Count; index++)
        {
            Unit unit = allUnits[index];

            if (unit == null || !unit.IsAlive())
            {
                continue;
            }

            if (!cooldowns.ContainsKey(unit))
            {
                cooldowns[unit] = 0f;
            }

            cooldowns[unit] -= amount;
        }
    }
}