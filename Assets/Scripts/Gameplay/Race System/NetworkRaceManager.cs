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

    private bool _isCountdownFinished = false;
    private List<CarController> _players = new List<CarController>();
    private List<CarController> _sortedPlayers = new List<CarController>();
    private Dictionary<int, CheckPoint> _checkPointDictionary = new Dictionary<int, CheckPoint>();

    private float lapTime;
    [Networked] private bool _raceStarted { get; set; }

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
    }

    void Update()
    {
        if (!_isCountdownFinished) return;
        CalculateDistance();
        UpdatePlayerPositions();
        SetLapTime();
    }

    private void SetLapTime()
    {
        float elapsedTime = Time.time - lapTime;
        _lapTimeText.text = $"Lap Time: {FormatTime(elapsedTime)}";
    }

    private void UpdatePlayerPositions()
    {
        // Sort players by checkpoints passed and distance to the next checkpoint
        _sortedPlayers = _players.OrderByDescending(p => p.Player.checkpointsPassed)
                                 .ThenBy(p => p.Player.distanceToNextCheckpoint)
                                 .ToList();

        // Update the positions and broadcast to clients
        for (int i = 0; i < _sortedPlayers.Count; i++)
        {
            int position = i + 1;
            _sortedPlayers[i].SetPosition(position);
        }

        // Update UI on all clients
        RPC_UpdatePlayerPositionUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdatePlayerPositionUI()
    {
        _players.Sort((a, b) => a.Player.position.CompareTo(b.Player.position));
        for (int i = 0; i < _playerPositionUI.Count; i++)
        {
            if(i < _players.Count)
            {
                _playerPositionUI[i].SetPlayerPosition(_players[i].Player.position, _players[i].Player.name);
            }
            else
            {
                _playerPositionUI[i].SetPlayerPosition(i + 1, "No Player");
            }
        }
    }


    private void CalculateDistance()
    {
        foreach (var player in _players)
        {
            if (player.Player.currentCheckpoint != null)
            {
                player.Player.distanceToNextCheckpoint = Vector3.Distance(player.transform.position, player.Player.currentCheckpoint.transform.position);
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

            StartRaceCountdown(5);
            _raceStarted = true;

            SetCheckPointText(-1);
        }
    }

    private void StartRaceCountdown(int countdown)
    {
        if (Object.HasStateAuthority)
        {
            RPC_StartCountdown(countdown);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StartCountdown(int countdown)
    {
        _countdownText.StartCountdown(countdown);
    }

    public CheckPoint GetNextCheckPoint(int checkPointIndex)
    {
        SetCheckPointText(checkPointIndex);
        return _checkPointDictionary[Mathf.Clamp(checkPointIndex, 0, _checkPoints.Count - 1)];
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
            _rankUI[i].SetLapTime(FormatTime(_sortedPlayers[i].Player.lapEndTime - _sortedPlayers[i].Player.lapStartTime));
        }

        for (int i = _sortedPlayers.Count; i < _rankUI.Count; i++)
        {
            _rankUI[i].SetPlayerPosition(i + 1, "No Player");
        }
    }
}
