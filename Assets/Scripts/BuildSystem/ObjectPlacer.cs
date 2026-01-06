using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct BuildData
{
    public GameObject building;
    public DateTime constructStartTime;// 건설 시작한 시간
    public DateTime remainTime;//남은 쿨타임
}
public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> placedGameObject   = new List<GameObject>();//맵에 배치한 오브젝트들
    private List<BuildData> constructGameObject = new List<BuildData>();//건설중인 오브젝트들

    public int PlaceObject(ObjectData objectData, Vector3 position, Vector3Int gridPosition,bool flipX)
    {
        GameObject obj= Instantiate(AddressableManager.Instance.GetAddressable<GameObject>(objectData.Name));//어드레서블 동기로 프리팹 가져오기

        if (obj.TryGetComponent<Farm>(out Farm farm)) //밭만 부모 설정
        {
            obj.transform.parent = transform.GetChild(1);
        }
        else
        {
            obj.transform.parent = transform.GetChild(0);
        }

        SpriteRenderer sprite_0 = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer sprite_1 = obj.transform.GetChild(1).GetComponent<SpriteRenderer>();

        //뒤에있는 이미지가 앞으로 넘어오지 않도록 레이어 순서 정렬
        sprite_0.sortingOrder = (int)(-gridPosition.x - gridPosition.y);
        sprite_0.flipX = flipX;
        sprite_1.sortingOrder = (int)(-gridPosition.x - gridPosition.y);
        sprite_1.flipX = !flipX;


        //캔버스UI 설정
        obj.GetComponent<BuildingObj>().setBuildingUI.gameObject.SetActive(false);
        //  obj.GetComponent<BuildingObj>().time.gameObject.SetActive(true);
        obj.transform.position = position;

        //부모 설정
        placedGameObject.Add(obj);
        StartCoroutine(CoolTime(obj, objectData.constructTime));
        return placedGameObject.Count - 1;
    }

    internal void RemoveObjectAt(int gameObjectIndex)
    {
        if (placedGameObject.Count <= gameObjectIndex || placedGameObject[gameObjectIndex] == null) return;

        BuildingObj buildingUI = placedGameObject[gameObjectIndex].GetComponent<BuildingObj>();
       // buildingUI.deleteUI.SetActive(true); 

        Destroy(placedGameObject[gameObjectIndex]);
        placedGameObject[gameObjectIndex] = null;

    }

    //건설 쿨타임
    private IEnumerator CoolTime(GameObject obj, int time)
    {
        int[] CompleteTime = Util.CalTime(time);


        TimeSpan timeSpan =new TimeSpan(CompleteTime[0], CompleteTime[1], CompleteTime[2]);//건설시간
        TimeSpan oneSecond =new TimeSpan(0, 0, 1);//건설시간
        DateTime startTime = TimeManager.Instance.GetAdjustedTime();//건설 시작 시간
        DateTime remainTime = startTime + timeSpan; //지나야 하는 시간

        
        constructGameObject.Add(new BuildData { building = obj, constructStartTime = TimeManager.Instance.GetAdjustedTime(), remainTime = remainTime });

        TextMeshProUGUI text = obj.GetComponent<BuildingObj>().time.transform.GetChild(0).GetComponent<TextMeshProUGUI>();


        while (true)
        {
            if (obj == null) break;

            text.text = timeSpan.ToString();
            timeSpan -= oneSecond;

            if (remainTime <= TimeManager.Instance.GetAdjustedTime())
            {
                obj.GetComponent<BuildingObj>().IsCompelete = true;
                CompeleteConstruct(obj);
                break;
            }
           
            yield return new WaitForSeconds(1);
        }
    }

    private void CompeleteConstruct(GameObject obj)
    {
        //설치 오브젝트 이미지 바꿔주기
        obj.transform.GetChild(0).gameObject.SetActive(false);
        obj.transform.GetChild(1).gameObject.SetActive(true);

        obj.GetComponent<BuildingObj>().time.SetActive(false);
        //placedGameObject.Add(obj);//순서 찾아라.

        Vector3Int position = BuildSystemManager.Instance.grid.WorldToCell(obj.transform.position);  // 월드 좌표를 그리드 좌표로 변환
        FirebaseDatabaseManager.Instance.UpdateBuildingState(
            FirebaseAuthManager.Instance.UserId,  // 현재 로그인한 사용자 ID
            position,                            // 건물 위치
            true                                 // 완성 상태
        );
    }

    public void PlantAllFarm()//설치한 밭에 모든 작물이 심겨져 있는지 확인
    {
        int count = transform.GetChild(1).childCount;
        for(int i=0; i< count; i++)
        {
            if(transform.GetChild(1).GetChild(i).TryGetComponent<Farm>(out Farm farm))
            {
                if(farm.cropsSprite.gameObject.activeSelf==false)
                {
                    return;
                }
            }
        }
        BuildSystemManager.Instance.seedUI.SetActive(false); //모두 심겨져있다면 작물 심는UI 끄기

    }

    public int PlaceLoadedObject(ObjectData objectData, Vector3 position, Vector3Int gridPosition, bool flipX, bool isComplete)
    {
        Debug.Log($"PlaceLoadedObject 시작 - Name: {objectData.Name}, Position: {position}");

       
        GameObject obj = Instantiate(AddressableManager.Instance.GetAddressable<GameObject>(objectData.Name));

        if (obj == null)
        {
            Debug.LogError($"Addressable에서 {objectData.Name} 프리팹을 가져오지 못했습니다");
            return -1;
        }
        if (obj.TryGetComponent<Farm>(out Farm farm))
        {
            obj.transform.parent = transform.GetChild(1);
        }
        else
        {
            obj.transform.parent = transform.GetChild(0);
        }

        SpriteRenderer sprite_0 = obj.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer sprite_1 = obj.transform.GetChild(1).GetComponent<SpriteRenderer>();

        sprite_0.sortingOrder = (int)(-gridPosition.x - gridPosition.y);
        sprite_0.flipX = flipX;
        sprite_1.sortingOrder = (int)(-gridPosition.x - gridPosition.y);
        sprite_1.flipX = !flipX;

        obj.GetComponent<BuildingObj>().setBuildingUI.gameObject.SetActive(false);
        obj.transform.position = position;
        placedGameObject.Add(obj);

        if (isComplete)
        {
            // 이미 완성된 건물
            obj.GetComponent<BuildingObj>().IsCompelete = true;
            obj.transform.GetChild(0).gameObject.SetActive(false);
            obj.transform.GetChild(1).gameObject.SetActive(true);
            obj.GetComponent<BuildingObj>().time.SetActive(false);
        }
        else
        {
            // 건설 중인 건물은 CoolTime 시작
            StartCoroutine(CoolTime(obj, objectData.constructTime));
        }

        return placedGameObject.Count - 1;
    }
    public void ClearAllBuildings()
    {
        // 모든 건물 제거
        foreach (var obj in placedGameObject)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        placedGameObject.Clear();
    }
}
