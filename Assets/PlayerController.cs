using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public PlayerInputActions inputs;
    public CharacterController controller;
    private Animator anim;
    [SerializeField]
    private float movementSpeed;
    public bool isAttacking;
    private int attackNum = 0;
    public bool nextAttack = false;
    public bool initialAttack = true;
    public bool resetAttack = false;
    public Vector3 attackDirection;

    bool lateUpdate = true;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }


    public void InitialAttack()
    {
        if(initialAttack)
        {
            initialAttack = false;
            isAttacking = true;
            attackDirection = new Vector3(inputs.Player.Walk.ReadValue<Vector2>().x, 0f, inputs.Player.Walk.ReadValue<Vector2>().y);
            transform.forward = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * attackDirection;
            anim.SetBool("attack", true);
            anim.SetInteger("attackNum", 0);
        }

    }

    public void CheckIfComboing()
    {
        if(!nextAttack && inputs.Player.Attack.phase == InputActionPhase.Performed && attackNum != 2)
        {
            attackNum = (attackNum + 1) % 3;
            Debug.Log(attackNum);

            nextAttack = true;
            
        }

    }

    private void OnAnimatorMove()
    {
        controller.Move(anim.GetFloat("Distance") * 5f * Time.deltaTime * transform.forward);
        
    }


    public void Attack()
    {
        //if(initialAttack)
        //{
        //    initialAttack = false;
        //    isAttacking = true;
        //    attackDirection = new Vector3(inputs.Player.Walk.ReadValue<Vector2>().x, 0f, inputs.Player.Walk.ReadValue<Vector2>().y);
        //    transform.forward = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * attackDirection;
        //    anim.SetBool("attack", true);
        //    anim.SetInteger("attackNum", 0);
        //}
        //else if(nextAttack)
        //{


        //    attackDirection = new Vector3(inputs.Player.Walk.ReadValue<Vector2>().x, 0f, inputs.Player.Walk.ReadValue<Vector2>().y);
        //    if (attackDirection.magnitude > 0)
        //        lateUpdate = true;



        //    Debug.Log(transform.forward);
        //    anim.SetInteger("attackNum", attackNum);
        //    nextAttack = false;
        //} 
        //else if(!resetAttack)
        //{
        //    Debug.Log("Called");
        //    anim.SetBool("attack", false);
        //    resetAttack = true;
        //    initialAttack = true;
        //    isAttacking = false;
        //    attackNum = 0;
        //}

        AnimatorStateInfo animStateInfo = anim.GetCurrentAnimatorStateInfo(0);

        attackDirection = new Vector3(inputs.Player.Walk.ReadValue<Vector2>().x, 0f, inputs.Player.Walk.ReadValue<Vector2>().y);

        if(((attackNum + 1) != 3) && !nextAttack && animStateInfo.IsTag("Attack") && animStateInfo.normalizedTime > .5f && animStateInfo.normalizedTime < 1f)
        {
            //Is currently doing an attack and there is another state for the combo
            nextAttack = true;
            Debug.Log("enxt");
            attackNum = (attackNum + 1) % 3;


        }
        else if(attackNum == 0)
        {
            anim.SetInteger("attackNum", attackNum);
            anim.SetBool("attack", true);
        }

    }

    private void CheckForNextAttack()
    {
        if(nextAttack)
        {
            lateUpdate = true;
            Debug.Log("sort a called " + attackNum);
            anim.SetInteger("attackNum", attackNum);
            nextAttack = false;
        } else
        {
            Debug.Log("Called");
            anim.SetBool("attack", false);
           
            attackNum = 0;
        }
    }

    private void LateUpdate()
    {
        if (lateUpdate)
        {
            transform.forward = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * attackDirection;
            lateUpdate = false;
        }
        
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
        
        if(!isAttacking)
        {
            Vector3 movementInput = new Vector3(inputs.Player.Walk.ReadValue<Vector2>().x, 0f, inputs.Player.Walk.ReadValue<Vector2>().y);

            anim.SetFloat("velocity", movementInput.magnitude);

            movementInput = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * movementInput;

            transform.forward = Vector3.Slerp(transform.forward, movementInput, .1f);

            controller.Move(movementInput * movementSpeed * Time.deltaTime);
        }

    }

 
}
