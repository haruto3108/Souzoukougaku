using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private Text timerText;
    [SerializeField] private float durationSeconds = 180f;

    private float remainingSeconds;
    private bool isRunning;

    private void Start()
    {
        remainingSeconds = durationSeconds;
        isRunning = true;
        UpdateDisplay();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        remainingSeconds -= Time.deltaTime;
        if (remainingSeconds <= 0f)
        {
            remainingSeconds = 0f;
            isRunning = false;
        }

        UpdateDisplay();
    }

    public void ReduceTime(float seconds)
    {
        if (!isRunning)
        {
            return;
        }

        remainingSeconds = Mathf.Max(0f, remainingSeconds - seconds);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
        int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
