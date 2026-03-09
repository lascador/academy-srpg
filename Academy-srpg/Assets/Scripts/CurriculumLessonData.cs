using UnityEngine;

[CreateAssetMenu(fileName = "NewCurriculumLesson", menuName = "Academy SRPG/Curriculum Lesson")]
public class CurriculumLessonData : ScriptableObject
{
    public string lessonName;

    [TextArea(2, 4)]
    public string description;

    public int hpGain;
    public int attackGain;
    public int defenseGain;
    public int stressGain;
    public DialogueData dialogueData;
}