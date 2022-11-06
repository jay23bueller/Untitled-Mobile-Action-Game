
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private float damageAmount;
    [SerializeField]
    private AudioClip[] weaponSwingSounds;
    private bool damageEnabled;

    private void Awake()
    {
        if(GetComponentInParent<CharacterController>() != null)
            Physics.IgnoreCollision(GetComponentInParent<CharacterController>(), GetComponent<BoxCollider>());

        if (GetComponentInParent<CapsuleCollider>() != null)
            Physics.IgnoreCollision(GetComponentInParent<CapsuleCollider>(), GetComponent<BoxCollider>());
    }

    public void EnableDamage(bool value)
    {
        damageEnabled = value;
    }

    public void PlayWeaponSwingSound()
    {
        HitManager.Instance.PlayWeaponSound(weaponSwingSounds[Mathf.FloorToInt(Random.Range(0f, weaponSwingSounds.Length - 1))]);
    }




    private void OnTriggerEnter(Collider other)
    {
        if (damageEnabled)
        {
            Debug.Log(other.name);
            if (other.GetComponent<IDamageable>() != null)
            {
                other.gameObject.GetComponent<IDamageable>().ApplyDamage(damageAmount, GetComponentInParent<PlayerController>().gameObject);

            }
                
        }

    }
}
