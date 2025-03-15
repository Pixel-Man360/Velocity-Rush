using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _distance = 6f;
    [SerializeField] private float _height = 2f;
    [SerializeField] private float _smoothSpeed = 0.125f;
    [SerializeField] private Vector3 _offset;

    [Header("Shake Settings")]
    [SerializeField] private float _collisionShakeDuration = 0.3f;
    [SerializeField] private float _collisionShakeStrength = 1.5f;
    [SerializeField] private int _collisionShakeVibrato = 20;

    private Vector3 _camVelocity = Vector3.zero;

    public static CameraManager Instance { get; private set; }

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }


    void FixedUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        Vector3 desiredPosition = _target.position - _target.forward * _distance + Vector3.up * _height + _offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _camVelocity, _smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(_target.position + Vector3.up * 1.5f);
    }

   
    public void DoCollisionShake()
    {
        transform.DOShakePosition(_collisionShakeDuration, _collisionShakeStrength, _collisionShakeVibrato);
    }
}
