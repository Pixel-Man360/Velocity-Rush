using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CheckPoint : NetworkBehaviour
{
    [SerializeField] private CheckPointType checkPointType;
    private int checkPointIndex;

    public CheckPointType CheckPointType => checkPointType;

    public int CheckPointIndex => checkPointIndex;

    public void SetCheckPointIndex(int index)
    {
        checkPointIndex = index;
    }
}

public enum CheckPointType
{
    StartLine,
    CheckPoint,
    FinishLine
}
