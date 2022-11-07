using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class Controller : MonoBehaviour
{
    protected Weapon weapon;
    [Task]
    protected bool isStunned;
  
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
        if (weapon.GetComponent<TrailRenderer>() != null)
            weapon.GetComponent<TrailRenderer>().enabled = value == 1 ? true : false;
    }


    protected void EnableEquippedWeaponDamage(int value)
    {
        weapon.EnableDamage(value == 1 ? true : false);
    }

    public void GetStunned(int value)
    {
        isStunned = value == 1 ? true : false;
    }
}
