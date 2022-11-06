using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public Transform playerTransform;

    

    [SerializeField]
    private float maxDistance;

    [SerializeField]
    private float minHeight;

    [SerializeField]
    private float viewingAngle;

    public bool playerIsInSight;

    public bool chasingPlayer;

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


    public bool HasLineOfSight()
    {
        if (playerTransform == null)
            return false;

        bool hasLineOfSight = false;
        RaycastHit hit;

        Vector3 enemyToPlayerDirection = playerTransform.position - transform.position;
        enemyToPlayerDirection.y = 0f;
        enemyToPlayerDirection = enemyToPlayerDirection.normalized;
        float dotProduct = Vector3.Dot(transform.forward, enemyToPlayerDirection);
        
        if (dotProduct >= Mathf.Cos(viewingAngle/2))
        {
            Debug.Log(dotProduct);
            Physics.Raycast(transform.position, enemyToPlayerDirection, out hit, maxDistance);

            if (hit.collider != null && hit.collider.gameObject == playerTransform.gameObject)
            {
                hasLineOfSight = true;
                playerIsInSight = true;
            }
        }


        return hasLineOfSight;
    }



    public bool IsChasingPlayer()
    {
        return chasingPlayer;
    }

}
