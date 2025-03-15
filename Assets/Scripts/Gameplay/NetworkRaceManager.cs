using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class NetworkRaceManager : NetworkBehaviour
{
    

    [Header("Race Settings")]
    [SerializeField] private int minPlayersToStart = 2;
    
    [SerializeField] private CountdownTextUI _countdownText;
    [SerializeField] private BasicSpawner _spawner;

    private bool raceStarted = false;
    

    public static NetworkRaceManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _countdownText.gameObject.SetActive(false);
    }

    public void OnPlayerJoined(int playerCount)
    {
        if (playerCount >= minPlayersToStart && !raceStarted)
        {
            _countdownText.StartCountdown(5);
            _countdownText.gameObject.SetActive(true);
            raceStarted = true;
        }
    }

}
