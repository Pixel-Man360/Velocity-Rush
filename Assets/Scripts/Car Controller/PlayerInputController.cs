using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class PlayerInputController : NetworkBehaviour
{
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool IsHandbraking { get; private set; }
    public bool IsBoosting { get; private set; }

    private NetworkInputData _inputData;

    void Awake()
    {
        _inputData = new NetworkInputData();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out _inputData))
        {
            Horizontal = _inputData.Horizontal;
            Vertical = _inputData.Vertical;
            IsHandbraking = _inputData.IsHandbraking;
            IsBoosting = _inputData.IsBoosting;
        }
    }
    
}
