using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
public class LogInSystem : MonoBehaviour
{
    
    public TMP_InputField email;
    public TMP_InputField password;

    public TMP_Text outputText;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    private void Awake()
    {
        InitializeFirebase();
    }
    // Start is called before the first frame update
    void Start()
    {
        FirebaseAuthManager.Instance.LoginState += OnChangedState;
        
        outputText.text = "Eunsu Yang Fighting: ";
        if (loginButton != null) loginButton.onClick.AddListener(LogIn);
        if (registerButton != null) registerButton.onClick.AddListener(Create);

        // 패스워드 필드 설정
        password.contentType = TMP_InputField.ContentType.Password;

       
    }
    private void OnDestroy()
    {
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.LoginState -= OnChangedState;
        }
    }
    private void OnChangedState(bool sign)
    {
        if (sign)
        {
            // 로그인 성공
            outputText.text = "Login Success: " + FirebaseAuthManager.Instance.UserId;
            // 로그인 정보 저장 (자동 로그인용)
            PlayerPrefs.SetString("UserEmail", email.text);
            PlayerPrefs.SetString("UserPassword", password.text);
            PlayerPrefs.Save();

            StartCoroutine(HideLoginUIAfterDelay());
        }
        else
        {
            // 로그아웃
            outputText.text = "Logout";
        }
    }
    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase 초기화
                FirebaseAuthManager.Instance.Init();
                FirebaseDatabaseManager.Instance.Init();
                // 자동 로그인 시도
                TryAutoLogin();
            }
        });
    }

    private void TryAutoLogin()
    {
        string savedEmail = PlayerPrefs.GetString("UserEmail", "");
        string savedPassword = PlayerPrefs.GetString("UserPassword", "");
        if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
        {
            FirebaseAuthManager.Instance.Login(savedEmail, savedPassword);
        }
    }
    private IEnumerator HideLoginUIAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
        LoadingSceneManager.Instance.LoadScene(LoadingSceneManager.SceneType.Build);  // 무조건 게임 씬으로 이동
    }

    public void Create()
    {
        if (string.IsNullOrEmpty(email.text) || string.IsNullOrEmpty(password.text))
        {
            outputText.text = "Input Eamil, Password.";
            return;
        }
        FirebaseAuthManager.Instance.Create(email.text, password.text);
    }

    public void LogIn()
    {
        if (string.IsNullOrEmpty(email.text) || string.IsNullOrEmpty(password.text))
        {
            outputText.text = "Input Eamil, Password.";
            return;
        }
        FirebaseAuthManager.Instance.Login(email.text, password.text);
    }
    public void LogOut()
    {
        FirebaseAuthManager.Instance.LogOut();
        outputText.text = "Logout success.";
        email.text = "";
        password.text = "";

        // 로그아웃 시 저장된 로그인 정보 삭제
        PlayerPrefs.DeleteKey("UserEmail");
        PlayerPrefs.DeleteKey("UserPassword");
        PlayerPrefs.Save();
    }
}
