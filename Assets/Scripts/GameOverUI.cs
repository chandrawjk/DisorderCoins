using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("Panel (harus punya CanvasGroup component)")]
    public CanvasGroup panelCanvasGroup;
    public float fadeDuration = 0.5f;

    [Header("Texts")]
    public TMP_Text stageText;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    [Header("Scene Navigation")]
    public string menuSceneName = "Menu"; // sesuaikan sama nama scene menu lo

    [Header("Background Blur")]
    public RawImage blurBackgroundImage; // RawImage full-screen, taruh sebagai child PALING ATAS di dalam panel (biar di-render paling belakang)
    public Camera captureCamera;         // biasanya Main Camera
    public Material blurMaterial;        // material pakai shader Custom/SimpleBoxBlur
    public int blurIterations = 3;       // makin banyak makin blur, tapi makin berat

    private const string HighScoreKey = "DisorderCoins_HighScore";

    void Awake()
    {
        Instance = this;

        // Panel disembunyiin total di awal (invisible + gak bisa diklik)
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        panelCanvasGroup.gameObject.SetActive(false);
    }

    // Dipanggil dari GameManager.cs pas waktu abis
    public void ShowGameOver(int stage, int score)
    {
        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        if (stageText != null) stageText.text = "Stage: " + stage;
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (highScoreText != null) highScoreText.text = "High Score: " + highScore;
        if (blurBackgroundImage != null && captureCamera != null && blurMaterial != null)
        {
            Texture2D blurredTexture = CaptureAndBlur();
            blurBackgroundImage.texture = blurredTexture;
            blurBackgroundImage.gameObject.SetActive(true);
        }

        panelCanvasGroup.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private Texture2D CaptureAndBlur()
    {
        int width = Screen.width;
        int height = Screen.height;

        // 1. Render tampilan sekarang ke RenderTexture
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture prevTarget = captureCamera.targetTexture;
        captureCamera.targetTexture = rt;
        captureCamera.Render();
        captureCamera.targetTexture = prevTarget;

        // 2. Blur berkali-kali (makin banyak iterasi, makin halus blur-nya)
        RenderTexture current = rt;
        for (int i = 0; i < blurIterations; i++)
        {
            RenderTexture next = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(current, next, blurMaterial);
            RenderTexture.ReleaseTemporary(current);
            current = next;
        }

        // 3. Baca hasilnya jadi Texture2D biasa
        RenderTexture.active = current;
        Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = null;

        RenderTexture.ReleaseTemporary(current);
        return result;
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        panelCanvasGroup.alpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        panelCanvasGroup.alpha = 1f;
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
    }

    // Hubungin ini ke OnClick() tombol Restart di Inspector
    public void OnRestartButton()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(currentScene);
        }
        else
        {
            // Fallback kalau SceneTransitionManager belum ke-setup di scene, biar gak error
            SceneManager.LoadScene(currentScene);
        }
    }

    // Hubungin ini ke OnClick() tombol Menu di Inspector
    public void OnMenuButton()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}