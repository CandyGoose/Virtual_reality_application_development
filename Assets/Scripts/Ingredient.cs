using UnityEngine;

public class Ingredient : Interactable
{
    [HideInInspector] public bool isGround = false;

    private bool isUsed = false;

    // --- NEW: исходная точка спавна/расположения ---
    private Transform originParent;
    private Vector3 originPosition;
    private Quaternion originRotation;
    private Vector3 originScale;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        originParent = transform.parent;
        originPosition = transform.position;
        originRotation = transform.rotation;
        originScale = transform.localScale;
    }

    public override void Interact(PlayerInteraction player)
    {
        if (isUsed) return;

        if (player.currentTool != null)
        {
            player.currentTool.AddIngredient(this);
            isUsed = true;
        }
        else
        {
            Debug.Log("Сначала взять инструмент");
        }
    }

    public void OnAddedToTool(Tool tool)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        if (tool.isGrinder)
        {
            isGround = true;
            GameManager.Instance?.OnIngredientGround(this);
        }
    }

    public void ResetUse()
    {
        isUsed = false;
    }

    // --- NEW: "вернуть на место" ---
    public void ReturnToOrigin()
    {
        // вернуть трансформ
        transform.SetParent(originParent);
        transform.position = originPosition;
        transform.rotation = originRotation;
        transform.localScale = originScale;

        // вернуть физику
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // вернуть состояние "можно снова брать"
        isGround = false;   // если нужно считать "как было изначально"
        ResetUse();
    }
}
