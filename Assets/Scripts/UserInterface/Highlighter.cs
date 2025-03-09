using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class Highlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // reference to MeshRenderer component
    [SerializeField] private MeshRenderer[] _meshRenderers;

    // Property Reference from Shader Graph
    [SerializeField] private string _highlightProperty = "_IsHighlighted";

    private bool _isEnabled;
    public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; } }


    private void Start()
    {
        _isEnabled = true;
        // use non-highlighted material by default
        ToggleHighlight(false);
    }

    // toggle glow on or off using Shader Graph property
    public void ToggleHighlight(bool onOff)
    {
        foreach (MeshRenderer meshRenderer in _meshRenderers)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material.SetFloat(_highlightProperty, onOff ? 1f : 0f);
            }
        }
    }

    // master toggle (off overrides highlight state)
    public void EnableHighlight(bool state)
    {
        _isEnabled = state;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.IsGameOver) return;

        ToggleHighlight(_isEnabled);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.Instance.IsGameOver) return;

        ToggleHighlight(false);
    }
}