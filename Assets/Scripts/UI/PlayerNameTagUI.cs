using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameTagUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;

    public void SetPlayerName(string playerName)
    {
        _playerNameText.text = playerName;
    }

}
