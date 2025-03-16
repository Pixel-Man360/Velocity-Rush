using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class PlayerPositionUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text txt;
    [SerializeField] private TMP_Text lapTimeText;

    public void SetPlayerPosition(int position, string playerName)
    {
        txt.text = $"{position}. {playerName}";
    }

    public void SetLapTime(string lapTime)
    {
        if(lapTimeText != null)
        {
            lapTimeText.text = lapTime;
        }
    }
}
