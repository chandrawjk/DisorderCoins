using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Wajib dipanggil untuk pindah scene

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
    
    [SerializeField] private TextMeshProUGUI savedScoreText;
    [SerializeField] private TextMeshProUGUI pageText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    private List<SavedScore> sortedScores = new List<SavedScore>();
    
    private int currentPage;
    private const int EntriesPerPage = 15;
    void Awake()
    {
        scorePanel.SetActive(false);
        namePanel.SetActive(false);
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        TutorialManager.ResetTutorialFlag();
    }
    [Serializable]
    private class SavedScore
    {
        public string playerName = "";
        public int score = 0;
        public string playedAt = "";
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
        public void NextPage()
    {
        int pageCount = GetPageCount();
        if (currentPage < pageCount - 1)
        {
            currentPage++;
            RenderCurrentPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RenderCurrentPage();
        }
    }
        private int GetPageCount()
    {
        return (sortedScores.Count + EntriesPerPage - 1) / EntriesPerPage;
    }

    private void RenderCurrentPage()
    {
        if (leaderboardListText == null)
        {
            return;
        }

        StringBuilder result = new StringBuilder();
        result.AppendLine("No. Tanggal\t\t\t|\t\tSkor\t\t|\tNama\n");

        if (sortedScores.Count == 0)
        {
            result.Append("Belum ada skor");
        }
        else
        {
            int firstIndex = currentPage * EntriesPerPage;
            int lastIndex = Mathf.Min(firstIndex + EntriesPerPage, sortedScores.Count);
            for (int index = firstIndex; index < lastIndex; index++)
            {
                SavedScore savedScore = sortedScores[index];
                result.Append(index + 1)
                    .Append(". ")
                    .Append(savedScore.playedAt)
                    .Append("\t|\t\t")
                    .Append(savedScore.score)
                    .Append("\t\t|\t")
                    .Append(savedScore.playerName)
                    .AppendLine();
            }
        }

        leaderboardListText.text = result.ToString();

        if (pageText != null)
            pageText.text = sortedScores.Count == 0 ? "0/0" : (currentPage + 1) + "/" + GetPageCount();
        if (previousButton != null)
            previousButton.interactable = currentPage > 0;
        if (nextButton != null)
            nextButton.interactable = currentPage < GetPageCount() - 1;
    }
    public void OnLeaderboardClicked()
    {
        List<LeaderboardEntry> entries = LeaderboardManager.GetTopEntries(LeaderboardManager.GetAllEntries().Count);
        sortedScores.Clear();
        foreach (LeaderboardEntry entry in entries)
        {
            if (entry != null)
            {
                sortedScores.Add(new SavedScore
                {
                    playerName = entry.playerName ?? "",
                    score = entry.score,
                    playedAt = string.IsNullOrEmpty(entry.lastPlayedAt) ? "-" : entry.lastPlayedAt
                });
            }
        }

        currentPage = 0;
        RenderCurrentPage();
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
    }
}