//Nils Klein

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class MarkButton : MonoBehaviour
{
    private Renderer rend;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        rend.material.color = normalColor;

        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        rend.material.color = hoverColor;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        rend.material.color = normalColor;
    }

    private void OnDestroy()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
    }
}