using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TimeManager : Singleton<TimeManager>
{
    private TimeSpan _serverTimeOffset = TimeSpan.Zero;
    private string url = "https://www.naver.com";
    private const float SyncInterval = 300f; // 5분마다 동기화

    private DateTime _loginTime;
    private DateTime _currentTime;
    private DateTime _lastLoginTime;


    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        StartCoroutine(InitializeTime());
    }

    private IEnumerator InitializeTime()
    {
        yield return StartCoroutine(SyncWithServerTime());
        LoadLastLoginTime();
        _loginTime = GetAdjustedTime();
        _currentTime = _loginTime;
        UpdateGameDataTime();
        StartCoroutine(PeriodicSync());
    }

    private IEnumerator SyncWithServerTime()
    {
        DateTime beforeRequest = DateTime.Now;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        DateTime afterRequest = DateTime.Now;

        if (request.result == UnityWebRequest.Result.Success)
        {
            string dateStr = request.GetResponseHeader("date");
            if (DateTime.TryParse(dateStr, out DateTime serverTime))
            {
                TimeSpan latency = (afterRequest - beforeRequest) / 2;
                _serverTimeOffset = serverTime - afterRequest + latency;
            }
            else
            {
                Debug.LogWarning("서버 시간 파싱 실패.");
            }
        }
        else
        {
            Debug.LogError($"서버 시간 요청 실패: {request.error}");
        }
    }

    public DateTime GetAdjustedTime()
    {
        return DateTime.Now + _serverTimeOffset;
    }

    private void LoadLastLoginTime()
    {
        string lastLoginStr = PlayerPrefs.GetString("LastLoginTime", "");
        if (DateTime.TryParse(lastLoginStr, out DateTime lastLogin))
        {
            _lastLoginTime = lastLogin;
        }
        else
        {
            _lastLoginTime = GetAdjustedTime();
        }
    }

    //public void UpdateCurrentTime()
    //{
    //    _currentTime = GetAdjustedTime();
    //    UpdateGameDataTime();
    //}

    //public TimeSpan GetTimeSinceLogin()
    //{
    //    UpdateCurrentTime();
    //    return _currentTime - _loginTime;
    //}

    //public TimeSpan GetTimeSinceLastLogin()
    //{
    //    UpdateCurrentTime();
    //    return _currentTime - _lastLoginTime;
    //}

    private IEnumerator PeriodicSync()
    {
        while (true)
        {
            yield return new WaitForSeconds(SyncInterval);
            yield return StartCoroutine(SyncWithServerTime());
        }
    }


    private void UpdateGameDataTime()
    {
        GameDataManager.Instance.UpdateTimeData(_loginTime, _currentTime, _lastLoginTime);
    }

}