using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Fusion;

public class PlayerNameTagUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;

    public void SetPlayerName(string playerName)
    {
        _playerNameText.text = playerName;
    }

}
