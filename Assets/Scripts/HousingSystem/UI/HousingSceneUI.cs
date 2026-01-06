using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HousingSceneUI : Singleton<HousingSceneUI>
{
    // 텍스트 UI 요소들
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI ticketsText;
    public TextMeshProUGUI bonusTicketText;
    public TextMeshProUGUI timerText;

    // 건물 관련 UI
    [SerializeField] private BuildingSelectionPanel buildingSelectionPanel;
    [SerializeField] private BuildingInfoPanel buildingInfoPanel;
    [SerializeField] private Button buildingButton;
    [Header("Visit System UI")]
    [SerializeField] private Button visitButton;
    [SerializeField] private GameObject visitPanel;
    [SerializeField] private Transform userListContent;
    [SerializeField] private GameObject userButtonPrefab;
    [SerializeField] private Button closeVisitButton;
    [SerializeField] private Button returnHomeButton;
    [SerializeField] private GameObject visitingStatusPanel;
    [SerializeField] private TextMeshProUGUI visitingUserText;

    // 로그인 관련 UI

    [SerializeField] private Button logoutButton;
    

    // UI 관리
    protected GameObject curUI;
    protected Dictionary<string, GameObject> uiElements = new Dictionary<string, GameObject>();

    protected override void Awake()
    {
        base.Awake();
        InitializeText();

        // 로그인 시스템 초기화
        
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        }
    }

    private void Start()
    {
        // 데이터 변경 이벤트 구독
        GameDataManager.Instance.OnDataChanged += UpdateUI;
       

        // Building UI 이벤트 구독
        buildingSelectionPanel.OnBuildingHovered += ShowBuildingInfo;
        buildingSelectionPanel.OnBuildingUnhovered += HideBuildingInfo;

        // UI 초기화


        // 버튼 이벤트 연결

        //closeButton.onClick.AddListener(CloseAllBuildingPanels);

        SetupVisitSystemEvents();

        UpdateUI();
        UpdateVisitingStatus();

        // 로그인 상태 체크

        // 저장된 로그인 정보가 있다면 자동 로그인 시도

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (GameDataManager.Instance != null)  // Instance가 이미 있을 때만 접근
        {
            GameDataManager.Instance.OnDataChanged -= UpdateUI;
        }
        buildingSelectionPanel.OnBuildingHovered -= ShowBuildingInfo;
        buildingSelectionPanel.OnBuildingUnhovered -= HideBuildingInfo;
        
        //closeButton.onClick.RemoveListener(CloseAllBuildingPanels);
        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveListener(OnLogoutButtonClicked);
        }
    }

    #region UI Initialization
    private void InitializeText()
    {
        Canvas mainUICanvas = FindObjectOfType<Canvas>();
        TextMeshProUGUI[] allTexts = mainUICanvas.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            switch (text.name)
            {
                case "LevelText": levelText = text; break;
                case "CurrencyText": currencyText = text; break;
                case "TicketText": ticketsText = text; break;
                case "BonusTicketText": bonusTicketText = text; break;
                case "TimerText": timerText = text; break;
            }
        }
    }

    
    #endregion

    #region UI State Management
    
    public void UpdateUI()
    {
        GameDataManager data = GameDataManager.Instance;
        currencyText.text = data.GetCurrency().ToString();
        levelText.text = $"Lv. {data.GetLevel()}";
        ticketsText.text = $"{data.GetTickets()} / {data._gameData.maxTickets}";
        bonusTicketText.text = data.GetBonusTickets().ToString();
    }
    #endregion

    #region Building UI Methods
    
    private void ShowBuildingInfo(ObjectData data, Vector2 position)
    {
        buildingInfoPanel.ShowInfo(data, position);
    }

    private void HideBuildingInfo()
    {
        buildingInfoPanel.Hide();
    }

   
    #endregion

    #region Login System Methods
  



   

    private void OnLogoutButtonClicked()
    {
        FirebaseAuthManager.Instance.LogOut();
        LoadingSceneManager.Instance.LoadScene(LoadingSceneManager.SceneType.Splash);
    }

  

    
    #endregion

    #region UI Registration
    
    #endregion

    #region UI Animation
    private IEnumerator CountNumber(int startNumber, int endNumber, TMP_Text text, int speedMultiplier = 4)
    {
        int current = startNumber;
        int totalChange = endNumber - startNumber;
        int increment = Mathf.Max(1, totalChange / 60 * speedMultiplier);

        while (current != endNumber)
        {
            text.text = $"{current}";
            if (totalChange > 0)
            {
                current = Mathf.Min(current + increment, endNumber);
            }
            else
            {
                current = Mathf.Max(current - increment, endNumber);
            }
            yield return new WaitForSeconds(1f / 60f);
        }
        text.text = $"{endNumber}";
    }

    public void SetResult(TMP_Text text, GameObject obj, int startNumber, int endNumber)
    {
        obj.SetActive(true);
        text.gameObject.SetActive(true);
        StartCoroutine(CountNumber(startNumber, endNumber, text, 1));
    }
    #endregion

    private void SetupVisitSystemEvents()
    {
        visitButton.onClick.AddListener(OpenVisitPanel);
        closeVisitButton.onClick.AddListener(CloseVisitPanel);
        returnHomeButton.onClick.AddListener(OnReturnHomeClicked);
    }

    private void CleanupVisitSystemEvents()
    {
        visitButton.onClick.RemoveListener(OpenVisitPanel);
        closeVisitButton.onClick.RemoveListener(CloseVisitPanel);
        returnHomeButton.onClick.RemoveListener(OnReturnHomeClicked);
    }

    // Visit 시스템 메서드들
    private void OpenVisitPanel()
    {
        visitPanel.SetActive(true);
        visitButton.gameObject.SetActive(false);
        buildingButton.gameObject.SetActive(false);
        LoadUserList();
    }

    private void CloseVisitPanel()
    {
        visitPanel.SetActive(false);
        visitButton.gameObject.SetActive(true);
        buildingButton.gameObject.SetActive(true);
    }

    private void LoadUserList()
    {
        // 기존 목록 클리어
        foreach (Transform child in userListContent)
        {
            Destroy(child.gameObject);
        }

        // 방문 가능한 유저 목록 로드
        VisitManager.Instance.LoadVisitableUsers((users) => {
            foreach (var user in users)
            {
                CreateUserButton(user);
            }
        });
    }

    private void CreateUserButton(UserProfile user)
    {
        GameObject buttonObj = Instantiate(userButtonPrefab, userListContent);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        text.text = $"Visit {user.UserId}";
        button.onClick.AddListener(() => OnVisitButtonClicked(user.UserId));
    }

    private void OnVisitButtonClicked(string userId)
    {
        VisitManager.Instance.StartVisit(userId);
        CloseVisitPanel();
        UpdateVisitingStatus();
    }

    private void OnReturnHomeClicked()
    {
        VisitManager.Instance.EndVisit();
        UpdateVisitingStatus();
    }

    private void UpdateVisitingStatus()
    {
        bool isVisiting = VisitManager.Instance.isVisiting;
        visitingStatusPanel.SetActive(isVisiting);
        returnHomeButton.gameObject.SetActive(isVisiting);

        if (isVisiting)
        {
            visitingUserText.text = $"Visiting: {VisitManager.Instance.currentVisitingUserId}";
        }
    }
}