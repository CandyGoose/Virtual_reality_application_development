using UnityEngine;

public class Tool : Interactable
{
    [Header("Settings")]
    public bool isGrinder = false;

    [Header("Hold Offset")]
    public Vector3 holdLocalPosition = Vector3.zero;
    public Vector3 holdLocalEuler = new Vector3(0f, 180f, 0f);
    public Vector3 holdLocalScale = Vector3.one;

    [Header("Indicator Sphere")]
    public Renderer sphereRenderer;

    private Color emptyBaseColor;      // базовый цвет «пустого» индикатора
    private Color emptyEmissionColor;  // базовый цвет эмиссии «пустого» индикатора
    private Ingredient currentIngredient;

    [Header("Animation")]
    public Animator animator;      
    public string grindTrigger = "Grind"; // имя триггера в Animator


    private bool isHeld = false;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (sphereRenderer != null)
        {
            var mat = sphereRenderer.material;

            // запоминаем текущие базовый и эмиссионный цвета как "пустые"
            if (mat.HasProperty("_BaseColor"))
                emptyBaseColor = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color"))
                emptyBaseColor = mat.color;

            if (mat.HasProperty("_EmissionColor"))
                emptyEmissionColor = mat.GetColor("_EmissionColor");
            else
                emptyEmissionColor = Color.black;

            // сразу гасим индикатор
            ResetVisual();
        }
    }

    // ---------- ПУБЛИЧНЫЙ СБРОС ВИЗУАЛА ----------
    public void ResetVisual()
    {
        currentIngredient = null;
        if (sphereRenderer == null) return;

        var mat = sphereRenderer.material;

        // базовый цвет – "пустой" (можешь сделать его серым, если хочешь)
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", emptyBaseColor);
        else if (mat.HasProperty("_Color"))
            mat.color = emptyBaseColor;

        // эмиссию гасим
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
        }
    }
    // ------------------------------------------------

public override void Interact(PlayerInteraction player)
{
    // Если в руке другой инструмент — выбрасываем его
    if (player.currentTool != null && player.currentTool != this)
    {
        player.currentTool.Drop();
        player.currentTool = null;
    }

    if (!isHeld)
    {
        PickUp(player);
    }
    else
    {
        Debug.Log($"{itemName} уже в руках");
    }
}


    private void PickUp(PlayerInteraction player)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale   = transform.localScale;

        transform.SetParent(player.handPoint);
        transform.localPosition = holdLocalPosition;
        transform.localRotation = Quaternion.Euler(holdLocalEuler);
        transform.localScale    = holdLocalScale;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        player.currentTool = this;
        isHeld = true;

        // в руках по умолчанию индикатор пустой
        ResetVisual();
    }

public void Drop()
{
    // NEW: если в инструменте был ингредиент — вернуть его на место
    if (currentIngredient != null)
    {
        currentIngredient.ReturnToOrigin();
        currentIngredient = null;
    }

    transform.SetParent(originalParent);
    transform.position = originalPosition;
    transform.rotation = originalRotation;
    transform.localScale = originalScale;

    if (rb != null)
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    isHeld = false;
    ResetVisual();
}


    public void AddIngredient(Ingredient ingredient)
    {
        currentIngredient = ingredient;
        Color newColor = GetColorForIngredient(ingredient.name);
        SetSphereColor(newColor);

        ingredient.OnAddedToTool(this);

        // <<< ЗАПУСК АНИМАЦИИ ПЕРЕМЕШИВАНИЯ >>>
        if (isGrinder && animator != null)
        {
            animator.SetTrigger(grindTrigger);
        }

        Debug.Log($"{ingredient.itemName} добавлен в {itemName}");
    }

    public bool HasIngredient() => currentIngredient != null;

    public Ingredient ExtractIngredient()
    {
        Ingredient ing = currentIngredient;
        currentIngredient = null;
        ResetVisual();
        return ing;
    }

    private void SetSphereColor(Color c)
    {
        if (sphereRenderer == null) return;

        var mat = sphereRenderer.material;

        // красим базовый цвет
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", c);
        else if (mat.HasProperty("_Color"))
            mat.color = c;

        // включаем и красим эмиссию тем же цветом
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", c);
            if (c.maxColorComponent > 0f)
                mat.EnableKeyword("_EMISSION");
            else
                mat.DisableKeyword("_EMISSION");
        }
    }

    private Color GetColorForIngredient(string ingredientName)
    {
        string lower = ingredientName.ToLower();

        if (lower.Contains("cristal") || lower.Contains("crystal"))
            return new Color(1f, 0.5f, 1f, 1f);

        if (lower.Contains("mushroom"))
            return new Color(0.3f, 0.8f, 1f, 1f);

        if (lower.Contains("flask"))
            return new Color(1f, 0.7f, 0.8f, 1f);

        if (lower.Contains("jar"))
            return new Color(0.6f, 1f, 0.8f, 1f);

        if (lower.Contains("bag"))
            return new Color(0.7f, 0.7f, 0.7f, 1f);

        if (lower.Contains("minipot"))
            return new Color(1f, 0.6f, 0.9f, 1f);

        return Color.white;
    }
}
