using UnityEngine;

[CreateAssetMenu(fileName = "NewMorningEvent", menuName = "Academy SRPG/Morning Event")]
public class MorningEventData : ScriptableObject
{
    public string eventName;
    public DialogueData dialogueData;
}