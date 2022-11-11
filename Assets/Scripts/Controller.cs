using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using System;

public class Controller : MonoBehaviour
{
    protected Weapon weapon;
    [Task]
    protected bool isStunned;
    [SerializeField]
    protected float stunDuration;

    [SerializeField]
    protected float movementSpeed;

    [SerializeField]
    protected string[] animationNames;

    protected Dictionary<string, int> animNameToId;
  
    protected Animator anim;

    protected virtual void OnAwake()
    {
        weapon = GetComponentInChildren<Weapon>();
        anim = GetComponent<Animator>();
        animNameToId = new Dictionary<string, int>();
        if(animationNames.Length > 0)
        {
            foreach(string animationName in animationNames)
            {
                animNameToId.Add(animationName, Animator.StringToHash(animationName));
            }
        }
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

    protected virtual void Stun()
    {
        anim.Play(animNameToId["Base Layer.Hit"]);
        ResetWeaponTrailAndDamage();
        
    }

    protected virtual void PlayFootstepSound(AnimationEvent evt)
    {
        
        if (evt.animatorClipInfo.weight > .5f)
        {
            bool isRunning = anim.GetFloat("velocity") > .6f * movementSpeed ? true : false;

            HitManager.Instance.PlayFootstepSound(anim.GetBoneTransform(HumanBodyBones.LeftFoot).position, isRunning);
        }
        

        

    }

    private IEnumerator ResetStun()
    {
        yield return new WaitForSecondsRealtime(stunDuration);

        isStunned = false;
    }
}
