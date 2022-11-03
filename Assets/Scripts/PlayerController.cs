using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private PlayerInputActions inputActions;
    private CharacterController controller;
    private Animator anim;


    //Movement Related Parameters
    [SerializeField]
    private float movementSpeed;


    //Attack Related Parameters
    [SerializeField]
    private float castRadius;
    private RaycastHit hit;
    [SerializeField]
    private float attackRange;
    private bool justHit = false;
    private Vector3 attackVelocity;
    private Vector3 playerToEnemyDirection;
    private bool nextAttack = false;
    [SerializeField]
    private float minDistanceFromEnemy;
    private Weapon playerWeapon;

    [SerializeField]
    private CinemachineFreeLook freeLookCam;
    [SerializeField]
    private float cameraMaxAmp;
    [SerializeField]
    private float cameraMaxFreq;

    [SerializeField]
    private float minTimeScale;




    private void OnAnimatorMove()
    {
        if(anim.GetBool("attack"))
        {

            if (!justHit)
            {
                Vector3 attackDirection = new Vector3(inputActions.Player.Walk.ReadValue<Vector2>().x, 0f, inputActions.Player.Walk.ReadValue<Vector2>().y);

                transform.forward = attackDirection.magnitude  == 0 ? transform.forward : Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * attackDirection;


                //Physics.CapsuleCast(
                //(controller.center + transform.position) + (transform.right * -5f),
                //(controller.center + transform.position) + (transform.right * 5f),
                //capsuleCastRadius,
                //transform.forward,
                //out hit, attackRange, LayerMask.GetMask("Enemy"));

                Physics.SphereCast(transform.position + controller.center, castRadius, transform.forward, out hit, attackRange, LayerMask.GetMask("Enemy"));

                


                
                if (hit.collider != null)
                {
                    playerToEnemyDirection = (hit.collider.transform.position - transform.position);
                    

                    Debug.DrawLine(transform.position + controller.center, hit.collider.transform.position + controller.center, Color.red, 2f);
                    attackVelocity = playerToEnemyDirection / (anim.GetCurrentAnimatorStateInfo(0).length - (anim.GetCurrentAnimatorStateInfo(0).length*.5f));
                    

                }
                justHit = true;

            }


            if(justHit && hit.collider != null && Vector3.Distance(transform.position, hit.transform.position) > minDistanceFromEnemy)
            {
                transform.forward = playerToEnemyDirection.normalized;
                controller.Move(attackVelocity * Time.deltaTime);
            }

            


        }
            
        
        
    }

    //private void LateUpdate()
    //{

    //}


    private void Attack()
    {

        if(!anim.GetBool("attack"))
        {

            anim.Play("Base Layer.Axe_Combo_1");
            anim.SetInteger("attackNum", 0);
            anim.SetBool("attack", true);
            playerWeapon.EnableDamage(true);
        }
        else
        {
            AnimatorStateInfo currentStateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if(currentStateInfo.IsTag("Attack") && currentStateInfo.normalizedTime > .3f && currentStateInfo.normalizedTime < 1f && anim.GetInteger("attackNum") < 2)
            {
                nextAttack = true;
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
            anim.SetBool("attack", false);
            anim.SetInteger("attackNum", 0);
            anim.Play("Base Layer.Idle");
            playerWeapon.EnableDamage(false);
        }

        justHit = false;
        

    }


    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Attack.performed += _ =>{ Attack(); };
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        playerWeapon = GetComponentInChildren<Weapon>();

    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();

    }

    private void MovePlayer()
    {
        
        if(!anim.GetBool("attack"))
        {
            Vector3 movementInput = new Vector3(inputActions.Player.Walk.ReadValue<Vector2>().x, 0f, inputActions.Player.Walk.ReadValue<Vector2>().y);

            anim.SetFloat("velocity", movementInput.magnitude);

            movementInput = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * movementInput;

            transform.forward = Vector3.Slerp(transform.forward, movementInput, .1f);

            //transform.forward = movementInput.magnitude != 0 ? movementInput : transform.forward;

            controller.Move(movementInput * movementSpeed * Time.deltaTime);
        }

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

    private void PlayEquippedWeaponSound()
    {
        playerWeapon.PlayWeaponSwingSound();
    }





}
