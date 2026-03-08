using System;
using UnityEngine;

[Serializable]
public class CharacterStats
{
    public int hp = 10;
    public int attack = 5;
    public int defense = 3;

    [Range(0, 100)]
    public int stress = 0;

    public int Stress
    {
        get => stress;
        set => stress = Mathf.Clamp(value, 0, 100);
    }
}