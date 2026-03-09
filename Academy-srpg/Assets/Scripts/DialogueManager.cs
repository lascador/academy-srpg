using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private float typeDelay = 0.03f;
    [SerializeField] private TMP_FontAsset dialogueFont;

    public bool IsPlaying { get; private set; }

    private readonly System.Collections.Generic.List<DialogueLine> currentLines = new System.Collections.Generic.List<DialogueLine>();

    private Canvas rootCanvas;
    private GameObject dialoguePanel;
    private TextMeshProUGUI speakerText;
    private TextMeshProUGUI contentText;
    private Coroutine typingCoroutine;
    private string currentFullText = string.Empty;
    private int currentLineIndex = -1;
    private bool isTyping;
    private Action onDialogueEnded;

    public static void Show(DialogueData dialogueData, Action onComplete = null)
    {
        if (dialogueData == null)
        {
            return;
        }

        Show(dialogueData.lines, onComplete);
    }

    public static void Show(DialogueLine[] lines, Action onComplete = null)
    {
        DialogueManager manager = EnsureInstance();
        manager.PlayDialogue(lines, onComplete);
    }

    private static DialogueManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        DialogueManager manager = FindFirstObjectByType<DialogueManager>();

        if (manager != null)
        {
            return manager;
        }

        GameObject managerObject = new GameObject("DialogueManager");
        return managerObject.AddComponent<DialogueManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateUI();
        SetDialogueVisible(false);
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.Return))
        {
            return;
        }

        if (isTyping)
        {
            CompleteCurrentLine();
            return;
        }

        ShowNextLine();
    }

    private void PlayDialogue(DialogueLine[] lines, Action onComplete)
    {
        currentLines.Clear();

        if (lines != null)
        {
            for (int index = 0; index < lines.Length; index++)
            {
                if (lines[index] != null)
                {
                    currentLines.Add(lines[index]);
                }
            }
        }

        onDialogueEnded = onComplete;
        currentLineIndex = -1;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        IsPlaying = true;
        SetDialogueVisible(true);
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        currentLineIndex += 1;

        if (currentLineIndex >= currentLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentLines[currentLineIndex];
        currentFullText = line.content ?? string.Empty;

        speakerText.text = line.speakerName ?? string.Empty;
        speakerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(speakerText.text));
        contentText.text = string.Empty;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;

        for (int index = 0; index < currentFullText.Length; index++)
        {
            contentText.text += currentFullText[index];
            yield return new WaitForSecondsRealtime(typeDelay);
        }

        typingCoroutine = null;
        isTyping = false;
    }

    private void CompleteCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        contentText.text = currentFullText;
        isTyping = false;
    }

    private void EndDialogue()
    {
        IsPlaying = false;
        isTyping = false;
        currentFullText = string.Empty;
        currentLineIndex = -1;
        currentLines.Clear();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        SetDialogueVisible(false);

        Action callback = onDialogueEnded;
        onDialogueEnded = null;
        callback?.Invoke();
    }

    private void CreateUI()
    {
        if (rootCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("DialogueCanvas");
        canvasObject.transform.SetParent(transform, false);

        rootCanvas = canvasObject.AddComponent<Canvas>();
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder = 500;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        TMP_FontAsset defaultFont = ResolveDialogueFont();

        GameObject panelObject = new GameObject("DialoguePanel");
        panelObject.transform.SetParent(canvasObject.transform, false);
        dialoguePanel = panelObject;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.02f);
        panelRect.anchorMax = new Vector2(0.95f, 0.28f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        speakerText = CreateTextElement("SpeakerText", panelObject.transform, defaultFont, 34f, FontStyles.Bold);
        RectTransform speakerRect = speakerText.rectTransform;
        speakerRect.anchorMin = new Vector2(0f, 0.72f);
        speakerRect.anchorMax = new Vector2(1f, 1f);
        speakerRect.offsetMin = new Vector2(30f, -10f);
        speakerRect.offsetMax = new Vector2(-30f, -10f);

        contentText = CreateTextElement("ContentText", panelObject.transform, defaultFont, 30f, FontStyles.Normal);
        contentText.textWrappingMode = TextWrappingModes.Normal;
        RectTransform contentRect = contentText.rectTransform;
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 0.76f);
        contentRect.offsetMin = new Vector2(30f, 20f);
        contentRect.offsetMax = new Vector2(-30f, -10f);
    }

    private TextMeshProUGUI CreateTextElement(string objectName, Transform parent, TMP_FontAsset fontAsset, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.font = fontAsset;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rectTransform = textComponent.rectTransform;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return textComponent;
    }

    private TMP_FontAsset ResolveDialogueFont()
    {
        if (dialogueFont != null)
        {
            Debug.Log($"DialogueManager is using assigned font: {dialogueFont.name}");
            return dialogueFont;
        }

        dialogueFont = TMPFontResolver.GetPreferredFont();
        return dialogueFont;
    }

    private void SetDialogueVisible(bool isVisible)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(isVisible);
        }
    }
}