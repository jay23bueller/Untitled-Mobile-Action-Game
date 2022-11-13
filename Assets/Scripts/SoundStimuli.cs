using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundStimuli : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float _maxRadius;

    [SerializeField]
    private float _minRadius;

    private float _currentRadius;

    //Used to enable OnDrawGizmos intermittently
    private bool _drawCooldown;

    #endregion

    #region Methods
    public void MakeNoise(float normalizedVelocity)
    {
        if(!_drawCooldown)
        {
            _drawCooldown = true;
            StartCoroutine(DrawCooldown());
        }

        _currentRadius = normalizedVelocity * _maxRadius;
        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,
            _currentRadius,
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
        _drawCooldown = false;
    }


    private void OnDrawGizmosSelected()
    {
        if(_drawCooldown)
        {
            Gizmos.DrawWireSphere(transform.position, _currentRadius);
        }
    }

    #endregion


}
