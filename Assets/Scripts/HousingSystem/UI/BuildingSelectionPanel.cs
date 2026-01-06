// BuildingSelectionPanel: 사용 가능한 건물 목록을 표시하고 관리하는 클래스
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
public class BuildingSelectionPanel : MonoBehaviour
{
    [SerializeField] private BuildingInfoPanel buildingInfoPanel;
    [SerializeField] private ObjectDatabaseSO objectDatabase;
    [SerializeField] private GameObject buildingButtonPrefab;
    [SerializeField] private Transform contentTransform;
    public event Action<ObjectData, Vector2> OnBuildingHovered;
    public event Action OnBuildingUnhovered;

    public GameObject buildingInfoUI=null; //현재 보여주는 정보판넬
    private void Start()
    {
        InitializeBuildingButtons();
    }
    private void ShowBuildingInfo(ObjectData data, Vector2 position)
    {
        buildingInfoPanel.ShowInfo(data, position);
        buildingInfoUI = buildingInfoPanel.gameObject;
    }
    private void InitializeBuildingButtons()
    {
        foreach (var buildingData in objectDatabase.objectData)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, contentTransform);
            BuildingButton buildingButton = buttonObj.GetComponent<BuildingButton>();
            buildingButton.SetObjectData(buildingData);

            // 이벤트 구독
            buildingButton.OnBuildingHovered += BuildingHovered;
            buildingButton.OnBuildingUnhovered += BuildingUnhovered;
        }
    }
    private void BuildingHovered(ObjectData data, Vector2 position)
    {
        OnBuildingHovered?.Invoke(data, position);
        buildingInfoPanel.ShowInfo(data, position);
    }

    private void BuildingUnhovered()
    {
        buildingInfoPanel.Hide();
    }
}