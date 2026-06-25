using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Ray Origin")]
    [Tooltip("Отсюда будет идти луч – поставь сюда RightHand Controller")]
    public Transform rayOrigin;

    [Header("UI (опционально)")]
    public Image crosshair;
    public Color defaultColor = Color.white;
    public Color hoverColor = Color.green;

    [Header("Settings")]
    public float rayDistance = 4f;
    public Transform handPoint;

    [Header("Held Items")]
    public Tool currentTool;
    public Ingredient currentIngredient;

    [Header("VR Input (OpenXR / Input System)")]
    public InputActionReference interactAction;
    public InputActionReference dropAction;

    private Interactable currentTarget;
    private Interactable highlightedTarget;

    private void OnEnable()
    {
        if (interactAction != null) interactAction.action.Enable();
        if (dropAction != null) dropAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null) interactAction.action.Disable();
        if (dropAction != null) dropAction.action.Disable();
    }

    void Update()
    {
        // 1) В меню / на паузе – не взаимодействуем
        if (GameManager.Instance != null && GameManager.Instance.paused)
        {
            ClearHighlight();
            return;
        }

        if (rayOrigin == null)
            return;

        // Луч из контроллера
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            // --- решаем, можно ли подсвечивать ---
            bool canHighlight = false;

            if (interactable != null)
            {
                // Ингредиенты подсвечиваем только если есть инструмент
                if (interactable is Ingredient)
                {
                    if (currentTool != null)
                        canHighlight = true;
                }
                else
                {
                    // все остальные типы – всегда
                    canHighlight = true;
                }
            }

            // --- обновляем хайлайт ---
            Interactable newHighlight = canHighlight ? interactable : null;

            if (highlightedTarget != newHighlight)
            {
                if (highlightedTarget != null)
                    highlightedTarget.SetHighlighted(false);

                highlightedTarget = newHighlight;

                if (highlightedTarget != null)
                    highlightedTarget.SetHighlighted(true);
            }

            // прицел (по желанию: тоже только когда canHighlight)
            if (crosshair != null)
                crosshair.color = canHighlight ? hoverColor : defaultColor;

            currentTarget = interactable;

            // --- нажатие триггера / E ---
            bool interactPressed = false;

            if (interactAction != null && interactAction.action.WasPerformedThisFrame())
                interactPressed = true;

            if (!interactPressed && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                interactPressed = true;

            if (interactPressed && canHighlight)
            {
                // interact только если объект "валиден" для взаимодействия
                interactable?.Interact(this);
            }
        }
        else
        {
            ClearHighlight();
            currentTarget = null;
        }

        // --- Drop ---
        bool dropPressed = false;

        if (dropAction != null && dropAction.action.WasPerformedThisFrame())
            dropPressed = true;

        if (!dropPressed && Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            dropPressed = true;

        if (dropPressed)
        {
            if (currentTool != null)
            {
                currentTool.Drop();
                currentTool = null;
            }
            else if (currentIngredient != null)
            {
                currentIngredient = null;
            }
        }
    }

    private void ClearHighlight()
    {
        if (highlightedTarget != null)
        {
            highlightedTarget.SetHighlighted(false);
            highlightedTarget = null;
        }

        if (crosshair != null)
            crosshair.color = defaultColor;
    }
}
