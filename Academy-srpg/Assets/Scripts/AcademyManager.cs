using System;
using UnityEngine;

public class AcademyManager : MonoBehaviour
{
    private const int MaxChoiceOptions = 4;

    public CharacterStats playerStats = new CharacterStats();
    public int currentDay = 1;
    public int currentWeek = 1;
    public AcademyTimeSlot currentTimeSlot = AcademyTimeSlot.Morning;
    public ActivityData[] activities;
    public MorningEventData[] morningEvents;
    public CurriculumLessonData[] curriculumLessons;
    public LunchChoiceData[] lunchChoices;
    public AfternoonChoiceData[] afternoonChoices;
    public EveningChoiceData[] eveningChoices;
    public SundayChoiceData[] sundayChoices;

    [SerializeField] private int daysPerWeek = 7;

    public event Action<ActivityData> OnActivityCompleted;
    public event Action OnScheduleChanged;

    public bool IsAutoTimeProcessing { get; private set; }
    public bool IsDayReadyToEnd { get; private set; }
    public int CompletedSaturdaySpecialActivityDay { get; private set; } = -1;

    private LunchChoiceData[] defaultLunchChoices;
    private AfternoonChoiceData[] defaultAfternoonChoices;
    private EveningChoiceData[] defaultEveningChoices;
    private SundayChoiceData[] defaultSundayChoices;

    private void Start()
    {
        EnsureDefaultLunchChoices();
        EnsureDefaultAfternoonChoices();
        EnsureDefaultEveningChoices();
        EnsureDefaultSundayChoices();
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
        return CanSelectLunchChoice() || CanSelectAfternoonChoice() || CanSelectEveningChoice() || CanSelectSundayChoice();
    }

    public bool CanSelectLunchChoice()
    {
        return !IsAutoTimeProcessing && currentTimeSlot == AcademyTimeSlot.Lunch;
    }

    public bool CanSelectAfternoonChoice()
    {
        return !IsAutoTimeProcessing && currentTimeSlot == AcademyTimeSlot.Afternoon;
    }

    public bool CanSelectEveningChoice()
    {
        return !IsAutoTimeProcessing && !IsDayReadyToEnd && currentTimeSlot == AcademyTimeSlot.AfterSchool;
    }

    public bool CanSelectSundayChoice()
    {
        return !IsAutoTimeProcessing && IsSunday() && currentTimeSlot == AcademyTimeSlot.Morning;
    }

    public LunchChoiceData[] GetAvailableLunchChoices()
    {
        EnsureDefaultLunchChoices();

        LunchChoiceData[] sourceChoices = lunchChoices != null && lunchChoices.Length > 0
            ? lunchChoices
            : defaultLunchChoices;

        int count = Mathf.Min(MaxChoiceOptions, sourceChoices.Length);
        LunchChoiceData[] result = new LunchChoiceData[count];

        for (int index = 0; index < count; index++)
        {
            result[index] = sourceChoices[index];
        }

        return result;
    }

    public AfternoonChoiceData[] GetAvailableAfternoonChoices()
    {
        EnsureDefaultAfternoonChoices();

        AfternoonChoiceData[] sourceChoices = afternoonChoices != null && afternoonChoices.Length > 0
            ? afternoonChoices
            : defaultAfternoonChoices;

        int count = Mathf.Min(MaxChoiceOptions, sourceChoices.Length);
        AfternoonChoiceData[] result = new AfternoonChoiceData[count];

        for (int index = 0; index < count; index++)
        {
            result[index] = sourceChoices[index];
        }

        return result;
    }

    public EveningChoiceData[] GetAvailableEveningChoices()
    {
        EnsureDefaultEveningChoices();

        EveningChoiceData[] sourceChoices = eveningChoices != null && eveningChoices.Length > 0
            ? eveningChoices
            : defaultEveningChoices;

        int count = Mathf.Min(MaxChoiceOptions, sourceChoices.Length);
        EveningChoiceData[] result = new EveningChoiceData[count];

        for (int index = 0; index < count; index++)
        {
            result[index] = sourceChoices[index];
        }

        return result;
    }

    public SundayChoiceData[] GetAvailableSundayChoices()
    {
        EnsureDefaultSundayChoices();

        SundayChoiceData[] sourceChoices = sundayChoices != null && sundayChoices.Length > 0
            ? sundayChoices
            : defaultSundayChoices;

        int count = Mathf.Min(MaxChoiceOptions, sourceChoices.Length);
        SundayChoiceData[] result = new SundayChoiceData[count];

        for (int index = 0; index < count; index++)
        {
            result[index] = sourceChoices[index];
        }

        return result;
    }

    public void ExecuteLunchChoice(LunchChoiceData lunchChoice)
    {
        if (lunchChoice == null || playerStats == null || !CanSelectLunchChoice())
        {
            return;
        }

        IsAutoTimeProcessing = true;
        ApplyLunchChoice(lunchChoice);
        NotifyScheduleChanged();

        if (lunchChoice.dialogueData != null)
        {
            DialogueManager.Show(lunchChoice.dialogueData, CompleteLunchChoice);
            return;
        }

        DialogueManager.Show(CreateLunchChoiceDialogue(lunchChoice), CompleteLunchChoice);
    }

    public void ExecuteAfternoonChoice(AfternoonChoiceData afternoonChoice)
    {
        if (afternoonChoice == null || playerStats == null || !CanSelectAfternoonChoice())
        {
            return;
        }

        IsAutoTimeProcessing = true;
        ApplyAfternoonChoice(afternoonChoice);
        NotifyScheduleChanged();

        if (afternoonChoice.dialogueData != null)
        {
            DialogueManager.Show(afternoonChoice.dialogueData, CompleteAfternoonChoice);
            return;
        }

        DialogueManager.Show(CreateAfternoonChoiceDialogue(afternoonChoice), CompleteAfternoonChoice);
    }

    public void ExecuteEveningChoice(EveningChoiceData eveningChoice)
    {
        if (eveningChoice == null || playerStats == null || !CanSelectEveningChoice())
        {
            return;
        }

        IsAutoTimeProcessing = true;
        ApplyEveningChoice(eveningChoice);
        NotifyScheduleChanged();

        if (eveningChoice.dialogueData != null)
        {
            DialogueManager.Show(eveningChoice.dialogueData, CompleteEveningChoice);
            return;
        }

        DialogueManager.Show(CreateEveningChoiceDialogue(eveningChoice), CompleteEveningChoice);
    }

    public void ExecuteSundayChoice(SundayChoiceData sundayChoice)
    {
        if (sundayChoice == null || playerStats == null || !CanSelectSundayChoice())
        {
            return;
        }

        IsAutoTimeProcessing = true;
        ApplySundayChoice(sundayChoice);
        NotifyScheduleChanged();

        if (sundayChoice.dialogueData != null)
        {
            DialogueManager.Show(sundayChoice.dialogueData, CompleteSundayChoice);
            return;
        }

        DialogueManager.Show(CreateSundayChoiceDialogue(sundayChoice), CompleteSundayChoice);
    }

    public void ProcessAutomaticTimeSlots()
    {
        if (IsAutoTimeProcessing)
        {
            return;
        }

        if (IsSunday() && currentTimeSlot == AcademyTimeSlot.Morning)
        {
            NotifyScheduleChanged();
            return;
        }

        if (currentTimeSlot == AcademyTimeSlot.Morning && IsSaturday() && !HasCompletedSaturdaySpecialActivityToday())
        {
            RunSaturdaySpecialActivity();
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
        IsDayReadyToEnd = false;
        NotifyScheduleChanged();
    }

    public void SetCompletedSaturdaySpecialActivityDay(int day)
    {
        CompletedSaturdaySpecialActivityDay = day;
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
                return "Evening Free Time";
            default:
                return currentTimeSlot.ToString();
        }
    }

    public AcademyDayOfWeek GetCurrentDayOfWeek()
    {
        int dayOfWeekCount = System.Enum.GetValues(typeof(AcademyDayOfWeek)).Length;
        int dayIndex = (Mathf.Max(1, currentDay) - 1) % dayOfWeekCount;
        return (AcademyDayOfWeek)dayIndex;
    }

    public bool IsSaturday()
    {
        return GetCurrentDayOfWeek() == AcademyDayOfWeek.Saturday;
    }

    public bool IsSunday()
    {
        return GetCurrentDayOfWeek() == AcademyDayOfWeek.Sunday;
    }

    public bool HasCompletedSaturdaySpecialActivityToday()
    {
        return CompletedSaturdaySpecialActivityDay == currentDay;
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

    private void RunSaturdaySpecialActivity()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Saturday special activity could not start because GameManager was not found.");
            return;
        }

        IsAutoTimeProcessing = true;
        NotifyScheduleChanged();

        DialogueManager.Show(new[]
        {
            new DialogueLine
            {
                speakerName = "Academy",
                content = "오늘은 토요일 특별활동이다. 일반 수업 대신 실전 전투 훈련이 진행된다."
            }
        }, TriggerSaturdaySpecialActivityBattle);
    }

    private void TriggerSaturdaySpecialActivityBattle()
    {
        if (GameManager.Instance == null)
        {
            IsAutoTimeProcessing = false;
            NotifyScheduleChanged();
            return;
        }

        bool didTriggerBattle = GameManager.Instance.TriggerSaturdaySpecialActivityBattle(currentDay);

        if (!didTriggerBattle)
        {
            IsAutoTimeProcessing = false;
            NotifyScheduleChanged();
        }
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

    private void ApplyLunchChoice(LunchChoiceData lunchChoice)
    {
        playerStats.hp += lunchChoice.hpGain;
        playerStats.attack += lunchChoice.attackGain;
        playerStats.defense += lunchChoice.defenseGain;
        playerStats.Stress += lunchChoice.stressGain;
    }

    private void CompleteLunchChoice()
    {
        AdvanceTime();
        IsAutoTimeProcessing = false;
        ProcessAutomaticTimeSlots();
    }

    private void ApplyAfternoonChoice(AfternoonChoiceData afternoonChoice)
    {
        playerStats.hp += afternoonChoice.hpGain;
        playerStats.attack += afternoonChoice.attackGain;
        playerStats.defense += afternoonChoice.defenseGain;
        playerStats.Stress += afternoonChoice.stressGain;
    }

    private void CompleteAfternoonChoice()
    {
        AdvanceTime();
        IsAutoTimeProcessing = false;
        ProcessAutomaticTimeSlots();
    }

    private void ApplyEveningChoice(EveningChoiceData eveningChoice)
    {
        playerStats.hp += eveningChoice.hpGain;
        playerStats.attack += eveningChoice.attackGain;
        playerStats.defense += eveningChoice.defenseGain;
        playerStats.Stress += eveningChoice.stressGain;
    }

    private void CompleteEveningChoice()
    {
        EndCurrentDay();
    }

    private void ApplySundayChoice(SundayChoiceData sundayChoice)
    {
        playerStats.hp += sundayChoice.hpGain;
        playerStats.attack += sundayChoice.attackGain;
        playerStats.defense += sundayChoice.defenseGain;
        playerStats.Stress += sundayChoice.stressGain;
    }

    private void CompleteSundayChoice()
    {
        EndSunday();
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

    private DialogueLine[] CreateLunchChoiceDialogue(LunchChoiceData lunchChoice)
    {
        return new[]
        {
            new DialogueLine
            {
                speakerName = string.IsNullOrWhiteSpace(lunchChoice.choiceName) ? "Lunch" : lunchChoice.choiceName,
                content = string.IsNullOrWhiteSpace(lunchChoice.description)
                    ? "점심 시간을 짧게 보내고 다음 일정으로 넘어간다."
                    : lunchChoice.description
            }
        };
    }

    private DialogueLine[] CreateAfternoonChoiceDialogue(AfternoonChoiceData afternoonChoice)
    {
        return new[]
        {
            new DialogueLine
            {
                speakerName = string.IsNullOrWhiteSpace(afternoonChoice.choiceName) ? "Afternoon" : afternoonChoice.choiceName,
                content = string.IsNullOrWhiteSpace(afternoonChoice.description)
                    ? "오후 수업을 마치고 저녁 자유시간으로 넘어간다."
                    : afternoonChoice.description
            }
        };
    }

    private DialogueLine[] CreateEveningChoiceDialogue(EveningChoiceData eveningChoice)
    {
        return new[]
        {
            new DialogueLine
            {
                speakerName = string.IsNullOrWhiteSpace(eveningChoice.choiceName) ? "Evening" : eveningChoice.choiceName,
                content = string.IsNullOrWhiteSpace(eveningChoice.description)
                    ? "저녁 자유시간을 보내고 오늘 일정을 정리할 준비를 한다."
                    : eveningChoice.description
            }
        };
    }

    private DialogueLine[] CreateSundayChoiceDialogue(SundayChoiceData sundayChoice)
    {
        return new[]
        {
            new DialogueLine
            {
                speakerName = string.IsNullOrWhiteSpace(sundayChoice.choiceName) ? "Sunday" : sundayChoice.choiceName,
                content = string.IsNullOrWhiteSpace(sundayChoice.description)
                    ? "일요일 자유시간을 보내고 다음 주를 맞이할 준비를 한다."
                    : sundayChoice.description
            }
        };
    }

    private void EnsureDefaultLunchChoices()
    {
        if (defaultLunchChoices != null && defaultLunchChoices.Length > 0)
        {
            return;
        }

        defaultLunchChoices = new[]
        {
            CreateDefaultLunchChoice("식사", "구내식당에서 든든하게 식사하며 기운을 회복한다.", 1, 0, 0, -10),
            CreateDefaultLunchChoice("대화", "같은 반 학생과 점심을 먹으며 짧게 이야기를 나눈다.", 0, 1, 0, -4),
            CreateDefaultLunchChoice("휴식", "빈 교실에서 잠깐 쉬며 긴장을 풀고 숨을 고른다.", 0, 0, 1, -12)
        };
    }

    private void EnsureDefaultAfternoonChoices()
    {
        if (defaultAfternoonChoices != null && defaultAfternoonChoices.Length > 0)
        {
            return;
        }

        defaultAfternoonChoices = new[]
        {
            CreateDefaultAfternoonChoice("검술 훈련", "실전 감각을 익히기 위해 기본 검술 훈련에 참가한다.", 0, 2, 0, 6),
            CreateDefaultAfternoonChoice("마법 이론", "강의실에서 마법 이론을 배우며 전술 이해를 넓힌다.", 0, 0, 2, 3),
            CreateDefaultAfternoonChoice("체력 단련", "운동장에서 반복 훈련으로 체력을 끌어올린다.", 2, 1, 0, 8),
            CreateDefaultAfternoonChoice("휴식", "무리하지 않고 잠깐 쉬며 몸 상태를 정비한다.", 1, 0, 0, -10)
        };
    }

    private void EnsureDefaultEveningChoices()
    {
        if (defaultEveningChoices != null && defaultEveningChoices.Length > 0)
        {
            return;
        }

        defaultEveningChoices = new[]
        {
            CreateDefaultEveningChoice("교류", "같은 반 학생과 어울리며 관계를 다질 시간을 가진다.", 0, 1, 0, -4),
            CreateDefaultEveningChoice("탐색", "교내를 천천히 둘러보며 작은 단서를 찾아본다.", 0, 0, 1, 2),
            CreateDefaultEveningChoice("휴식", "기숙사 방에서 쉬며 하루의 피로를 푼다.", 1, 0, 0, -12),
            CreateDefaultEveningChoice("개인훈련", "남는 시간을 써서 스스로 부족한 부분을 단련한다.", 0, 2, 0, 5)
        };
    }

    private void EnsureDefaultSundayChoices()
    {
        if (defaultSundayChoices != null && defaultSundayChoices.Length > 0)
        {
            return;
        }

        defaultSundayChoices = new[]
        {
            CreateDefaultSundayChoice("휴식", "늦잠을 자고 느긋하게 쉬며 한 주의 피로를 푼다.", 2, 0, 0, -15),
            CreateDefaultSundayChoice("교류", "친한 학생과 함께 시간을 보내며 마음을 가볍게 만든다.", 0, 1, 0, -6),
            CreateDefaultSundayChoice("외출", "아카데미 밖으로 잠깐 나가 바람을 쐬고 돌아온다.", 1, 0, 1, -8)
        };
    }

    private LunchChoiceData CreateDefaultLunchChoice(string choiceName, string description, int hpGain, int attackGain, int defenseGain, int stressGain)
    {
        LunchChoiceData lunchChoice = ScriptableObject.CreateInstance<LunchChoiceData>();
        lunchChoice.choiceName = choiceName;
        lunchChoice.description = description;
        lunchChoice.hpGain = hpGain;
        lunchChoice.attackGain = attackGain;
        lunchChoice.defenseGain = defenseGain;
        lunchChoice.stressGain = stressGain;
        return lunchChoice;
    }

    private AfternoonChoiceData CreateDefaultAfternoonChoice(string choiceName, string description, int hpGain, int attackGain, int defenseGain, int stressGain)
    {
        AfternoonChoiceData afternoonChoice = ScriptableObject.CreateInstance<AfternoonChoiceData>();
        afternoonChoice.choiceName = choiceName;
        afternoonChoice.description = description;
        afternoonChoice.hpGain = hpGain;
        afternoonChoice.attackGain = attackGain;
        afternoonChoice.defenseGain = defenseGain;
        afternoonChoice.stressGain = stressGain;
        return afternoonChoice;
    }

    private EveningChoiceData CreateDefaultEveningChoice(string choiceName, string description, int hpGain, int attackGain, int defenseGain, int stressGain)
    {
        EveningChoiceData eveningChoice = ScriptableObject.CreateInstance<EveningChoiceData>();
        eveningChoice.choiceName = choiceName;
        eveningChoice.description = description;
        eveningChoice.hpGain = hpGain;
        eveningChoice.attackGain = attackGain;
        eveningChoice.defenseGain = defenseGain;
        eveningChoice.stressGain = stressGain;
        return eveningChoice;
    }

    private SundayChoiceData CreateDefaultSundayChoice(string choiceName, string description, int hpGain, int attackGain, int defenseGain, int stressGain)
    {
        SundayChoiceData sundayChoice = ScriptableObject.CreateInstance<SundayChoiceData>();
        sundayChoice.choiceName = choiceName;
        sundayChoice.description = description;
        sundayChoice.hpGain = hpGain;
        sundayChoice.attackGain = attackGain;
        sundayChoice.defenseGain = defenseGain;
        sundayChoice.stressGain = stressGain;
        return sundayChoice;
    }

    private void EndCurrentDay()
    {
        IsAutoTimeProcessing = false;
        IsDayReadyToEnd = true;
        NotifyScheduleChanged();

        currentDay += 1;
        currentWeek = CalculateWeekFromDay(currentDay);
        currentTimeSlot = AcademyTimeSlot.Morning;
        IsDayReadyToEnd = false;

        NotifyScheduleChanged();
        ProcessAutomaticTimeSlots();
    }

    private void EndSunday()
    {
        IsAutoTimeProcessing = false;
        currentDay += 1;
        currentWeek = CalculateWeekFromDay(currentDay);
        currentTimeSlot = AcademyTimeSlot.Morning;
        IsDayReadyToEnd = false;

        NotifyScheduleChanged();
        ProcessAutomaticTimeSlots();
    }

    private void NotifyScheduleChanged()
    {
        OnScheduleChanged?.Invoke();
    }
}