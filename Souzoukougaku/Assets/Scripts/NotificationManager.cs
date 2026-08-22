using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] private Text notificationText;
    [SerializeField] private float displayDuration = 3f;

    private float hideTimer;

    private void Awake()
    {
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (hideTimer <= 0f)
        {
            return;
        }

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f && notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message)
    {
        if (notificationText == null)
        {
            return;
        }

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        hideTimer = displayDuration;
    }
}
