using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildingInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject actionButton; // 인스펙터에서 할당된 버튼

    public void ShowInfo(string name, string description)
    {
        gameObject.SetActive(true);
        buildingNameText.text = name;
        descriptionText.text = description;
        actionButton.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        actionButton.SetActive(false);
    }
}