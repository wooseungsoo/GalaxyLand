using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadingSceneManager : Singleton<LoadingSceneManager>
{
    [Header("Loading UI")]
    [SerializeField] private GameObject loadingCanvasPrefab; // UI 프리팹만 참조

    private GameObject loadingCanvasInstance;
    private Slider progressBar;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI loadingText;

    private bool isVisitLoading = false;
    private string targetUserId = null;
    public enum SceneType
    {
        Splash,
        Build,
        Bingo  // 기존 SceneManager의 BingoScene도 포함
    }
    protected override void Awake()
    {
        base.Awake();
        InitializeLoadingUI();
    }


    public void LoadScene(SceneType sceneType)
    {
        StartCoroutine(LoadSceneRoutine(sceneType));
    }

    private IEnumerator LoadSceneRoutine(SceneType sceneType)
    {
        loadingCanvasInstance.SetActive(true);
        loadingText.text = "로딩 중...";

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneType.ToString() + "Scene");
        asyncLoad.allowSceneActivation = false;

        float timeElapsed = 0;
        while (asyncLoad.progress < 0.9f)
        {
            timeElapsed += Time.deltaTime;
            UpdateLoadingProgress(asyncLoad.progress);
            yield return null;
        }

        // 최소 로딩 시간 보장
        while (timeElapsed < 1.0f)
        {
            timeElapsed += Time.deltaTime;
            UpdateLoadingProgress(Mathf.Lerp(0.9f, 1f, timeElapsed));
            yield return null;
        }

        loadingCanvasInstance.SetActive(false);
        asyncLoad.allowSceneActivation = true;
    }

    private IEnumerator ShowLoadingUI() 
    {
        // 페이드 인 등의 UI 효과
        yield return null;
    }

    private void UpdateLoadingProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;
        if (progressText != null)
            progressText.text = $"{(progress * 100):F0}%";
    }

    public void StartVisitLoading(string userId)
    {
        isVisitLoading = true;
        targetUserId = userId;
        StartCoroutine(VisitLoadingRoutine());
    }

    private IEnumerator VisitLoadingRoutine()
    {
        // 로딩 UI 표시
        loadingCanvasPrefab.SetActive(true);
        loadingText.text = "친구의 마을로 이동 중...";

        // 현재 건물들 제거
        BuildSystemManager.Instance.objectPlacer.ClearAllBuildings();

        float fakeProgress = 0f;

        // 방문할 유저의 데이터를 미리 로드
        bool dataLoaded = false;
        Dictionary<string, BuildingSaveData> buildingData = null;

        FirebaseDatabaseManager.Instance.LoadUserBuildings(targetUserId, (buildings) => {
            buildingData = buildings;
            dataLoaded = true;
        });

        // 데이터 로드하는 동안 로딩바 진행
        while (!dataLoaded)
        {
            fakeProgress = Mathf.Min(fakeProgress + Time.deltaTime * 0.5f, 0.9f);
            UpdateLoadingProgress(fakeProgress);
            yield return null;
        }

        // 최소 로딩 시간 보장
        while (fakeProgress < 1.0f)
        {
            fakeProgress = Mathf.Min(fakeProgress + Time.deltaTime * 2f, 1.0f);
            UpdateLoadingProgress(fakeProgress);
            yield return null;
        }

        // 건물 배치
        if (buildingData != null)
        {
            foreach (var building in buildingData)
            {
                BuildSystemManager.Instance.BuildDataLoad(building.Value);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // 로딩 UI 숨기기
        loadingCanvasPrefab.SetActive(false);

        // 방문 상태 업데이트
        VisitManager.Instance.SetVisitingStatus(targetUserId);

        isVisitLoading = false;
        targetUserId = null;
    }
    private void InitializeLoadingUI()
    {
        // Resources 폴더에서 프리팹 로드
        loadingCanvasPrefab = Resources.Load<GameObject>("UI/LoadingCanvas");

        if (loadingCanvasPrefab == null)
        {
            Debug.LogError("LoadingCanvas 프리팹을 찾을 수 없습니다!");
            return;
        }

        // UI 생성
        loadingCanvasInstance = Instantiate(loadingCanvasPrefab);
        DontDestroyOnLoad(loadingCanvasInstance); // 씬 전환시에도 유지

        // 컴포넌트 참조
        progressBar = loadingCanvasInstance.GetComponentInChildren<Slider>();
        progressText = loadingCanvasInstance.transform.Find("ProgressText").GetComponent<TextMeshProUGUI>();
        loadingText = loadingCanvasInstance.transform.Find("LoadingText").GetComponent<TextMeshProUGUI>();

        // 초기에는 비활성화
        loadingCanvasInstance.SetActive(false);
    }
}