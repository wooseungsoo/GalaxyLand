using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildingState
{
    public void OnAction(Vector3Int vector3Int);
    public void UpdateState(Vector3Int vector3Int);
    public void EndState();

}
public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;//건물 클릭 했는지 안했는지 판단하는 변수
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    ObjectDatabaseSO database;
    GridData floorData;
    GridData buildingData;
    ObjectPlacer objectPlacer;

    public PlacementState(int iD, Grid grid, PreviewSystem previewSystem, ObjectDatabaseSO database, GridData floorData, GridData buildingData, ObjectPlacer objectPlacer)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.floorData = floorData;
        this.buildingData = buildingData;
        this.objectPlacer = objectPlacer;

        //Find함수를 덜 쓰는 방법을 하고싶은데 이건 나중에 데이터처리 방식을 바꾸면 갈아엎어질거 같긴하다
        selectedObjectIndex = database.objectData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1)
        {
            previewSystem.StartShowingPlacementPreview(database.objectData[selectedObjectIndex].Name, database.objectData[selectedObjectIndex].Size);
        }

    }

    public void EndState()
    {
        previewSystem.StopShowingPreiview();
    }

    public void OnAction(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);//설치 가능 확인
       
        if (placementValidity == false)//설치 불가시 리턴
        {
            return;
        }

        int index = objectPlacer.PlaceObject(database.objectData[selectedObjectIndex], grid.CellToWorld(gridPosition),gridPosition,BuildSystemManager.Instance.curBuildingFlip);
        DateTime completionTime = TimeManager.Instance.GetAdjustedTime().AddSeconds(database.objectData[selectedObjectIndex].constructTime);

        FirebaseDatabaseManager.Instance.SaveBuildingData(
        FirebaseAuthManager.Instance.UserId,  // 현재 로그인한 사용자 ID
        gridPosition,                         // 건물 위치
        database.objectData[selectedObjectIndex],  // 건물 데이터
        completionTime
    );

        GridData selectedData = database.objectData[selectedObjectIndex].ID <= 0 ? floorData : buildingData; //현재 데이터가 건물인지 바닥인지 확인하는용?인듯
        selectedData.AddObjectAt(gridPosition, database.objectData[selectedObjectIndex].Size, database.objectData[selectedObjectIndex].ID, index);//리스트에 건물, 바닥사이즈넣기

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);
    
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectObjectIndex) //배치 종류 확인
    {
        GridData selectedData = database.objectData[selectObjectIndex].ID <= 0 ? floorData : buildingData;

        return selectedData.CanPlaceObjectAt(gridPosition, database.objectData[selectObjectIndex].Size);//그리드에 배치 가능한지 체크
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);//배치 종류 확인
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition),placementValidity);//미리보기 건물 좌표 갱신
    }


}
