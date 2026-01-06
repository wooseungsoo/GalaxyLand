using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedUI : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public FarmSeedSO seedData;
    private Image image;
    private Vector3 curPos;

    private void Start()
    {
        image = transform.GetChild(0).GetComponent<Image>();
        curPos= image.transform.localPosition;
    }


    public void OnDrag(PointerEventData eventData)
    {
        image.transform.position = Input.mousePosition;

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(image.transform.position), Vector2.zero, 50f);
        if (hit.collider!=null && hit.transform.gameObject.TryGetComponent<Farm>(out Farm farm))
        {
            if(farm.seedData ==null)
            {
                farm.OnDrop(seedData);
            }
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.transform.localPosition = curPos;
    }
}
