using UnityEngine;

[CreateAssetMenu(fileName = "NewSundayChoice", menuName = "Academy SRPG/Sunday Choice")]
public class SundayChoiceData : ScriptableObject
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