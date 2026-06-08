using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    const string DefaultChineseFontPath = "Assets/Fonts/OpenSource/NotoSansCJKsc-Dynamic.asset";
    public static UIManager Instance;

    [Header("UI面板")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject endGamePanel;

    [Header("身份提示面板")]
    public GameObject identityPanel;
    public TextMeshProUGUI identityText;

    [Header("游戏界面")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI turnInfoText;
    public TextMeshProUGUI messageText;
    public Transform playerListContainer;
    public GameObject playerButtonPrefab;
    public Button skipButton;
    public TextMeshProUGUI skipButtonText;

    [Header("接受/拒绝面板")]
    public GameObject responsePanel;
    public TextMeshProUGUI responseText;
    public Button acceptButton;
    public Button rejectButton;

    [Header("回合结束面板")]
    public GameObject roundEndPanel;
    public TextMeshProUGUI roundResultText;
    public Button nextRoundButton;

    [Header("结束界面")]
    public TextMeshProUGUI finalScoresText;

    [Header("测试模式面板")]
    public GameObject testMenuPanel;
    public TextMeshProUGUI testStatsText;
    public TMP_InputField batchCountInput;

    [Header("讨论阶段面板")]
    public GameObject discussionPanel;
    public TextMeshProUGUI discussionTitleText;
    public Transform discussionMessageContainer;
    public GameObject discussionMessagePrefab;
    public TextMeshProUGUI discussionPromptText;
    public GameObject humanDiscussionInputPanel;
    public Button btnAnnounceScore;
    public Button btnAccuse;
    public Button btnDefend;
    public Transform accuseTargetContainer;
    public GameObject discussionTargetButtonPrefab;
    public Button btnEndDiscussion;              // 讨论结束确认按钮

    [Header("分数猜测面板")]
    public GameObject scoreGuessingPanel;
    public TextMeshProUGUI guessingTitleText;
    public TextMeshProUGUI guessingPromptText;
    public Transform guessingInputContainer;
    public GameObject guessingInputRowPrefab;
    public Button btnSubmitGuess;
    public Button btnSkipGuess;
    public TextMeshProUGUI guessingResultText;

    [Header("AI设置")]
    public float aiActionDelay = 1.2f;
    public float testAiActionDelay = 0.3f;
    public float randomSkipProbability = 0.15f;

    [Header("字体")]
    public TMP_FontAsset chineseFont;

    private List<Button> playerButtons = new List<Button>();
    private bool isBatchRunning = false;
    private int batchRemaining = 0;

    private bool humanDiscussionSubmitted = false;
    private DiscussionMessage humanPendingMessage = null;
    private bool discussionEndConfirmed = false;
    private int activeHumanDiscussionSpeaker = -1;
    private bool canConfirmDiscussionEnd = false;
    private Coroutine autoAdvanceRoutine;
    private Coroutine discussionRoutine;
    private Coroutine scoreGuessingRoutine;
    private Image identityCardImage;
    private Button identityBackButton;
    private Image testMenuCardImage;
    private Image gameStatusCardImage;
    private Image playerListCardImage;
    private Image discussionCardImage;
    private Image discussionMessagesCardImage;
    private RectTransform discussionChoiceContainer;
    private Image mainMenuCardImage;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        PortfolioConfig.ApplyTo(this);
        ConfigureMainMenuLayout();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        UnityEditor.EditorApplication.delayCall -= ConfigureMainMenuLayoutInEditor;
        UnityEditor.EditorApplication.delayCall += ConfigureMainMenuLayoutInEditor;
    }

    void ConfigureMainMenuLayoutInEditor()
    {
        if (this == null || Application.isPlaying) return;
        ResolveChineseFont();
        ApplyChineseFontToSceneTexts();
        ConfigureMainMenuLayout();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    [UnityEditor.MenuItem("Tools/负一和一/Apply Main Menu Layout")]
    static void ApplyMainMenuLayoutMenu()
    {
        UIManager manager = FindFirstObjectByType<UIManager>();
        if (manager == null) return;
        manager.ConfigureMainMenuLayout();
        UnityEditor.Selection.activeGameObject = manager.mainMenuPanel;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
    }

    [UnityEditor.MenuItem("Tools/负一和一/Apply Chinese Font")]
    static void ApplyChineseFontMenu()
    {
        UIManager manager = FindFirstObjectByType<UIManager>();
        if (manager == null) return;
        manager.ResolveChineseFont();
        manager.ApplyChineseFontToSceneTexts();
        manager.ApplyChineseFontToPrefab("Assets/PlayerButton.prefab");
        manager.ApplyChineseFontToPrefab("Assets/DiscussionTargetButton.prefab");
        manager.ApplyChineseFontToPrefab("Assets/DiscussionMessageItem.prefab");
        manager.ApplyChineseFontToPrefab("Assets/Prefabs/GuessingInputRow.prefab");
        UnityEditor.EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
    }
#endif

    // ─── 开始游戏（正常模式）─────────────────────────
    public void StartGame()
    {
        GameManager.Instance.humanPlayerIndex = 0;
        GameManager.Instance.InitializeGame(false);
        mainMenuPanel.SetActive(false);
        endGamePanel.SetActive(false);
        SetResponsePanelActive(false);
        SetRoundEndPanelActive(false);
        ShowIdentityPanel();
    }

    void ShowIdentityPanel()
    {
        if (identityPanel == null)
        {
            gamePanel.SetActive(true);
            RefreshGameUI();
            return;
        }

        var players = GameManager.Instance.GetPlayers();
        int humanIdx = GameManager.Instance.humanPlayerIndex;
        bool isVillain = players[humanIdx].isVillain;

        identityPanel.SetActive(true);
        gamePanel.SetActive(false);
        ConfigureIdentityLayout(isVillain);

        if (identityText != null)
        {
            if (isVillain)
            {
                var allies = new System.Collections.Generic.List<string>();
                foreach (var p in players)
                    if (p.isVillain && p.playerId != humanIdx)
                        allies.Add(p.playerName);
                string allyInfo = allies.Count > 0
                    ? $"\n\n同伴线索\n{string.Join("、", allies)}"
                    : "";
                identityText.text =
                    $"你的身份\n\n" +
                    $"负一阵营\n" +
                    $"初始分数：-1\n\n" +
                    $"目标\n" +
                    $"伪装成安全对象，通过接触好人获得分数。\n\n" +
                    $"接触规则\n" +
                    $"与好人接触：你 +1，对方 -1\n" +
                    $"与负一接触：分数不变，但会留下坏人行动线索\n" +
                    $"{allyInfo}";
            }
            else
            {
                identityText.text =
                    $"你的身份\n\n" +
                    $"一阵营\n" +
                    $"初始分数：1\n\n" +
                    $"目标\n" +
                    $"寻找可靠玩家，通过安全接触提高总收益。\n\n" +
                    $"接触规则\n" +
                    $"与好人接触：双方各 +1\n" +
                    $"与负一接触：你 -1，对方 +1\n\n" +
                    $"提示\n" +
                    $"观察谁拒绝、谁发言、谁的分数变化异常。";
            }
        }
    }

    void ConfigureIdentityLayout(bool isVillain)
    {
        if (identityPanel == null) return;

        Image panelImage = identityPanel.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = new Color(0.075f, 0.094f, 0.133f, 0.98f);

        RectTransform panelRect = identityPanel.transform as RectTransform;
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        RectTransform textRect = identityText != null ? identityText.rectTransform : null;
        RectTransform buttonRect = FindIdentityConfirmButton();

        RectTransform cardRect = EnsureIdentityCard();
        if (cardRect != null)
        {
            cardRect.SetAsFirstSibling();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(0f, 12f);
            cardRect.sizeDelta = new Vector2(620f, 560f);
        }

        if (textRect != null)
        {
            identityText.fontSize = 22f;
            identityText.color = isVillain
                ? new Color(1f, 0.745f, 0.475f, 1f)
                : new Color(0.965f, 0.82f, 0.32f, 1f);
            identityText.alignment = TextAlignmentOptions.TopLeft;
            identityText.textWrappingMode = TextWrappingModes.Normal;
            identityText.lineSpacing = -2f;
            identityText.overflowMode = TextOverflowModes.Truncate;

            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, 62f);
            textRect.sizeDelta = new Vector2(540f, 400f);
        }

        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(136f, -220f);
            buttonRect.sizeDelta = new Vector2(260f, 64f);

            Image buttonImage = buttonRect.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = isVillain
                    ? new Color(0.46f, 0.20f, 0.16f, 1f)
                    : new Color(0.16f, 0.36f, 0.25f, 1f);

            TextMeshProUGUI buttonText = buttonRect.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "确认身份，开始行动";
                buttonText.fontSize = 22f;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
            }
        }

        Button backButton = EnsureIdentityBackButton();
        if (backButton != null)
        {
            RectTransform backRect = backButton.transform as RectTransform;
            if (backRect != null)
            {
                backRect.anchorMin = new Vector2(0.5f, 0.5f);
                backRect.anchorMax = new Vector2(0.5f, 0.5f);
                backRect.pivot = new Vector2(0.5f, 0.5f);
                backRect.anchoredPosition = new Vector2(-136f, -220f);
                backRect.sizeDelta = new Vector2(220f, 64f);
            }

            Image backImage = backButton.GetComponent<Image>();
            if (backImage != null)
                backImage.color = new Color(0.18f, 0.227f, 0.322f, 1f);

            TextMeshProUGUI backText = backButton.GetComponentInChildren<TextMeshProUGUI>();
            if (backText != null)
            {
                backText.text = "返回主菜单";
                backText.fontSize = 22f;
                backText.color = Color.white;
                backText.alignment = TextAlignmentOptions.Center;
            }

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    RectTransform EnsureIdentityCard()
    {
        Transform existing = identityPanel.transform.Find("IdentityCard");
        if (existing != null)
            return existing as RectTransform;

        GameObject card = new GameObject("IdentityCard", typeof(RectTransform), typeof(Image));
        RectTransform cardRect = card.GetComponent<RectTransform>();
        card.transform.SetParent(identityPanel.transform, false);
        identityCardImage = card.GetComponent<Image>();
        identityCardImage.color = new Color(0.105f, 0.129f, 0.184f, 0.96f);
        return cardRect;
    }

    Button EnsureIdentityBackButton()
    {
        if (identityBackButton != null)
            return identityBackButton;

        Transform existing = identityPanel.transform.Find("IdentityBackButton");
        if (existing != null)
        {
            identityBackButton = existing.GetComponent<Button>();
            return identityBackButton;
        }

        GameObject buttonObj = new GameObject("IdentityBackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(identityPanel.transform, false);
        identityBackButton = buttonObj.GetComponent<Button>();

        GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.font = identityText != null ? identityText.font : tmp.font;
        tmp.text = "返回主菜单";
        tmp.raycastTarget = false;

        return identityBackButton;
    }

    RectTransform FindIdentityConfirmButton()
    {
        if (identityPanel == null) return null;

        Button[] buttons = identityPanel.GetComponentsInChildren<Button>(true);
        if (buttons.Length == 0) return null;

        foreach (Button button in buttons)
        {
            if (button.name.Contains("Confirm") || button.name.Contains("Start") || button.name.Contains("Button"))
                return button.transform as RectTransform;
        }

        return buttons[0].transform as RectTransform;
    }

    public void OnConfirmIdentity()
    {
        if (identityPanel != null) identityPanel.SetActive(false);
        gamePanel.SetActive(true);
        RefreshGameUI();
    }

    // ─── 测试模式入口 ─────────────────────────────
    public void OnOpenTestMenu()
    {
        mainMenuPanel.SetActive(false);
        testMenuPanel.SetActive(true);
        ConfigureTestMenuLayout();
        RefreshTestStats();
    }

    public void OnCloseTestMenu()
    {
        if (isBatchRunning) return;
        testMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnResetTestStats()
    {
        if (isBatchRunning) return;
        GameManager.ResetTestStats();
        RefreshTestStats();
    }

    void RefreshTestStats()
    {
        if (testStatsText == null) return;
        ConfigureTestStatsText();
        int total = GameManager.testTotalGames;
        if (total == 0)
        {
            testStatsText.text = BuildTestParameterSummary() + "\n\n尚无测试数据\n运行单局或批量测试后，将在这里显示阵营胜率与平均分。";
            return;
        }

        float goodRate = (float)GameManager.testGoodWins / total * 100f;
        float villainRate = (float)GameManager.testVillainWins / total * 100f;
        float drawRate = (float)GameManager.testDraws / total * 100f;
        float goodAvg = GameManager.testGoodScoreTotal / total;
        float villainAvg = GameManager.testVillainScoreTotal / total;
        float acceptRate = GameManager.testTotalContacts > 0
            ? (float)GameManager.testAcceptedContacts / GameManager.testTotalContacts * 100f
            : 0f;
        int guessDecisions = GameManager.testSubmittedGuesses + GameManager.testSkippedGuesses;
        float guessJoinRate = guessDecisions > 0
            ? (float)GameManager.testSubmittedGuesses / guessDecisions * 100f
            : 0f;
        float averageCorrect = GameManager.testSubmittedGuesses > 0
            ? (float)GameManager.testTotalGuessCorrect / GameManager.testSubmittedGuesses
            : 0f;

        testStatsText.text =
            $"{BuildTestParameterSummary()}\n\n" +
            $"样本：{total} 局  结论：{BuildBalanceConclusion(goodRate, villainRate)}\n" +
            $"一阵营胜：{GameManager.testGoodWins} ({goodRate:F1}%)  负一胜：{GameManager.testVillainWins} ({villainRate:F1}%)  平局：{GameManager.testDraws} ({drawRate:F1}%)\n" +
            $"平均分：一阵营 {goodAvg:F2} / 负一 {villainAvg:F2}\n" +
            $"接触：{GameManager.testTotalContacts} 次，接受率 {acceptRate:F1}% ，拒绝 {GameManager.testRejectedContacts} 次，放弃 {GameManager.testSkippedTurns} 次\n" +
            $"猜分：参与率 {guessJoinRate:F1}% ，平均猜对 {averageCorrect:F2} 人";
    }

    string BuildTestParameterSummary()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return "参数版本：配置未加载";

        return $"参数版本：Docs/Portfolio CSV  人数 {gm.totalPlayers} / 负一 {gm.villainCount} / 回合 {gm.maxRounds} / AI延迟 {testAiActionDelay:0.###}s";
    }

    string BuildBalanceConclusion(float goodRate, float villainRate)
    {
        float gap = Mathf.Abs(goodRate - villainRate);
        if (gap <= 10f) return "基础平衡";
        return goodRate > villainRate ? "偏向一阵营" : "偏向负一";
    }

    // ─── 单局测试 ─────────────────────────────────
    public void OnSingleTest()
    {
        if (isBatchRunning) return;
        StartTestGame();
    }

    void StartTestGame()
    {
        GameManager.Instance.InitializeGame(true);
        testMenuPanel.SetActive(false);
        gamePanel.SetActive(true);
        SetResponsePanelActive(false);
        SetRoundEndPanelActive(false);
        RefreshGameUI();
    }

    // ─── 批量测试 ─────────────────────────────────
    public void OnBatchTest()
    {
        if (isBatchRunning) return;
        int count = 10;
        if (batchCountInput != null && !string.IsNullOrEmpty(batchCountInput.text))
            int.TryParse(batchCountInput.text, out count);
        count = Mathf.Clamp(count, 1, 10000);
        StartCoroutine(RunBatchTest(count));
    }

    IEnumerator RunBatchTest(int total)
    {
        isBatchRunning = true;
        batchRemaining = total;
        ConfigureTestMenuLayout();

        gamePanel.SetActive(false);
        SetResponsePanelActive(false);
        SetRoundEndPanelActive(false);
        testMenuPanel.SetActive(true);  // 保持测试菜单显示，用 StatsText 显示进度

        for (int i = 0; i < total; i++)
        {
            batchRemaining = total - i;
            if (testStatsText != null)
                testStatsText.text = $"{BuildTestParameterSummary()}\n\n批量测试运行中\n已完成：{i} / {total}\n剩余：{total - i}";
            GameManager.Instance.InitializeGame(true);
            yield return StartCoroutine(RunOneTestGame());
        }

        isBatchRunning = false;
        RefreshTestStats();
    }

    void ConfigureTestMenuLayout()
    {
        if (testMenuPanel == null) return;

        Image panelImage = testMenuPanel.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = new Color(0.075f, 0.094f, 0.133f, 0.98f);

        LayoutGroup panelLayout = testMenuPanel.GetComponent<LayoutGroup>();
        if (panelLayout != null)
            panelLayout.enabled = false;

        ContentSizeFitter panelFitter = testMenuPanel.GetComponent<ContentSizeFitter>();
        if (panelFitter != null)
            panelFitter.enabled = false;

        RectTransform panelRect = testMenuPanel.transform as RectTransform;
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        RectTransform cardRect = EnsureTestMenuCard();
        if (cardRect != null)
        {
            cardRect.SetAsFirstSibling();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(620f, 620f);
        }

        Transform title = FindChildRecursive(testMenuPanel.transform, "TitleText");
        ConfigurePanelText(title, "游戏测试", 34f, new Color(0.965f, 0.82f, 0.32f, 1f), TextAlignmentOptions.Center);
        SetRect(title, new Vector2(0f, 240f), new Vector2(520f, 56f));

        ConfigureTestStatsText();
        if (testStatsText != null)
            SetRect(testStatsText.transform, new Vector2(0f, 118f), new Vector2(540f, 220f));

        ConfigureTestButton("SingleTestButton", "单局测试", new Vector2(0f, -26f), new Color(0.16f, 0.36f, 0.25f, 1f));
        ConfigureBatchInput();
        ConfigureTestButton("BatchTestButton", "批量测试", new Vector2(0f, -150f), new Color(0.165f, 0.333f, 0.502f, 1f));
        ConfigureTestButton("ResetStatsButton", "重置统计", new Vector2(-140f, -242f), new Color(0.353f, 0.290f, 0.165f, 1f), 220f);
        ConfigureTestButton("BackButton", "返回主菜单", new Vector2(140f, -242f), new Color(0.18f, 0.227f, 0.322f, 1f), 220f);
    }

    RectTransform EnsureTestMenuCard()
    {
        Transform existing = testMenuPanel.transform.Find("TestMenuCard");
        if (existing != null)
            return existing as RectTransform;

        GameObject card = new GameObject("TestMenuCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(testMenuPanel.transform, false);
        testMenuCardImage = card.GetComponent<Image>();
        testMenuCardImage.color = new Color(0.105f, 0.129f, 0.184f, 0.96f);
        return card.GetComponent<RectTransform>();
    }

    void ConfigureTestStatsText()
    {
        if (testStatsText == null) return;
        ApplyChineseFont(testStatsText);
        testStatsText.fontSize = 18f;
        testStatsText.color = Color.white;
        testStatsText.alignment = TextAlignmentOptions.Center;
        testStatsText.textWrappingMode = TextWrappingModes.Normal;
        testStatsText.lineSpacing = -2f;
        testStatsText.overflowMode = TextOverflowModes.Truncate;
    }

    void ConfigureBatchInput()
    {
        if (batchCountInput == null) return;

        RectTransform inputRect = batchCountInput.transform as RectTransform;
        if (inputRect != null)
            SetRect(inputRect, new Vector2(0f, -88f), new Vector2(320f, 46f));

        Image inputImage = batchCountInput.GetComponent<Image>();
        if (inputImage != null)
            inputImage.color = new Color(0.078f, 0.102f, 0.165f, 1f);

        TMP_Text text = batchCountInput.textComponent;
        if (text != null)
        {
            text.fontSize = 22f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }

        TMP_Text placeholder = batchCountInput.placeholder as TMP_Text;
        if (placeholder != null)
        {
            placeholder.text = "批量局数";
            placeholder.fontSize = 20f;
            placeholder.color = new Color(1f, 1f, 1f, 0.48f);
            placeholder.alignment = TextAlignmentOptions.Center;
        }
    }

    void ConfigureTestButton(string objectName, string label, Vector2 position, Color color, float width = 320f)
    {
        Transform buttonTransform = FindChildRecursive(testMenuPanel.transform, objectName);
        if (buttonTransform == null) return;

        SetRect(buttonTransform, position, new Vector2(width, 56f));

        Image image = buttonTransform.GetComponent<Image>();
        if (image != null)
            image.color = color;

        TextMeshProUGUI text = buttonTransform.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            ApplyChineseFont(text);
            text.text = label;
            text.fontSize = 24f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }
    }

    void ConfigurePanelText(Transform target, string label, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        if (target == null) return;
        TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
        if (text == null) return;
        ApplyChineseFont(text);
        text.text = label;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
    }

    void ResolveChineseFont()
    {
#if UNITY_EDITOR
        TMP_FontAsset fullChineseFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultChineseFontPath);
        if (fullChineseFont != null && chineseFont == null)
            chineseFont = fullChineseFont;
#endif
    }

    void ApplyChineseFont(TextMeshProUGUI target)
    {
        if (target == null) return;
        ResolveChineseFont();
        if (chineseFont == null) return;

        target.font = chineseFont;
        target.fontSharedMaterial = chineseFont.material;
        target.fontMaterial = chineseFont.material;
    }

    void ApplyChineseFontToSceneTexts()
    {
        ResolveChineseFont();
        if (chineseFont == null) return;

        foreach (TextMeshProUGUI text in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ApplyChineseFont(text);
            text.raycastTarget = false;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(text);
#endif
        }
    }

#if UNITY_EDITOR
    void ApplyChineseFontToPrefab(string path)
    {
        ResolveChineseFont();
        if (chineseFont == null) return;

        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        bool changed = false;
        foreach (TextMeshProUGUI text in prefab.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.font = chineseFont;
            text.fontSharedMaterial = chineseFont.material;
            text.fontMaterial = chineseFont.material;
            text.raycastTarget = false;
            UnityEditor.EditorUtility.SetDirty(text);
            changed = true;
        }

        if (changed)
        {
            UnityEditor.EditorUtility.SetDirty(prefab);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(prefab);
        }
    }
#endif

    void SetRect(Transform target, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = target as RectTransform;
        if (rect == null) return;
        SetRect(rect, anchoredPosition, size);
    }

    void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    void ConfigureMainMenuLayout()
    {
        if (mainMenuPanel == null) return;

        Image panelImage = mainMenuPanel.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = new Color(0.055f, 0.071f, 0.102f, 1f);

        LayoutGroup panelLayout = mainMenuPanel.GetComponent<LayoutGroup>();
        if (panelLayout != null)
            panelLayout.enabled = false;

        ContentSizeFitter fitter = mainMenuPanel.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;

        RectTransform panelRect = mainMenuPanel.transform as RectTransform;
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        RectTransform cardRect = EnsureMainMenuCard();
        if (cardRect != null)
        {
            SetRect(cardRect, Vector2.zero, new Vector2(620f, 520f));
            cardRect.SetAsFirstSibling();
        }

        ConfigureMainMenuText("MainMenuTitle", "负一和一", new Vector2(0f, 150f), new Vector2(520f, 70f), 44f, new Color(0.965f, 0.82f, 0.32f, 1f));
        ConfigureMainMenuText("MainMenuSubtitle", "在接触、声明与猜分之间识破负一", new Vector2(0f, 96f), new Vector2(520f, 40f), 21f, new Color(0.82f, 0.86f, 0.92f, 1f));

        Transform startButton = FindChildRecursive(mainMenuPanel.transform, "StartButton");
        Transform testButton = FindChildRecursive(mainMenuPanel.transform, "GameTest");

        ConfigureMainMenuButton(startButton, "开始游戏", new Vector2(0f, 12f), new Color(0.16f, 0.36f, 0.25f, 1f));
        ConfigureMainMenuButton(testButton, "游戏测试", new Vector2(0f, -92f), new Color(0.165f, 0.333f, 0.502f, 1f));
    }

    void ConfigureMainMenuText(string objectName, string content, Vector2 position, Vector2 size, float fontSize, Color color)
    {
        Transform textTransform = mainMenuPanel.transform.Find(objectName);
        TextMeshProUGUI text;

        if (textTransform == null)
        {
            GameObject textObj = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(mainMenuPanel.transform, false);
            textTransform = textObj.transform;
            text = textObj.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            text = textTransform.GetComponent<TextMeshProUGUI>();
            if (text == null)
                text = textTransform.gameObject.AddComponent<TextMeshProUGUI>();
        }

        SetRect(textTransform, position, size);
        textTransform.SetAsLastSibling();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Truncate;
        ApplyChineseFont(text);
    }

    void ApplyMainMenuFont(TextMeshProUGUI target)
    {
        if (target == null || mainMenuPanel == null) return;

        TextMeshProUGUI source = null;
        Transform startButton = FindChildRecursive(mainMenuPanel.transform, "StartButton");
        if (startButton != null)
            source = startButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (source == null)
        {
            Transform testButton = FindChildRecursive(mainMenuPanel.transform, "GameTest");
            if (testButton != null)
                source = testButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (source == null || source.font == null) return;

        target.font = source.font;
        target.fontSharedMaterial = source.fontSharedMaterial;
        target.fontMaterial = source.fontMaterial;
    }

    RectTransform EnsureMainMenuCard()
    {
        Transform existing = mainMenuPanel.transform.Find("MainMenuCard");
        if (existing != null)
        {
            mainMenuCardImage = existing.GetComponent<Image>();
            if (mainMenuCardImage != null)
            {
                mainMenuCardImage.color = new Color(0.102f, 0.129f, 0.184f, 0.94f);
                mainMenuCardImage.raycastTarget = false;
            }
            return existing as RectTransform;
        }

        GameObject card = new GameObject("MainMenuCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(mainMenuPanel.transform, false);
        mainMenuCardImage = card.GetComponent<Image>();
        mainMenuCardImage.color = new Color(0.102f, 0.129f, 0.184f, 0.94f);
        mainMenuCardImage.raycastTarget = false;
        return card.GetComponent<RectTransform>();
    }

    void ConfigureMainMenuButton(Transform buttonTransform, string label, Vector2 position, Color color)
    {
        if (buttonTransform == null) return;

        SetRect(buttonTransform, position, new Vector2(360f, 68f));
        buttonTransform.SetAsLastSibling();

        Image image = buttonTransform.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
            image.raycastTarget = true;
        }

        Button button = buttonTransform.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = true;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.94f, 0.72f, 1f);
            colors.pressedColor = new Color(0.84f, 0.74f, 0.50f, 1f);
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.6f);
            colors.fadeDuration = 0.06f;
            button.colors = colors;
        }

        TextMeshProUGUI text = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.text = label;
            text.fontSize = 28f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }
    }

    void ConfigureGameLayout()
    {
        if (gamePanel == null) return;

        Image panelImage = gamePanel.GetComponent<Image>();
        if (panelImage != null)
            panelImage.color = new Color(0.055f, 0.071f, 0.102f, 1f);

        RectTransform panelRect = gamePanel.transform as RectTransform;
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        LayoutGroup panelLayout = gamePanel.GetComponent<LayoutGroup>();
        if (panelLayout != null)
            panelLayout.enabled = false;

        ContentSizeFitter panelFitter = gamePanel.GetComponent<ContentSizeFitter>();
        if (panelFitter != null)
            panelFitter.enabled = false;

        RectTransform statusCard = EnsureGameCard("GameStatusCard", ref gameStatusCardImage, new Color(0.102f, 0.129f, 0.184f, 0.88f));
        if (statusCard != null)
        {
            SetRect(statusCard, new Vector2(0f, 250f), new Vector2(780f, 168f));
            statusCard.SetAsFirstSibling();
        }

        StyleGameText(roundText, 32f, new Color(0.965f, 0.82f, 0.32f, 1f), TextAlignmentOptions.Center, new Vector2(0f, 302f), new Vector2(720f, 44f));
        StyleGameText(turnInfoText, 27f, new Color(0.86f, 0.89f, 0.95f, 1f), TextAlignmentOptions.Center, new Vector2(0f, 252f), new Vector2(720f, 38f));
        StyleGameText(messageText, 29f, Color.white, TextAlignmentOptions.Center, new Vector2(0f, 204f), new Vector2(720f, 42f));

        ConfigurePlayerListArea();
        ConfigureSkipButton();
    }

    RectTransform EnsureGameCard(string objectName, ref Image cachedImage, Color color)
    {
        Transform existing = gamePanel.transform.Find(objectName);
        if (existing != null)
        {
            cachedImage = existing.GetComponent<Image>();
            if (cachedImage != null)
            {
                cachedImage.color = color;
                cachedImage.raycastTarget = false;
            }
            return existing as RectTransform;
        }

        GameObject card = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(gamePanel.transform, false);
        cachedImage = card.GetComponent<Image>();
        cachedImage.color = color;
        cachedImage.raycastTarget = false;
        return card.GetComponent<RectTransform>();
    }

    void StyleGameText(TextMeshProUGUI text, float fontSize, Color color, TextAlignmentOptions alignment, Vector2 position, Vector2 size)
    {
        if (text == null) return;

        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate;
        text.lineSpacing = -3f;
        SetRect(text.rectTransform, position, size);
    }

    void ConfigurePlayerListArea()
    {
        if (playerListContainer == null) return;

        ScrollRect scrollRect = playerListContainer.GetComponentInParent<ScrollRect>();
        RectTransform listRect = scrollRect != null
            ? scrollRect.transform as RectTransform
            : playerListContainer as RectTransform;

        RectTransform cardRect = EnsureGameCard("PlayerListCard", ref playerListCardImage, new Color(0.102f, 0.129f, 0.184f, 0.92f));
        if (cardRect != null)
        {
            SetRect(cardRect, new Vector2(0f, -96f), new Vector2(820f, 500f));
            cardRect.SetSiblingIndex(1);
        }

        if (listRect != null)
            SetRect(listRect, new Vector2(0f, -96f), new Vector2(780f, 448f));

        if (scrollRect != null)
        {
            Image scrollImage = scrollRect.GetComponent<Image>();
            if (scrollImage != null)
                scrollImage.color = new Color(0.075f, 0.094f, 0.133f, 0.68f);

            if (scrollRect.viewport != null)
            {
                Image viewportImage = scrollRect.viewport.GetComponent<Image>();
                if (viewportImage != null)
                    viewportImage.color = new Color(0.075f, 0.094f, 0.133f, 0.42f);
            }

            if (scrollRect.verticalScrollbar != null)
            {
                Image scrollbarImage = scrollRect.verticalScrollbar.GetComponent<Image>();
                if (scrollbarImage != null)
                    scrollbarImage.color = new Color(0.12f, 0.15f, 0.21f, 1f);

                Image handleImage = scrollRect.verticalScrollbar.handleRect != null
                    ? scrollRect.verticalScrollbar.handleRect.GetComponent<Image>()
                    : null;
                if (handleImage != null)
                    handleImage.color = new Color(0.72f, 0.74f, 0.78f, 1f);
            }
        }

        RectTransform contentRect = playerListContainer as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
        }

        foreach (LayoutGroup layout in playerListContainer.GetComponents<LayoutGroup>())
            layout.enabled = layout is VerticalLayoutGroup;

        VerticalLayoutGroup vertical = playerListContainer.GetComponent<VerticalLayoutGroup>();
        if (vertical == null)
            vertical = playerListContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        vertical.enabled = true;
        vertical.padding = new RectOffset(16, 16, 16, 16);
        vertical.spacing = 10f;
        vertical.childAlignment = TextAnchor.UpperCenter;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        ContentSizeFitter fitter = playerListContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = playerListContainer.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void ConfigureSkipButton()
    {
        if (skipButton == null) return;

        RectTransform skipRect = skipButton.transform as RectTransform;
        if (skipRect != null)
            SetRect(skipRect, new Vector2(0f, -372f), new Vector2(320f, 58f));

        Image skipImage = skipButton.GetComponent<Image>();
        if (skipImage != null)
            skipImage.color = skipButton.interactable
                ? new Color(0.353f, 0.290f, 0.165f, 1f)
                : new Color(0.165f, 0.165f, 0.208f, 1f);

        ColorBlock colors = skipButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.94f, 0.72f, 1f);
        colors.pressedColor = new Color(0.84f, 0.74f, 0.50f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.72f);
        skipButton.colors = colors;

        if (skipButtonText != null)
        {
            ApplyChineseFont(skipButtonText);
            skipButtonText.fontSize = 22f;
            skipButtonText.color = Color.white;
            skipButtonText.alignment = TextAlignmentOptions.Center;
            skipButtonText.overflowMode = TextOverflowModes.Truncate;
        }
    }

    void StylePlayerButton(GameObject btnObj, Button btn, TextMeshProUGUI btnText, int playerIndex, bool inContact, bool isSelf, bool alreadyContacted, bool canClick)
    {
        LayoutElement layoutElement = btnObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = btnObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 58f;
        layoutElement.preferredHeight = 58f;

        Image img = btnObj.GetComponent<Image>();
        if (img != null)
        {
            if (playerIndex == GameManager.Instance.humanPlayerIndex)
                img.color = new Color(0.16f, 0.36f, 0.25f, 1f);
            else if (isSelf)
                img.color = new Color(0.165f, 0.333f, 0.502f, 1f);
            else if (canClick)
                img.color = new Color(0.22f, 0.29f, 0.42f, 1f);
            else if (alreadyContacted)
                img.color = new Color(0.353f, 0.290f, 0.165f, 1f);
            else if (inContact)
                img.color = new Color(0.13f, 0.15f, 0.19f, 1f);
            else
                img.color = new Color(0.18f, 0.227f, 0.322f, 1f);
        }

        if (btn != null)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.94f, 0.72f, 1f);
            colors.pressedColor = new Color(0.78f, 0.84f, 0.95f, 1f);
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.06f;
            btn.colors = colors;
        }

        if (btnText != null)
        {
            ApplyChineseFont(btnText);
            btnText.fontSize = 22f;
            btnText.color = canClick ? Color.white : new Color(0.78f, 0.82f, 0.88f, 1f);
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.textWrappingMode = TextWrappingModes.Normal;
            btnText.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    void ConfigureDiscussionLayout()
    {
        if (discussionPanel == null) return;

        ResolveDiscussionReferences();

        Image panelImage = discussionPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(0.025f, 0.034f, 0.052f, 0.76f);
            panelImage.raycastTarget = false;
        }

        RectTransform panelRect = discussionPanel.transform as RectTransform;
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        LayoutGroup panelLayout = discussionPanel.GetComponent<LayoutGroup>();
        if (panelLayout != null)
            panelLayout.enabled = false;

        ContentSizeFitter panelFitter = discussionPanel.GetComponent<ContentSizeFitter>();
        if (panelFitter != null)
            panelFitter.enabled = false;

        RectTransform cardRect = EnsureDiscussionCard("DiscussionCard", ref discussionCardImage, new Color(0.102f, 0.129f, 0.184f, 0.98f));
        if (cardRect != null)
        {
            SetRect(cardRect, new Vector2(0f, 0f), new Vector2(680f, 620f));
            cardRect.SetAsFirstSibling();
        }

        StyleGameText(discussionTitleText, 33f, new Color(0.965f, 0.82f, 0.32f, 1f), TextAlignmentOptions.Center, new Vector2(0f, 238f), new Vector2(600f, 50f));
        StyleGameText(discussionPromptText, 24f, new Color(0.88f, 0.91f, 0.96f, 1f), TextAlignmentOptions.Center, new Vector2(0f, 178f), new Vector2(560f, 76f));

        if (discussionTitleText != null && discussionTitleText.text == "New Text")
            discussionTitleText.text = "讨论阶段";
        if (discussionPromptText != null && discussionPromptText.text == "New Text")
            discussionPromptText.text = "正在整理本回合发言...";

        ConfigureDiscussionMessagesArea();
        ConfigureHumanDiscussionControls();
        ConfigureDiscussionEndButton();
        DisableNonButtonRaycasts(discussionPanel.transform);
        RefreshDiscussionButtonVisibility();
    }

    void DisableNonButtonRaycasts(Transform root)
    {
        if (root == null) return;

        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.GetComponent<Button>() != null)
                continue;
            if (graphic.GetComponentInParent<Button>() != null)
            {
                graphic.raycastTarget = false;
                continue;
            }
            if (graphic.GetComponent<Scrollbar>() != null)
                continue;

            graphic.raycastTarget = false;
        }
    }

    void RefreshDiscussionButtonVisibility()
    {
        bool showInput = GetInteractiveDiscussionSpeaker() >= 0;
        bool showEnd = CanShowDiscussionEndButton();

        if (humanDiscussionInputPanel != null)
            humanDiscussionInputPanel.SetActive(showInput);
        if (!showInput)
            HideDiscussionChoiceContainer();
        if (btnEndDiscussion != null)
            btnEndDiscussion.gameObject.SetActive(showEnd);
    }

    void ResolveDiscussionReferences()
    {
        if (discussionPanel == null) return;

        Transform title = FindChildRecursive(discussionPanel.transform, "TitleText");
        if (title != null)
            discussionTitleText = title.GetComponent<TextMeshProUGUI>();

        Transform prompt = FindChildRecursive(discussionPanel.transform, "PromptText");
        if (prompt != null)
            discussionPromptText = prompt.GetComponent<TextMeshProUGUI>();

        Transform messageContent = FindChildRecursive(discussionPanel.transform, "Content");
        if (messageContent != null && messageContent.GetComponentInParent<ScrollRect>() != null)
            discussionMessageContainer = messageContent;

        Transform inputPanel = FindChildRecursive(discussionPanel.transform, "HumanInputPanel");
        if (inputPanel != null)
            humanDiscussionInputPanel = inputPanel.gameObject;

        btnAnnounceScore = ResolveButton("BtnAnnounceScore", btnAnnounceScore);
        btnAccuse = ResolveButton("BtnAccuse", btnAccuse);
        btnDefend = ResolveButton("BtnDefend", btnDefend);
        btnEndDiscussion = ResolveButton("BtnEndDiscussion", btnEndDiscussion);

        Transform targetContainer = FindChildRecursive(discussionPanel.transform, "TargetContainer");
        if (targetContainer != null)
            accuseTargetContainer = targetContainer;
    }

    Button ResolveButton(string objectName, Button fallback)
    {
        if (discussionPanel == null) return fallback;
        Transform target = FindChildRecursive(discussionPanel.transform, objectName);
        Button button = target != null ? target.GetComponent<Button>() : null;
        return button != null ? button : fallback;
    }

    RectTransform EnsureDiscussionCard(string objectName, ref Image cachedImage, Color color)
    {
        Transform existing = discussionPanel.transform.Find(objectName);
        if (existing != null)
        {
            cachedImage = existing.GetComponent<Image>();
            if (cachedImage != null)
            {
                cachedImage.color = color;
                cachedImage.raycastTarget = false;
            }
            return existing as RectTransform;
        }

        GameObject card = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(discussionPanel.transform, false);
        cachedImage = card.GetComponent<Image>();
        cachedImage.color = color;
        cachedImage.raycastTarget = false;
        return card.GetComponent<RectTransform>();
    }

    void ConfigureDiscussionMessagesArea()
    {
        if (discussionMessageContainer == null) return;

        ScrollRect scrollRect = discussionMessageContainer.GetComponentInParent<ScrollRect>();
        RectTransform messagesRect = scrollRect != null
            ? scrollRect.transform as RectTransform
            : discussionMessageContainer as RectTransform;

        RectTransform cardRect = EnsureDiscussionCard("DiscussionMessagesCard", ref discussionMessagesCardImage, new Color(0.075f, 0.094f, 0.133f, 0.92f));
        if (cardRect != null)
        {
            SetRect(cardRect, new Vector2(0f, -20f), new Vector2(580f, 300f));
            cardRect.SetSiblingIndex(1);
        }

        if (messagesRect != null)
            SetRect(messagesRect, new Vector2(0f, -20f), new Vector2(540f, 260f));

        if (scrollRect != null)
        {
            Image scrollImage = scrollRect.GetComponent<Image>();
            if (scrollImage != null)
            {
                scrollImage.color = new Color(0.075f, 0.094f, 0.133f, 0.72f);
                scrollImage.raycastTarget = false;
            }

            if (scrollRect.viewport != null)
            {
                Image viewportImage = scrollRect.viewport.GetComponent<Image>();
                if (viewportImage != null)
                {
                    viewportImage.color = new Color(0.075f, 0.094f, 0.133f, 0.48f);
                    viewportImage.raycastTarget = false;
                }
            }

            if (scrollRect.verticalScrollbar != null)
            {
                Image scrollbarImage = scrollRect.verticalScrollbar.GetComponent<Image>();
                if (scrollbarImage != null)
                    scrollbarImage.color = new Color(0.12f, 0.15f, 0.21f, 1f);

                Image handleImage = scrollRect.verticalScrollbar.handleRect != null
                    ? scrollRect.verticalScrollbar.handleRect.GetComponent<Image>()
                    : null;
                if (handleImage != null)
                    handleImage.color = new Color(0.72f, 0.74f, 0.78f, 1f);
            }
        }

        RectTransform contentRect = discussionMessageContainer as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
        }

        foreach (LayoutGroup layout in discussionMessageContainer.GetComponents<LayoutGroup>())
            layout.enabled = layout is VerticalLayoutGroup;

        VerticalLayoutGroup vertical = discussionMessageContainer.GetComponent<VerticalLayoutGroup>();
        if (vertical == null)
            vertical = discussionMessageContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        vertical.enabled = true;
        vertical.padding = new RectOffset(14, 14, 12, 12);
        vertical.spacing = 8f;
        vertical.childAlignment = TextAnchor.UpperLeft;
        vertical.childControlWidth = true;
        vertical.childControlHeight = true;
        vertical.childForceExpandWidth = true;
        vertical.childForceExpandHeight = false;

        ContentSizeFitter fitter = discussionMessageContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = discussionMessageContainer.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    void ConfigureHumanDiscussionControls()
    {
        if (humanDiscussionInputPanel == null) return;

        int interactiveSpeaker = GetInteractiveDiscussionSpeaker();
        humanDiscussionInputPanel.SetActive(interactiveSpeaker >= 0);

        RectTransform inputRect = humanDiscussionInputPanel.transform as RectTransform;
        if (inputRect != null)
            SetRect(inputRect, new Vector2(0f, -214f), new Vector2(580f, 112f));

        LayoutGroup inputLayout = humanDiscussionInputPanel.GetComponent<LayoutGroup>();
        if (inputLayout != null)
            inputLayout.enabled = false;

        Image inputImage = humanDiscussionInputPanel.GetComponent<Image>();
        if (inputImage != null)
            inputImage.raycastTarget = false;

        StyleDiscussionButton(btnAnnounceScore, "公开分数变化", new Vector2(-190f, 24f), 176f, new Color(0.16f, 0.36f, 0.25f, 1f));
        StyleDiscussionButton(btnAccuse, "指控玩家", new Vector2(0f, 24f), 176f, new Color(0.46f, 0.20f, 0.16f, 1f));
        StyleDiscussionButton(btnDefend, "信任玩家", new Vector2(190f, 24f), 176f, new Color(0.165f, 0.333f, 0.502f, 1f));
        BindDiscussionInputButtons();

        if (accuseTargetContainer != null)
        {
            RectTransform targetRect = accuseTargetContainer as RectTransform;
            if (targetRect != null)
                SetRect(targetRect, new Vector2(0f, -42f), new Vector2(580f, 54f));
        }
    }

    void ConfigureDiscussionEndButton()
    {
        StyleDiscussionButton(btnEndDiscussion, "下一回合", new Vector2(0f, -250f), 260f, new Color(0.16f, 0.36f, 0.25f, 1f));
        BindDiscussionEndButton();
        if (btnEndDiscussion != null)
            btnEndDiscussion.gameObject.SetActive(CanShowDiscussionEndButton());
    }

    int GetInteractiveDiscussionSpeaker()
    {
        if (activeHumanDiscussionSpeaker >= 0)
            return activeHumanDiscussionSpeaker;

        if (GameManager.Instance == null || GameManager.Instance.GetPhase() != RoundPhase.Discussion)
            return -1;

        if (GameManager.Instance.IsDiscussionComplete())
            return -1;

        int speaker = GameManager.Instance.GetDiscussionSpeaker();
        return GameManager.Instance.IsHumanPlayer(speaker) ? speaker : -1;
    }

    bool CanShowDiscussionEndButton()
    {
        return canConfirmDiscussionEnd;
    }

    void BindDiscussionInputButtons()
    {
        if (btnAnnounceScore != null)
        {
            btnAnnounceScore.onClick.RemoveAllListeners();
            btnAnnounceScore.onClick.AddListener(OnClickAnnounceScore);
            btnAnnounceScore.interactable = GetInteractiveDiscussionSpeaker() >= 0;
        }

        if (btnAccuse != null)
        {
            btnAccuse.onClick.RemoveAllListeners();
            btnAccuse.onClick.AddListener(() => OnClickTargetDiscussion(true));
            btnAccuse.interactable = GetInteractiveDiscussionSpeaker() >= 0;
        }

        if (btnDefend != null)
        {
            btnDefend.onClick.RemoveAllListeners();
            btnDefend.onClick.AddListener(() => OnClickTargetDiscussion(false));
            btnDefend.interactable = GetInteractiveDiscussionSpeaker() >= 0;
        }
    }

    void BindDiscussionEndButton()
    {
        if (btnEndDiscussion == null) return;
        btnEndDiscussion.onClick.RemoveAllListeners();
        btnEndDiscussion.onClick.AddListener(OnClickEndDiscussion);
        btnEndDiscussion.interactable = CanShowDiscussionEndButton();
    }

    void StyleDiscussionButton(Button button, string label, Vector2 position, float width, Color color)
    {
        if (button == null) return;

        button.transform.SetAsLastSibling();

        RectTransform rect = button.transform as RectTransform;
        if (rect != null)
            SetRect(rect, position, new Vector2(width, 52f));

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = color;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.94f, 0.72f, 1f);
        colors.pressedColor = new Color(0.84f, 0.74f, 0.50f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.66f);
        colors.fadeDuration = 0.06f;
        button.colors = colors;

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            ApplyChineseFont(text);
            text.text = label;
            text.fontSize = 20f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Truncate;
        }
    }

    void StyleDynamicDiscussionButton(GameObject btnObj, string label)
    {
        if (btnObj == null) return;

        LayoutElement layoutElement = btnObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = btnObj.AddComponent<LayoutElement>();
        layoutElement.minWidth = 92f;
        layoutElement.preferredWidth = 104f;
        layoutElement.minHeight = 44f;
        layoutElement.preferredHeight = 44f;

        Image image = btnObj.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.18f, 0.227f, 0.322f, 1f);
            image.raycastTarget = true;
        }

        Button button = btnObj.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = true;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.94f, 0.72f, 1f);
            colors.pressedColor = new Color(0.84f, 0.74f, 0.50f, 1f);
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.06f;
            button.colors = colors;
        }

        TextMeshProUGUI text = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            ApplyChineseFont(text);
            text.text = label;
            text.fontSize = 19f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    Button CreateDiscussionChoiceButton(string label, UnityEngine.Events.UnityAction onClick)
    {
        if (discussionTargetButtonPrefab == null || accuseTargetContainer == null) return null;

        GameObject btnObj = Instantiate(discussionTargetButtonPrefab, accuseTargetContainer);
        btnObj.SetActive(true);
        StyleDynamicDiscussionButton(btnObj, label);

        Button button = btnObj.GetComponent<Button>();
        if (button == null)
            button = btnObj.GetComponentInChildren<Button>(true);

        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }

        return button;
    }

    void ClearDiscussionChoiceContainer()
    {
        if (accuseTargetContainer == null) return;

        foreach (Transform child in accuseTargetContainer)
            Destroy(child.gameObject);
        accuseTargetContainer.gameObject.SetActive(true);
    }

    void LayoutDiscussionChoiceButtons()
    {
        if (accuseTargetContainer == null) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(accuseTargetContainer as RectTransform);
    }

    void HideDiscussionChoiceContainer()
    {
        if (accuseTargetContainer != null)
            accuseTargetContainer.gameObject.SetActive(false);
    }

    IEnumerator RunOneTestGame()
    {
        while (GameManager.Instance.GetPhase() != RoundPhase.GameOver)
        {
            var phase = GameManager.Instance.GetPhase();

            if (phase == RoundPhase.Discussion)
            {
                yield return StartCoroutine(RunDiscussionPhaseTest());
                continue;
            }

            if (phase == RoundPhase.ScoreGuessing)
            {
                yield return StartCoroutine(RunScoreGuessingPhaseTest());
                continue;
            }

            if (phase == RoundPhase.RoundEnd)
            {
                GameManager.Instance.StartNewRound();
                yield return null;
                continue;
            }

            if (phase == RoundPhase.Selecting)
            {
                yield return StartCoroutine(AISelectTargetTest());
            }
            else if (phase == RoundPhase.Responding)
            {
                yield return StartCoroutine(AIRespondTest());
            }
            else
            {
                yield return null;
            }
        }
    }

    // ─── 刷新整体界面 ─────────────────────────────
    void RefreshGameUI()
    {
        var phase = GameManager.Instance.GetPhase();
        if (phase == RoundPhase.RoundEnd || phase == RoundPhase.GameOver) return;

        int round = GameManager.Instance.GetCurrentRound();
        if (roundText != null)
        {
            string prefix = GameManager.Instance.isTestMode
                ? (isBatchRunning ? $"[批量测试 剩余{batchRemaining}局] " : "[单局测试] ")
                : "";
            roundText.text = $"{prefix}第 {round + 1} 回合（共 {GameManager.Instance.maxRounds} 回合）";
        }

        int turn = GameManager.Instance.GetCurrentPlayerTurn();
        var players = GameManager.Instance.GetPlayers();
        bool isHumanTurn = GameManager.Instance.IsHumanPlayer(turn);

        if (turnInfoText != null)
            turnInfoText.text = isHumanTurn
                ? "你的回合 - 选择一名玩家发起接触"
                : $"{players[turn].playerName} 正在行动...";

        if (messageText != null)
            messageText.text = isHumanTurn
                ? "请点击一名玩家发起接触，或点击放弃"
                : "请等待...";

        ConfigureGameLayout();
        UpdateSkipButton(isHumanTurn, turn);
        RebuildPlayerList();

        if (!isHumanTurn)
            EnsureAutoAdvanceRunning();
    }

    void UpdateSkipButton(bool isHumanTurn, int humanTurn)
    {
        if (skipButton == null) return;

        bool canSkip = GameManager.Instance.CanPlayerSkip(humanTurn);
        skipButton.gameObject.SetActive(isHumanTurn);
        skipButton.interactable = canSkip;

        if (skipButtonText != null)
            skipButtonText.text = canSkip ? "放弃本回合接触" : "不能连续放弃/拒绝";

        ConfigureSkipButton();
    }

    // ─── 重建玩家按钮列表 ─────────────────────────
    void RebuildPlayerList()
    {
        if (playerListContainer == null || playerButtonPrefab == null) return;

        ConfigurePlayerListArea();

        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);
        playerButtons.Clear();

        var players = GameManager.Instance.GetPlayers();
        int humanIdx = GameManager.Instance.humanPlayerIndex;
        int currentTurn = GameManager.Instance.GetCurrentPlayerTurn();
        bool isHumanTurn = GameManager.Instance.IsHumanPlayer(currentTurn);
        bool isSelecting = GameManager.Instance.GetPhase() == RoundPhase.Selecting;
        var availableTargets = isHumanTurn
            ? GameManager.Instance.GetAvailableTargets(currentTurn)
            : new List<int>();

        for (int i = 0; i < players.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(playerButtonPrefab, playerListContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            string label = players[i].GetDisplayInfo();
            if (i == humanIdx) label += "（你）";
            if (btnText != null)
                btnText.text = label;

            bool inContact = GameManager.Instance.IsPlayerInContact(i);
            bool isSelf = (i == currentTurn);
            bool alreadyContacted = isHumanTurn
                && players[humanIdx].contactedTargets.Contains(i);

            bool canClick = isHumanTurn && isSelecting && !inContact && !isSelf && !alreadyContacted;

            if (btn != null)
            {
                btn.interactable = canClick;
                btn.onClick.AddListener(() => OnPlayerClicked(index));
                playerButtons.Add(btn);
            }

            StylePlayerButton(btnObj, btn, btnText, i, inContact, isSelf, alreadyContacted, canClick);
        }
    }

    // ─── 人类玩家点击目标 ─────────────────────────
    void OnPlayerClicked(int targetIndex)
    {
        if (GameManager.Instance.GetPhase() != RoundPhase.Selecting) return;
        int currentTurn = GameManager.Instance.GetCurrentPlayerTurn();
        if (!GameManager.Instance.IsHumanPlayer(currentTurn)) return;

        bool success = GameManager.Instance.InitiateContact(currentTurn, targetIndex);
        if (!success) return;

        var players = GameManager.Instance.GetPlayers();
        RebuildPlayerList();
        if (skipButton != null) skipButton.gameObject.SetActive(false);
        if (messageText != null)
            messageText.text = $"你选择了 {players[targetIndex].playerName}，等待对方决定...";

        if (!GameManager.Instance.IsHumanPlayer(targetIndex))
            EnsureAutoAdvanceRunning();
        else
            ShowHumanResponsePanel(currentTurn, targetIndex);
    }

    // ─── 人类玩家放弃本回合 ───────────────────────
    public void OnSkipTurn()
    {
        int currentTurn = GameManager.Instance.GetCurrentPlayerTurn();
        if (!GameManager.Instance.IsHumanPlayer(currentTurn)) return;
        if (!GameManager.Instance.CanPlayerSkip(currentTurn)) return;

        if (skipButton != null) skipButton.gameObject.SetActive(false);
        string result = GameManager.Instance.SkipTurn(currentTurn);
        HandlePhaseAfterResponse(result);
    }

    // ─── 人类被AI选中时的弹窗 ─────────────────────
    void ShowHumanResponsePanel(int contactorIndex, int targetIndex)
    {
        var players = GameManager.Instance.GetPlayers();
        bool canReject = GameManager.Instance.CanPlayerReject(targetIndex);

        SetResponsePanelActive(true);

        if (responseText != null)
        {
            if (canReject)
                responseText.text = $"{players[contactorIndex].playerName} 想与你接触\n你是否接受？";
            else
                responseText.text = $"{players[contactorIndex].playerName} 想与你接触\n上回合已拒绝，本回合必须接受！";
        }

        if (rejectButton != null)
            rejectButton.interactable = canReject;
    }

    public void OnAccept()
    {
        SetResponsePanelActive(false);
        string result = GameManager.Instance.RespondToContact(true);
        HandlePhaseAfterResponse(result);
    }

    public void OnReject()
    {
        SetResponsePanelActive(false);
        string result = GameManager.Instance.RespondToContact(false);
        HandlePhaseAfterResponse(result);
    }

    void HandlePhaseAfterResponse(string resultMsg)
    {
        var phase = GameManager.Instance.GetPhase();
        if (messageText != null) messageText.text = resultMsg;

        if (phase == RoundPhase.GameOver)
        {
            if (GameManager.Instance.isTestMode)
                ShowTestResult();
            else
                ShowEndGame();
        }
        else if (phase == RoundPhase.ScoreGuessing)
        {
            RebuildPlayerList();
            StartScoreGuessingRoutine();
        }
        else if (phase == RoundPhase.Discussion)
        {
            RebuildPlayerList();
            StartDiscussionRoutine();
        }
        else if (phase == RoundPhase.RoundEnd)
        {
            // RoundEnd 现在只在 GameOver 前不经过讨论的情况下触发（理论上不会出现）
            GameManager.Instance.StartNewRound();
            RefreshGameUI();
        }
        else
        {
            RefreshGameUI();
            EnsureAutoAdvanceRunning();
        }
    }

    void StartDiscussionRoutine()
    {
        if (discussionRoutine != null) return;
        StopAutoAdvanceRoutine();
        discussionRoutine = StartCoroutine(RunDiscussionPhase());
    }

    void StartScoreGuessingRoutine()
    {
        if (scoreGuessingRoutine != null) return;
        StopAutoAdvanceRoutine();
        scoreGuessingRoutine = StartCoroutine(RunScoreGuessingPhase());
    }

    void EnsureAutoAdvanceRunning()
    {
        if (autoAdvanceRoutine != null) return;
        if (GameManager.Instance == null) return;

        RoundPhase phase = GameManager.Instance.GetPhase();
        if (phase == RoundPhase.Selecting)
        {
            int turn = GameManager.Instance.GetCurrentPlayerTurn();
            if (GameManager.Instance.IsHumanPlayer(turn)) return;
        }
        else if (phase == RoundPhase.Responding)
        {
            int target = GameManager.Instance.GetPendingTarget();
            if (GameManager.Instance.IsHumanPlayer(target)) return;
        }
        else
        {
            return;
        }

        autoAdvanceRoutine = StartCoroutine(AutoAdvanceAIPlayers());
    }

    void StopAutoAdvanceRoutine()
    {
        if (autoAdvanceRoutine == null) return;
        StopCoroutine(autoAdvanceRoutine);
        autoAdvanceRoutine = null;
    }

    IEnumerator AutoAdvanceAIPlayers()
    {
        while (GameManager.Instance != null)
        {
            RoundPhase phase = GameManager.Instance.GetPhase();

            if (phase == RoundPhase.Selecting)
            {
                int turn = GameManager.Instance.GetCurrentPlayerTurn();
                if (GameManager.Instance.IsHumanPlayer(turn))
                    break;

                yield return StartCoroutine(AISelectTarget());
                continue;
            }

            if (phase == RoundPhase.Responding)
            {
                int target = GameManager.Instance.GetPendingTarget();
                if (GameManager.Instance.IsHumanPlayer(target))
                    break;

                yield return StartCoroutine(AIRespond(target));
                continue;
            }

            if (phase == RoundPhase.Discussion)
            {
                StartDiscussionRoutine();
                break;
            }

            if (phase == RoundPhase.ScoreGuessing)
            {
                StartScoreGuessingRoutine();
                break;
            }

            if (phase == RoundPhase.GameOver)
            {
                ShowEndGame();
                break;
            }

            break;
        }

        autoAdvanceRoutine = null;
    }

    // ─── 测试模式单局结束 ─────────────────────────
    void ShowTestResult()
    {
        gamePanel.SetActive(false);
        testMenuPanel.SetActive(true);
        RefreshTestStats();
    }

    // ─── 讨论阶段（正常模式）─────────────────────
    IEnumerator RunDiscussionPhase()
    {
        SetDiscussionPanelActive(true);
        ConfigureDiscussionLayout();
        ClearDiscussionMessages();
        activeHumanDiscussionSpeaker = -1;
        canConfirmDiscussionEnd = false;
        if (humanDiscussionInputPanel != null) humanDiscussionInputPanel.SetActive(false);
        if (btnEndDiscussion != null) btnEndDiscussion.gameObject.SetActive(false);
        HideDiscussionChoiceContainer();
        if (discussionChoiceContainer != null)
            discussionChoiceContainer.SetAsLastSibling();

        int round = GameManager.Instance.GetCurrentRound();
        if (discussionTitleText != null)
            discussionTitleText.text = $"第 {round} 回合 · 讨论阶段";

        while (!GameManager.Instance.IsDiscussionComplete())
        {
            int speaker = GameManager.Instance.GetDiscussionSpeaker();
            bool isHuman = GameManager.Instance.IsHumanPlayer(speaker);

            if (isHuman)
            {
                if (discussionPromptText != null)
                    discussionPromptText.text = "轮到你发言，请选择发布的信息类型：";
                yield return StartCoroutine(WaitForHumanDiscussionInput(speaker));
            }
            else
            {
                activeHumanDiscussionSpeaker = -1;
                canConfirmDiscussionEnd = false;
                if (humanDiscussionInputPanel != null) humanDiscussionInputPanel.SetActive(false);
                if (btnEndDiscussion != null) btnEndDiscussion.gameObject.SetActive(false);
                HideDiscussionChoiceContainer();
                if (discussionPromptText != null)
                    discussionPromptText.text = $"{GameManager.Instance.GetPlayers()[speaker].playerName} 正在发言...";
                yield return new WaitForSeconds(aiActionDelay * 0.6f);
                var msg = GameManager.Instance.AIGenerateMessage(speaker);
                GameManager.Instance.SubmitDiscussionMessage(msg);
                AppendDiscussionMessage(msg.displayText);
            }
        }

        if (humanDiscussionInputPanel != null) humanDiscussionInputPanel.SetActive(false);
        HideDiscussionChoiceContainer();
        if (discussionPromptText != null) discussionPromptText.text = "所有人已发言，点击按钮进入下一回合。";
        activeHumanDiscussionSpeaker = -1;
        canConfirmDiscussionEnd = true;
        ConfigureDiscussionLayout();

        discussionEndConfirmed = false;
        if (btnEndDiscussion != null)
        {
            btnEndDiscussion.gameObject.SetActive(true);
            BindDiscussionEndButton();
        }
        else
        {
            discussionEndConfirmed = true;
        }
        yield return new WaitUntil(() => discussionEndConfirmed);
        if (btnEndDiscussion != null) btnEndDiscussion.gameObject.SetActive(false);

        SetDiscussionPanelActive(false);
        GameManager.Instance.FinishDiscussion();
        GameManager.Instance.StartNewRound();
        discussionRoutine = null;
        RefreshGameUI();
    }

    IEnumerator WaitForHumanDiscussionInput(int speakerId)
    {
        humanDiscussionSubmitted = false;
        humanPendingMessage = null;
        activeHumanDiscussionSpeaker = speakerId;
        canConfirmDiscussionEnd = false;

        ConfigureDiscussionLayout();
        if (humanDiscussionInputPanel != null) humanDiscussionInputPanel.SetActive(true);
        if (btnEndDiscussion != null) btnEndDiscussion.gameObject.SetActive(false);
        HideDiscussionChoiceContainer();
        BindDiscussionInputButtons();

        yield return new WaitUntil(() => humanDiscussionSubmitted);

        GameManager.Instance.SubmitDiscussionMessage(humanPendingMessage);
        AppendDiscussionMessage(humanPendingMessage.displayText);
        activeHumanDiscussionSpeaker = -1;
        if (humanDiscussionInputPanel != null) humanDiscussionInputPanel.SetActive(false);
        HideDiscussionChoiceContainer();
    }

    public void OnClickAnnounceScore()
    {
        int speaker = GetInteractiveDiscussionSpeaker();
        Debug.Log($"[UI] Click announce score, speaker={speaker}");
        if (speaker < 0) return;
        activeHumanDiscussionSpeaker = speaker;
        OnHumanAnnounceScore(speaker);
    }

    public void OnClickTargetDiscussion(bool isAccuse)
    {
        int speaker = GetInteractiveDiscussionSpeaker();
        Debug.Log($"[UI] Click discussion target, accuse={isAccuse}, speaker={speaker}");
        if (speaker < 0) return;
        activeHumanDiscussionSpeaker = speaker;
        ShowTargetListForHuman(speaker, isAccuse);
    }

    public void OnClickEndDiscussion()
    {
        bool canConfirm = CanShowDiscussionEndButton();
        Debug.Log($"[UI] Click end discussion, canConfirm={canConfirm}");
        if (!canConfirm) return;
        discussionEndConfirmed = true;
    }

    void OnHumanAnnounceScore(int speakerId)
    {
        ClearDiscussionChoiceContainer();

        for (int delta = -2; delta <= 2; delta++)
        {
            int d = delta;
            string label = d >= 0 ? $"+{d}" : $"{d}";
            CreateDiscussionChoiceButton(label, () =>
            {
                Debug.Log($"[UI] Click score announce choice {d}, speaker={speakerId}");
                SubmitHumanScoreAnnounce(speakerId, d);
            });
        }

        LayoutDiscussionChoiceButtons();
        Canvas.ForceUpdateCanvases();
    }

    void SubmitHumanScoreAnnounce(int speakerId, int claimedDelta)
    {
        var players = GameManager.Instance.GetPlayers();
        string sign = claimedDelta >= 0 ? "+" : "";
        humanPendingMessage = new DiscussionMessage
        {
            speakerId = speakerId,
            type = DiscussionMessageType.AnnounceScore,
            targetId = -1,
            claimedScoreChange = claimedDelta,
            displayText = $"[{players[speakerId].playerName}] 本回合分数变化：{sign}{claimedDelta}"
        };
        humanDiscussionSubmitted = true;
    }

    void ShowTargetListForHuman(int speakerId, bool isAccuse)
    {
        ClearDiscussionChoiceContainer();

        var players = GameManager.Instance.GetPlayers();
        foreach (var p in players)
        {
            if (p.playerId == speakerId) continue;
            int targetId = p.playerId;
            CreateDiscussionChoiceButton(p.playerName, () =>
            {
                Debug.Log($"[UI] Click discussion target {targetId}, accuse={isAccuse}, speaker={speakerId}");
                humanPendingMessage = new DiscussionMessage
                {
                    speakerId = speakerId,
                    type = isAccuse ? DiscussionMessageType.Accuse : DiscussionMessageType.Defend,
                    targetId = targetId,
                    claimedScoreChange = 0,
                    displayText = isAccuse
                        ? $"[{players[speakerId].playerName}] 指控 {players[targetId].playerName} 是坏人！"
                        : $"[{players[speakerId].playerName}] 认为 {players[targetId].playerName} 是好人。"
                };
                humanDiscussionSubmitted = true;
            });
        }

        LayoutDiscussionChoiceButtons();
        Canvas.ForceUpdateCanvases();
    }

    void AppendDiscussionMessage(string text)
    {
        if (discussionMessageContainer == null || discussionMessagePrefab == null) return;
        GameObject item = Instantiate(discussionMessagePrefab, discussionMessageContainer);
        var tmp = item.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            ApplyChineseFont(tmp);
            tmp.text = text;
            tmp.fontSize = 22f;
            tmp.color = new Color(0.92f, 0.94f, 0.98f, 1f);
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
        }

        Image itemImage = item.GetComponent<Image>();
        if (itemImage != null)
            itemImage.color = new Color(0.14f, 0.18f, 0.25f, 1f);

        LayoutElement layoutElement = item.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = item.AddComponent<LayoutElement>();
        layoutElement.minHeight = 42f;
        layoutElement.preferredHeight = 42f;

        Canvas.ForceUpdateCanvases();
        var scrollRect = discussionMessageContainer.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    void ClearDiscussionMessages()
    {
        if (discussionMessageContainer == null) return;
        foreach (Transform child in discussionMessageContainer)
            Destroy(child.gameObject);
    }

    void SetDiscussionPanelActive(bool active)
    {
        if (discussionPanel != null)
        {
            discussionPanel.SetActive(active);
            if (active)
            {
                ConfigureDiscussionLayout();
            if (humanDiscussionInputPanel != null) humanDiscussionInputPanel.SetActive(false);
            if (btnEndDiscussion != null) btnEndDiscussion.gameObject.SetActive(false);
            if (accuseTargetContainer != null) accuseTargetContainer.gameObject.SetActive(false);
            RefreshDiscussionButtonVisibility();
        }
    }
    }

    // ─── 讨论阶段（测试模式）─────────────────────
    IEnumerator RunDiscussionPhaseTest()
    {
        while (!GameManager.Instance.IsDiscussionComplete())
        {
            int speaker = GameManager.Instance.GetDiscussionSpeaker();
            var msg = GameManager.Instance.AIGenerateMessage(speaker);
            GameManager.Instance.SubmitDiscussionMessage(msg);
            yield return new WaitForSeconds(testAiActionDelay * 0.2f);
        }
        GameManager.Instance.FinishDiscussion();
    }

    // ─── 分数猜测阶段（正常模式）─────────────────
    IEnumerator RunScoreGuessingPhase()
    {
        if (scoreGuessingPanel != null) scoreGuessingPanel.SetActive(true);
        if (guessingResultText != null) guessingResultText.text = "";

        var players = GameManager.Instance.GetPlayers();

        while (!GameManager.Instance.IsGuessingComplete())
        {
            int guesser = GameManager.Instance.GetGuessingPlayerIndex();
            bool isHuman = GameManager.Instance.IsHumanPlayer(guesser);

            if (guessingTitleText != null)
                guessingTitleText.text = $"分数猜测 · {players[guesser].playerName}";

            if (isHuman)
                yield return StartCoroutine(WaitForHumanGuess(guesser));
            else
            {
                yield return new WaitForSeconds(aiActionDelay * 0.4f);
                if (GameManager.Instance.AIShouldGuess(guesser))
                {
                    int[] guesses = GameManager.Instance.AIGuessScores(guesser);
                    GameManager.Instance.SubmitGuess(guesser, guesses);
                }
                else
                    GameManager.Instance.SkipGuessing();
            }
        }

        string result = GameManager.Instance.GetGuessingResultText();
        if (guessingResultText != null) guessingResultText.text = result;
        yield return new WaitForSeconds(2f);

        if (scoreGuessingPanel != null) scoreGuessingPanel.SetActive(false);

        if (GameManager.Instance.isTestMode)
            ShowTestResult();
        else
            ShowEndGame();

        scoreGuessingRoutine = null;
    }

    IEnumerator WaitForHumanGuess(int guesserIndex)
    {
        if (guessingInputContainer != null)
            foreach (Transform child in guessingInputContainer) Destroy(child.gameObject);

        var players = GameManager.Instance.GetPlayers();
        var inputFields = new TMP_InputField[players.Count];

        if (guessingInputContainer != null && guessingInputRowPrefab != null)
        {
            for (int i = 0; i < players.Count; i++)
            {
                GameObject row = Instantiate(guessingInputRowPrefab, guessingInputContainer);
                var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
                if (labels.Length > 0) labels[0].text = players[i].playerName;
                inputFields[i] = row.GetComponentInChildren<TMP_InputField>();
                if (inputFields[i] != null) inputFields[i].text = "0";
            }
        }

        bool submitted = false, skipped = false;

        if (btnSubmitGuess != null)
        {
            btnSubmitGuess.onClick.RemoveAllListeners();
            btnSubmitGuess.onClick.AddListener(() => submitted = true);
        }
        if (btnSkipGuess != null)
        {
            btnSkipGuess.onClick.RemoveAllListeners();
            btnSkipGuess.onClick.AddListener(() => skipped = true);
        }

        if (guessingPromptText != null)
            guessingPromptText.text = "请为每位玩家填写你猜测的分数，然后提交：";

        yield return new WaitUntil(() => submitted || skipped);

        if (skipped)
        {
            GameManager.Instance.SkipGuessing();
        }
        else
        {
            int[] guesses = new int[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                if (inputFields[i] != null)
                    int.TryParse(inputFields[i].text, out guesses[i]);
            }
            GameManager.Instance.SubmitGuess(guesserIndex, guesses);
        }
    }

    // ─── 分数猜测阶段（测试模式）─────────────────
    IEnumerator RunScoreGuessingPhaseTest()
    {
        while (!GameManager.Instance.IsGuessingComplete())
        {
            int guesser = GameManager.Instance.GetGuessingPlayerIndex();
            if (GameManager.Instance.AIShouldGuess(guesser))
            {
                int[] guesses = GameManager.Instance.AIGuessScores(guesser);
                GameManager.Instance.SubmitGuess(guesser, guesses);
            }
            else
                GameManager.Instance.SkipGuessing();
            yield return new WaitForSeconds(testAiActionDelay * 0.1f);
        }
    }

    // ─── AI：选择目标（正常模式）─────────────────
    IEnumerator AISelectTarget()
    {
        float delay = GameManager.Instance.isTestMode ? testAiActionDelay : aiActionDelay;
        yield return new WaitForSeconds(delay);

        if (GameManager.Instance.GetPhase() != RoundPhase.Selecting) yield break;

        int currentTurn = GameManager.Instance.GetCurrentPlayerTurn();
        if (GameManager.Instance.IsHumanPlayer(currentTurn)) yield break;

        var players = GameManager.Instance.GetPlayers();
        var available = GameManager.Instance.GetAvailableTargets(currentTurn);
        bool mustAct = !GameManager.Instance.CanPlayerSkip(currentTurn);

        if (available.Count == 0)
        {
            if (messageText != null)
                messageText.text = $"{players[currentTurn].playerName} 没有可选目标，放弃本回合。";
            yield return new WaitForSeconds(delay * 0.5f);
            string skipResult = GameManager.Instance.SkipTurn(currentTurn);
            HandlePhaseAfterResponse(skipResult);
            yield break;
        }

        if (!mustAct && Random.value < randomSkipProbability)
        {
            if (messageText != null)
                messageText.text = $"{players[currentTurn].playerName} 放弃了本回合接触。";
            yield return new WaitForSeconds(delay * 0.5f);
            string skipResult = GameManager.Instance.SkipTurn(currentTurn);
            HandlePhaseAfterResponse(skipResult);
            yield break;
        }

        int targetIndex = GameManager.Instance.AIPickTarget(currentTurn, available);
        bool success = GameManager.Instance.InitiateContact(currentTurn, targetIndex);
        if (!success) yield break;

        RebuildPlayerList();
        if (messageText != null)
            messageText.text = $"{players[currentTurn].playerName} 选择了 {players[targetIndex].playerName}";

        yield return new WaitForSeconds(delay);

        if (GameManager.Instance.IsHumanPlayer(targetIndex))
            ShowHumanResponsePanel(currentTurn, targetIndex);
        else
            yield return StartCoroutine(AIRespond(targetIndex));
    }

    // ─── AI：决定接受或拒绝（正常模式）──────────
    IEnumerator AIRespond(int targetIndex)
    {
        float delay = GameManager.Instance.isTestMode ? testAiActionDelay : aiActionDelay;
        yield return new WaitForSeconds(delay);

        bool canReject = GameManager.Instance.CanPlayerReject(targetIndex);
        int contactor = GameManager.Instance.GetPendingContactor();
        bool willAccept = !canReject || GameManager.Instance.AIDecideAccept(targetIndex, contactor);

        var players = GameManager.Instance.GetPlayers();
        string action = willAccept ? "接受" : "拒绝";
        if (messageText != null)
            messageText.text = $"{players[targetIndex].playerName} {action}了接触";

        yield return new WaitForSeconds(delay * 0.5f);

        string result = GameManager.Instance.RespondToContact(willAccept);
        HandlePhaseAfterResponse(result);
    }

    // ─── AI：选择目标（测试批量模式，无UI更新）──
    IEnumerator AISelectTargetTest()
    {
        yield return new WaitForSeconds(testAiActionDelay);

        if (GameManager.Instance.GetPhase() != RoundPhase.Selecting) yield break;

        int currentTurn = GameManager.Instance.GetCurrentPlayerTurn();
        var available = GameManager.Instance.GetAvailableTargets(currentTurn);
        bool mustAct = !GameManager.Instance.CanPlayerSkip(currentTurn);

        if (available.Count == 0)
        {
            GameManager.Instance.SkipTurn(currentTurn);
            yield break;
        }

        if (!mustAct && Random.value < randomSkipProbability)
        {
            GameManager.Instance.SkipTurn(currentTurn);
            yield break;
        }

        int targetIndex = GameManager.Instance.AIPickTarget(currentTurn, available);
        bool success = GameManager.Instance.InitiateContact(currentTurn, targetIndex);
        if (!success) yield break;

        yield return new WaitForSeconds(testAiActionDelay);
        yield return StartCoroutine(AIRespondTest());
    }

    IEnumerator AIRespondTest()
    {
        yield return new WaitForSeconds(testAiActionDelay * 0.5f);

        int targetIndex = GameManager.Instance.GetPendingTarget();
        bool canReject = GameManager.Instance.CanPlayerReject(targetIndex);
        int contactor2 = GameManager.Instance.GetPendingContactor();
        bool willAccept = !canReject || GameManager.Instance.AIDecideAccept(targetIndex, contactor2);

        GameManager.Instance.RespondToContact(willAccept);
        // Discussion / RoundEnd / GameOver 由 RunOneTestGame 的主循环处理
    }

    // ─── 下一回合 ─────────────────────────────────
    public void OnNextRound()
    {
        SetRoundEndPanelActive(false);
        GameManager.Instance.StartNewRound();
        RefreshGameUI();
    }

    // ─── 游戏结束界面（正常模式）─────────────────
    void ShowEndGame()
    {
        gamePanel.SetActive(false);
        endGamePanel.SetActive(true);
        ConfigureEndGameLayout();

        var players = GameManager.Instance.GetPlayers();

        // 坏人结算不加分

        List<Player> ranked = new List<Player>(players);
        ranked.Sort((a, b) => b.score.CompareTo(a.score));

        string text = "游戏结束\n\n";
        text += GameManager.Instance.GetCampResultText() + "\n\n";
        text += "最终排名\n";
        for (int i = 0; i < ranked.Count; i++)
            text += $"第{i + 1}名  {ranked[i].GetFullInfo()}\n";

        string guessingSummary = GameManager.Instance.GetGuessingResultText();
        text += "\n猜分结果摘要\n";
        text += string.IsNullOrWhiteSpace(guessingSummary) ? "本局无猜分结果。" : guessingSummary.Trim();

        if (finalScoresText != null)
            finalScoresText.text = text;
    }

    void ConfigureEndGameLayout()
    {
        if (finalScoresText != null)
        {
            ApplyChineseFont(finalScoresText);
            finalScoresText.fontSize = 24f;
            finalScoresText.alignment = TextAlignmentOptions.TopLeft;
            finalScoresText.textWrappingMode = TextWrappingModes.Normal;
            finalScoresText.overflowMode = TextOverflowModes.Truncate;
            finalScoresText.lineSpacing = -8f;

            RectTransform textRect = finalScoresText.rectTransform;
            textRect.anchorMin = new Vector2(0.08f, 0.18f);
            textRect.anchorMax = new Vector2(0.92f, 0.92f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.pivot = new Vector2(0f, 1f);
        }

        Transform returnButton = FindChildRecursive(endGamePanel != null ? endGamePanel.transform : null, "ReturnButton");
        if (returnButton != null)
        {
            RectTransform buttonRect = returnButton as RectTransform;
            if (buttonRect != null)
            {
                buttonRect.anchorMin = new Vector2(0.5f, 0.06f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.06f);
                buttonRect.pivot = new Vector2(0.5f, 0.5f);
                buttonRect.anchoredPosition = Vector2.zero;
                buttonRect.sizeDelta = new Vector2(260f, 64f);
            }
        }
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null) return null;
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }

    public void ReturnToMainMenu()
    {
        if (identityPanel != null) identityPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);
        if (testMenuPanel != null) testMenuPanel.SetActive(false);
        if (discussionPanel != null) discussionPanel.SetActive(false);
        if (scoreGuessingPanel != null) scoreGuessingPanel.SetActive(false);
        SetResponsePanelActive(false);
        SetRoundEndPanelActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        ConfigureMainMenuLayout();
    }

    void SetResponsePanelActive(bool active)
    {
        if (responsePanel != null) responsePanel.SetActive(active);
    }

    void SetRoundEndPanelActive(bool active)
    {
        if (roundEndPanel != null) roundEndPanel.SetActive(active);
    }
}
