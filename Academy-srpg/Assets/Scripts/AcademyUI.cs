using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcademyUI : MonoBehaviour
{
    [SerializeField] private AcademyManager academyManager;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button trainingButton;
    [SerializeField] private Button socialButton;
    [SerializeField] private Button restButton;
    [SerializeField] private ActivityData trainingActivity;
    [SerializeField] private ActivityData socialActivity;
    [SerializeField] private ActivityData restActivity;

    private readonly DialogueLine[] trainingTestDialogue =
    {
        new DialogueLine
        {
            speakerName = "Instructor",
            content = "좋아. 오늘은 기본기부터 다시 점검한다. 집중해."
        },
        new DialogueLine
        {
            speakerName = "Protagonist",
            content = "네. 한 번 더 제대로 몸에 익혀보겠습니다."
        }
    };

    private readonly DialogueLine[] restTestDialogue =
    {
        new DialogueLine
        {
            speakerName = "Protagonist",
            content = "오늘은 조금 쉬어가자. 너무 몰아붙이면 오래 못 간다."
        },
        new DialogueLine
        {
            speakerName = "Narration",
            content = "짧은 휴식 덕분에 긴장이 조금 풀렸다."
        }
    };

    private void Awake()
    {
        if (trainingButton != null)
        {
            trainingButton.onClick.AddListener(ExecuteTraining);
        }

        if (socialButton != null)
        {
            socialButton.onClick.AddListener(ExecuteSocial);
        }

        if (restButton != null)
        {
            restButton.onClick.AddListener(ExecuteRest);
        }
    }

    private void OnEnable()
    {
        if (academyManager != null)
        {
            academyManager.OnActivityCompleted += HandleActivityCompleted;
            academyManager.OnScheduleChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (academyManager != null)
        {
            academyManager.OnActivityCompleted -= HandleActivityCompleted;
            academyManager.OnScheduleChanged -= RefreshUI;
        }
    }

    private void OnDestroy()
    {
        if (trainingButton != null)
        {
            trainingButton.onClick.RemoveListener(ExecuteTraining);
        }

        if (socialButton != null)
        {
            socialButton.onClick.RemoveListener(ExecuteSocial);
        }

        if (restButton != null)
        {
            restButton.onClick.RemoveListener(ExecuteRest);
        }
    }

    private void ExecuteTraining()
    {
        ExecuteActivity(trainingActivity, trainingTestDialogue);
    }

    private void ExecuteSocial()
    {
        ExecuteActivity(socialActivity);
    }

    private void ExecuteRest()
    {
        ExecuteActivity(restActivity, restTestDialogue);
    }

    private void ExecuteActivity(ActivityData activity, DialogueLine[] postActivityDialogue = null)
    {
        if (academyManager == null || activity == null || !academyManager.CanExecutePlayerActivity())
        {
            return;
        }

        academyManager.ExecuteActivity(activity);

        if (postActivityDialogue != null && postActivityDialogue.Length > 0)
        {
            DialogueManager.Show(postActivityDialogue, academyManager.ProcessAutomaticTimeSlots);
            return;
        }

        academyManager.ProcessAutomaticTimeSlots();
    }

    private void HandleActivityCompleted(ActivityData activity)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (statsText == null)
        {
            return;
        }

        if (academyManager == null || academyManager.playerStats == null)
        {
            statsText.text = "AcademyManager is not assigned.";
            return;
        }

        CharacterStats stats = academyManager.playerStats;
        bool canExecuteActivity = academyManager.CanExecutePlayerActivity();

        if (trainingButton != null)
        {
            trainingButton.interactable = canExecuteActivity && trainingActivity != null;
        }

        if (socialButton != null)
        {
            socialButton.interactable = canExecuteActivity && socialActivity != null;
        }

        if (restButton != null)
        {
            restButton.interactable = canExecuteActivity && restActivity != null;
        }

        statsText.text =
            $"Week: {academyManager.currentWeek}\n" +
            $"Day: {academyManager.currentDay}\n" +
            $"Time: {academyManager.GetCurrentTimeSlotLabel()}\n" +
            $"HP: {stats.hp}\n" +
            $"Attack: {stats.attack}\n" +
            $"Defense: {stats.defense}\n" +
            $"Stress: {stats.Stress}";
    }
}