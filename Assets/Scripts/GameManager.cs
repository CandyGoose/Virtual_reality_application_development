using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Effects")]
    public GameObject explosionEffect;
    public Transform explosionPoint;

    [System.Serializable]
    public class Task
    {
        public string description;
        public TaskConditionType condition;
        public IngredientKind ingredientKind;
        public bool requireGround;
        [HideInInspector] public bool completed;
    }

    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public float timeLimit;
        public Task[] tasks;
    }

    [Header("Levels")]
    public LevelData[] levels;

    [Header("UI")]
    public TextMeshProUGUI boardText;
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Stability")]
    public UnityEngine.UI.Slider stabilitySlider;
    public float stability = 0f;
    public float maxStability = 100f;

    [Header("Stability Visuals")]
    public UnityEngine.UI.Image stabilityFill;

    private int currentLevelIndex = 0;
    private float timeLeft;
    private bool gameOver = false;
    public Gradient stabilityGradient;
    public bool paused = true;
private bool cauldronIsHot = true; // исходное состояние: горячо

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartLevel(0);
    }

    void Update()
    {
        if (gameOver || paused) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            OnLose();
        }

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    // ------------------- LEVELS ------------------------

    LevelData CurrentLevel => levels[currentLevelIndex];

    void StartLevel(int index)
    {
        currentLevelIndex = index;
        gameOver = false;

        foreach (var t in CurrentLevel.tasks)
            t.completed = false;

        timeLeft = CurrentLevel.timeLimit;

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        // сброс всех индикаторов у инструментов
        ResetAllToolsIndicators();

        RefreshBoard();
    }

    void ResetAllToolsIndicators()
    {
        var tools = FindObjectsOfType<Tool>();
        foreach (var tool in tools)
        {
            tool.ResetVisual();
        }
    }

    // ------------------- BOARD -------------------------

    void RefreshBoard()
    {
        if (boardText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine($"Задание {CurrentLevel.levelName}:\n");

        var tasks = CurrentLevel.tasks;
        for (int i = 0; i < tasks.Length; i++)
        {
            var t = tasks[i];
            string prefix = $"{i + 1}) ";

            if (t.completed)
                sb.AppendLine(prefix + $"<s>{t.description}</s>");
            else
                sb.AppendLine(prefix + t.description);
        }

        boardText.text = sb.ToString();
    }

    // ------------------- EVENTS -------------------------

public void OnCauldronHeated()
{
    cauldronIsHot = true;

    TryCompleteTask(TaskConditionType.HeatCauldron, IngredientKind.None, false);

    ValidatePreviousTemperatureTask();
}

public void OnCauldronCooled()
{
    cauldronIsHot = false;

    TryCompleteTask(TaskConditionType.CoolCauldron, IngredientKind.None, false);

    ValidatePreviousTemperatureTask();
}

    public void OnIngredientGround(Ingredient ingredient)
    {
        IngredientKind kind = IngredientUtils.GetKindByName(ingredient.name);
        TryCompleteTask(TaskConditionType.GrindIngredient, kind, true);
    }

    public void OnIngredientAddedToCauldron(Ingredient ingredient)
    {
        IngredientKind kind = IngredientUtils.GetKindByName(ingredient.name);
        bool isGround = ingredient.isGround;

        bool success = TryCompleteTask(TaskConditionType.AddIngredientToCauldron, kind, isGround);

        if (!success)
        {
            Debug.Log("Неверный порядок или ингредиент");
            AddInstability(30f);
        }
    }

    // ------------------- TASK CHECK ----------------------

    bool TryCompleteTask(TaskConditionType cond, IngredientKind kind, bool ground)
    {
        if (gameOver) return false;

        var tasks = CurrentLevel.tasks;

        Task currentTask = null;
        for (int i = 0; i < tasks.Length; i++)
        {
            if (!tasks[i].completed)
            {
                currentTask = tasks[i];
                break;
            }
        }

        if (currentTask == null)
            return false;

        if (currentTask.condition != cond)
            return false;

        if (currentTask.ingredientKind != IngredientKind.None &&
            currentTask.ingredientKind != kind)
            return false;

        if (currentTask.requireGround && !ground)
            return false;

        currentTask.completed = true;
        Debug.Log($"TASK COMPLETE: {currentTask.description}");

        RefreshBoard();

        if (AllTasksDone())
            OnLevelComplete();

        return true;
    }

    bool AllTasksDone()
    {
        foreach (var t in CurrentLevel.tasks)
            if (!t.completed)
                return false;

        return true;
    }

    public void AddInstability(float amount)
    {
        if (gameOver) return;

        stability += amount;
        if (stability > maxStability)
            stability = maxStability;

        if (stabilitySlider != null)
            stabilitySlider.value = stability;

        UpdateStabilityColor();

        if (stability >= maxStability)
            OnLose();
    }

    void UpdateStabilityColor()
    {
        if (stabilityFill == null || stabilitySlider == null || stabilityGradient == null)
            return;

        float t = stability / maxStability;
        stabilityFill.color = stabilityGradient.Evaluate(t);
    }

    private void ValidatePreviousTemperatureTask()
{
    if (gameOver) return;

    var tasks = CurrentLevel.tasks;

    // Находим индекс первого НЕвыполненного
    int firstNotCompleted = -1;
    for (int i = 0; i < tasks.Length; i++)
    {
        if (!tasks[i].completed)
        {
            firstNotCompleted = i;
            break;
        }
    }

    // Если всё выполнено — "прошлый пункт" = последний
    int prevIndex = (firstNotCompleted == -1) ? tasks.Length - 1 : firstNotCompleted - 1;

    if (prevIndex < 0 || prevIndex >= tasks.Length)
        return;

    var prev = tasks[prevIndex];

    // Нас интересуют только пункты про температуру
    if (prev.condition != TaskConditionType.HeatCauldron &&
        prev.condition != TaskConditionType.CoolCauldron)
        return;

    // Проверяем соответствие текущему состоянию котла
    bool shouldBeHot = (prev.condition == TaskConditionType.HeatCauldron);

    if (cauldronIsHot != shouldBeHot)
    {
        prev.completed = false;
        Debug.Log($"TEMP TASK ROLLBACK: {prev.description}");

        RefreshBoard();
    }
}


    void OnLevelComplete()
    {
        Debug.Log("LEVEL COMPLETE!");

        if (currentLevelIndex < levels.Length - 1)
        {
            AudioManager.Instance?.PlayWin();
            StartLevel(currentLevelIndex + 1);
        }
        else
        {
            OnWin();
        }
    }

    void OnWin()
    {
        Debug.Log("GAME WON!");
        gameOver = true;

        if (winPanel) winPanel.SetActive(true);
        AudioManager.Instance?.PlayWinThenSecret();
    }

    void OnLose()
    {
        if (gameOver) return;

        Debug.Log("GAME LOST!");
        gameOver = true;

        if (explosionEffect != null)
        {
            explosionEffect.transform.position = explosionPoint.position;

            var ps = explosionEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }
        }

        if (losePanel) losePanel.SetActive(true);
        AudioManager.Instance?.PlayBoomFailThenSecret();
    }

    public void RestartGame()
    {
        AudioManager.Instance?.StopMenuMusic();

        if (explosionEffect != null)
        {
            var ps = explosionEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) winPanel.SetActive(false);

        gameOver = false;
        paused = false;

        stability = 0f;
        if (stabilitySlider != null)
            stabilitySlider.value = stability;
        UpdateStabilityColor();
// --- Сброс температуры котла только при перезапуске ---
cauldronIsHot = true;

var lever = FindObjectOfType<LeverSwitch>();
if (lever != null)
    lever.SetHot(true); // без выполнения задач

        StartLevel(0);

        AudioManager.Instance?.PlayGameMusic(); 

        Debug.Log("Игра перезапущена");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
