using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;

public class Controller : MonoBehaviour
{


    #region Variables

    protected const int ANIM_TRUE = 1;
    protected const int ANIM_FALSE = 0;

    protected const string BASE_LAYER = "Base Layer.";

    protected Weapon _weapon;

    [SerializeField]
    protected float _stunDuration;

    [SerializeField]
    protected float _normalMovementSpeed;

    protected float _currentMovmentSpeed;

    [SerializeField]
    protected string[] _animationNames;

    protected Dictionary<string, int> _animToId;
  
    protected Animator _anim;

    #endregion
    #region Methods

    //Allow child classes to initialize their own items when Awake callback is executed
    protected virtual void OnAwake()
    {
        _weapon = GetComponentInChildren<Weapon>();
        _anim = GetComponent<Animator>();
        _animToId = new Dictionary<string, int>();
        _currentMovmentSpeed = _normalMovementSpeed;
        if(_animationNames.Length > 0)
        {
            foreach(string animationName in _animationNames)
            {
                _animToId.Add(BASE_LAYER + animationName, Animator.StringToHash(animationName));
            }
        }
    }

    private void Awake()
    {
        OnAwake();
    }

    protected void PlayEquippedWeaponSound()
    {
        _weapon.PlayWeaponSwingSound();
    }

    protected void EnableWeaponTrail(int value)
    {
        _weapon.EnableWeaponTrail(value == ANIM_TRUE ? true : false);
    }


    protected void EnableEquippedWeaponDamage(int value)
    {
        _weapon.EnableDamage(value == ANIM_TRUE ? true : false);
    }

    protected void ResetWeaponTrailAndDamage()
    {
        EnableWeaponTrail(ANIM_FALSE);
        EnableEquippedWeaponDamage(ANIM_FALSE);
    }

    public void GetStunned()
    {
        if(!IsStunned)
        {
            IsStunned = true;
            Stun();
            StartCoroutine(ResetStun());
        }

    }

    protected virtual void Stun()
    {
        _anim.Play(_animToId["Base Layer.Hit"]);
        ResetWeaponTrailAndDamage();
        
    }

    protected virtual void PlayFootstepSound(AnimationEvent evt)
    {
        
        if (evt.animatorClipInfo.weight > .5f)
        {
            bool isRunning = _anim.GetFloat("velocity") > .6f * _currentMovmentSpeed ? true : false;
            Transform footTransform = Random.Range(0, 24) % 2 == 0 ? _anim.GetBoneTransform(HumanBodyBones.LeftFoot) : _anim.GetBoneTransform(HumanBodyBones.RightFoot);
            HitManager.Instance.PlayFootstepSound(footTransform.position, isRunning);
        }
        

        

    }

    private IEnumerator ResetStun()
    {
        yield return new WaitForSecondsRealtime(_stunDuration);
        _anim.SetBool("hit", false);
        IsStunned = false;
    }

    #endregion
    #region Tasks

    [Task]
    protected bool IsStunned;

    #endregion 
}
