using System;
using UnityEngine;

public class AcademyManager : MonoBehaviour
{
    public CharacterStats playerStats = new CharacterStats();
    public int currentWeek = 1;
    public ActivityData[] activities;

    public event Action<ActivityData> OnActivityCompleted;

    public void ExecuteActivity(ActivityData activity)
    {
        if (activity == null || playerStats == null)
        {
            return;
        }

        playerStats.attack += activity.attackGain;
        playerStats.defense += activity.defenseGain;
        playerStats.hp += activity.hpGain;
        playerStats.Stress += activity.stressGain;

        currentWeek += 1;

        OnActivityCompleted?.Invoke(activity);
    }
}