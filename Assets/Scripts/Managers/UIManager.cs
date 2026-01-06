//using UnityEngine;
//using TMPro;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.UI;
//public class UIManager : MonoBehaviour
//{
//    public TextMeshProUGUI levelText;
//    public TextMeshProUGUI currencyText;
//    public TextMeshProUGUI ticketsText;
//    public TextMeshProUGUI bonusTicketText;
//    public TextMeshProUGUI timerText;

//    [SerializeField] protected GameObject topUI;
//    [SerializeField] private GameObject loginUI;      // 로그인/회원가입 UI
//    [SerializeField] private Button logoutButton;     // TopUI의 로그아웃 버튼
//    private LogInSystem loginSystem;

//    //public ItemAcquireFx prefabItem;
//    //public RectTransform target; // CurrencyIcon의 RectTransform
//    //public Canvas canvas;
//    //public float spawnInterval = 0.1f;
//    //private Camera mainCamera;
//    protected GameObject curUI;
//    protected Dictionary<string, GameObject> uiElements = new Dictionary<string, GameObject>();

//    private void Awake()
//    {
//        Managers.Instance.UI = this;
//        Managers.Instance.InitializeManagers(this.gameObject);

//        InitializeText();

//        // 로그인 시스템 초기화
//        loginSystem = loginUI.GetComponent<LogInSystem>();

//        // 로그아웃 버튼 이벤트 연결
//        if (logoutButton != null)
//        {
//            logoutButton.onClick.AddListener(OnLogoutButtonClicked);
//        }
//    }
    
//    protected virtual void Start()
//    {
//        Managers.Instance.GameData.OnDataChanged += UpdateUI;
//        UpdateUI();

//        // 로그인 시스템 초기화
//        FirebaseAuthManager.Instance.LoginState += OnChangedState;
//        FirebaseAuthManager.Instance.Init();

//        // UI 초기 상태 설정
//        CheckLoginState();
//    }
//    private void CheckLoginState()
//    {
//        if (!IsLoggedIn())
//        {
//            ShowLoginUI();
//            // 로그인되지 않은 상태에서는 다른 UI 비활성화
//            basicUI.SetActive(false);
//            SetBuildingUIState(false);
//        }
//        else
//        {
//            // 이미 로그인된 상태라면 기본 UI 표시
//            HideLoginUI();
//            basicUI.SetActive(true);
//            topUI.SetActive(true);
//        }
//    }

//    private bool IsLoggedIn()
//    {
//        return FirebaseAuthManager.Instance.user != null;
//    }

//    private void OnLogoutButtonClicked()
//    {
//        FirebaseAuthManager.Instance.LogOut();
//        ShowLoginUI();
//    }
//    private void ShowLoginUI()
//    {
//        loginUI.SetActive(true);
//        // 다른 UI들은 비활성화
//        if (topUI != null) topUI.SetActive(false);
//    }
//    public void OnLoginSuccess()
//    {
//        // 로그인 성공 시 필요한 UI 활성화
//        if (topUI != null) topUI.SetActive(true);
//        // 다른 초기 UI 설정들...
//    }
//    private void HideLoginUI()
//    {
//        loginUI.SetActive(false);
//        // 다른 UI들은 활성화
//        if (topUI != null) topUI.SetActive(true);
//    }

   
//    private void OnDestroy()
//    {
//        Managers.Instance.GameData.OnDataChanged -= UpdateUI;
//    }

//    private void InitializeText()
//    {
//        Canvas mainUICanvas = FindObjectOfType<Canvas>();
//        TextMeshProUGUI[] allTexts = mainUICanvas.GetComponentsInChildren<TextMeshProUGUI>();

//        foreach (var text in allTexts)
//        {
//            switch (text.name)
//            {
//                case "LevelText":
//                    levelText = text;
//                    break;
//                case "CurrencyText":
//                    currencyText = text;
//                    break;
//                case "TicketText":
//                    ticketsText = text;
//                    break;
//                case "BonusTicketText":
//                    bonusTicketText = text;
//                    break;
//                case "TimerText":
//                    timerText = text;
//                    break;
//            }
//        }
//    }

//    public void UpdateUI()
//    {
//        GameDataManager data = Managers.Instance.GameData;
//        currencyText.text = data.GetCurrency().ToString();
//        levelText.text = $"Lv. {data.GetLevel()}";
//        ticketsText.text =    $"{data.GetTickets()} / {data._gameData.maxTickets}";
//        bonusTicketText.text = data.GetBonusTickets().ToString();
//    }

//    private IEnumerator CountNumber(int startNumber, int endNumber, TMP_Text text, int speedMultiplier = 4)
//    {
//        int current = startNumber;
//        int totalChange = endNumber - startNumber;
//        int increment = Mathf.Max(1, totalChange / 60 * speedMultiplier); // 최소 1로 설정

//        while (current != endNumber)
//        {
//            text.text = $"{current}";
//            if (totalChange > 0)
//            {
//                current = Mathf.Min(current + increment, endNumber);
//            }
//            else
//            {
//                current = Mathf.Max(current - increment, endNumber);
//            }
//            yield return new WaitForSeconds(1f / 60f);
//        }
//        text.text = $"{endNumber}";
//    }
//    public void SetResult(TMP_Text text, GameObject obj, int startNumber, int endNumber)
//    {
//        obj.SetActive(true);
//        text.gameObject.SetActive(true);
//        StartCoroutine(CountNumber(startNumber, endNumber, text, 1));
//    }

//    protected void RegisterUI(string uiName, GameObject uiObject, bool initiallyActive = true)
//    {
//        if (!uiElements.ContainsKey(uiName))
//        {
//            uiElements.Add(uiName, uiObject);
//            uiObject.SetActive(initiallyActive);
//        }
//    }

//    protected void ChangeUI(string newUIName)
//    {
//        if (uiElements.TryGetValue(newUIName, out GameObject newUI))
//        {
//            if (curUI != null)
//            {
//                curUI.SetActive(false);
//            }
//            newUI.SetActive(true);
//            curUI = newUI;
//        }
//    }

//    protected GameObject GetUI(string uiName)
//    {
//        if (uiElements.TryGetValue(uiName, out GameObject ui))
//        {
//            return ui;
//        }
//        return null;
//    }

//}