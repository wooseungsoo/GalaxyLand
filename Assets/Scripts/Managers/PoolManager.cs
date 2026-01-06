using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField] private GameObject[] _obj;
    [SerializeField] private int[] _initialSizes; // 각 오브젝트 타입의 초기 풀 크기
    private GameObject[] _parents;  // 각 오브젝트 타입별 부모 오브젝트
    private List<GameObject>[] pools;




    protected override void Awake()
    {
        base.Awake();
        InitializePools();
    }

    private void InitializePools()
    {
        int poolCount = _obj.Length;
        pools = new List<GameObject>[poolCount];
        _parents = new GameObject[poolCount];

        for (int i = 0; i < poolCount; i++)
        {
            pools[i] = new List<GameObject>();
            _parents[i] = new GameObject($"Pool_{_obj[i].name}");
            _parents[i].transform.SetParent(transform);

            int size;
            if (i < _initialSizes.Length)
            {
                size = _initialSizes[i];
            }
            else
            {
                size = 5; // 기본값 5 사용
            }

            for (int j = 0; j < size; j++)
            {
                CreateNewObject(i);
            }
        }
    }

    private GameObject CreateNewObject(int index)
    {
        GameObject newObj = Instantiate(_obj[index], _parents[index].transform);
        newObj.SetActive(false);
        pools[index].Add(newObj);
        return newObj;
    }

    public GameObject Get(int index)
    {
        if (index < 0 || index >= pools.Length)
        {
            return null;
        }

        GameObject obj = null;
        for (int i = 0; i < pools[index].Count; i++)
        {
            if (!pools[index][i].activeInHierarchy)
            {
                obj = pools[index][i];
                break;
            }
        }

        if (obj == null)
        {
            obj = CreateNewObject(index);
        }

        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj, int index)
    {
        if (index < 0 || index >= pools.Length)
        {
            return;
        }

        if (!pools[index].Contains(obj))
        {
            return;
        }

        obj.SetActive(false);
    }

    public void Clear(int index)
    {
        if (index < 0 || index >= pools.Length)
        {
            return;
        }

        foreach (GameObject obj in pools[index])
        {
            obj.SetActive(false);
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < pools.Length; i++)
        {
            Clear(i);
        }
    }
}
