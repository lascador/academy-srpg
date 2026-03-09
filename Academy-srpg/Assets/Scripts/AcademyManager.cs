using System;
using UnityEngine;

public class AcademyManager : MonoBehaviour
{
    public CharacterStats playerStats = new CharacterStats();
    public int currentDay = 1;
    public int currentWeek = 1;
    public AcademyTimeSlot currentTimeSlot = AcademyTimeSlot.Morning;
    public ActivityData[] activities;
    public MorningEventData[] morningEvents;
    public CurriculumLessonData[] curriculumLessons;

    [SerializeField] private int daysPerWeek = 5;

    public event Action<ActivityData> OnActivityCompleted;
    public event Action OnScheduleChanged;

    public bool IsAutoTimeProcessing { get; private set; }

    private void Start()
    {
        ProcessAutomaticTimeSlots();
    }

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

        AdvanceTime();

        OnActivityCompleted?.Invoke(activity);
        NotifyScheduleChanged();
    }

    public bool CanExecutePlayerActivity()
    {
        return !IsAutoTimeProcessing && currentTimeSlot != AcademyTimeSlot.Morning && currentTimeSlot != AcademyTimeSlot.LateMorning;
    }

    public void ProcessAutomaticTimeSlots()
    {
        if (IsAutoTimeProcessing)
        {
            return;
        }

        if (currentTimeSlot == AcademyTimeSlot.Morning)
        {
            RunMorningEvent();
            return;
        }

        if (currentTimeSlot == AcademyTimeSlot.LateMorning)
        {
            RunCurriculumLesson();
            return;
        }

        NotifyScheduleChanged();
    }

    public void SetCalendarState(int day, AcademyTimeSlot timeSlot)
    {
        currentDay = Mathf.Max(1, day);
        currentTimeSlot = timeSlot;
        currentWeek = CalculateWeekFromDay(currentDay);
        NotifyScheduleChanged();
    }

    public string GetCurrentTimeSlotLabel()
    {
        switch (currentTimeSlot)
        {
            case AcademyTimeSlot.Morning:
                return "Morning";
            case AcademyTimeSlot.LateMorning:
                return "Late Morning";
            case AcademyTimeSlot.Lunch:
                return "Lunch";
            case AcademyTimeSlot.Afternoon:
                return "Afternoon";
            case AcademyTimeSlot.AfterSchool:
                return "After School";
            default:
                return currentTimeSlot.ToString();
        }
    }

    private void AdvanceTime()
    {
        if (currentTimeSlot == AcademyTimeSlot.AfterSchool)
        {
            currentDay += 1;
            currentTimeSlot = AcademyTimeSlot.Morning;
            currentWeek = CalculateWeekFromDay(currentDay);
            NotifyScheduleChanged();
            return;
        }

        currentTimeSlot += 1;
        NotifyScheduleChanged();
    }

    private int CalculateWeekFromDay(int day)
    {
        int safeDaysPerWeek = Mathf.Max(1, daysPerWeek);
        return ((Mathf.Max(1, day) - 1) / safeDaysPerWeek) + 1;
    }

    private void RunMorningEvent()
    {
        IsAutoTimeProcessing = true;
        NotifyScheduleChanged();

        MorningEventData morningEvent = GetMorningEventForCurrentDay();

        if (morningEvent != null && morningEvent.dialogueData != null)
        {
            DialogueManager.Show(morningEvent.dialogueData, CompleteMorningEvent);
            return;
        }

        DialogueManager.Show(new[]
        {
            new DialogueLine
            {
                speakerName = "Morning",
                content = "아침 조회가 짧게 지나가고 하루가 시작된다."
            }
        }, CompleteMorningEvent);
    }

    private void CompleteMorningEvent()
    {
        AdvanceTime();
        IsAutoTimeProcessing = false;
        ProcessAutomaticTimeSlots();
    }

    private void RunCurriculumLesson()
    {
        IsAutoTimeProcessing = true;
        NotifyScheduleChanged();

        CurriculumLessonData lesson = GetCurriculumLessonForCurrentDay();

        if (lesson != null)
        {
            ApplyCurriculumLesson(lesson);

            if (lesson.dialogueData != null)
            {
                DialogueManager.Show(lesson.dialogueData, CompleteCurriculumLesson);
                return;
            }

            DialogueManager.Show(CreateCurriculumDialogue(lesson), CompleteCurriculumLesson);
            return;
        }

        DialogueManager.Show(new[]
        {
            new DialogueLine
            {
                speakerName = "Curriculum",
                content = "오전 수업이 평소처럼 진행되었다."
            }
        }, CompleteCurriculumLesson);
    }

    private void CompleteCurriculumLesson()
    {
        AdvanceTime();
        IsAutoTimeProcessing = false;
        ProcessAutomaticTimeSlots();
    }

    private MorningEventData GetMorningEventForCurrentDay()
    {
        if (morningEvents == null || morningEvents.Length == 0)
        {
            return null;
        }

        int index = (Mathf.Max(1, currentDay) - 1) % morningEvents.Length;
        return morningEvents[index];
    }

    private CurriculumLessonData GetCurriculumLessonForCurrentDay()
    {
        if (curriculumLessons == null || curriculumLessons.Length == 0)
        {
            return null;
        }

        int index = (Mathf.Max(1, currentDay) - 1) % curriculumLessons.Length;
        return curriculumLessons[index];
    }

    private void ApplyCurriculumLesson(CurriculumLessonData lesson)
    {
        if (lesson == null || playerStats == null)
        {
            return;
        }

        playerStats.hp += lesson.hpGain;
        playerStats.attack += lesson.attackGain;
        playerStats.defense += lesson.defenseGain;
        playerStats.Stress += lesson.stressGain;
        NotifyScheduleChanged();
    }

    private DialogueLine[] CreateCurriculumDialogue(CurriculumLessonData lesson)
    {
        return new[]
        {
            new DialogueLine
            {
                speakerName = string.IsNullOrWhiteSpace(lesson.lessonName) ? "Curriculum" : lesson.lessonName,
                content = string.IsNullOrWhiteSpace(lesson.description)
                    ? "오전 수업이 자동으로 진행되었다."
                    : lesson.description
            }
        };
    }

    private void NotifyScheduleChanged()
    {
        OnScheduleChanged?.Invoke();
    }
}