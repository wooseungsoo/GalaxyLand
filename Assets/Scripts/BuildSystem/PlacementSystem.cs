using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    private BuildSystemManager buildSystem; //마우스 위치 확인

    [SerializeField] private ObjectDatabaseSO database; //임시로 만든 건물 데이터 베이스SO
    [SerializeField] private GameObject gridVisualization;//배치 활성화시 보이는 격자 패턴바닥

    private GridData floorData, buildingData; //설치된 바닥 판정용
    [SerializeField] private ObjectPlacer objectPlacer;

    [SerializeField] private PreviewSystem preview;
    private Vector3Int lastDetectedPosition = Vector3Int.zero;


    //중복되는 변수 많은데 차라리 전역으로 빼주는게 효율적인지 좀 더 고민해보기
    private void Awake()
    {
        floorData = new GridData();
        buildingData = new GridData();
    }

    private void Start()
    {
        buildSystem = BuildSystemManager.Instance;
        StopPlacement();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();

        gridVisualization.SetActive(true);//격자 보이게

        buildSystem.buildingState = new PlacementState(ID, buildSystem.grid, preview, database, floorData, buildingData, objectPlacer);

        
        buildSystem.Oncliked += PlaceStructure;
        buildSystem.OnExit += StopPlacement;
    }

    public  void StartRemoving()
    {
        StopPlacement();

        buildSystem.removeBuilding = true;
        gridVisualization.SetActive(true);
        buildSystem.buildingState = new RemovingState(buildSystem.grid, preview, floorData, buildingData, objectPlacer);
        buildSystem.Oncliked += PlaceStructure;
        buildSystem.OnExit += StopPlacement;
    }
    public void StopPlacement()
    {
        if (buildSystem.buildingState == null) return;

        buildSystem.removeBuilding = false;
        gridVisualization.SetActive(false);
        buildSystem.buildingState.EndState();

        buildSystem.Oncliked -= PlaceStructure;
        buildSystem.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        buildSystem.buildingState = null;
    }

    private void PlaceStructure() //건물 선택 후 마우스 클릭시 실행
    {
        buildSystem.buildingState.OnAction(buildSystem.curBuildingPosition);
    }

    public void PlaceLoadedStructure(ObjectData objectData, Vector3Int position, bool isComplete)
    {
        Debug.Log($"PlaceLoadedStructure 시작 - Position: {position}, ObjectID: {objectData.ID}, IsComplete: {isComplete}");

        if (buildSystem == null)
        {
            Debug.LogError("BuildSystem이 null입니다");
            return;
        }

        if (objectPlacer == null)
        {
            Debug.LogError("ObjectPlacer가 null입니다");
            return;
        }

        // 배치 가능 여부 체크
        GridData selectedData = objectData.ID <= 0 ? floorData : buildingData;
        if (!selectedData.CanPlaceObjectAt(position, objectData.Size))
        {
            Debug.LogError($"Position {position}에 건물을 배치할 수 없습니다. Size: {objectData.Size}");
            return;
        }

        Debug.Log($"건물 배치 시작: {objectData.Name}");
        int index = objectPlacer.PlaceLoadedObject(
            objectData,
            buildSystem.grid.CellToWorld(position),
            position,
            false,
            isComplete
        );

        if (index >= 0)
        {
            selectedData.AddLoadedObjectAt(position, objectData.Size, objectData.ID, index);
            Debug.Log($"건물 배치 완료: Index={index}, Position={position}");
        }
        else
        {
            Debug.LogError("건물 배치 실패");
        }
    }
}
