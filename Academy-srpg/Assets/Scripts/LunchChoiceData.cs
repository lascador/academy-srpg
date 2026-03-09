using UnityEngine;

[CreateAssetMenu(fileName = "NewLunchChoice", menuName = "Academy SRPG/Lunch Choice")]
public class LunchChoiceData : ScriptableObject
{
    public string choiceName;

    [TextArea(2, 4)]
    public string description;

    public int hpGain;
    public int attackGain;
    public int defenseGain;
    public int stressGain;
    public DialogueData dialogueData;
}