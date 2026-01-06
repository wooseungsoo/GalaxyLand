using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : Singleton<AddressableManager>
{
    protected override void Awake()
    {
        base.Awake();
    }
    public T GetAddressable<T>(string name)//주소 이름 찾아 불러오기
    {
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(name);
        handle.WaitForCompletion();

        return handle.Result;
    }
}
