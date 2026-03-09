using UnityEngine;

[CreateAssetMenu(fileName = "NewAfternoonChoice", menuName = "Academy SRPG/Afternoon Choice")]
public class AfternoonChoiceData : ScriptableObject
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