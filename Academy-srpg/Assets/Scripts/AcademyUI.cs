using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcademyUI : MonoBehaviour
{
    private const int MaxChoiceButtons = 4;

    [SerializeField] private AcademyManager academyManager;
    [SerializeField] private TextMeshProUGUI statsText;

    private LunchChoiceData[] currentLunchChoices = System.Array.Empty<LunchChoiceData>();
    private AfternoonChoiceData[] currentAfternoonChoices = System.Array.Empty<AfternoonChoiceData>();
    private EveningChoiceData[] currentEveningChoices = System.Array.Empty<EveningChoiceData>();
    private SundayChoiceData[] currentSundayChoices = System.Array.Empty<SundayChoiceData>();
    private GameObject lunchPanel;
    private TextMeshProUGUI lunchTitleText;
    private Button[] lunchButtons;
    private TextMeshProUGUI[] lunchButtonTexts;

    private void Awake()
    {
        EnsureEventSystem();
        CreateLunchChoiceUI();
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
        if (lunchButtons == null)
        {
            return;
        }

        for (int index = 0; index < lunchButtons.Length; index++)
        {
            if (lunchButtons[index] != null)
            {
                lunchButtons[index].onClick.RemoveAllListeners();
            }
        }
    }

    private void SelectLunchChoice(int index)
    {
        if (academyManager == null || !academyManager.CanSelectLunchChoice())
        {
            return;
        }

        if (index < 0 || index >= currentLunchChoices.Length)
        {
            return;
        }

        academyManager.ExecuteLunchChoice(currentLunchChoices[index]);
    }

    private void SelectAfternoonChoice(int index)
    {
        if (academyManager == null || !academyManager.CanSelectAfternoonChoice())
        {
            return;
        }

        if (index < 0 || index >= currentAfternoonChoices.Length)
        {
            return;
        }

        academyManager.ExecuteAfternoonChoice(currentAfternoonChoices[index]);
    }

    private void SelectEveningChoice(int index)
    {
        if (academyManager == null || !academyManager.CanSelectEveningChoice())
        {
            return;
        }

        if (index < 0 || index >= currentEveningChoices.Length)
        {
            return;
        }

        academyManager.ExecuteEveningChoice(currentEveningChoices[index]);
    }

    private void SelectSundayChoice(int index)
    {
        if (academyManager == null || !academyManager.CanSelectSundayChoice())
        {
            return;
        }

        if (index < 0 || index >= currentSundayChoices.Length)
        {
            return;
        }

        academyManager.ExecuteSundayChoice(currentSundayChoices[index]);
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
        currentLunchChoices = academyManager.GetAvailableLunchChoices();
        currentAfternoonChoices = academyManager.GetAvailableAfternoonChoices();
        currentEveningChoices = academyManager.GetAvailableEveningChoices();
        currentSundayChoices = academyManager.GetAvailableSundayChoices();
        bool canSelectLunchChoice = academyManager.CanSelectLunchChoice();
        bool canSelectAfternoonChoice = academyManager.CanSelectAfternoonChoice();
        bool canSelectEveningChoice = academyManager.CanSelectEveningChoice();
        bool canSelectSundayChoice = academyManager.CanSelectSundayChoice();

        UpdateChoiceUI(canSelectLunchChoice, canSelectAfternoonChoice, canSelectEveningChoice, canSelectSundayChoice);

        statsText.text =
            $"Week: {academyManager.currentWeek}\n" +
            $"Day: {academyManager.currentDay} ({academyManager.GetCurrentDayOfWeek()})\n" +
            $"Time: {academyManager.GetCurrentTimeSlotLabel()}\n" +
            $"HP: {stats.hp}\n" +
            $"Attack: {stats.attack}\n" +
            $"Defense: {stats.defense}\n" +
            $"Stress: {stats.Stress}\n" +
            GetScheduleHint();
    }

    private void UpdateChoiceUI(bool canSelectLunchChoice, bool canSelectAfternoonChoice, bool canSelectEveningChoice, bool canSelectSundayChoice)
    {
        if (lunchPanel == null)
        {
            return;
        }

        bool showChoicePanel = canSelectLunchChoice || canSelectAfternoonChoice || canSelectEveningChoice || canSelectSundayChoice;
        lunchPanel.SetActive(showChoicePanel);

        if (lunchTitleText != null)
        {
            if (canSelectSundayChoice)
            {
                lunchTitleText.text = "일요일 자유시간\n무엇을 할까?";
            }
            else if (canSelectEveningChoice)
            {
                lunchTitleText.text = "저녁 자유시간\n어떻게 보낼까?";
            }
            else if (canSelectAfternoonChoice)
            {
                lunchTitleText.text = "오후 시간\n무엇을 배울까?";
            }
            else
            {
                lunchTitleText.text = "점심 시간\n무엇을 할까?";
            }
        }

        if (lunchButtons == null)
        {
            return;
        }

        for (int index = 0; index < lunchButtons.Length; index++)
        {
            Button button = lunchButtons[index];

            if (button == null)
            {
                continue;
            }

            bool hasChoice = canSelectSundayChoice
                ? index >= 0 && index < currentSundayChoices.Length && currentSundayChoices[index] != null
                : canSelectEveningChoice
                    ? index >= 0 && index < currentEveningChoices.Length && currentEveningChoices[index] != null
                    : canSelectAfternoonChoice
                        ? index >= 0 && index < currentAfternoonChoices.Length && currentAfternoonChoices[index] != null
                        : index >= 0 && index < currentLunchChoices.Length && currentLunchChoices[index] != null;

            button.gameObject.SetActive(showChoicePanel && hasChoice);

            if (!hasChoice)
            {
                continue;
            }

            button.interactable = showChoicePanel;

            if (canSelectSundayChoice)
            {
                SetButtonLabel(button, currentSundayChoices[index]);
            }
            else if (canSelectEveningChoice)
            {
                SetButtonLabel(button, currentEveningChoices[index]);
            }
            else if (canSelectAfternoonChoice)
            {
                SetButtonLabel(button, currentAfternoonChoices[index]);
            }
            else
            {
                SetButtonLabel(button, currentLunchChoices[index]);
            }
        }
    }

    private void CreateLunchChoiceUI()
    {
        if (lunchPanel != null)
        {
            return;
        }

        Canvas parentCanvas = statsText != null ? statsText.GetComponentInParent<Canvas>() : null;

        if (parentCanvas == null)
        {
            parentCanvas = CreateFallbackCanvas();
        }

        GameObject panelObject = new GameObject("LunchChoicePanel");
        panelObject.transform.SetParent(parentCanvas.transform, false);
        lunchPanel = panelObject;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        VerticalLayoutGroup layoutGroup = panelObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 12f;
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        ContentSizeFitter contentSizeFitter = panelObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 40f);

        TMP_FontAsset uiFont = ResolveUIFont();
        lunchTitleText = CreatePanelText("LunchTitle", panelObject.transform, uiFont, 34f, FontStyles.Bold);
        lunchTitleText.alignment = TextAlignmentOptions.Center;
        lunchTitleText.text = "점심 시간\n무엇을 할까?";

        lunchButtons = new Button[MaxChoiceButtons];
        lunchButtonTexts = new TextMeshProUGUI[MaxChoiceButtons];

        for (int index = 0; index < MaxChoiceButtons; index++)
        {
            int capturedIndex = index;
            Button button = CreateLunchButton(panelObject.transform, uiFont, out TextMeshProUGUI buttonText);
            button.onClick.AddListener(() => HandleChoiceSelection(capturedIndex));

            lunchButtons[index] = button;
            lunchButtonTexts[index] = buttonText;
        }

        lunchPanel.SetActive(false);
    }

    private Button CreateLunchButton(Transform parent, TMP_FontAsset uiFont, out TextMeshProUGUI buttonText)
    {
        GameObject buttonObject = new GameObject("LunchChoiceButton");
        buttonObject.transform.SetParent(parent, false);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.22f, 0.3f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = new Color(1f, 1f, 1f, 1f);
        colorBlock.highlightedColor = new Color(0.85f, 0.9f, 1f, 1f);
        colorBlock.pressedColor = new Color(0.75f, 0.82f, 0.95f, 1f);
        colorBlock.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.65f);
        button.colors = colorBlock;

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 88f;

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0f, 88f);

        buttonText = CreatePanelText("Label", buttonObject.transform, uiFont, 24f, FontStyles.Normal);
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.textWrappingMode = TextWrappingModes.Normal;
        buttonText.text = string.Empty;

        RectTransform labelRect = buttonText.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 10f);
        labelRect.offsetMax = new Vector2(-16f, -10f);

        return button;
    }

    private TextMeshProUGUI CreatePanelText(string objectName, Transform parent, TMP_FontAsset uiFont, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.font = uiFont;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.color = Color.white;

        RectTransform rectTransform = textComponent.rectTransform;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return textComponent;
    }

    private Canvas CreateFallbackCanvas()
    {
        GameObject canvasObject = new GameObject("AcademyUICanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private TMP_FontAsset ResolveUIFont()
    {
        TMP_FontAsset preferredFont = TMPFontResolver.GetPreferredFont();

        if (preferredFont != null)
        {
            return preferredFont;
        }

        if (statsText != null && statsText.font != null)
        {
            return statsText.font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private void HandleChoiceSelection(int index)
    {
        if (academyManager == null)
        {
            return;
        }

        if (academyManager.CanSelectLunchChoice())
        {
            SelectLunchChoice(index);
            return;
        }

        if (academyManager.CanSelectAfternoonChoice())
        {
            SelectAfternoonChoice(index);
            return;
        }

        if (academyManager.CanSelectEveningChoice())
        {
            SelectEveningChoice(index);
            return;
        }

        if (academyManager.CanSelectSundayChoice())
        {
            SelectSundayChoice(index);
        }
    }

    private void SetButtonLabel(Button button, LunchChoiceData lunchChoice)
    {
        if (button == null || lunchChoice == null)
        {
            return;
        }

        TextMeshProUGUI tmpLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmpLabel == null)
        {
            return;
        }

        string description = string.IsNullOrWhiteSpace(lunchChoice.description)
            ? "짧은 점심 자유시간을 보낸다."
            : lunchChoice.description;

        tmpLabel.text = $"{lunchChoice.choiceName}\n<size=70%>{description}</size>";
    }

    private void SetButtonLabel(Button button, AfternoonChoiceData afternoonChoice)
    {
        if (button == null || afternoonChoice == null)
        {
            return;
        }

        TextMeshProUGUI tmpLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmpLabel == null)
        {
            return;
        }

        string description = string.IsNullOrWhiteSpace(afternoonChoice.description)
            ? "짧은 오후 수업을 선택한다."
            : afternoonChoice.description;

        tmpLabel.text = $"{afternoonChoice.choiceName}\n<size=70%>{description}</size>";
    }

    private void SetButtonLabel(Button button, EveningChoiceData eveningChoice)
    {
        if (button == null || eveningChoice == null)
        {
            return;
        }

        TextMeshProUGUI tmpLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmpLabel == null)
        {
            return;
        }

        string description = string.IsNullOrWhiteSpace(eveningChoice.description)
            ? "저녁 자유시간 행동을 선택한다."
            : eveningChoice.description;

        tmpLabel.text = $"{eveningChoice.choiceName}\n<size=70%>{description}</size>";
    }

    private void SetButtonLabel(Button button, SundayChoiceData sundayChoice)
    {
        if (button == null || sundayChoice == null)
        {
            return;
        }

        TextMeshProUGUI tmpLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (tmpLabel == null)
        {
            return;
        }

        string description = string.IsNullOrWhiteSpace(sundayChoice.description)
            ? "일요일 자유시간 행동을 선택한다."
            : sundayChoice.description;

        tmpLabel.text = $"{sundayChoice.choiceName}\n<size=70%>{description}</size>";
    }

    private string GetScheduleHint()
    {
        if (academyManager.IsAutoTimeProcessing)
        {
            return "Schedule: automatic event in progress.";
        }

        if (academyManager.currentTimeSlot == AcademyTimeSlot.Lunch)
        {
            return "Schedule: choose a lunch activity.";
        }

        if (academyManager.currentTimeSlot == AcademyTimeSlot.Afternoon)
        {
            return "Schedule: choose one afternoon class or training.";
        }

        if (academyManager.currentTimeSlot == AcademyTimeSlot.AfterSchool)
        {
            return academyManager.IsDayReadyToEnd
                ? "Schedule: wrapping up the day and preparing the next morning."
                : "Schedule: choose one evening free-time activity.";
        }

        if (academyManager.IsSunday())
        {
            return "Schedule: choose one Sunday free-time activity.";
        }

        return "Schedule: follow the academy timetable.";
    }
}