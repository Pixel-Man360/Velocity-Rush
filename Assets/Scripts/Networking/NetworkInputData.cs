using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public float Horizontal;
    public float Vertical;
    public bool IsHandbraking;
    public bool IsBoosting;
}