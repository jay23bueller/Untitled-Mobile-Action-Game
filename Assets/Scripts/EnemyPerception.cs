using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class EnemyPerception : MonoBehaviour
{


    [SerializeField]
    private float maxDistance;

    [SerializeField]
    private float minHeight;

    [SerializeField]
    private float initialViewingAngle;

    [SerializeField]
    private float inSightViewingAngle;

    private bool _playerIsInSight;

    public bool PlayerIsInSight
    {
        get { return _playerIsInSight; }
    }

    public bool chasingPlayer;

    private bool hasLineOfSight;

    private bool playerWasSeenLastFrame;

    private bool _playerWasHeard;

    private Vector3 _detectedPosition;

    [SerializeField]
    private float _hearingCooldownTimer;

    public Vector3 DetectedPosition
    {
        get => _detectedPosition;
    }


    // Start is called before the first frame update

    public bool GetPlayerWasSeenLastFrame()
    {
        return playerWasSeenLastFrame;
    }


    public bool HasLineOfSight()
    {

    if (AIManager.Instance.PlayerTransform != null)
    {

        playerWasSeenLastFrame = hasLineOfSight;

        hasLineOfSight = false;
        float currentViewingAngle = _playerIsInSight ? inSightViewingAngle : initialViewingAngle;
        _playerIsInSight = false;
        Vector3 enemyToPlayerDirection = AIManager.Instance.PlayerTransform.position - transform.position;
        enemyToPlayerDirection.y = 0f;
        enemyToPlayerDirection.Normalize();
        float dotProduct = Vector3.Dot(transform.forward, enemyToPlayerDirection);

        if (dotProduct >= Mathf.Cos(currentViewingAngle / 2))
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + GetComponent<CapsuleCollider>().center, enemyToPlayerDirection, out hit, maxDistance, ~LayerMask.GetMask("Ignore Vision"));
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                
                hasLineOfSight = true;
                _detectedPosition = hit.collider.transform.position;
                _playerIsInSight = true;
            }

                if (hit.collider != null)
                    Debug.Log(hit.collider.name);

        }

    }

        return hasLineOfSight;
    }

    public bool PlayerWasHeard
    {
        get => _playerWasHeard;
    }



    public bool IsChasingPlayer()
    {
        return chasingPlayer;
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
}
