using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform playerTransform;

    [SerializeField]
    private float maxDistance;

    [SerializeField]
    private float minHeight;

    [SerializeField]
    private float initialViewingAngle;

    [SerializeField]
    private float inSightViewingAngle;

    public bool playerIsInSight;

    public bool chasingPlayer;

    private bool hasLineOfSight;

    private bool playerWasSeenLastFrame;

    private Vector3 _playerPosition;

    public Vector3 PlayerPosition
    {
        get => _playerPosition;
    }

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            playerTransform = null;
        }
    }

    public bool GetPlayerWasSeenLastFrame()
    {
        return playerWasSeenLastFrame;
    }


    public bool HasLineOfSight()
    {
        if (playerTransform == null)
            return false;

        playerWasSeenLastFrame = hasLineOfSight;  
            
        hasLineOfSight = false;
        float currentViewingAngle = playerIsInSight ? inSightViewingAngle : initialViewingAngle;
        playerIsInSight = false;
        RaycastHit hit;

        Vector3 enemyToPlayerDirection = playerTransform.position - transform.position;
        enemyToPlayerDirection.y = 0f;
        enemyToPlayerDirection = enemyToPlayerDirection.normalized;
        float dotProduct = Vector3.Dot(transform.forward, enemyToPlayerDirection);



        if (dotProduct >= Mathf.Cos(currentViewingAngle/2))
        {

            Physics.Raycast(transform.position, enemyToPlayerDirection, out hit, maxDistance, ~LayerMask.GetMask("Ignore Vision"));

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {

                hasLineOfSight = true;
                _playerPosition = hit.collider.transform.position;
                playerIsInSight = true;
            }
            else
            {
                if (hit.collider != null)
                    Debug.Log(hit.collider.tag);
            }
        }


        return hasLineOfSight;
    }



    public bool IsChasingPlayer()
    {
        return chasingPlayer;
    }

}
