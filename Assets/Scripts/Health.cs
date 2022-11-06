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
        Debug.Log("Attack?");
        if(!isInvulernable)
        {
            Debug.Log("Applied " + damageAmount + "!");
            StartCoroutine(other.GetComponent<PlayerController>().ShakeCameraAndSlowDownTime());
            isInvulernable = true;
            HitManager.Instance.PlayHitSound();

            if (GetComponent<CharacterController>() != null)
                HitManager.Instance.PlaySwordHitEffect(transform.position + (Vector3.up * (GetComponent<CharacterController>().height / 2)));
            else
                HitManager.Instance.PlaySwordHitEffect(transform.position + (Vector3.up * (GetComponent<CapsuleCollider>().height / 2)));
            StartCoroutine(ResetInvulernability());
        }
        
    }

    private IEnumerator ResetInvulernability()
    {
        yield return new WaitForSeconds(invulernabilityDuration);
        isInvulernable = false;
    }
}
