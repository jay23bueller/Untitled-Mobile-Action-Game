using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class Controller : MonoBehaviour
{
    protected Weapon weapon;
    [Task]
    protected bool isStunned;
    [SerializeField]
    protected float stunDuration;
  
    protected Animator anim;

    protected virtual void OnAwake()
    {
        weapon = GetComponentInChildren<Weapon>();
        anim = GetComponent<Animator>();
    }

    private void Awake()
    {
        OnAwake();
    }

    protected void PlayEquippedWeaponSound()
    {
        weapon.PlayWeaponSwingSound();
    }

    protected void EnableWeaponTrail(int value)
    {
        weapon.EnableWeaponTrail(value == 1 ? true : false);
    }


    protected void EnableEquippedWeaponDamage(int value)
    {
        weapon.EnableDamage(value == 1 ? true : false);
    }

    protected void ResetWeaponTrailAndDamage()
    {
        EnableWeaponTrail(0);
        EnableEquippedWeaponDamage(0);
    }

    public void GetStunned()
    {
        if(!isStunned)
        {
            isStunned = true;
            Stun();
            StartCoroutine(ResetStun());
        }

    }

    public void Stun()
    {
        anim.Play("Base Layer.Hit");
        ResetWeaponTrailAndDamage();
        
    }

    private IEnumerator ResetStun()
    {
        yield return new WaitForSecondsRealtime(stunDuration);

        isStunned = false;
    }
}
