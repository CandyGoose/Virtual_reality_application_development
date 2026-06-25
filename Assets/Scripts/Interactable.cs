using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Tooltip("Имя предмета для отладки")]
    public string itemName;

    [Header("Highlight")]
    public Renderer highlightRenderer;        // сюда в инспекторе кидаешь меш, который надо светить
    public Color highlightColor = Color.yellow;

    private Color _originalColor;
    private bool _hasOriginalColor = false;

    public virtual void SetHighlighted(bool highlighted)
    {
        if (highlightRenderer == null) return;

        var mat = highlightRenderer.material;

        if (!_hasOriginalColor)
        {
            if (mat.HasProperty("_BaseColor"))
                _originalColor = mat.GetColor("_BaseColor");
            else if (mat.HasProperty("_Color"))
                _originalColor = mat.color;

            _hasOriginalColor = true;
        }

        if (highlighted)
        {
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", highlightColor);
            else if (mat.HasProperty("_Color"))
                mat.color = highlightColor;
        }
        else
        {
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", _originalColor);
            else if (mat.HasProperty("_Color"))
                mat.color = _originalColor;
        }
    }

    public abstract void Interact(PlayerInteraction player);
}
