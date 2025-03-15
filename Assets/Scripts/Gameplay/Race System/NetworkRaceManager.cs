using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System.Linq;

public class NetworkRaceManager : NetworkBehaviour
{
    [Header("Race Settings")]
    [SerializeField] private int _minPlayersToStart = 2;
    [SerializeField] private CountdownTextUI _countdownText;
    [SerializeField] private BasicSpawner _spawner;
    [SerializeField] private List<CheckPoint> _checkPoints;
    [SerializeField] private List<PlayerPositionUI> _playerPositionUI;
    [SerializeField] private List<PlayerPositionUI> _rankUI;
    [SerializeField] private Transform gameOverPanel;
    [SerializeField] private TMP_Text _lapTimeText;
    [SerializeField] private TMP_Text _checkPointText;
    private bool _raceStarted = false;
    private bool _isCountdownFinished = false;
    private List<CarController> _players = new List<CarController>();
    private Dictionary<int, CheckPoint> _checkPointDictionary = new Dictionary<int, CheckPoint>();

    private float lapTime;

    public static NetworkRaceManager Instance { get; private set; }

    private List<CarController> _sortedPlayers = new List<CarController>();

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

        for (int i = 0; i < _checkPoints.Count; i++)
        {
            _checkPoints[i].SetCheckPointIndex(i);
            _checkPointDictionary.Add(i, _checkPoints[i]);
        }
    }

    void Start()
    {
        _countdownText.gameObject.SetActive(false);
        CountdownTextUI.OnCountDownFinished += OnRaceStarted;
    }

    void OnDestroy()
    {
        CountdownTextUI.OnCountDownFinished -= OnRaceStarted;
    }

    private void OnRaceStarted()
    {
        _isCountdownFinished = true;
        lapTime = Time.time;

        for (int i = 0; i < _players.Count; i++)
        {
            _players[i].Player.lapStartTime = Time.time;
        }
    }

    void Update()
    {
        if (!_raceStarted) return;
        if (!_isCountdownFinished) return;
        CalculateDistance();
        CheckPlayerLap();
        SetLapTime();
    }

    private void SetLapTime()
    {
        float elapsedTime = Time.time - lapTime;
        _lapTimeText.text = $"Lap Time: {FormatTime(elapsedTime)}";
    }

    private void CheckPlayerLap()
    {
        _sortedPlayers = _players.OrderByDescending(p => p.Player.checkpointsPassed)
                                   .ThenBy(p => p.Player.distanceToNextCheckpoint)
                                   .ToList();

        for (int i = 0; i < _sortedPlayers.Count; i++)
        {
            _sortedPlayers[i].Player.position = i + 1; // 1-based position
        }

        for (int i = 0; i < _sortedPlayers.Count; i++)
        {
            _playerPositionUI[i].SetPlayerPosition(_sortedPlayers[i].Player.position, _sortedPlayers[i].Player.name);
        }

        for (int i = _sortedPlayers.Count; i < _playerPositionUI.Count; i++)
        {
            _playerPositionUI[i].SetPlayerPosition(i + 1, "No Player");
        }
    }

    private void CalculateDistance()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].Player.currentCheckpoint != null)
            {
                _players[i].Player.distanceToNextCheckpoint = Vector3.Distance(_players[i].transform.position, _players[i].Player.currentCheckpoint.transform.position);
            }
        }
    }

    public void OnPlayerJoined(int playerCount, CarController player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
        }
        if (playerCount >= _minPlayersToStart && !_raceStarted)
        {
            _countdownText.StartCountdown(5);
            _countdownText.gameObject.SetActive(true);
            _raceStarted = true;

            SetCheckPointText(-1);
        }
    }

    public CheckPoint GetNextCheckPoint(int checkPointIndex)
    {
        SetCheckPointText(checkPointIndex);

        if (checkPointIndex >= _checkPoints.Count)
        {
            checkPointIndex = _checkPoints.Count - 1;
        }
        return _checkPointDictionary[checkPointIndex];
    }

    private void SetCheckPointText(int checkPointIndex)
    {
        _checkPointText.text = $"Checkpoint {checkPointIndex + 1} / {_checkPoints.Count}";
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void OnPlayerReachedFinishLine(Player player)
    {
        gameOverPanel.gameObject.SetActive(true);
        player.lapEndTime = Time.time;
        _raceStarted = false;
        _isCountdownFinished = false;

        for (int i = 0; i < _sortedPlayers.Count; i++)
        {
            _rankUI[i].SetPlayerPosition(i + 1, _sortedPlayers[i].Player.name);

            if(_sortedPlayers[i].Player == player)
            {
                _rankUI[i].SetLapTime(FormatTime(player.lapEndTime - player.lapStartTime));
            }

            else 
            {
                _rankUI[i].SetLapTime(FormatTime(Time.time - _sortedPlayers[i].Player.lapStartTime));
            }
        }

        for (int i = _sortedPlayers.Count; i < _rankUI.Count; i++)
        {
            _rankUI[i].SetPlayerPosition(i + 1, "No Player");
        }
    }
}
