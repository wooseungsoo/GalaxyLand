using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class WeightedImage
{
    public Sprite image;
    public float weight;
    public int bingoCheck;
}
public class BingoController : MonoBehaviour
{
    public Transform cardsParent;
    private List<Card> cards;
    public Button shotButton;
    public Button bettingButton;
    public TextMeshProUGUI bettingText;
    public float animationDuration = 1f; // 애니메이션 지속 시간
    public float animationInterval = 0.1f; // 이미지 변경 간격
    public WeightedImage[] possibleImages;
    private float totalWeight;
    private int[,] board = new int[5, 5];
    private const int BINGO_LINE = 5;
    private int[] bets = { 1, 2, 4, 8, 25 };
    private int currentIndex = 0;
    public BingoControllerUI bingoControllerUI;

    public DOTweenActionObj dOTweenActionObj;

    private void Awake()
    {
        InitializeCards();
        CalculateTotalWeight();
        shotButton.onClick.AddListener(OnShot);
        bettingButton.onClick.AddListener(OnBetting);
    }

    private void InitializeCards()
    {
        int childCount = cardsParent.childCount;
        cards = new List<Card>(childCount);
        Card[] cardComponents = cardsParent.GetComponentsInChildren<Card>();

        for (int i = 0; i < cardComponents.Length; i++)
        {
            Card card = cardComponents[i];
            cards.Add(card);
            card.SetBingoManager(this);

            // 카드의 row와 col 설정
            card.row = i / 5;
            card.col = i % 5;
        }
    }

    private void CalculateTotalWeight()
    {
        totalWeight = 0f;
        foreach (WeightedImage wi in possibleImages)
        {
            totalWeight += wi.weight;
        }
    }

    public void OnShot()
    {
        if (GameDataManager.Instance._gameData.Tickets <= 0)
        {
            Debug.Log("티켓이 부족합니다");
            return;
        }

        StartCoroutine(AnimateAllCards());
        GameDataManager.Instance.AddTickets(-bets[currentIndex]);
        DateTime currentTime = TimeManager.Instance.GetAdjustedTime();
        TicketManager.Instance.UseTicket();
    }

    private IEnumerator AnimateAllCards()
    {
        SetButtonInteractable(false);
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            foreach (Card card in cards)
            {
                card.SetRandomImageForAnimation();
            }
            elapsedTime += animationInterval;
            yield return new WaitForSeconds(animationInterval);
        }
        foreach (Card card in cards)
        {
            card.SetFinalWeightedImage();
        }
        SetButtonInteractable(true);

        int bingoCount = CheckAllBingos();
        Sprite bingoSprite = GetBingoImage(bingoCount);

        if (bingoControllerUI != null && bingoSprite != null)
        {
            bingoControllerUI.UpdateBingoImage(bingoSprite);
        }

        // 보상 부분
        int coinde = 10;
        int bingoR = BingoReward();

        if (CheckAllBingos() > 0)
        {
            dOTweenActionObj.StartCoroutine(dOTweenActionObj.SpawnCoins());
            bingoControllerUI.UpdateCount(bingoR * coinde, bingoR);

            yield return new WaitForSeconds(1.9f);

            GameDataManager.Instance.AddCurrency(bingoR * coinde);
            GameDataManager.Instance.AddBonusTickets(bingoR);
        }
    }


    public Sprite GetBingoImage(int bingoCount)
    {
        Sprite[] bingoImages = bingoControllerUI.bingoCountImage;

        if (bingoImages == null || bingoImages.Length == 0)
        {
            return null;
        }

        int index = Mathf.Clamp(bingoCount, 0, bingoImages.Length - 1);
        return bingoImages[index];
    }
    private int BingoReward()
    {
        int ch = CheckAllBingos();
        int bet = bets[currentIndex];
        int def = 5;
        int rew;

        return rew = def * bet * ch;
    }

    private void SetButtonInteractable(bool interactable)
    {
        shotButton.interactable = interactable;
    }

    public Sprite GetRandomImageForAnimation()
    {
        int randomIndex = UnityEngine.Random.Range(0, possibleImages.Length);
        return possibleImages[randomIndex].image;
    }

    public Sprite GetRandomWeightedImage()
    {
        float random = UnityEngine.Random.Range(0f, 100f);
        float cumulativeWeight = 0f;

        foreach (WeightedImage wi in possibleImages)
        {
            cumulativeWeight += wi.weight;
            if (random <= cumulativeWeight)
            {
                return wi.image;
            }
        }

        return possibleImages[possibleImages.Length - 1].image;
    }

    public int CheckAllBingos()
    {
        int bingoCount = 0;

        // 가로줄 체크
        for (int row = 0; row < 5; row++)
        {
            if (CheckHorizontal(row))
            {
                bingoCount++;
                ActivateOutlineForRow(row);
            }
        }

        // 세로줄 체크
        for (int col = 0; col < 5; col++)
        {
            if (CheckVertical(col))
            {
                bingoCount++;
                ActivateOutlineForColumn(col);
            }
        }

        // 대각선 체크
        if (CheckDiagonal1())
        {
            bingoCount++;
            ActivateOutlineForDiagonal1();
        }
        if (CheckDiagonal2())
        {
            bingoCount++;
            ActivateOutlineForDiagonal2();
        }

        return bingoCount;
    }

    private bool CheckHorizontal(int row)
    {
        int sum = 0;
        for (int col = 0; col < 5; col++)
        {
            sum += board[row, col];
        }
        return sum % (100 * BINGO_LINE) == 0 && sum != 0;
    }

    private bool CheckVertical(int col)
    {
        int sum = 0;
        for (int row = 0; row < 5; row++)
        {
            sum += board[row, col];
        }
        return sum % (100 * BINGO_LINE) == 0 && sum != 0;
    }

    private bool CheckDiagonal1()
    {
        int sum = 0;
        for (int i = 0; i < 5; i++)
        {
            sum += board[i, i];
        }
        return sum % (100 * BINGO_LINE) == 0 && sum != 0;
    }

    private bool CheckDiagonal2()
    {
        int sum = 0;
        for (int i = 0; i < 5; i++)
        {
            sum += board[i, 4 - i];
        }
        return sum % (100 * BINGO_LINE) == 0 && sum != 0;
    }

    public void UpdateBoard(int row, int col, int value)
    {
        board[row, col] = value;
    }

    private void OnBetting()
    {
        if (GameDataManager.Instance._gameData.Tickets == 0)
        {
            Debug.Log("티켓이 부족합니다");
            return;
        }

        currentIndex++;

        if (currentIndex >= bets.Length)
        {
            currentIndex = 0;
        }
        bettingText.text = bets[currentIndex].ToString();
    }

    private void ActivateOutlineForRow(int row)
    {
        for (int col = 0; col < 5; col++)
        {
            ActivateOutlineForCard(row * 5 + col);
        }
    }

    private void ActivateOutlineForColumn(int col)
    {
        for (int row = 0; row < 5; row++)
        {
            ActivateOutlineForCard(row * 5 + col);
        }
    }

    private void ActivateOutlineForDiagonal1()
    {
        for (int i = 0; i < 5; i++)
        {
            ActivateOutlineForCard(i * 5 + i);
        }
    }

    private void ActivateOutlineForDiagonal2()
    {
        for (int i = 0; i < 5; i++)
        {
            ActivateOutlineForCard(i * 5 + (4 - i));
        }
    }

    private void ActivateOutlineForCard(int index)
    {
        if (index >= 0 && index < cards.Count)
        {
            Outline outline = cards[index].GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = true;
            }
            else
            {
                Debug.LogWarning($"Outline component not found on card at index {index}");
            }
        }
    }
}
