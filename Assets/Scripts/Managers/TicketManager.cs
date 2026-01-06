using System;
using System.Collections;
using UnityEngine;

public class TicketManager : Singleton<TicketManager>
{
    private int _ticketRespawnTime = 600;
    private Coroutine _respawnCoroutine;
    private DateTime _lastTicketAddTime;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _lastTicketAddTime = GameDataManager.Instance._gameData.CurrentTime;
        AddAccumulatedTickets();
        CheckAndStartTicketRespawn();
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
        GameDataManager.Instance._gameData.CurrentTime = TimeManager.Instance.GetAdjustedTime();
        GameDataManager.Instance.SaveData();
    }

    private void AddAccumulatedTickets()
    {
        DateTime currentTime = TimeManager.Instance.GetAdjustedTime();
        TimeSpan timeDifference = currentTime - GameDataManager.Instance._gameData.CurrentTime;
        int ticketsToAdd = (int)(timeDifference.TotalSeconds / _ticketRespawnTime);

        if (ticketsToAdd > 0)
        {
            int currentTickets = GameDataManager.Instance._gameData.Tickets;
            GameDataManager.Instance.AddTickets(ticketsToAdd);
            _lastTicketAddTime = currentTime - TimeSpan.FromSeconds(timeDifference.TotalSeconds % _ticketRespawnTime);
            GameDataManager.Instance._gameData.CurrentTime = currentTime;
            GameDataManager.Instance.SaveData();
        }
    }

    private void CheckAndStartTicketRespawn()
    {
        if (GameDataManager.Instance._gameData.Tickets < GameDataManager.Instance._gameData.maxTickets)
        {
            if (_respawnCoroutine != null)
            {
                StopCoroutine(_respawnCoroutine);
            }
            _respawnCoroutine = StartCoroutine(TicketRespawnCoroutine());
        }
        else
        {
            UpdateTimerDisplay(0);
        }
    }

    private IEnumerator TicketRespawnCoroutine()
    {
        while (true)
        {
            DateTime currentTime = TimeManager.Instance.GetAdjustedTime();
            TimeSpan timeDifference = currentTime - _lastTicketAddTime;
            int currentTickets = GameDataManager.Instance._gameData.Tickets;
            int maxTickets = GameDataManager.Instance._gameData.maxTickets;

            if (currentTickets < maxTickets)
            {
                float remainingTime = _ticketRespawnTime - (float)timeDifference.TotalSeconds % _ticketRespawnTime;
                UpdateTimerDisplay(remainingTime);

                if (timeDifference.TotalSeconds >= _ticketRespawnTime)
                {
                    int ticketsToAdd = Math.Min((int)(timeDifference.TotalSeconds / _ticketRespawnTime), maxTickets - currentTickets);
                    if (ticketsToAdd > 0)
                    {
                        GameDataManager.Instance.AddTickets(ticketsToAdd);
                        _lastTicketAddTime = currentTime - TimeSpan.FromSeconds(timeDifference.TotalSeconds % _ticketRespawnTime);
                        GameDataManager.Instance._gameData.CurrentTime = currentTime;
                        GameDataManager.Instance.SaveData();
                    }
                }
            }
            else
            {
                UpdateTimerDisplay(0);
                _lastTicketAddTime = currentTime;
            }

            yield return new WaitForSeconds(1);
        }
    }

    public void UseTicket()
    {
        if (GameDataManager.Instance._gameData.Tickets > 0)
        {
            GameDataManager.Instance.SaveData();
            CheckAndStartTicketRespawn();
        }
    }

    private void UpdateTimerDisplay(float timeRemaining)
    {
        int currentTickets = GameDataManager.Instance._gameData.Tickets;
        int maxTickets = GameDataManager.Instance._gameData.maxTickets;

        if (currentTickets >= maxTickets)
        {
            HousingSceneUI.Instance.timerText.text = "MAX";
        }
        else
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            HousingSceneUI.Instance.timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}