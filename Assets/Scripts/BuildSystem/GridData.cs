using System;
using System.Collections.Generic;
using UnityEngine;

// 배치한 타일 확인용
public class GridData
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new Dictionary<Vector3Int, PlacementData>();//

    public void AddObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex)//오브젝트 설치
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);//건물 크기만큼 점령할 그리드 계산
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);//설치한 건물데이터 생성

        foreach (var pos in positionToOccupy)
        {
            placedObjects[pos] = data;
        }
    }
    public void AddLoadedObjectAt(Vector3Int gridPosition, Vector2Int objectSize, int ID, int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        PlacementData data = new PlacementData(positionToOccupy, ID, placedObjectIndex);
        foreach (var pos in positionToOccupy)
        {
            placedObjects[pos] = data;
        }
    }
    private List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize)//건물 설치 좌표 계산
    {
        List<Vector3Int> returnVal = new List<Vector3Int>();

        for(int x=0; x< objectSize.x; x++) //건물의  x, y만큼 처리
        {
            for(int y=0; y< objectSize.y; y++)
            {
                returnVal.Add(gridPosition + new Vector3Int(x, y, 0));
            }
        }
        return returnVal;
    }
    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)//설치좌표에 건설된 빌딩이 있는지 체크
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);

        foreach (var pos in positionToOccupy)
        {
            if (placedObjects.ContainsKey(pos))//해당 좌표가 이미 점령되어있다면
            {
                return false;
            }
            //그리드의 범위 체크
            if (pos.x< BuildSystemManager.Instance.startX || pos.x> BuildSystemManager.Instance.endX|| pos.y < BuildSystemManager.Instance.startY || pos.y > BuildSystemManager.Instance.endY)
            {
                return false;
            }
        }
        return true;
    }

    internal int GetRepresentationIndex(Vector3Int gridPosition)//설치된 오브젝트 찾기
    {
        if(placedObjects.ContainsKey(gridPosition) ==false) return -1;
        return placedObjects[gridPosition].PlacedObjectIndex;
    }

    internal void RemoveObjectAt(Vector3Int gridPosition)
    {
        foreach (var pos in placedObjects[gridPosition].occpiedPositons)
        {
            placedObjects.Remove(pos);
        }
    }
}

public class PlacementData 
{
    public List<Vector3Int> occpiedPositons;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }  

    public PlacementData(List<Vector3Int> occpiedPositons, int iD, int placedObjectIndex)
    {
        this.occpiedPositons = occpiedPositons;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}
