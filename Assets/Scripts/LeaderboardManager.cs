using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public string lastPlayedAt;
    public long lastPlayedTicks;
}

[Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public static class LeaderboardManager
{
    private const string LEADERBOARD_KEY = "Reset_Leaderboard";
    private const string CURRENT_PLAYER_KEY = "Reset_CurrentPlayer";

    public static string CurrentPlayerName
    {
        get { return PlayerPrefs.GetString(CURRENT_PLAYER_KEY, ""); }
    }

    public static void SetCurrentPlayer(string playerName)
    {
        PlayerPrefs.SetString(CURRENT_PLAYER_KEY, playerName.Trim());
        PlayerPrefs.Save();
    }

    public static List<LeaderboardEntry> GetAllEntries()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");

        if (string.IsNullOrEmpty(json))
        {
            return new List<LeaderboardEntry>();
        }

        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
        return data != null && data.entries != null
            ? data.entries
            : new List<LeaderboardEntry>();
    }

    private static void SaveAllEntries(List<LeaderboardEntry> entries)
    {
        LeaderboardData data = new LeaderboardData { entries = entries };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();
    }

    public static bool NameExists(string playerName)
    {
        string normalized = NormalizeName(playerName);
        return GetAllEntries().Any(e => NormalizeName(e.playerName) == normalized);
    }

    public static LeaderboardEntry GetEntryByName(string playerName)
    {
        string normalized = NormalizeName(playerName);
        return GetAllEntries().FirstOrDefault(e => NormalizeName(e.playerName) == normalized);
    }

    public static void CreateNewEntry(string playerName)
    {
        playerName = ResolvePlayerName(playerName);
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Tidak dapat membuat entry karena nama pemain kosong.");
            return;
        }

        SetCurrentPlayer(playerName);
        List<LeaderboardEntry> entries = GetAllEntries();
        DateTime now = DateTime.Now;
        entries.Add(new LeaderboardEntry
        {
            playerName = playerName.Trim(),
            score = 0,
            lastPlayedAt = now.ToString("dd/MM/yyyy HH:mm:ss"),
            lastPlayedTicks = now.Ticks
        });
        SaveAllEntries(entries);
    }

    public static void TrySaveScore(string playerName, int score, string playedAt = null)
    {
        playerName = ResolvePlayerName(playerName);
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Tidak dapat menyimpan skor karena nama pemain kosong.");
            return;
        }

        SetCurrentPlayer(playerName);
        List<LeaderboardEntry> entries = GetAllEntries();
        string normalized = NormalizeName(playerName);
        LeaderboardEntry entry = entries.FirstOrDefault(e => NormalizeName(e.playerName) == normalized);
        DateTime now = DateTime.Now;
        string timestamp = playedAt ?? now.ToString("dd/MM/yyyy HH:mm:ss");

        if (entry == null)
        {
            entries.Add(new LeaderboardEntry
            {
                playerName = playerName.Trim(),
                score = score,
                lastPlayedAt = timestamp,
                lastPlayedTicks = now.Ticks
            });
        }
        else
        {
            if (score > entry.score)
            {
                entry.score = score;
            }

            entry.lastPlayedAt = timestamp;
            entry.lastPlayedTicks = now.Ticks;
        }

        SaveAllEntries(entries);
    }

    public static void ResetPlayerProgress(string playerName)
    {
        List<LeaderboardEntry> entries = GetAllEntries();
        string normalized = NormalizeName(playerName);
        LeaderboardEntry entry = entries.FirstOrDefault(e => NormalizeName(e.playerName) == normalized);

        if (entry != null)
        {
            entry.score = 0;
            SaveAllEntries(entries);
        }
    }

    public static List<LeaderboardEntry> GetTopEntries(int count = 10)
    {
        return GetAllEntries()
            .OrderByDescending(GetLastPlayedTime)
            .Take(count)
            .ToList();
    }

    private static DateTime GetLastPlayedTime(LeaderboardEntry entry)
    {
        if (entry.lastPlayedTicks > 0)
        {
            return new DateTime(entry.lastPlayedTicks);
        }

        DateTime playedAt;
        return DateTime.TryParseExact(
            entry.lastPlayedAt,
            "dd/MM/yyyy HH:mm:ss",
            null,
            System.Globalization.DateTimeStyles.None,
            out playedAt) ? playedAt : DateTime.MinValue;
    }

    public static void ClearAllEntries()
    {
        PlayerPrefs.DeleteKey(LEADERBOARD_KEY);
        PlayerPrefs.Save();
    }

    private static string NormalizeName(string name)
    {
        return (name ?? "").Trim().ToLowerInvariant();
    }

    private static string ResolvePlayerName(string playerName)
    {
        string resolvedName = (playerName ?? "").Trim();
        return string.IsNullOrEmpty(resolvedName) ? CurrentPlayerName.Trim() : resolvedName;
    }
}