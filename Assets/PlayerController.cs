using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private PlayerInputActions inputs;
    private CharacterController controller;
    private Animator anim;
    [SerializeField]
    private float movementSpeed;
    private bool nextAttack = false;

    [SerializeField]
    private float attackRange;

    [SerializeField]
    private float capsuleCastRadius;

    private RaycastHit hit;

    private bool justHit = false;
    private Vector3 attackVelocity;
    private Vector3 attackOffSettedLocation;



    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }


    private void OnAnimatorMove()
    {
        if(anim.GetBool("attack"))
        {

            if (!justHit)
            {
                Vector3 inputDirection = new Vector3(inputs.Player.Walk.ReadValue<Vector2>().x, 0f, inputs.Player.Walk.ReadValue<Vector2>().y);

                transform.forward = inputDirection.magnitude  == 0 ? transform.forward : Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * inputDirection;


                Physics.CapsuleCast(
                (controller.height / 2 * transform.up) + transform.position,
                (controller.height / 2 * -transform.up) + transform.position,
                capsuleCastRadius,
                transform.forward,
                out hit, attackRange, LayerMask.GetMask("Enemy"));


                
                if (hit.collider != null)
                {
                    attackOffSettedLocation = (hit.collider.transform.position - transform.position);
                    transform.forward =  Quaternion.AngleAxis(Vector3.Angle(transform.forward, attackOffSettedLocation), Vector3.up) * transform.forward;

                    Debug.DrawLine(transform.position, attackOffSettedLocation + transform.position, Color.red, 2f);
                    attackVelocity = attackOffSettedLocation / (anim.GetCurrentAnimatorStateInfo(0).length - (anim.GetCurrentAnimatorStateInfo(0).length*.5f));
                    
                    Debug.Log(hit.collider.gameObject.name);

                }
                justHit = true;

            }

            if(justHit && hit.collider != null && Vector3.Distance(transform.position, attackOffSettedLocation) > 1f)
            {
                controller.Move(attackVelocity * Time.deltaTime);
            }

            


        }
            
        
        
    }


    public void Attack()
    {

        if(!anim.GetBool("attack"))
        {

            anim.Play("Base Layer.Axe_Combo_1");
            anim.SetInteger("attackNum", 0);
            anim.SetBool("attack", true);
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
        }

        justHit = false;
        

    }


    private void Awake()
    {
        inputs = new PlayerInputActions();
        inputs.Player.Attack.performed += _ =>{ Attack(); };
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        inputs.Enable();
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
            Vector3 movementInput = new Vector3(inputs.Player.Walk.ReadValue<Vector2>().x, 0f, inputs.Player.Walk.ReadValue<Vector2>().y);

            anim.SetFloat("velocity", movementInput.magnitude);

            movementInput = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * movementInput;

            transform.forward = Vector3.Slerp(transform.forward, movementInput, .1f);

            //transform.forward = movementInput.magnitude != 0 ? movementInput : transform.forward;

            controller.Move(movementInput * movementSpeed * Time.deltaTime);
        }

    }

 
}
