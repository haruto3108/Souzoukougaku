using System;
using UnityEngine;

public class PropDisguise : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactRange = 30f;
    [SerializeField] private LayerMask propMask = ~0;
    [SerializeField] private float maxPropSize = 8f;

    [Header("Input")]
    [SerializeField] private KeyCode disguiseKey = KeyCode.E;
    [SerializeField] private KeyCode fixPositionKey = KeyCode.F;
    [SerializeField] private KeyCode redisguiseKey = KeyCode.T;

    [Header("Player Model")]
    [SerializeField] private Renderer[] playerRenderers;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);

    [Header("Rotation")]
    [SerializeField] private float manualRotationSpeed = 120f;

    private CharacterController playerController;
    private PlayerMovement playerMovement;
    private GameObject disguiseObject;
    private GameObject lookedAtProp;
    private GameObject currentDisguisedProp;
    private GameObject lastDisguisedProp;
    private bool isDisguised;
    private bool isPositionLocked;
    private Vector3 rotationPivotLocalOffset;

    private Material highlightMaterial;
    private Renderer highlightedRenderer;
    private Material[] highlightedOriginalMaterials;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            playerRenderers = GetComponentsInChildren<Renderer>();
        }

        playerController = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        highlightMaterial = new Material(shader);
        highlightMaterial.color = highlightColor;
    }

    private void Update()
    {
        if (isDisguised)
        {
            if (!isPositionLocked)
            {
                HandleManualRotation();
            }

            if (Input.GetKeyDown(fixPositionKey))
            {
                isPositionLocked = !isPositionLocked;
                if (playerMovement != null)
                {
                    playerMovement.SetPositionLocked(isPositionLocked);
                }
            }

            if (Input.GetKeyDown(disguiseKey))
            {
                RevertDisguise();
            }
            return;
        }

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

        if (Input.GetKeyDown(disguiseKey) && lookedAtProp != null)
        {
            ApplyDisguise(lookedAtProp);
        }
        else if (Input.GetKeyDown(redisguiseKey) && lastDisguisedProp != null)
        {
            ApplyDisguise(lastDisguisedProp);
        }
    }

    private void HandleManualRotation()
    {
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotationInput -= 1f;
        }
        if (Input.GetKey(KeyCode.R))
        {
            rotationInput += 1f;
        }

        if (rotationInput == 0f)
        {
            return;
        }

        Vector3 pivot = transform.TransformPoint(rotationPivotLocalOffset);
        transform.RotateAround(pivot, Vector3.up, rotationInput * manualRotationSpeed * Time.deltaTime);
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

            MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (meshFilter != null && meshFilter.sharedMesh != null && renderer != null && IsWithinSizeLimit(renderer))
            {
                lookedAtProp = hit.collider.gameObject;
                break;
            }
        }
    }

    private bool IsWithinSizeLimit(Renderer renderer)
    {
        Vector3 size = renderer.bounds.size;
        float maxDimension = Mathf.Max(size.x, size.y, size.z);
        return maxDimension <= maxPropSize;
    }

    private void SetHighlight(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
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

    private void ApplyDisguise(GameObject prop)
    {
        MeshFilter propMeshFilter = prop.GetComponent<MeshFilter>();
        MeshRenderer propMeshRenderer = prop.GetComponent<MeshRenderer>();

        if (propMeshFilter == null || propMeshRenderer == null)
        {
            return;
        }

        ClearHighlight();

        if (disguiseObject != null)
        {
            Destroy(disguiseObject);
        }

        disguiseObject = new GameObject("Disguise");
        disguiseObject.transform.SetParent(transform, false);
        disguiseObject.transform.localPosition = Vector3.zero;
        disguiseObject.transform.rotation = prop.transform.rotation;
        disguiseObject.transform.localScale = prop.transform.lossyScale;

        MeshFilter disguiseFilter = disguiseObject.AddComponent<MeshFilter>();
        disguiseFilter.sharedMesh = propMeshFilter.sharedMesh;

        MeshRenderer disguiseRenderer = disguiseObject.AddComponent<MeshRenderer>();
        disguiseRenderer.sharedMaterials = propMeshRenderer.sharedMaterials;

        AlignBottomToPlayer(propMeshRenderer);

        rotationPivotLocalOffset = transform.InverseTransformPoint(disguiseRenderer.bounds.center);

        SetPlayerRenderersVisible(false);
        isDisguised = true;
        isPositionLocked = false;
        currentDisguisedProp = prop;

        if (playerMovement != null)
        {
            playerMovement.SetRotationLocked(true);
            playerMovement.SetPositionLocked(false);
        }
    }

    private void AlignBottomToPlayer(MeshRenderer propMeshRenderer)
    {
        float propBottomOffset = propMeshRenderer.bounds.min.y - propMeshRenderer.transform.position.y;

        float playerBottomY = transform.position.y;
        if (playerController != null)
        {
            playerBottomY += playerController.center.y - playerController.height * 0.5f - playerController.skinWidth;
        }

        Vector3 position = disguiseObject.transform.position;
        position.y = playerBottomY - propBottomOffset;
        disguiseObject.transform.position = position;
    }

    private void RevertDisguise()
    {
        if (disguiseObject != null)
        {
            Destroy(disguiseObject);
            disguiseObject = null;
        }

        lastDisguisedProp = currentDisguisedProp;
        currentDisguisedProp = null;

        SetPlayerRenderersVisible(true);
        isDisguised = false;
        isPositionLocked = false;

        if (playerMovement != null)
        {
            playerMovement.SetRotationLocked(false);
            playerMovement.SetPositionLocked(false);
            playerMovement.FaceForward();
        }
    }

    private void SetPlayerRenderersVisible(bool visible)
    {
        foreach (Renderer r in playerRenderers)
        {
            if (r != null)
            {
                r.enabled = visible;
            }
        }
    }
}
