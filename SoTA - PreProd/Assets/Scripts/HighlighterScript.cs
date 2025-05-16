using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlighterScript : MonoBehaviour
{
    [Tooltip("Renderer to highlight. If left empty, the first Renderer in children will be used.")]
    [SerializeField] private Renderer targetRenderer;

    [Tooltip("Material to use for highlighting.")]
    [SerializeField] private Material highlightMaterial;

    private Material originalMaterial;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            originalMaterial = targetRenderer.material;
        }
        else
        {
            Debug.LogWarning("Highlighter: No Renderer found on " + gameObject.name);
        }
    }

    public void EnableHighlight()
    {
        if (targetRenderer != null && highlightMaterial != null)
        {
            targetRenderer.material = highlightMaterial;
        }
    }

    public void DisableHighlight()
    {
        if (targetRenderer != null && originalMaterial != null)
        {
            targetRenderer.material = originalMaterial;
        }
    }
}
