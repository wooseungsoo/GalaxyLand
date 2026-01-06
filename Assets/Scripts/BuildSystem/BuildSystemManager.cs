using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSystemManager : Singleton<BuildSystemManager> 
{
    [SerializeField] private Camera sceneCamera;
    
    [Header("UI")]
    public GameObject seedUI;//작물 심기 UI
    public GameObject buildingUI;//건설모드 UI

    public ObjectPlacer objectPlacer; // 설치된 오브젝트 확인 및 접근

    private Vector3Int lastDetectedPosition = Vector3Int.zero;
    private Vector3 lasPosition;
    float MaxDistance = 100f;

    [SerializeField] private LayerMask placementLayermask;
    public IBuildingState buildingState; //현재 빌드 상태(설치중인지 제거중인지)
    public Vector3Int curBuildingPosition;
    public bool curBuildingFlip = false;
    public bool removeBuilding = false;

    public Grid grid;

    Vector3 mousePos;
    public event Action Oncliked, OnExit;

    [Header("GridRange")] //그리드 설치 가능 범위 지정
    public int startX;
    public int endX;
    public int startY;
    public int endY;

    private GameObject cuActiveUI;
    private BuildingInfoUI curBuildingInfo; // 새로 추가된 변수

    bool dragging=false;
    protected override void Awake()
    {
        // base.Awake() 호출하지 않음 - 의도적으로 DontDestroyOnLoad 방지
        sceneCamera = Camera.main;
    }
    private void Start()
    {
        GameDataManager.Instance.BuildDataLoad();
    }
    public bool IsPointerOverUI() 
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
   
    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(sceneCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 50f, 1 << 7);
        CheckDrag(hit);
       
        if (buildingState == null ) //건설중이 아닐때 건물 클릭시
        {
            BuildingInfoCheck(hit);
        }
        else if(dragging == true) //건설과 구조물 삭제
        {
            BuildingSetUp(hit);
        }
        
    }
    private void BuildingInfoCheck(RaycastHit2D hit)
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (hit.collider != null && hit.collider.gameObject.TryGetComponent<Farm>(out Farm farm) )//밭 클릭시
        {
            if(farm.seedData==null)//아무것도 안심겨진 밭이라면 씨앗UI활성화
            {
                seedUI.SetActive(true);
            }
            else if (farm.IsGrown == false)//심겨져있는 거라면 시간 UI 활성화
            {
                if (cuActiveUI != null) cuActiveUI.SetActive(false);//이미 활성화 되있는 타이머가 있다면 끔

                cuActiveUI = farm.timer;
                cuActiveUI.SetActive(true);
            }
        }
        else if(hit.collider != null && hit.collider.gameObject.TryGetComponent<BuildingObj>(out BuildingObj building))//건물 클릭시
        {
            if(building.IsCompelete==false)
            {
                if (cuActiveUI != null) cuActiveUI.SetActive(false);
                cuActiveUI = building.time;
                cuActiveUI.SetActive(true);
            }
            if (curBuildingInfo != null)
            {
                curBuildingInfo.Hide();//우승수 추가 코드
                curBuildingInfo = building.infoUI;
                curBuildingInfo.ShowInfo(building.buildingName, building.buildingDescription);
            }
        }
        else// 아무것도 아닌곳을 클릭시 활성화된 UI 끔
        {
            // 기존 코드 유지
            if (cuActiveUI != null)
            {
                cuActiveUI.SetActive(false);
                cuActiveUI = null;
            }
            // 건물 정보 UI 숨기기 추가
            if (curBuildingInfo != null)
            {
                curBuildingInfo.Hide();
                curBuildingInfo = null;
            }
        }
    }

    private void BuildingSetUp(RaycastHit2D hit)//건물 이동
    {
        if (!IsPointerOverUI())
        {
            if (lastDetectedPosition != GetGridPosition() && removeBuilding == false)
            {
                buildingState.UpdateState(GetGridPosition());
                lastDetectedPosition = GetGridPosition();
            }
            else if (removeBuilding == true)
            {
                Oncliked?.Invoke();
            }
        }
    }

    public Vector2 GetSelectedMapPosition()// 그리드 맵의 포지션 가져오기
    {
        mousePos = Input.mousePosition;
        mousePos = sceneCamera.ScreenToWorldPoint(mousePos);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, transform.forward, MaxDistance, placementLayermask);

        if (hit)
        {
            lasPosition =hit.point;
        }

        return lasPosition;
    }
    public Vector3Int GetGridPosition()
    {
        Vector2 mousePosition = GetSelectedMapPosition();//마우스의 그리드 좌표 할당
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);//월드좌표를 그리드 좌표로 변환
        curBuildingPosition = gridPosition;

        return gridPosition;
    }
    private Vector3 GridToWorldPosition(Vector3Int position)
    {
        Vector3 gridPosition = grid.CellToWorld(position);//그리드좌표를  월드좌표로 변환
        return gridPosition;
    }
    public void BuildObject()
    {
        Oncliked?.Invoke();
    }
    public void ExitBuildSystem()
    {
        OnExit?.Invoke();
        buildingState = null;
        buildingUI.SetActive(false);
    }

    private void CheckDrag(RaycastHit2D hit)
    {
        if(Input.touchCount==1)
        {
            if(hit.collider!=null&&Input.GetTouch(0).phase==TouchPhase.Moved)
            {
                dragging= true;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                dragging= false;
            }
        }
    }

    public void BuildDataLoad(BuildingSaveData data)
    {
         objectPlacer.PlaceLoadedObject(data, GridToWorldPosition(data.position), data.position, false,data.isComplete);
    }
    //public void Initialize()
    //{
    //    sceneCamera = Camera.main;  // 현재 씬의 메인 카메라로 재설정
    //    // 기타 필요한 초기화
    //}

}
