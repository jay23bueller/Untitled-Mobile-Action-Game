using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Controller
{

    private PlayerInputActions inputActions;
    private CharacterController controller;




    //Attack Related Parameters
    [SerializeField]
    private float castRadius;
    private RaycastHit hit;
    [SerializeField]
    private float attackRange;
    private bool justHit = false;
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







    private void OnAnimatorMove()
    {
        if(anim.GetBool("attack"))
        {

            if (!justHit)
            {
                Vector3 attackDirection = new Vector3(inputActions.Player.Walk.ReadValue<Vector2>().x, 0f, inputActions.Player.Walk.ReadValue<Vector2>().y);

                transform.forward = attackDirection.magnitude  == 0 ? transform.forward : Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * attackDirection;


                Physics.SphereCast(transform.position + controller.center, castRadius, transform.forward, out hit, attackRange);





                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    playerToEnemyDirection = (hit.collider.transform.position - transform.position);
                    lastKnownEnemyPosition = hit.collider.transform.position;

                    Debug.DrawLine(transform.position + controller.center, hit.collider.transform.position + controller.center, Color.red, 2f);
                    attackVelocity = playerToEnemyDirection / (anim.GetCurrentAnimatorStateInfo(0).length - (anim.GetCurrentAnimatorStateInfo(0).length*.5f));
                    

                }
                justHit = true;

            }



            if(justHit && hit.collider != null)
            {
                playerToEnemyDirection.y = 0f;
                transform.forward = playerToEnemyDirection.normalized;
                if (Vector3.Distance(transform.position, lastKnownEnemyPosition) > minDistanceFromEnemy)
                {
                    
                    controller.Move(attackVelocity * Time.deltaTime);
                }
                //else if(Vector3.Distance(transform.position, lastKnownEnemyPosition) < minDistanceFromEnemy)
                //{
                //    controller.Move(-attackVelocity * Time.deltaTime);
                //}
                
            }

            


        }

        if(anim.GetBool("dodging"))
        {
            ModifyCharacterControllerHeight();
            anim.ApplyBuiltinRootMotion();
        }
            
        
        
    }



    private void Attack()
    {

        if(!anim.GetBool("dodging"))
        {
            if (!anim.GetBool("attack"))
            {

                anim.Play(animNameToId["Base Layer.Axe_Combo_1"]);
                anim.SetInteger("attackNum", 0);
                anim.SetBool("attack", true);
            }
            else
            {
                AnimatorStateInfo currentStateInfo = anim.GetCurrentAnimatorStateInfo(0);
                if (currentStateInfo.IsTag("Attack") && currentStateInfo.normalizedTime > .3f && currentStateInfo.normalizedTime < 1f && anim.GetInteger("attackNum") < 2)
                {
                    nextAttack = true;
                }
            }
        }




    }

    private void CheckForNextAttack()
    {

        if(nextAttack)
        {
            anim.SetInteger("attackNum", anim.GetInteger("attackNum") + 1);
            nextAttack = false;
        }
        else
        {
            ResetAttack();
            anim.Play(animNameToId["Base Layer.Idle_Walk_Run"]);
            weapon.EnableDamage(false);
        }

        justHit = false;
        

    }

    private void ResetAttack()
    {
        anim.SetBool("attack", false);
        anim.SetInteger("attackNum", 0);

    }    
    
    private void Dodge()
    {
        if (!anim.GetBool("dodging"))
        {
            Vector3 movementInput = new Vector3(); 
            CalculateMovementInputRelativeToCamera(ref movementInput);
            transform.forward = movementInput.magnitude > 0f ? movementInput : transform.forward;
            controller.height = rollingHeight;
            anim.Play(animNameToId["Base Layer.Dodge"]);
            anim.SetBool("dodging", true);
            ResetAttack();
            ResetWeaponTrailAndDamage();
        }
 
    }

    private void ResetAnimationValues()
    {
        if (anim.GetBool("dodging"))
            ResetDodge();

        if (anim.GetBool("attack"))
            ResetAttack();
    }

    private void ResetDodge()
    {
        anim.SetBool("dodging", false);
        controller.height = standingHeight;
    }

    private void ModifyCharacterControllerHeight()
    {
        controller.height = anim.GetFloat("height") * standingHeight;
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

    private void OnEnable()
    {
        inputActions.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movementInput = new Vector3();
        MovePlayer(ref movementInput);
        ApplyGravity(ref movementInput);

        controller.Move(movementInput * Time.deltaTime);
        anim.SetFloat("velocity", controller.velocity.magnitude);

    }

    private void CalculateMovementInputRelativeToCamera(ref Vector3 movementInput)
    {
       Vector2 input = inputActions.Player.Walk.ReadValue<Vector2>();
        movementInput.x = input.x;
        movementInput.z = input.y;
        movementInput = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * movementInput;


    }

    private void MovePlayer(ref Vector3 movementInput)
    {
        
        if(!anim.GetBool("attack") && !anim.GetBool("dodging"))
        {
            CalculateMovementInputRelativeToCamera(ref movementInput);

            transform.forward = Vector3.Slerp(transform.forward, movementInput, .1f);

            movementInput = movementInput * movementSpeed;

        }

    }

    private void ApplyGravity(ref Vector3 movementInput)
    {
        if (!controller.isGrounded)
            movementInput.y = -9.8f;
    }

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
        float currentVelocity = anim.GetFloat("velocity");

        if (currentVelocity > 0f)
            soundStimuli.MakeNoise(currentVelocity / movementSpeed);
            
        
    }



}
