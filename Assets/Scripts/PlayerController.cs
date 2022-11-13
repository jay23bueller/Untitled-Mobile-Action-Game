using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Controller
{
    #region Variables
    private PlayerInputActions inputActions;
    private CharacterController controller;

    //Attack Related Parameters
    [SerializeField]
    private float castRadius;
    private RaycastHit hit;
    [SerializeField]
    private float attackRange;
    private bool _acquiredTarget;
    private Vector3 attackVelocity;
    private Vector3 playerToEnemyDirection;
    private Vector3 lastKnownEnemyPosition;
    private bool nextAttack = false;
    [SerializeField]
    private float minDistanceFromEnemy;


    // Camera Related Parameters
    [SerializeField]
    private CinemachineFreeLook freeLookCam;
    [SerializeField]
    private float cameraMaxAmp;
    [SerializeField]
    private float cameraMaxFreq;

    [SerializeField]
    private float minTimeScale;


    [SerializeField]
    private float standingHeight = 1.72f;

    [SerializeField]
    private float rollingHeight;

    private SoundStimuli soundStimuli;

    #endregion


    #region Methods

    private void AnimatorDodge()
    {
        if (_anim.GetBool("dodging"))
        {
            ModifyCharacterControllerHeight();
            _anim.ApplyBuiltinRootMotion();
        }
    }

    // When attacking, try to acquire a target in the direction the player is facing or based on current input value.
    // If target has been acquired, move the player closer to the enemy to land the attack.
    private void AnimatorAttack()
    {
        if (_anim.GetBool("attack"))
        {

            if (!_acquiredTarget)
            {
                Vector3 attackDirection = new Vector3(inputActions.Player.Walk.ReadValue<Vector2>().x, 0f, inputActions.Player.Walk.ReadValue<Vector2>().y);

                transform.forward = attackDirection.magnitude == 0 ? transform.forward : Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * attackDirection;


                Physics.SphereCast(transform.position + controller.center, castRadius, transform.forward, out hit, attackRange);


                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    playerToEnemyDirection = (hit.collider.transform.position - transform.position);
                    lastKnownEnemyPosition = hit.collider.transform.position;

                    Debug.DrawLine(transform.position + controller.center, hit.collider.transform.position + controller.center, Color.red, 2f);
                    attackVelocity = playerToEnemyDirection / (_anim.GetCurrentAnimatorStateInfo(0).length - (_anim.GetCurrentAnimatorStateInfo(0).length * .5f));


                }
                _acquiredTarget = true;

            }

            if (_acquiredTarget && hit.collider != null)
            {
                playerToEnemyDirection.y = 0f;
                transform.forward = playerToEnemyDirection.normalized;
                if (Vector3.Distance(transform.position, lastKnownEnemyPosition) > minDistanceFromEnemy)
                {

                    controller.Move(attackVelocity * Time.deltaTime);
                }

            }

        }
    }



    //Preceded by Dodging, on initial attack the animation will start. On successive attacks to chain a combo,
    //the input will be registered if the attack animation is more than 30% through and not the final attack. 
    private void Attack()
    {

        if(!_anim.GetBool("dodging"))
        {
            if (!_anim.GetBool("attack"))
            {

                _anim.Play(_animToId["Base Layer.Axe_Combo_1"]);
                _anim.SetInteger("attackNum", 0);
                _anim.SetBool("attack", true);
            }
            else
            {
                AnimatorStateInfo currentStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
                if (currentStateInfo.IsTag("Attack") && currentStateInfo.normalizedTime > .3f && currentStateInfo.normalizedTime < 1f && _anim.GetInteger("attackNum") < 2)
                {
                    nextAttack = true;
                }
            }
        }

    }

    //An animation event set on certain attack animations in order to chain attacks together.
    private void CheckForNextAttack()
    {

        if(nextAttack)
        {
            _anim.SetInteger("attackNum", _anim.GetInteger("attackNum") + 1);
            nextAttack = false;
        }
        else
        {
            ResetAttack();
            _anim.Play(_animToId["Base Layer.Idle_Walk_Run"]);
            _weapon.EnableDamage(false);
        }

        _acquiredTarget = false;
        

    }

    //Can be called by events that take precedence over attacking
    private void ResetAttack()
    {
        _anim.SetBool("attack", false);
        _anim.SetInteger("attackNum", 0);
        ResetWeaponTrailAndDamage();
    }


    //Dodge in the direction the player is facing or in the direction specified
    //by input.
    private void Dodge()
    {
        if (!_anim.GetBool("dodging"))
        {
            Vector3 movementInput = new Vector3(); 
            CalculateMovementInputRelativeToCamera(ref movementInput);
            transform.forward = movementInput.magnitude > 0f ? movementInput : transform.forward;
            controller.height = rollingHeight;
            _anim.Play(_animToId["Base Layer.Dodge"]);
            _anim.SetBool("dodging", true);
            ResetAttack();
        }
 
    }

    //Reset all values tied to animations that have events.
    private void ResetAnimationValues()
    {
        if (_anim.GetBool("dodging"))
            ResetDodge();

        if (_anim.GetBool("attack"))
            ResetAttack();
    }

    //Can be called by events that take precedence over dodging
    private void ResetDodge()
    {
        _anim.SetBool("dodging", false);
        controller.height = standingHeight;
    }

    //Currently used to modify the character controller's height using a smooth curve tied
    //to the dodging animation.
    private void ModifyCharacterControllerHeight()
    {
        controller.height = _anim.GetFloat("height") * standingHeight;
    }





    #region Unity
    private void OnEnable()
    {
        inputActions.Enable();
    }

    void Update()
    {
        Vector3 movementInput = new Vector3();
        MovePlayer(ref movementInput);
        ApplyGravity(ref movementInput);

        controller.Move(movementInput * Time.deltaTime);
        _anim.SetFloat("velocity", controller.velocity.magnitude);

    }

    private void Awake()
    {
        OnAwake();

    }

    protected override void OnAwake()
    {
        base.OnAwake();
        inputActions = new PlayerInputActions();
        inputActions.Player.Attack.performed += _ => { Attack(); };
        inputActions.Player.Dodge.performed += _ => { Dodge(); };
        controller = GetComponent<CharacterController>();
        soundStimuli = GetComponentInChildren<SoundStimuli>();
    }

    private void OnAnimatorMove()
    {
        AnimatorAttack();

        AnimatorDodge();

    }
    #endregion

    private void CalculateMovementInputRelativeToCamera(ref Vector3 movementInput)
    {
       Vector2 input = inputActions.Player.Walk.ReadValue<Vector2>();
        movementInput.x = input.x;
        movementInput.z = input.y;
        movementInput = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * movementInput;


    }

    private void MovePlayer(ref Vector3 movementInput)
    {
        
        if(!_anim.GetBool("attack") && !_anim.GetBool("dodging"))
        {
            CalculateMovementInputRelativeToCamera(ref movementInput);

            transform.forward = Vector3.Slerp(transform.forward, movementInput, .1f);

            movementInput = movementInput * _movementSpeed;

        }

    }

    private void ApplyGravity(ref Vector3 movementInput)
    {
        if (!controller.isGrounded)
            movementInput.y = -9.8f;
    }

 

    //Currently called when the player hurts an enemy.
    public IEnumerator ShakeCameraAndSlowDownTime()
    {
        for(int i = 0; i < 3; i++)
        {
            freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = cameraMaxAmp;
            freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = cameraMaxFreq;
        }

        Time.timeScale = minTimeScale;

        yield return new WaitForSecondsRealtime(.2f);

        for (int i = 0; i < 3; i++)
        {
            freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0f;
            freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0f;
        }

        Time.timeScale = 1f;
    }


    protected override void PlayFootstepSound(AnimationEvent evt)
    {
        base.PlayFootstepSound(evt);
        float currentVelocity = _anim.GetFloat("velocity");

        if (currentVelocity > 0f)
            soundStimuli.MakeNoise(currentVelocity / _movementSpeed);   
    }

    #endregion

}
