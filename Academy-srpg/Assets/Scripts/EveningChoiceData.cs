using UnityEngine;

[CreateAssetMenu(fileName = "NewEveningChoice", menuName = "Academy SRPG/Evening Choice")]
public class EveningChoiceData : ScriptableObject
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