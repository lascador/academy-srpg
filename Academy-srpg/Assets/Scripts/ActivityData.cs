using UnityEngine;

[CreateAssetMenu(fileName = "NewActivityData", menuName = "Academy SRPG/Activity Data")]
public class ActivityData : ScriptableObject
{
    public string activityName;

    [TextArea]
    public string description;

    public int attackGain;
    public int defenseGain;
    public int hpGain;
    public int stressGain;
}