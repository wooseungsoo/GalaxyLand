using UnityEngine;
using System.Collections;

public class DOTweenActionObj : MonoBehaviour
{
    public ItemAcquireFx prefabItem;
    public ItemAcquireFx prefabItem2;
    public RectTransform target;
    public RectTransform target2;
    public Canvas canvas;
    public float spawnInterval = 0.1f;
    private Camera mainCamera;

    public RectTransform startTarget;
    public RectTransform startTarget2;

    void Start()
    {
        mainCamera = Camera.main;
    }


    public IEnumerator SpawnCoins()
    {
        int randCount = Random.Range(3, 8);
        for (int i = 0; i < randCount; ++i)
        {
            SpawnCoin();
            SpawnCoin2();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCoin()
    {
        Vector3 startPos = startTarget.position;
        startPos.z = 0;


        Vector3 targetWorldPos = target.position;
        targetWorldPos.z = 0;

        GameObject itemObj = PoolManager.Instance.Get(0);
        ItemAcquireFx itemFx = itemObj.GetComponent<ItemAcquireFx>();
        if (itemFx == null)
        {
            itemFx = itemObj.AddComponent<ItemAcquireFx>();
        }

        itemFx.SetPoolIndex(0);

        itemObj.transform.position = startPos;
        Vector3 startPoss = startPos + (Vector3)Random.insideUnitCircle * 0.5f;
        itemFx.Explosion(startPos, targetWorldPos, 2f);
    }

    private void SpawnCoin2()
    {
        Vector3 startPos = startTarget2.position;
        startPos.z = 0;


        Vector3 targetWorldPos = target2.position;
        targetWorldPos.z = 0;

        GameObject itemObj = PoolManager.Instance.Get(2);
        ItemAcquireFx itemFx = itemObj.GetComponent<ItemAcquireFx>();
        if (itemFx == null)
        {
            itemFx = itemObj.AddComponent<ItemAcquireFx>();
        }

        itemFx.SetPoolIndex(2);

        itemObj.transform.position = startPos;
        Vector3 startPoss = startPos + (Vector3)Random.insideUnitCircle * 0.5f;
        itemFx.Explosion(startPos, targetWorldPos, 2f);
    }
}