using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinManager : MonoBehaviour
{
    [SerializeField] private Text victoryText;

    private int remainingPlayers;
    private readonly HashSet<PropDisguise> capturedPlayers = new HashSet<PropDisguise>();

    private void Start()
    {
        PropDisguise[] players = FindObjectsByType<PropDisguise>(FindObjectsSortMode.None);
        remainingPlayers = players.Length;

        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(false);
        }
    }

    public void CapturePlayer(PropDisguise player)
    {
        if (player == null || capturedPlayers.Contains(player))
        {
            return;
        }

        capturedPlayers.Add(player);
        remainingPlayers--;

        if (remainingPlayers <= 0)
        {
            ShowVictory();
        }
    }

    private void ShowVictory()
    {
        if (victoryText == null)
        {
            return;
        }

        victoryText.text = "勝利";
        victoryText.gameObject.SetActive(true);
    }
}
