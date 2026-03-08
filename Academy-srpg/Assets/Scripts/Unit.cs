using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int hp;
    public int maxHp;
    public int attack;
    public int defense;
    public int speed = 5;
    public int moveRange = 3;
    public int attackRange = 1;
    public Vector2Int gridPosition;
    public bool isPlayerUnit;
    public bool hasActed;

    public void TakeDamage(int damage)
    {
        hp = Mathf.Max(0, hp - damage);
    }

    public bool IsAlive()
    {
        return hp > 0;
    }

    public void ResetTurn()
    {
        hasActed = false;
    }
}