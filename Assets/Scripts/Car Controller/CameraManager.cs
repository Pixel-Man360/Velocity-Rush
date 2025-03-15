using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Fusion;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    
    [SerializeField] private float _distance = 6f;
    [SerializeField] private float _height = 1.5f;
    [SerializeField] private float _smoothSpeed = 0.125f;
    [SerializeField] private float _rotationSpeed = 2f;

    [Header("Shake Settings")]
    [SerializeField] private float _collisionShakeDuration = 0.3f;
    [SerializeField] private float _collisionShakeStrength = 1.5f;
    [SerializeField] private int _collisionShakeVibrato = 20;

    private Vector3 _camVelocity = Vector3.zero;

    [SerializeField]private Transform _targetTransform;

    public void SetTarget(Transform target)
    {
        _targetTransform = target.transform;
    }


    void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if(_targetTransform == null) return;
 
        Vector3 desiredPosition = _targetTransform.position - _targetTransform.forward * _distance + Vector3.up * _height;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _camVelocity, _smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(_targetTransform.position + Vector3.up * 1.5f);
    }

   
    public void DoCollisionShake()
    {
        transform.DOShakePosition(_collisionShakeDuration, _collisionShakeStrength, _collisionShakeVibrato);
    }
}
