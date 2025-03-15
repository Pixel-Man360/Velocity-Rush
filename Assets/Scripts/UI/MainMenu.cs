using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Threading.Tasks;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Transform _buttonsHolder;
    [SerializeField] private Transform _loadingHolder;
    [SerializeField] private BasicSpawner _basicSpawner;

    public static MainMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        hostButton.onClick.AddListener(() => StartGame(true));
        joinButton.onClick.AddListener(() => StartGame(false));
    }

    private void StartGame(bool isHost)
    {
        _basicSpawner.StartGame(isHost ? GameMode.Host : GameMode.Client);
        _buttonsHolder.gameObject.SetActive(false);
        _loadingHolder.gameObject.SetActive(true);
    }

    public void OnPlayerJoined()
    {
        gameObject.SetActive(false);
    }

}
