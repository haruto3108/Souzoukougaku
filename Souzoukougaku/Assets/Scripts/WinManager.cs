using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinManager : MonoBehaviour
{
    [SerializeField] private Text victoryText;
    [SerializeField] private Text defeatText;
    [SerializeField] private Text remainingPlayersText;
    [SerializeField] private NotificationManager notificationManager;

    private int remainingPlayers;
    private bool isGameOver;
    private readonly HashSet<PropDisguise> capturedPlayers = new HashSet<PropDisguise>();

    private void Start()
    {
        PropDisguise[] players = FindObjectsByType<PropDisguise>(FindObjectsSortMode.None);
        remainingPlayers = players.Length;
        UpdateRemainingPlayersDisplay();

        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(false);
        }
        if (defeatText != null)
        {
            defeatText.gameObject.SetActive(false);
        }
    }

    public void CapturePlayer(PropDisguise player)
    {
        if (isGameOver || player == null || capturedPlayers.Contains(player))
        {
            return;
        }

        capturedPlayers.Add(player);
        remainingPlayers--;
        UpdateRemainingPlayersDisplay();

        if (notificationManager != null)
        {
            notificationManager.ShowMessage("味方が確保されました");
        }

        if (remainingPlayers <= 0)
        {
            ShowVictory();
        }
    }

    public void OnTimeUp()
    {
        if (isGameOver)
        {
            return;
        }

        if (remainingPlayers > 0)
        {
            ShowDefeat();
        }
    }

    private void UpdateRemainingPlayersDisplay()
    {
        if (remainingPlayersText == null)
        {
            return;
        }

        remainingPlayersText.text = $"残り: {remainingPlayers}";
    }

    private void ShowVictory()
    {
        isGameOver = true;

        if (victoryText == null)
        {
            return;
        }

        victoryText.text = "勝利";
        victoryText.gameObject.SetActive(true);
    }

    private void ShowDefeat()
    {
        isGameOver = true;

        if (defeatText == null)
        {
            return;
        }

        defeatText.text = "敗北";
        defeatText.gameObject.SetActive(true);
    }
}
