using UnityEngine;

public class LeverSwitch : Interactable
{
    public ParticleSystem fire;
    public Renderer sphereRenderer;
    public Color redColor = Color.red;
    public Color blueColor = Color.cyan;

    private bool isRed = true;

    public override void Interact(PlayerInteraction player)
    {
        ToggleColor();
    }

        public void ToggleColor()
    {
        isRed = !isRed;

        if (fire != null)
        {
            var main = fire.main;
            main.startColor = isRed ? redColor : blueColor;
        }
        if (sphereRenderer != null)
        {
            sphereRenderer.material.SetColor("_BaseColor", isRed ? redColor : blueColor);
        }

        if (isRed)
        {
            // рычаг в красное → горячо
            AudioManager.Instance?.PlayHot();
            GameManager.Instance?.OnCauldronHeated();
        }
        else
        {
            // рычаг в синее → холодно
            AudioManager.Instance?.PlayCold();
            GameManager.Instance?.OnCauldronCooled();
        }

        Debug.Log("Lever switched → " + (isRed ? "Red" : "Blue"));
    }

    public void SetHot(bool hot)
{
    // hot=true => красный (горячо), hot=false => синий (холодно)
    isRed = hot;

    if (fire != null)
    {
        var main = fire.main;
        main.startColor = isRed ? redColor : blueColor;
    }

    if (sphereRenderer != null)
    {
        sphereRenderer.material.SetColor("_BaseColor", isRed ? redColor : blueColor);
    }

    Debug.Log("Lever forced state → " + (isRed ? "Hot/Red" : "Cold/Blue"));
}

}
