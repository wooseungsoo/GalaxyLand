using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovingState : IBuildingState
{
    private int gameObjectIndex = -1;//건물 클릭 했는지 안했는지 판단하는 변수
    Grid grid;
    PreviewSystem previewSystem;
    GridData floorData;
    GridData buildingData;
    ObjectPlacer objectPlacer;

    public RemovingState(Grid grid, PreviewSystem previewSystem, GridData floorData, GridData buildingData, ObjectPlacer objectPlacer)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.floorData = floorData;
        this.buildingData = buildingData;
        this.objectPlacer = objectPlacer;

        previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        previewSystem.StopShowingPreiview();
    }

    public void OnAction(Vector3Int gridPosition)//해당 마우스 클릭 좌표에 건물이나 바닥이 설치되있는지 확인
    {
        GridData selectedData = null;
        if (buildingData.CanPlaceObjectAt(gridPosition, Vector2Int.one) == false)
        {
            selectedData = buildingData;
        }
        else if (floorData.CanPlaceObjectAt(gridPosition, Vector2Int.one) == false)
        {
            selectedData = floorData;
        }

        if(selectedData == null)
        {
            //경고를 넣어야함
        }
        else
        {
            gameObjectIndex = selectedData.GetRepresentationIndex(gridPosition);
            if (gameObjectIndex == -1) return;

            if (selectedData == buildingData) // 건물인 경우에만 Firebase 업데이트
            {
                FirebaseDatabaseManager.Instance.RemoveBuilding(
                    FirebaseAuthManager.Instance.UserId,
                    gridPosition
                );
            }

            selectedData.RemoveObjectAt(gridPosition);
            objectPlacer.RemoveObjectAt(gameObjectIndex);
        }

        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition));
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition)//해당 좌표에 건물이 있는지 체크
    {
        return !(buildingData.CanPlaceObjectAt(gridPosition, Vector2Int.one) && buildingData.CanPlaceObjectAt(gridPosition, Vector2Int.one));
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity);
    }
}
