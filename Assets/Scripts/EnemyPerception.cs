using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class EnemyPerception : MonoBehaviour
{

    #region Variables

    [SerializeField]
    private float _maxViewingDistance;


    [SerializeField]
    private float _initialViewingAngle;

    [SerializeField]
    private float _inSightViewingAngle;

    private bool _playerIsInSight;

    public bool PlayerIsInSight
    {
        get { return _playerIsInSight; }
    }

    public bool PlayerWasHeard
    {
        get => _playerWasHeard;
    }

    private bool _chasingPlayer;

    public bool ChasingPlayer
    {
        get => _chasingPlayer;
        set => _chasingPlayer = value;
    }

    private bool _hasLineOfSight;

    private bool _playerWasSeenLastFrame;

    private bool _playerWasHeard;

    private Vector3 _detectedPosition;

    [SerializeField]
    private float _hearingCooldownTimer;

    public Vector3 DetectedPosition
    {
        get => _detectedPosition;
    }

    #endregion

    #region Methods

    public bool GetPlayerWasSeenLastFrame()
    {
        return _playerWasSeenLastFrame;
    }


    // If player is within the viewing angle of the enemy,
    // check if there are any obstacles in the way.
    public bool HasLineOfSight()
    {

        if (AIManager.Instance.PlayerTransform != null)
        {

            _playerWasSeenLastFrame = _hasLineOfSight;

            _hasLineOfSight = false;
            float currentViewingAngle = _playerIsInSight ? _inSightViewingAngle : _initialViewingAngle;
            _playerIsInSight = false;
            Vector3 enemyToPlayerDirection = AIManager.Instance.PlayerTransform.position - transform.position;
            enemyToPlayerDirection.y = 0f;
            enemyToPlayerDirection.Normalize();
            float dotProduct = Vector3.Dot(transform.forward, enemyToPlayerDirection);

            if (dotProduct >= Mathf.Cos(currentViewingAngle / 2))
            {
                RaycastHit hit;
                Physics.Raycast(transform.position + GetComponent<CapsuleCollider>().center, enemyToPlayerDirection, out hit, _maxViewingDistance, ~LayerMask.GetMask("Ignore Vision"));
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                
                    _hasLineOfSight = true;
                    _detectedPosition = hit.collider.transform.position;
                    _playerIsInSight = true;
                }

                    //if (hit.collider != null)
                    //    Debug.Log(hit.collider.name);

            }

        }

        return _hasLineOfSight;
    }



    public void HeardSomething(Vector3 position)
    {
        if(!_playerIsInSight)
        {
            _playerWasHeard = true;
            _detectedPosition = position;
        }

        StartCoroutine(ResetHearing());

    }

    private IEnumerator ResetHearing()
    {
        yield return new WaitForSecondsRealtime(_hearingCooldownTimer);
        _playerWasHeard = false;
    }

    #endregion
}
