using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
// BuildingButton: 개별 건물 버튼을 나타내는 클래스
public class BuildingButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    private RectTransform rectTransform;

    public event Action<ObjectData, Vector2> OnBuildingHovered;
    public event Action OnBuildingUnhovered;
    public event Action<ObjectData> OnBuildingSelected;

    private ObjectData objectData;
    private PlacementSystem placementSystem;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        placementSystem = FindObjectOfType<PlacementSystem>();
        button.onClick.AddListener(OnButtonClicked);
    }

    public void SetObjectData(ObjectData data)
    {
        objectData = data;
        iconImage.sprite = data.BuildingIcon;
        nameText.text = data.Name;
    }
    //모바일 기준이기때문에 마우스 포인트가 아닌 클릭기준으로 수정
    public void OnPointerEnter()
    {
        Vector2 position = rectTransform.position;
        OnBuildingHovered?.Invoke(objectData, position);
    }

    public void OnPointerExit()
    {
        OnBuildingUnhovered?.Invoke();
    }

    private void OnButtonClicked()
    {
        OnBuildingSelected?.Invoke(objectData);
        if (placementSystem != null)
        {
            placementSystem.StartPlacement(objectData.ID);
        }

        OnPointerEnter();
    }
}