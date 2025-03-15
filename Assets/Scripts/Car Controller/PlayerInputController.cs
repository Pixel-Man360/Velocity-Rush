using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool IsHandbraking { get; private set; }
    public bool IsBoosting { get; private set; }

    void Update()
    {
        Horizontal = Input.GetAxis("Horizontal");
        Vertical = Input.GetAxis("Vertical");
        IsHandbraking = Input.GetKey(KeyCode.Space);
        IsBoosting = Input.GetKey(KeyCode.X);
    }
}
