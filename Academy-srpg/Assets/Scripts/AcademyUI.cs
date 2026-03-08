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
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (academyManager != null)
        {
            academyManager.OnActivityCompleted -= HandleActivityCompleted;
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
        ExecuteActivity(trainingActivity);
    }

    private void ExecuteSocial()
    {
        ExecuteActivity(socialActivity);
    }

    private void ExecuteRest()
    {
        ExecuteActivity(restActivity);
    }

    private void ExecuteActivity(ActivityData activity)
    {
        if (academyManager == null || activity == null)
        {
            return;
        }

        academyManager.ExecuteActivity(activity);
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
        statsText.text =
            $"Week: {academyManager.currentWeek}\n" +
            $"HP: {stats.hp}\n" +
            $"Attack: {stats.attack}\n" +
            $"Defense: {stats.defense}\n" +
            $"Stress: {stats.Stress}";
    }
}