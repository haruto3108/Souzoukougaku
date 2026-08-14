using System;
using UnityEngine;

public class DemonPropChecker : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactRange = 45f;
    [SerializeField] private LayerMask propMask = ~0;
    [SerializeField] private float maxPropSize = 8f;

    [Header("Input")]
    [SerializeField] private KeyCode checkKey = KeyCode.E;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);

    [Header("Target")]
    [SerializeField] private PropDisguise playerDisguise;

    private GameObject lookedAtProp;

    private Material highlightMaterial;
    private Renderer highlightedRenderer;
    private Material[] highlightedOriginalMaterials;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        highlightMaterial = new Material(shader);
        highlightMaterial.color = highlightColor;
    }

    private void OnDisable()
    {
        ClearHighlight();
    }

    private void Update()
    {
        GameObject previousLookedAt = lookedAtProp;
        DetectLookedAtProp();

        if (lookedAtProp != previousLookedAt)
        {
            ClearHighlight();
            if (lookedAtProp != null)
            {
                SetHighlight(lookedAtProp);
            }
        }

        if (Input.GetKeyDown(checkKey) && lookedAtProp != null)
        {
            CheckIsPlayer(lookedAtProp);
        }
    }

    private void DetectLookedAtProp()
    {
        lookedAtProp = null;

        if (cameraTransform == null)
        {
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(cameraTransform.position, cameraTransform.forward, interactRange, propMask, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            if (IsPlayer(hit.collider.gameObject))
            {
                lookedAtProp = hit.collider.gameObject;
                break;
            }

            MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (meshFilter != null && meshFilter.sharedMesh != null && renderer != null && IsWithinSizeLimit(renderer))
            {
                lookedAtProp = hit.collider.gameObject;
                break;
            }
        }
    }

    private bool IsPlayer(GameObject candidate)
    {
        return playerDisguise != null
            && (candidate == playerDisguise.gameObject || candidate.transform.IsChildOf(playerDisguise.transform));
    }

    private Renderer FindVisibleRenderer(GameObject target)
    {
        Renderer direct = target.GetComponent<Renderer>();
        if (direct != null && direct.enabled)
        {
            return direct;
        }

        Renderer[] children = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer candidate in children)
        {
            if (candidate.enabled)
            {
                return candidate;
            }
        }

        return null;
    }

    private bool IsWithinSizeLimit(Renderer renderer)
    {
        Vector3 size = renderer.bounds.size;
        float maxDimension = Mathf.Max(size.x, size.y, size.z);
        return maxDimension <= maxPropSize;
    }

    private void SetHighlight(GameObject target)
    {
        Renderer renderer = FindVisibleRenderer(target);
        if (renderer == null)
        {
            return;
        }

        highlightedRenderer = renderer;
        highlightedOriginalMaterials = renderer.sharedMaterials;

        Material[] highlightMaterials = new Material[renderer.sharedMaterials.Length];
        for (int i = 0; i < highlightMaterials.Length; i++)
        {
            highlightMaterials[i] = highlightMaterial;
        }
        renderer.sharedMaterials = highlightMaterials;
    }

    private void ClearHighlight()
    {
        if (highlightedRenderer != null)
        {
            highlightedRenderer.sharedMaterials = highlightedOriginalMaterials;
        }

        highlightedRenderer = null;
        highlightedOriginalMaterials = null;
    }

    private void CheckIsPlayer(GameObject target)
    {
        bool isPlayer = IsPlayer(target);

        Debug.Log(isPlayer
            ? $"[DemonPropChecker] {target.name} はプレイヤーです。"
            : $"[DemonPropChecker] {target.name} はプレイヤーではありません。");
    }
}
