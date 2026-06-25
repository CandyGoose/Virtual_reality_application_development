using UnityEngine;

public class Cauldron : Interactable
{
    [Header("Water / Stirring")]
    public Animator animator;
    public Renderer waterRenderer;
    public ParticleSystem boilVfx;

    private void Awake()
    {
        // На всякий случай пытаемся найти Animator автоматически,
        // если его забыли проставить в инспекторе
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
    }

    public override void Interact(PlayerInteraction player)
    {
        Ingredient ingredient = null;

        // 1) Ингредиент прямо в руках (если когда-то так используешь)
        if (player.currentIngredient != null)
        {
            ingredient = player.currentIngredient;
            player.currentIngredient = null;
        }
        // 2) Ингредиент лежит в инструменте
        else if (player.currentTool != null && player.currentTool.HasIngredient())
        {
            ingredient = player.currentTool.ExtractIngredient();
        }

        if (ingredient != null)
        {
            Debug.Log($"Cauldron.Interact: получен ингредиент {ingredient.itemName}, вызываю AddIngredient");
            AddIngredient(ingredient);
        }
        else
        {
            Debug.Log("Cauldron.Interact: Нет ингредиента, чтобы добавить в котел");
        }
    }

    private void AddIngredient(Ingredient ingredient)
    {
        Debug.Log($"В котел добавлен {ingredient.itemName}");

        // цвет воды
        SetWaterColor(GetColorForIngredient(ingredient.name));
        SetVfxColor(GetColorForIngredient(ingredient.name));

        // анимация
        if (animator != null)
        {
            Debug.Log("Cauldron: animator найден, триггерю 'Stir'");
            animator.ResetTrigger("Stir");   // не обязательно, но лишним не будет
            animator.SetTrigger("Stir");
        }
        else
        {
            Debug.LogWarning("Cauldron: animator == null, анимация не может быть воспроизведена!");
        }

        if (boilVfx != null && !boilVfx.isPlaying)
            boilVfx.Play();

        ingredient.ResetUse();

        GameManager.Instance?.OnIngredientAddedToCauldron(ingredient);
    }

    private void SetWaterColor(Color c)
    {
        if (waterRenderer == null) return;

        var mat = waterRenderer.material;
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", c);
        else if (mat.HasProperty("_Color"))
            mat.color = c;
    }

    private void SetVfxColor(Color c)
    {
        if (boilVfx == null) return;
        {
            var main = boilVfx.main;
            main.startColor = c;
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
