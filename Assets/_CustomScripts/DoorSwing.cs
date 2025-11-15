using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;

public class DoorSwing : UdonSharpBehaviour
{
    [SerializeField] private float _swingAngle = 90f;
    [SerializeField] private float _swingDuration = 1f;
    [SerializeField] private AnimationCurve _swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Transform _doorTransform;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private bool _isSwinging = false;
    private bool _isOpen = false;
    private float _swingTimer = 0f;
    private Quaternion _startRotation;
    private Quaternion _targetRotation;

    void Start()
    {
        _doorTransform = transform;
        _closedRotation = _doorTransform.rotation;
        _openRotation = _closedRotation * Quaternion.Euler(0, _swingAngle, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartDoorSwing();
        }

        if (_isSwinging)
        {
            _swingTimer += Time.deltaTime;
            float progress = _swingTimer / _swingDuration;
            
            if (progress >= 1f)
            {
                _doorTransform.rotation = _targetRotation;
                _isOpen = !_isOpen;
                _isSwinging = false;
                _swingTimer = 0f;
            }
            else
            {
                float curveProgress = _swingCurve.Evaluate(progress);
                _doorTransform.rotation = Quaternion.Lerp(_startRotation, _targetRotation, curveProgress);
            }
        }
    }

    public void StartDoorSwing()
    {
        if (!_isSwinging)
        {
            _isSwinging = true;
            _swingTimer = 0f;
            _startRotation = _isOpen ? _openRotation : _closedRotation;
            _targetRotation = _isOpen ? _closedRotation : _openRotation;
        }
    }
}
