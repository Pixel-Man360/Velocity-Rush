using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameTagUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;

    private void Start()
    {
        _playerNameText.text = $"Player {Random.Range(5000, 10000)}";
    }

}
