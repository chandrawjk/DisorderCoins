using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk pindah scene

public class MainMenu : MonoBehaviour
{
    public GameObject scorePanel;
    public GameObject namePanel;
    public TMPro.TMP_InputField nameInputField;
    public TMPro.TMP_Text nameWarningText;
    private string pendingName;
    private TMPro.TMP_Text existingNameInfoText;
    public GameObject leaderboardPanel;
    public TMPro.TMP_Text leaderboardListText;
    public TMPro.TMP_Text leaderboardListText2;
    void Awake()
    {
        scorePanel.SetActive(false);
        namePanel.SetActive(false);
        TutorialManager.ResetTutorialFlag();
    }

    public void PlayGame()
    {
        SaveEnteredPlayerName();

        // Pindah ke scene Gameplay pakai transisi fade (kalau manager-nya ada di scene)
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("MainGameplay");
        }
        else
        {
            // Fallback: kalau SceneTransitionManager belum ke-setup, pindah langsung tanpa transisi
            SceneManager.LoadScene("MainGameplay");
        }
    }
    public void ScoreGame()
    {
        scorePanel.SetActive(true);
    }
    public void CloseScorePanel()
    {
        scorePanel.SetActive(false);
    }
    public void NameGame()
    {
        namePanel.SetActive(true);
    }
    public void CloseNamePanel()
    {
        namePanel.SetActive(false);
    }

    public void OnNameEntryConfirm()
    {
        string name = nameInputField != null ? nameInputField.text.Trim() : "";

        if (string.IsNullOrEmpty(name))
        {
            if (nameWarningText != null)
            {
                nameWarningText.text = "Nama harus diisi!";
                nameWarningText.gameObject.SetActive(true);
            }
            return;
        }

        if (nameWarningText != null) nameWarningText.gameObject.SetActive(false);
        SaveEnteredPlayerName();

        LeaderboardEntry existing = LeaderboardManager.GetEntryByName(name);
        if (existing != null)
        {
            if (existingNameInfoText != null)
            {
                existingNameInfoText.text = $"{existing.playerName} sudah mencapai skor {existing.score}";
            }

            if (namePanel != null) namePanel.SetActive(false);
            PlayGame();
        }
        else
        {
            if (namePanel != null) namePanel.SetActive(false);
            PlayGame();
        }
    }

    private void SaveEnteredPlayerName()
    {
        if (nameInputField == null)
        {
            return;
        }

        string name = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        pendingName = name;
        LeaderboardManager.SetCurrentPlayer(name);

        if (!LeaderboardManager.NameExists(name))
        {
            LeaderboardManager.CreateNewEntry(name);
        }
    }
    public void QuitGame()
    {
        // Keluar dari aplikasi game
        Application.Quit();
        Debug.Log("Game Quit"); // Untuk mengecek di Editor Unity bahwa fungsi dipanggil
    }
    public void OnLeaderboardClicked()
    {
        if (leaderboardListText != null)
        {
            var top = LeaderboardManager.GetTopEntries(10);
            StringBuilder sb = new StringBuilder();
            StringBuilder sc = new StringBuilder();

            if (top.Count == 0)
            {
                sb.AppendLine("Belum ada data. Ayo main duluan!");
            }
            else
            {
                for (int i = 0; i < top.Count; i++)
                {
                    string playedAt = string.IsNullOrEmpty(top[i].lastPlayedAt) ? "-" : top[i].lastPlayedAt;
                    sb.AppendLine($"{i + 1}. {top[i].playerName}");
                }
                for (int j = 0; j < top.Count; j++)
                {
                    string playedAt = string.IsNullOrEmpty(top[j].lastPlayedAt) ? "-" : top[j].lastPlayedAt;
                    sc.AppendLine($"{top[j].score}     {playedAt}");
                }
            }

            leaderboardListText.text = sb.ToString();
            leaderboardListText2.text = sc.ToString();
        }
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
    }
}