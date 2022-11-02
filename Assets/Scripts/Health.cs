using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    // Start is called before the first frame update
    [SerializeField]
    private float maxHealth = 100f;
    private float currentHealth;
    private bool isInvulernable;
    [SerializeField]
    private float invulernabilityDuration = .5f;
    [SerializeField]
    private ParticleSystem hurtParticleSystem;
    [SerializeField]
    private AudioClip hurtAudioClip;

    void Awake()
    {
        currentHealth = maxHealth;
        
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyDamage(float damageAmount, GameObject other)
    {
        if(!isInvulernable)
        {
            Debug.Log("Applied " + damageAmount + "!");
            StartCoroutine(other.GetComponent<PlayerController>().ShakeCameraAndSlowDownTime());
            isInvulernable = true;
            AudioSource.PlayClipAtPoint(hurtAudioClip, transform.position);
            if(!hurtParticleSystem.isPlaying)
                hurtParticleSystem.Play();
            StartCoroutine(ResetInvulernability());
        }
        
    }

    private IEnumerator ResetInvulernability()
    {
        yield return new WaitForSeconds(invulernabilityDuration);
        isInvulernable = false;
    }
}
