using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerMovement demonMovement;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private KeyCode switchKey = KeyCode.G;

    private bool isPlayerActive = true;

    private void Start()
    {
        SetActiveCharacter(isPlayerActive);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            isPlayerActive = !isPlayerActive;
            SetActiveCharacter(isPlayerActive);
        }
    }

    private void SetActiveCharacter(bool playerActive)
    {
        PlayerMovement activeMovement = playerActive ? playerMovement : demonMovement;
        PlayerMovement inactiveMovement = playerActive ? demonMovement : playerMovement;

        if (inactiveMovement != null)
        {
            inactiveMovement.enabled = false;
        }
        if (activeMovement != null)
        {
            activeMovement.enabled = true;
        }

        if (playerMovement != null)
        {
            PropDisguise disguise = playerMovement.GetComponent<PropDisguise>();
            if (disguise != null)
            {
                disguise.enabled = playerActive;
            }
        }

        if (demonMovement != null)
        {
            DemonPropChecker checker = demonMovement.GetComponent<DemonPropChecker>();
            if (checker != null)
            {
                checker.enabled = !playerActive;
            }
        }

        if (cameraFollow != null && activeMovement != null)
        {
            cameraFollow.SetTarget(activeMovement.transform);
        }
    }
}
