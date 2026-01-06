using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField] private GameObject cellIndicator;
    private GameObject previewObject;

    private void Start()
    {
        cellIndicator.SetActive(false);
    }
   
    public void StartShowingPlacementPreview(string name, Vector2Int size)
    {
        cellIndicator.SetActive(true);
        previewObject = Instantiate(AddressableManager.Instance.GetAddressable<GameObject>(name));
        PrepareCursor(size);
        MoveCursor(previewObject.transform.position);
    }

    public void StopShowingPreiview()
    {
        cellIndicator.SetActive(false);
        if (previewObject != null) Destroy(previewObject);
    }

    public void UpdatePosition(Vector3 position, bool validity)
    {
        if(previewObject !=null)
        {
            MovePreview(position);
            WarningColorChange(validity);

        }
        MoveCursor(position);
        WarningColorChange(validity);

    }

    private void PrepareCursor(Vector2Int size)//이미지 크기 만큼 보이는 범위를 늘려줌 
    {
        if(size.x > 0 || size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x, size.y, 1);
            cellIndicator.GetComponentInChildren<Renderer>().material.mainTextureScale = size;
        }
    }
    private void WarningColorChange(bool validity)//범위 경고
    {
        if (validity)
        {
            cellIndicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
            if(previewObject!=null)
            previewObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            cellIndicator.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
            if (previewObject != null)
             previewObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
    
    private void MoveCursor(Vector3 position)
    {
        cellIndicator.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        previewObject.transform.position = position;
    }

    public void StartShowingRemovePreview()
    {
        cellIndicator.SetActive(true);
        PrepareCursor(Vector2Int.one);
        WarningColorChange(true);
    }

   
}
