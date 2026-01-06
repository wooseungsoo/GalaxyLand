using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class Farm : MonoBehaviour
{
    public FarmSeedSO seedData;//자라는 작물 데이터

    public SpriteRenderer farmSprite;//땅 이미지
    public SpriteRenderer cropsSprite;//작물성장 이미지

    public GameObject timer;
    TextMeshProUGUI timeTxt;
    public bool IsGrown;

    private void Start()
    {
        timeTxt = timer.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        ResetFarm(); // 초기 상태 설정
    }
    private void OnMouseDown()
    {
        if (IsGrown)
        {
            HarvestCrop();
        }
    }
    private void HarvestCrop()
    {
        if (seedData != null)
        {
            // 플레이어 데이터에 수확물 추가
            GameDataManager.Instance.AddCrop(seedData.seedName);

            // Farm 상태 초기화
            ResetFarm();
        }
    }

    private void ResetFarm()
    {
        seedData = null;
        IsGrown = false;
        timer.SetActive(false);
        cropsSprite.gameObject.SetActive(false);//작물 스프라이트 끄기
    }

    public void OnDrop(FarmSeedSO farmSeed)
    {
        seedData = farmSeed;
        cropsSprite.gameObject.SetActive(true);//작물 스프라이트 켜기
        cropsSprite.sprite = seedData.growSprites[0];
        cropsSprite.sortingOrder = farmSprite.sortingOrder + 1;
        StartCoroutine(CoolTime(Util.CalTime(seedData.timeData)));
        BuildSystemManager.Instance.objectPlacer.PlantAllFarm();

    }

    private IEnumerator CoolTime(int[] time)
    {
        TimeSpan timeSpan = new TimeSpan(time[0], time[1], time[2]);//건설
        TimeSpan oneSecond = new TimeSpan(0, 0, 1);//건설시간
        DateTime startTime = TimeManager.Instance.GetAdjustedTime();//시작 시간
        DateTime remainTime = startTime + timeSpan; //지나야 하는 시간


        while (true)
        {
            timeTxt.text = timeSpan.ToString();
            timeSpan -= oneSecond;
           

            if (remainTime <= TimeManager.Instance.GetAdjustedTime())
            {
                cropsSprite.sprite = seedData.growSprites[1];
                timer.SetActive(false);
                IsGrown = true;
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }

}
