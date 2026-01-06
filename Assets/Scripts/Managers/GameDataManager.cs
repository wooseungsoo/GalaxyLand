using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Utilities;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameDataManager : Singleton<GameDataManager>
{
    [System.Serializable]
    public class GameData
    {
        public string lastLoggedInUserId;
        public DateTime lastLoginTime;

        public int Currency;
        public int Level;
        public int BonusTickets;
        public int maxTickets;
        public Dictionary<string, int> crops = new Dictionary<string, int>();
        [field: SerializeField]
        public int Tickets { get; private set; }

        [SerializeField] private string _loginTimeString;
        [SerializeField] private string _lastLoginTimeString;
        [SerializeField] private string _currentTimeString;

        public DateTime LoginTime
        {
            get { return string.IsNullOrEmpty(_loginTimeString) ? DateTime.MinValue : DateTime.Parse(_loginTimeString); }
            set { _loginTimeString = value.ToString("O"); }
        }

        public DateTime LastLoginTime
        {
            get { return string.IsNullOrEmpty(_lastLoginTimeString) ? DateTime.MinValue : DateTime.Parse(_lastLoginTimeString); }
            set { _lastLoginTimeString = value.ToString("O"); }
        }

        public DateTime CurrentTime
        {
            get { return string.IsNullOrEmpty(_currentTimeString) ? DateTime.MinValue : DateTime.Parse(_currentTimeString); }
            set { _currentTimeString = value.ToString("O"); }
        }

        public void SetTickets(int value)
        {
            Tickets = Mathf.Clamp(value, 0, maxTickets);
        }

        public void AddTickets(int amount)
        {
            SetTickets(Tickets + amount);
        }
        public GameData(int currency, int level, int tickets, int bonusTickets, int maxTickets, DateTime time)
        {
            Currency = currency;
            Level = level;
            SetTickets(tickets);
            BonusTickets = bonusTickets;
            this.maxTickets = maxTickets;
            LoginTime = time;
            LastLoginTime = time;
            CurrentTime = time;
        }

        public GameData() { }
    }

    public event Action OnDataChanged;

    [SerializeField] public GameData _gameData;


    // 초기 데이터 값 설정
    private int initialCurrency = 0;
    private int initialLevel = 1;
    private int initialTickets = 0;
    private int initialBonusTickets = 0;
    private int initialMaxTickets = 30;


    

    //private GameData cachedLoadedData = null;
    //private bool needToLoadBuildings = false;
    private bool isInitialized = false;
    //private Queue<Action> pendingActions = new Queue<Action>();
    protected override void Awake()
    {
        base.Awake();
    
        StartCoroutine(InitializeCoroutine());
        //LoadData();
    }

   

    


    private IEnumerator InitializeCoroutine()
    {
        // 필요한 모든 시스템이 초기화될 때까지 대기
        yield return new WaitUntil(() =>
            BuildSystemManager.Instance != null &&
            BuildSystemManager.Instance.grid != null);

        isInitialized = true;

        // 대기 중인 작업 실행
        //while (pendingActions.Count > 0)
        //{
        //    pendingActions.Dequeue()?.Invoke();
        //}
        //LoadData();
    }


    private void OnApplicationQuit()
    {
        _gameData.LastLoginTime = TimeManager.Instance.GetAdjustedTime();
        SaveData();
    }
    //public void LoadData()
    //{
    //    if (!isInitialized)
    //    {
    //        //pendingActions.Enqueue(() => LoadData());
    //        return;
    //    }

       
    //}


    public void OnBuildingsDataLoaded(Dictionary<string, BuildingSaveData> buildings)
    {
        if (buildings == null || buildings.Count == 0) return;

        var placementSystem = FindObjectOfType<PlacementSystem>();
        if (placementSystem == null)
        {
            Debug.LogError("PlacementSystem을 찾을 수 없습니다");
            return;
        }

        foreach (var building in buildings)
        {
            Vector3Int position = building.Value.position;
            ObjectData buildingData = GetBuildingDataById(building.Value.ID);
            if (buildingData != null)
            {
                placementSystem.PlaceLoadedStructure(
                    buildingData,
                    position,
                    building.Value.isComplete
                );
            }
        }
    }

    public void BuildDataLoad()
    {
        string userId = FirebaseAuthManager.Instance.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            FirebaseDatabaseManager.Instance.LoadUserBuildings(userId, OnBuildingsDataLoaded);
        }

    }

    
    


    public void SaveData()
    {
        _gameData.CurrentTime = TimeManager.Instance.GetAdjustedTime();

        // 로컬 저장
        string jsonData = JsonUtility.ToJson(_gameData);
        PlayerPrefs.SetString("GameData", jsonData);
        PlayerPrefs.Save();

        // Firebase 저장
        if (FirebaseAuthManager.Instance.UserId != null)
        {
            FirebaseDatabaseManager.Instance.SavePlayerData(
                FirebaseAuthManager.Instance.UserId,
                _gameData
            );
        }

        OnDataChanged?.Invoke();
    }

    public void InitializeData()
    {
        DateTime now = TimeManager.Instance.GetAdjustedTime();
        _gameData = new GameData(
            initialCurrency,
            initialLevel,
            initialTickets,
            initialBonusTickets,
            initialMaxTickets,
            now
        );
        SaveData();
    }


    public void ResetData()
    {
        PlayerPrefs.DeleteKey("GameData");
        InitializeData();
    }

    public void ResetDataFromInspector()
    {
        ResetData();
    }



    // Currency 관련 메서드
    public int GetCurrency() => _gameData.Currency;
    public void SetCurrency(int value)
    {
        if (_gameData.Currency != value)
        {
            _gameData.Currency = value;
            SaveData();
        }
    }
    public void AddCurrency(int amount)
    {
        if (amount != 0)
        {
            int oldValue = _gameData.Currency;
            _gameData.Currency += amount;
            SaveData();

            // UI 업데이트를 위해 SetResult 호출
            TMP_Text currencyText = HousingSceneUI.Instance.currencyText;
            GameObject currencyObject = currencyText.gameObject;
            HousingSceneUI.Instance.SetResult(currencyText, currencyObject, oldValue, _gameData.Currency);
        }
    }

    // Level 관련 메서드
    public int GetLevel() => _gameData.Level;
    public void SetLevel(int value)
    {
        if (_gameData.Level != value)
        {
            _gameData.Level = value;
            SaveData();
        }
    }
    public void AddLevel(int amount)
    {
        if (amount != 0)
        {
            _gameData.Level += amount;
            SaveData();
        }
    }

    // Tickets 관련 메서드
    public int GetTickets() => _gameData.Tickets;
    public void SetTickets(int value)
    {
        _gameData.SetTickets(value);
        SaveData();
    }
    public void AddTickets(int amount)
    {
        _gameData.AddTickets(amount);
        SaveData();
    }

    // BonusTickets 관련 메서드
    public int GetBonusTickets() => _gameData.BonusTickets;
    public void SetBonusTickets(int value)
    {
        if (_gameData.BonusTickets != value)
        {
            _gameData.BonusTickets = value;
            SaveData();
        }
    }
    public void AddBonusTickets(int amount)
    {
        if (amount != 0)
        {
            int oldValue = _gameData.BonusTickets;
            _gameData.BonusTickets += amount;
            SaveData();

            // UI 업데이트를 위해 SetResult 호출
            TMP_Text bonusTicketText = HousingSceneUI.Instance.bonusTicketText;
            GameObject bonusTicketObject = bonusTicketText.gameObject;
            HousingSceneUI.Instance.SetResult(bonusTicketText, bonusTicketObject, oldValue, _gameData.BonusTickets);
        }
    }

    public void UpdateTimeData(DateTime loginTime, DateTime currentTime, DateTime lastLoginTime)
    {
        _gameData.LoginTime = loginTime;
        _gameData.CurrentTime = currentTime;
        _gameData.LastLoginTime = lastLoginTime;
        SaveData();
    }
    public void AddCrop(string cropName, int amount = 1)
    {
        if (!_gameData.crops.ContainsKey(cropName))
        {
            _gameData.crops[cropName] = 0;
        }
        _gameData.crops[cropName] += amount;
        SaveData();
    }

    // 작물 개수 확인 메서드
    public int GetCropCount(string cropName)
    {
        if (_gameData.crops.ContainsKey(cropName))
        {
            return _gameData.crops[cropName];
        }
        return 0;
    }

    public void OnUserLoggedIn(string userId)
    {
        if (_gameData == null)
        {
            _gameData = new GameData();
        }
        _gameData.lastLoggedInUserId = userId;
        _gameData.lastLoginTime = DateTime.Now;
        SaveData();  // 로그인 정보 저장
        
            //LoadData();
        
    }
    public void ClearUserData()
    {
        _gameData.lastLoggedInUserId = string.Empty;
        _gameData.lastLoginTime = DateTime.MinValue;
        SaveData();
    }


    public ObjectData GetBuildingDataById(int id)
    {
        ObjectDatabaseSO database = AddressableManager.Instance.GetAddressable<ObjectDatabaseSO>("BuildingObjectData");
        if (database == null || database.objectData == null)
        {
            Debug.LogError("BuildingObjectData를 찾을 수 없습니다.");
            return null;
        }

        ObjectData buildingData = database.objectData.Find(data => data.ID == id);
        if (buildingData == null)
        {
            Debug.LogError($"ID {id}에 해당하는 건물을 찾을 수 없습니다.");
        }

        return buildingData;
    }

}