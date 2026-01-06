using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
    public Image touchToStart;
    public float fadeDuration = 1f;
    private Coroutine fadeCoroutine;

    [Header("Login UI")]
    [SerializeField] private GameObject loginPanel;  // 로그인/회원가입 UI 패널
    [SerializeField] private LogInSystem loginSystem;  // 로그인 시스템

    private void Awake()
    {
        // 초기 UI 설정
        loginPanel.SetActive(false);
        touchToStart.gameObject.SetActive(true);
    }

    private void Start()
    {
        fadeCoroutine = StartCoroutine(FadeInOut());
        Debug.Log("Firebase 초기화 시작");
        // 로그인 상태 변경 이벤트 구독
        FirebaseAuthManager.Instance.LoginState += OnLoginStateChanged;
    }

    private void OnDestroy()
    {
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.LoginState -= OnLoginStateChanged;
        }
    }

    private void OnLoginStateChanged(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            // 로그인 성공 시 게임 씬으로 이동
            LoadGameScene();
        }
    }

    private IEnumerator FadeInOut()
    {
        while (true)
        {
            yield return StartCoroutine(FadeImage(touchToStart, 50f / 255f, 200f / 255f));
            yield return StartCoroutine(FadeImage(touchToStart, 200f / 255f, 50f / 255f));
        }
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        Color startColor = image.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha);
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            image.color = Color.Lerp(new Color(startColor.r, startColor.g, startColor.b, startAlpha), endColor, t);
            yield return null;
        }
        image.color = endColor;
    }

    // 이미지 클릭 처리
    public void OnTouchToStartClicked()
    {
        Debug.Log("터치 버튼 클릭됨");
        if (FirebaseAuthManager.Instance == null)
        {
            Debug.LogError("FirebaseAuthManager가 초기화되지 않았습니다.");
            return;
        }
        if (string.IsNullOrEmpty(FirebaseAuthManager.Instance.UserId))
        {
            Debug.Log("로그인 필요: 로그인 UI 표시");
            // 로그인되지 않은 상태면 로그인 UI 표시
            ShowLoginUI();
        }
        else
        {
            Debug.Log("이미 로그인됨: 게임 씬으로 이동");
            // 이미 로그인된 상태면 게임 씬으로 이동
            LoadGameScene();
        }
    }

    private void ShowLoginUI()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        touchToStart.gameObject.SetActive(false);
        loginPanel.SetActive(true);
    }

    private void LoadGameScene()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        LoadingSceneManager.Instance.LoadScene(LoadingSceneManager.SceneType.Build);
    }
}