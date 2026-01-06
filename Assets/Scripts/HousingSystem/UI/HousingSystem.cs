using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// HousingSystem: 주택 시스템의 전반적인 로직을 관리하는 클래스
public class HousingSystem : MonoBehaviour
{
    public static HousingSystem Instance { get; private set; } // 싱글톤 인스턴스

    [SerializeField] private ObjectDatabaseSO objectDatabase; // 건물 데이터베이스 SO
    private List<ObjectData> availableBuildings = new List<ObjectData>(); // 사용 가능한 건물 목록

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeAvailableBuildings();
    }

    // 사용 가능한 건물 목록 초기화
    private void InitializeAvailableBuildings()
    {
        foreach (var objectData in objectDatabase.objectData)
        {
            if (objectData.ID != 0) // ID가 0이 아닌 것만 건물로 간주
            {
                availableBuildings.Add(objectData);
            }
        }
    }

    // 사용 가능한 건물 목록 반환
    public List<ObjectData> GetAvailableBuildings()
    {
        return availableBuildings;
    }

    // 건물 비용 계산
    public int GetBuildingCost(ObjectData building)
    {
        // TODO: 실제 게임 로직에 맞는 비용 계산 구현
        return building.ID * 100; // 예시: ID에 100을 곱한 값을 비용으로 사용
    }

    // 건물 건설 가능 여부 확인
    public bool CanBuildBuilding(ObjectData building)
    {
        int cost = GetBuildingCost(building);
        return GameDataManager.Instance.GetCurrency() >= cost;
    }

    // 건물 건설 시도
    public bool TryBuildBuilding(ObjectData building)
    {
        int cost = GetBuildingCost(building);
        if (GameDataManager.Instance.GetCurrency() >= cost)
        {
            GameDataManager.Instance.AddCurrency(-cost);
            // TODO: 실제 건물 배치 로직 구현
            return true;
        }
        return false;
    }
}