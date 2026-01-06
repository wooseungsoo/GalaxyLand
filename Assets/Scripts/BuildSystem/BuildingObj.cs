using System;
using UnityEngine;

public class BuildingObj : MonoBehaviour
{
    BuildSystemManager buildSystemManager;
    Camera cam;

    public GameObject setBuildingUI;
    public GameObject time;

    public bool IsCompelete;

    public BuildingInfoUI infoUI; // Inspector에서 할당
    public string buildingName;
    public string buildingDescription; // 인스펙터에서 설정
    // public GameObject deleteUI; 보류 또는 제거
    private void Start()
    {
        cam = Camera.main;
        buildSystemManager = BuildSystemManager.Instance;
    }

    public void RotationObject()
    {
        SpriteRenderer previewSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer BuildingSpriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();

        previewSpriteRenderer.flipX = !previewSpriteRenderer.flipX;
        BuildingSpriteRenderer.flipX = !BuildingSpriteRenderer.flipX;

        buildSystemManager.curBuildingFlip = previewSpriteRenderer.flipX;
    }

    public void DeleteObject()
    {
        buildSystemManager.BuildObject();
    }

    public void BuildObject()
    {
        buildSystemManager.BuildObject();
        
    }

    public void Initialize(string name)
    {
        buildingName = name;
        
    }

}
