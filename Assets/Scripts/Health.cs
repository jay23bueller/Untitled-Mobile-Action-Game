using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    #region Variables
    // Start is called before the first frame update
    [SerializeField]
    private float maxHealth = 100f;
    private float currentHealth;
    private bool isInvulernable;
    [SerializeField]
    private float invulernabilityDuration = .5f;

    #endregion

    #region Methods
    void Awake()
    {
        currentHealth = maxHealth;
    }


    public void ApplyDamage(float damageAmount, GameObject attacker)
    {
        Debug.Log("Attack?");
        if(!attacker.gameObject.CompareTag(gameObject.tag))
        {
            if (!isInvulernable)
            {
                Debug.Log("Applied " + damageAmount + "!");
                if (attacker.GetComponent<PlayerController>() != null)
                {
                    StartCoroutine(attacker.GetComponent<PlayerController>().ShakeCameraAndSlowDownTime());
                    GetComponent<Controller>().GetStunned();
                }




                isInvulernable = true;
                HitManager.Instance.PlayHitSound();

                if (GetComponent<CharacterController>() != null)
                    HitManager.Instance.PlaySwordHitEffect(transform.position + (Vector3.up * (GetComponent<CharacterController>().height / 2)));
                else
                    HitManager.Instance.PlaySwordHitEffect(transform.position + (Vector3.up * (GetComponent<CapsuleCollider>().height / 2)));
                StartCoroutine(ResetInvulernability());
            }
        }

        
    }

    private IEnumerator ResetInvulernability()
    {
        yield return new WaitForSeconds(invulernabilityDuration);
        isInvulernable = false;
    }
    #endregion
}
