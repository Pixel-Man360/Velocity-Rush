using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
public class CarController : NetworkBehaviour
{
    [SerializeField] private CarMovement _carMovement; //To separate the car movement from the car controller
    [SerializeField] private CarEffects _carEffects; //To separate the car feedbacks from the car controller
    [SerializeField] private CameraManager _cameraPrefab;
    [SerializeField] private PlayerNameTagUI _nameTagUI;

    private CameraManager _currentCameraManager;
    private NetworkInputData _input;

    private bool _canMove = false;
    private Player _player = new Player();
    public Player Player => _player;

    void Start()
    {
        _carEffects.StartEngineSound();

        if (Object.HasInputAuthority)
        {
            _currentCameraManager = Instantiate(_cameraPrefab);
            _currentCameraManager.SetTarget(transform);
        }

        CountdownTextUI.OnCountDownFinished += () => _canMove = true;

        SetPlayerInfo();
    }

    void OnDestroy()
    {
        CountdownTextUI.OnCountDownFinished -= () => _canMove = true;
    }

    private void SetPlayerInfo()
    {
        if (Object.HasStateAuthority)
        {
            _player.name = $"Player {Random.Range(5000, 10000)}";
            _nameTagUI.SetPlayerName(_player.name);
            _player.currentCheckpoint = NetworkRaceManager.Instance.GetNextCheckPoint(0);
            _player.checkpointsPassed = 0;
            _player.distanceToNextCheckpoint = 0;
            _player.position = 0;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out _input) && _canMove)
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
        if (_currentCameraManager != null)
            _currentCameraManager.DoCollisionShake();

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CheckPoint checkPoint))
        {
            if (checkPoint.CheckPointType == CheckPointType.FinishLine)
            {
                NetworkRaceManager.Instance.OnPlayerReachedFinishLine(_player);
            }

            else
            {
                if (Object.HasStateAuthority)
                {
                    _player.checkpointsPassed++;
                }

                _player.currentCheckpoint = NetworkRaceManager.Instance.GetNextCheckPoint(checkPoint.CheckPointIndex + 1);
            }
        }
    }
}

[System.Serializable]
public class Player
{
    [Networked] public string name { get; set; }
    [Networked] public int checkpointsPassed { get; set; }
    [Networked] public CheckPoint currentCheckpoint { get; set; }
    [Networked] public float distanceToNextCheckpoint { get; set; }
    [Networked] public int position { get; set; }
    [Networked] public float lapStartTime { get; set; }
    [Networked] public float lapEndTime { get; set; }

}

[System.Serializable]
public class CarMovement
{
    [SerializeField] private WheelCollider[] _wheelColliders;
    [SerializeField] private Transform[] _wheelMeshes;
    [SerializeField] private NetworkRigidbody3D _rb;
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
            _wheelColliders[i].brakeTorque = isHandbraking ? _handbrakeForce : 0f;
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
        return _rb.Rigidbody.velocity.magnitude;
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
    }
}
