using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundStimuli : MonoBehaviour
{
    [SerializeField]
    private float maxRadius;

    [SerializeField]
    private float minRadius;

    private float currentRadius;


    bool drawCooldown;

    // Update is called once per frame
    public void MakeNoise(float normalizedVelocity)
    {
        if(!drawCooldown)
        {
            drawCooldown = true;
            StartCoroutine(DrawCooldown());
        }

        currentRadius = normalizedVelocity * maxRadius;
        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            currentRadius,
            transform.forward,
            0f
            );
        
        foreach(RaycastHit hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                if(hit.collider.GetComponent<EnemyPerception>() != null)
                    hit.collider.GetComponent<EnemyPerception>().HeardSomething(transform.position);
            }
                
        }

    }


    private IEnumerator DrawCooldown()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        drawCooldown = false;
    }


    private void OnDrawGizmosSelected()
    {
        if(drawCooldown)
        {
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }


}
