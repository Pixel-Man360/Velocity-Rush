using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
public class CarController : NetworkBehaviour
{
    // [SerializeField] private PlayerInputController _input; //To separate the input from the car controller
    [SerializeField] private CarMovement _carMovement; //To separate the car movement from the car controller
    [SerializeField] private CarEffects _carEffects; //To separate the car feedbacks from the car controller

    private NetworkInputData _input;
    void Start()
    {
        _carEffects.StartEngineSound();
    }

    void OnEnable()
    {
        CameraManager.Instance.SetTarget(transform);
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out _input))
        {
            _carMovement.Move(_input.Horizontal, _input.Vertical, _input.IsHandbraking, _input.IsBoosting);
            _carEffects.BrakeFeel(_input.IsHandbraking, _carMovement.IsSlipping, _input.Vertical);
        }
       
        _carMovement.UpdateWheelMeshes();
        _carMovement.CheckTraction();
        
        _carEffects.UpdateEngineSound(_carMovement.GetSpeed());
    }

    void OnCollisionEnter(Collision collision)
    {
        _carEffects.PlayCollisionSound();
    }
}

[System.Serializable]
public class CarMovement
{
    [SerializeField] private WheelCollider[] _wheelColliders;
    [SerializeField] private Transform[] _wheelMeshes;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _maxMotorTorque;
    [SerializeField] private float _maxSteerAngle;
    [SerializeField] private float _handbrakeForce;
    [SerializeField] private float _nitroBoost;
    [SerializeField] private float _slipThreshold;

    public bool IsSlipping { get; private set; }

  
    internal void Move(float steerInput, float throttleInput, bool isHandbraking, bool isNitro)
    {
        float currentTorque = _maxMotorTorque * (isNitro ? _nitroBoost : 1f) * throttleInput;
        
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].motorTorque = currentTorque;
            _wheelColliders[i].brakeTorque = isHandbraking ? _handbrakeForce : 0;
        }

        _wheelColliders[0].steerAngle = _maxSteerAngle * steerInput;
        _wheelColliders[1].steerAngle = _maxSteerAngle * steerInput;
    }

    internal void UpdateWheelMeshes()
    {
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].GetWorldPose(out Vector3 pos, out Quaternion rot); // Updating the wheel mesh position and rotation
            _wheelMeshes[i].position = pos;
            _wheelMeshes[i].rotation = rot;
        }
    }

    internal void CheckTraction()
    {
        IsSlipping = false;
        foreach (WheelCollider wheel in _wheelColliders)
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit))
            {
                if (Mathf.Abs(hit.sidewaysSlip) > _slipThreshold) //Checking sideways slip
                {
                    IsSlipping = true;
                    break;
                }
            }
        }

    }

    internal float GetSpeed()
    {
        return _rb.velocity.magnitude;
    }
}

[System.Serializable]
public class CarEffects
{
    [SerializeField] private AudioSource _engineSound;
    [SerializeField] private AudioSource _brakeSound;
    [SerializeField] private AudioSource _collisionSound;
    [SerializeField] private List<ParticleSystem> _skidsmoke;
    [SerializeField] private List<TrailRenderer> _tireMarks;

    internal void StartEngineSound()
    {
        _engineSound.loop = true;
        _engineSound.Play();
    }

  
    internal void UpdateEngineSound(float speed)
    {
        _engineSound.pitch = Mathf.Lerp(1f, 3f, speed / 100f);
    }

    internal void BrakeFeel(bool isHandbraking, bool isSlipping, float throttleInput)
    {
        if (isHandbraking || isSlipping || throttleInput < 0f)
        {
            if (!_brakeSound.isPlaying)
                _brakeSound.Play();
            _skidsmoke.ForEach(smoke => smoke.Play());
            _tireMarks.ForEach(trail => trail.emitting = true);
        }
        else
        {
            _brakeSound.Stop();
            _skidsmoke.ForEach(smoke => smoke.Stop());
            _tireMarks.ForEach(trail => trail.emitting = false);
        }
    }

    internal void PlayCollisionSound()
    {
        _collisionSound.Play();
        CameraManager.Instance.DoCollisionShake();
    }
}
