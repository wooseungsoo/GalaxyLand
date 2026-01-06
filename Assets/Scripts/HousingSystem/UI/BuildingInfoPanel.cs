// BuildingInfoPanel: 선택된 건물의 상세 정보를 표시하는 클래스
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BuildingInfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI buildingSizeText;
    //[SerializeField] private TextMeshProUGUI costText;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void ShowInfo(ObjectData building, Vector2 buttonPosition)
    {
        gameObject.SetActive(true);
        buildingNameText.text = $"Name: {building.Name}";
        buildingSizeText.text = $"Size: {building.Size.x}x{building.Size.y}";
        //costText.text = $"Cost:{building.cost}

        Vector2 newPosition = buttonPosition + new Vector2(0, rectTransform.rect.height / 2 + 10f);
        rectTransform.position = newPosition;
    }

    // 건설 버튼 상태 업데이트


    public void Hide()
    {
        gameObject.SetActive(false);
    }
}