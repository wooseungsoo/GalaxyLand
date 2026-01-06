using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BingoControllerUI : MonoBehaviour
{
    public TextMeshProUGUI coinCount;
    public TextMeshProUGUI bonusTicketCount;
    

    public Sprite[] bingoCountImage;
    public Image bingoImage;



    public void UpdateCount(int coinAmount, int bonusTicketAmount)
    {
        coinCount.text = coinAmount.ToString();
        bonusTicketCount.text = bonusTicketAmount.ToString();
    }

    public void UpdateBingoImage(Sprite sprite)
    {
        if (bingoImage != null && sprite != null)
        {
            bingoImage.sprite = sprite;
        }
    }

}
