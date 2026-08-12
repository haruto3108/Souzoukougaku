using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 0f, 0f);

    private CharacterController targetController;

    [Header("Distance")]
    [SerializeField] private float desiredDistance = 4f;
    [SerializeField] private float minDistance = 0.3f;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private LayerMask obstructionMask = ~0;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -35f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.05f;

    private float yaw;
    private float pitch = 15f;
    private Vector3 currentVelocity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
        {
            yaw = target.eulerAngles.y;
            targetController = target.GetComponent<CharacterController>();
        }
    }

    private Vector3 GetPivot()
    {
        if (targetController != null)
        {
            return target.TransformPoint(targetController.center);
        }

        return target.position + pivotOffset;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = GetPivot();

        float finalDistance = desiredDistance;
        Vector3 castDirection = rotation * Vector3.back;

        RaycastHit[] hits = Physics.SphereCastAll(pivot, collisionRadius, castDirection, desiredDistance, obstructionMask, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == target || hit.transform.IsChildOf(target))
            {
                continue;
            }

            if (hit.distance < finalDistance)
            {
                finalDistance = hit.distance;
            }
        }

        finalDistance = Mathf.Clamp(finalDistance, minDistance, desiredDistance);

        Vector3 desiredPosition = pivot + castDirection * finalDistance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, positionSmoothTime);
        transform.rotation = rotation;
    }
}
