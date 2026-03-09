using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum BattleActivityType
    {
        None,
        WeekTrigger,
        SaturdaySpecialActivity
    }

    public static GameManager Instance { get; private set; }

    public int currentDay = 1;
    public int currentWeek = 1;
    public AcademyTimeSlot currentTimeSlot = AcademyTimeSlot.Morning;
    public BattleActivityType CurrentBattleActivityType { get; private set; }

    [SerializeField] private string academySceneName = "AcademyScene";
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private List<int> battleTriggerWeeks = new List<int> { 4 };

    private readonly HashSet<int> triggeredBattleWeeks = new HashSet<int>();
    private readonly CharacterStats savedPlayerStats = new CharacterStats();

    private AcademyManager academyManager;
    private TurnManager turnManager;
    private bool hasSavedPlayerStats;
    private int lastSaturdaySpecialBattleDay = -1;
    private int completedSaturdaySpecialActivityDay = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentWeek = Mathf.Max(1, currentWeek);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
    }

    private void Start()
    {
        if (Instance == this)
        {
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }

    private void OnDisable()
    {
        if (Instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindSceneManagers();
    }

    public void SetCurrentWeek(int week)
    {
        currentWeek = Mathf.Max(1, week);

        if (academyManager != null)
        {
            academyManager.currentWeek = currentWeek;
        }

        TryTriggerBattleForCurrentWeek();
    }

    public void LoadAcademyScene()
    {
        LoadSceneByName(academySceneName);
    }

    public void LoadBattleScene()
    {
        LoadSceneByName(battleSceneName);
    }

    public bool TriggerSaturdaySpecialActivityBattle(int day)
    {
        int safeDay = Mathf.Max(1, day);

        if (lastSaturdaySpecialBattleDay == safeDay)
        {
            Debug.Log($"Saturday special activity battle was already triggered for day {safeDay}.");
            return false;
        }

        lastSaturdaySpecialBattleDay = safeDay;
        CurrentBattleActivityType = BattleActivityType.SaturdaySpecialActivity;
        LoadBattleScene();
        return true;
    }

    public void LoadGameOverScene()
    {
        LoadSceneByName(gameOverSceneName);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        UnbindSceneManagers();
        BindAcademyManager();
        BindTurnManager(scene.name);
    }

    private void BindAcademyManager()
    {
        academyManager = FindFirstObjectByType<AcademyManager>();

        if (academyManager == null)
        {
            return;
        }

        if (hasSavedPlayerStats)
        {
            ApplySavedStatsToAcademyManager();
            academyManager.SetCalendarState(currentDay, currentTimeSlot);
        }
        else
        {
            currentDay = Mathf.Max(1, academyManager.currentDay);
            currentWeek = Mathf.Max(1, academyManager.currentWeek);
            currentTimeSlot = academyManager.currentTimeSlot;
            SavePlayerStats(academyManager.playerStats);
        }

        academyManager.SetCompletedSaturdaySpecialActivityDay(completedSaturdaySpecialActivityDay);

        academyManager.OnActivityCompleted += HandleActivityCompleted;
        academyManager.OnScheduleChanged += HandleAcademyScheduleChanged;
    }

    private void BindTurnManager(string sceneName)
    {
        turnManager = FindFirstObjectByType<TurnManager>();

        if (turnManager == null)
        {
            return;
        }

        turnManager.OnBattleEnd += HandleBattleEnd;

        if (sceneName == battleSceneName)
        {
            ApplySavedStatsToPlayerUnits();
            turnManager.InitBattle();
        }
    }

    private void UnbindSceneManagers()
    {
        if (academyManager != null)
        {
            academyManager.OnActivityCompleted -= HandleActivityCompleted;
            academyManager.OnScheduleChanged -= HandleAcademyScheduleChanged;
            academyManager = null;
        }

        if (turnManager != null)
        {
            turnManager.OnBattleEnd -= HandleBattleEnd;
            turnManager = null;
        }
    }

    private void HandleActivityCompleted(ActivityData activity)
    {
        if (academyManager == null)
        {
            return;
        }

        currentDay = Mathf.Max(1, academyManager.currentDay);
        currentWeek = Mathf.Max(1, academyManager.currentWeek);
        currentTimeSlot = academyManager.currentTimeSlot;
        SavePlayerStats(academyManager.playerStats);
        TryTriggerBattleForCurrentWeek();
    }

    private void HandleAcademyScheduleChanged()
    {
        if (academyManager == null)
        {
            return;
        }

        currentDay = Mathf.Max(1, academyManager.currentDay);
        currentWeek = Mathf.Max(1, academyManager.currentWeek);
        currentTimeSlot = academyManager.currentTimeSlot;
        SavePlayerStats(academyManager.playerStats);

        if (academyManager.IsSaturday() && currentTimeSlot == AcademyTimeSlot.Morning)
        {
            return;
        }

        TryTriggerBattleForCurrentWeek();
    }

    private void HandleBattleEnd()
    {
        if (CurrentBattleActivityType == BattleActivityType.SaturdaySpecialActivity)
        {
            completedSaturdaySpecialActivityDay = currentDay;
            currentTimeSlot = AcademyTimeSlot.AfterSchool;
            CurrentBattleActivityType = BattleActivityType.None;
            LoadAcademyScene();
            return;
        }

        if (turnManager == null)
        {
            CurrentBattleActivityType = BattleActivityType.None;
            LoadAcademyScene();
            return;
        }

        if (turnManager.CurrentBattleResult == TurnManager.BattleResult.Victory)
        {
            CurrentBattleActivityType = BattleActivityType.None;
            LoadAcademyScene();
            return;
        }

        CurrentBattleActivityType = BattleActivityType.None;
        LoadGameOverScene();
    }

    private void TryTriggerBattleForCurrentWeek()
    {
        if (!ShouldTriggerBattle(currentWeek))
        {
            return;
        }

        triggeredBattleWeeks.Add(currentWeek);
        CurrentBattleActivityType = BattleActivityType.WeekTrigger;
        LoadBattleScene();
    }

    private bool ShouldTriggerBattle(int week)
    {
        if (triggeredBattleWeeks.Contains(week))
        {
            return false;
        }

        for (int index = 0; index < battleTriggerWeeks.Count; index++)
        {
            if (battleTriggerWeeks[index] == week)
            {
                return true;
            }
        }

        return false;
    }

    private void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("GameManager scene name is empty.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' cannot be loaded. Add it to Build Settings and verify the name.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SavePlayerStats(CharacterStats sourceStats)
    {
        if (sourceStats == null)
        {
            return;
        }

        savedPlayerStats.hp = sourceStats.hp;
        savedPlayerStats.attack = sourceStats.attack;
        savedPlayerStats.defense = sourceStats.defense;
        savedPlayerStats.Stress = sourceStats.Stress;
        hasSavedPlayerStats = true;
    }

    private void ApplySavedStatsToAcademyManager()
    {
        if (academyManager == null || !hasSavedPlayerStats)
        {
            return;
        }

        if (academyManager.playerStats == null)
        {
            academyManager.playerStats = new CharacterStats();
        }

        academyManager.playerStats.hp = savedPlayerStats.hp;
        academyManager.playerStats.attack = savedPlayerStats.attack;
        academyManager.playerStats.defense = savedPlayerStats.defense;
        academyManager.playerStats.Stress = savedPlayerStats.Stress;
    }

    private void ApplySavedStatsToPlayerUnits()
    {
        CharacterStats sourceStats = hasSavedPlayerStats ? savedPlayerStats : new CharacterStats();
        Unit[] units = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        for (int index = 0; index < units.Length; index++)
        {
            Unit unit = units[index];

            if (unit == null || !unit.isPlayerUnit)
            {
                continue;
            }

            int appliedHp = Mathf.Max(0, sourceStats.hp);
            unit.maxHp = Mathf.Max(1, appliedHp);
            unit.hp = appliedHp;
            unit.attack = sourceStats.attack;
            unit.defense = sourceStats.defense;
            return;
        }

        Debug.LogWarning("GameManager could not find a player Unit in the battle scene to apply academy stats.");
    }
}