using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

public class PlayerController : Controller
{

    #region Variables

    //Constants
    private const float MOVEMENT_ROTATION_DELTA_ANGLE = 5f;
    private const float AIM_ROTATION_PITCH_MAX_ANGLE = 89f;
    private const float AIM_ROTATION_PITCH_MIN_ANGLE = -89f;
    private const float AIM_ROTATION_YAW_SPEED = 60f;
    private const String ANIM_BOW_LAYER = "Bow Layer.";

    private PlayerInputActions _inputActions;
    private CharacterController _controller;

    //Attack Parameters
    [SerializeField]
    private float _castRadius;
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private float _strafeSpeed;
    private bool _acquiredTarget;
    private Vector3 _attackVelocity;
    private Vector3 _playerToEnemyDirection;
    private Vector3 _lastKnownEnemyPosition;
    private bool _nextAttack = false;
    private int _enemyIdx = -1;
    [SerializeField]
    private float _minDistanceFromEnemy;


    // Camera Related Parameters
    [SerializeField]
    private CinemachineFreeLook _freeLookCam;
    [SerializeField]
    private float _cameraMaxAmp;
    [SerializeField]
    private float _cameraMaxFreq;

    [SerializeField]
    private float _minTimeScale;

    [SerializeField]
    private CinemachineVirtualCamera _cinemachineThirdPersonCamera;

    //Dodge Parameters
    [SerializeField]
    private float _standingHeight = 1.72f;

    [SerializeField]
    private float _rollingHeight;

    private SoundStimuli _soundStimuli;

    private bool _drawAttackSphereCast;

    private Transform _lookAtTransform;
    private float _lookAtTransformPitchAngle = 0f;

    // Bow and Arrow
    [SerializeField]
    private GameObject _bow;

    [SerializeField]
    private GameObject _arrowPrefab;
    private GameObject _arrow;
    [SerializeField]
    private Transform _arrowSpawnTransform;

    private Coroutine _loweringBowRoutine;
    private Coroutine _raisingBowRoutine;

    [SerializeField]
    private String[] _bowAnimationNames;

    private bool _releaseArrow;
    private bool _removeArrow;
    private static System.Object arrowLock = new System.Object();

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
            RaycastHit[] hits = null;
            
            
            if (!_acquiredTarget)
            {
                Vector3 attackDirection = new Vector3();
                CalculateMovementInputRelativeToCamera(ref attackDirection);

               
                
                hits = Physics.SphereCastAll(transform.position + _controller.center, _castRadius, transform.forward, _attackRange);
                if (attackDirection.magnitude > 0)
                    hits.OrderByDescending(hit => Vector3.Dot(attackDirection, (hit.collider.transform.position - transform.position).normalized));

                for(int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider != null && hits[i].collider.CompareTag("Enemy"))
                    {
                        hits[i].collider.GetComponent<EnemyController>().ReadyForPlayerAttack();
                        
                        _playerToEnemyDirection = (hits[i].collider.transform.position - transform.position);
                        _lastKnownEnemyPosition = hits[i].collider.transform.position;

                        Debug.DrawLine(transform.position + _controller.center, hits[i].collider.transform.position + _controller.center, Color.red, 4f);
                        _attackVelocity = _playerToEnemyDirection / (_anim.GetCurrentAnimatorStateInfo(0).length - (_anim.GetCurrentAnimatorStateInfo(0).length * .5f));
                        _enemyIdx = i;
                        _acquiredTarget = true;
                        break;


                    }

                }



            }

            if (_acquiredTarget && _enemyIdx != -1)
            {
                _playerToEnemyDirection.y = 0f;
                transform.forward = _playerToEnemyDirection.normalized;
                if (Vector3.Dot(transform.forward, (_lastKnownEnemyPosition - transform.position).normalized) > .92 && (Vector3.Distance(transform.position, _lastKnownEnemyPosition) > _minDistanceFromEnemy))
                {
                    Debug.Log($"Current attack velocity : {_attackVelocity}");
                    _drawAttackSphereCast = true;
                    _controller.Move(_attackVelocity * Time.deltaTime);
                }

            }

        }
    }

    private void OnDrawGizmos()
    {
        if(_drawAttackSphereCast)
        {
            Gizmos.DrawWireSphere(_lastKnownEnemyPosition + _controller.center, _castRadius);
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
                    _nextAttack = true;
                }
            }
        }

    }

    //An animation event set on certain attack animations in order to chain attacks together.
    private void CheckForNextAttack()
    {
        _acquiredTarget = false;
        _drawAttackSphereCast = false;
        if (_nextAttack)
        {
            _anim.SetInteger("attackNum", _anim.GetInteger("attackNum") + 1);
            _nextAttack = false;
        }
        else
        {
            ResetAttack();
            _anim.Play(_animToId["Base Layer.Idle_Walk_Run"]);
            _weapon.EnableDamage(false);
        }


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
            _controller.height = _rollingHeight;
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
        _controller.height = _standingHeight;
    }

    //Currently used to modify the character controller's height using a smooth curve tied
    //to the dodging animation.
    private void ModifyCharacterControllerHeight()
    {
        _controller.height = _anim.GetFloat("height") * _standingHeight;
    }

    private void Aim()
    {
        _freeLookCam.Priority = 0;
        _weapon.gameObject.SetActive(false);
        _bow.SetActive(true);
        _releaseArrow = false;
       
        
        _anim.SetBool("aiming", true);
        if(_loweringBowRoutine != null)
            StopCoroutine(_loweringBowRoutine);

        _raisingBowRoutine = StartCoroutine(LowerOrRaiseBow(false));
        _currentMovmentSpeed = _strafeSpeed;
        
    }

    private void CreateArrow()
    {

        _releaseArrow = false;
        _arrow = Instantiate(_arrowPrefab, _arrowSpawnTransform.position, _arrowSpawnTransform.rotation, _arrowSpawnTransform);
        
        Physics.IgnoreCollision(_arrow.GetComponent<BoxCollider>(), _controller);
    }

    private void ResetAim()
    {
        _anim.SetBool("shoot", false);
    }

    private void ReadyToShoot()
    {
        _anim.SetBool("shoot", true);
    }

    private void ReleaseArrow()
    {
        _releaseArrow = true;
        //shoot arrow
    }

    private void ShootArrow()
    {

            _arrow.transform.SetParent(null);
            _arrow.GetComponent<Rigidbody>().isKinematic = false;
            _arrow.GetComponent<Rigidbody>().AddForce(_lookAtTransform.forward * 15f, ForceMode.Impulse);
            Destroy(_arrow, 5f);
            _arrow = null;
            _releaseArrow = false;
            _anim.SetBool("shoot", false);


    }

    private void DestroyArrow(GameObject arrow)
    {
            Destroy(arrow);

    }


    private void ReleaseAim()
    {
        if (!_releaseArrow)
            Destroy(_arrow);
        _arrow = null;
        _releaseArrow = false;
        _anim.SetBool("aiming", false);

        _bow.SetActive(false);

        if (_raisingBowRoutine != null)
            StopCoroutine(_raisingBowRoutine);

        _loweringBowRoutine = StartCoroutine(LowerOrRaiseBow(true));
        _freeLookCam.Priority = 10;
        _lookAtTransform.localRotation = Quaternion.identity;
        _lookAtTransformPitchAngle = 0f;
        _currentMovmentSpeed = _normalMovementSpeed;
        _weapon.gameObject.SetActive(true);
    }

    private IEnumerator LowerOrRaiseBow(bool lower)
    {

        for(int i = 0; i < 20; i++)
        {
            yield return new WaitForSecondsRealtime(.01f);
            
            _anim.SetLayerWeight(1, Mathf.Clamp(_anim.GetLayerWeight(1) + (lower == true ? -.05f: .05f),0f, 1f));
        }
        
    }


    #region Unity
    private void OnEnable()
    {
        _inputActions.Enable();
    }

    void Update()
    {
        Vector3 movementInput = new Vector3();
        MovePlayer(ref movementInput);
        ApplyGravity(ref movementInput);
        _controller.Move(movementInput * Time.deltaTime);

        RotatePlayer();

        if(_arrow != null && _releaseArrow)
        {
            ShootArrow();
        }


        _anim.SetFloat("velocity", _controller.velocity.magnitude);
        _anim.SetFloat("velocityX", _controller.velocity.x);
        _anim.SetFloat("velocityZ", _controller.velocity.z);

    }

    private void Awake()
    {
        OnAwake();

    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Attack.performed += _ => { Attack(); };
        _inputActions.Player.Dodge.performed += _ => { Dodge(); };
        _inputActions.Player.Aim.performed += _ => { Aim(); };
        _inputActions.Player.Aim.canceled += _ => { ReleaseAim(); };
        _lookAtTransform = GameObject.FindGameObjectWithTag("CinemachineTarget").transform;
        _controller = GetComponent<CharacterController>();
        _soundStimuli = GetComponentInChildren<SoundStimuli>();

        foreach (string animName in _bowAnimationNames)
            _animToId.Add(ANIM_BOW_LAYER + animName, Animator.StringToHash(ANIM_BOW_LAYER + animName));
    }

    private void OnAnimatorMove()
    {
        AnimatorAttack();

        AnimatorDodge();


    }

    // Use in late update to update the rotation of the spine bone so the character is oriented in the same direction as the look at 
    // rotation when aiming (if it becomes a problem, set up the animation rigging with aim constraints)
    private void UpdateSpineRotation()
    {
        if (_anim.GetBool("aiming") && _anim.GetLayerWeight(1) >= 1f)
        {

            _anim.GetBoneTransform(HumanBodyBones.Spine).RotateAround(_anim.GetBoneTransform(HumanBodyBones.Spine).position,_anim.GetBoneTransform(HumanBodyBones.Spine).up, -_lookAtTransform.localRotation.eulerAngles.x);


        }

    }

    private void LateUpdate()
    {
        if(_anim.GetBool("aiming"))
            UpdateSpineRotation();
    }
    #endregion

    private void CalculateMovementInputRelativeToCamera(ref Vector3 movementInput)
    {
       Vector2 input = _inputActions.Player.Walk.ReadValue<Vector2>();
        movementInput.x = input.x;
        movementInput.z = input.y;
        movementInput = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * movementInput;


    }

    private void RotatePlayer()
    {
        if (!_anim.GetBool("dodging"))
        {
            if (!_anim.GetBool("aiming"))
            {
                //The character is rotated based on the movement input
                Vector2 input = _inputActions.Player.Walk.ReadValue<Vector2>();

                if (input.sqrMagnitude > 0f)
                {
                    float yawAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, yawAngle, 0f), MOVEMENT_ROTATION_DELTA_ANGLE);
                }

            }
            else
            {
                //The 3rd-Person camera is looking at a gameobject that is a child of the player game object. The look input is used to
                //rotate the child gameobject around it's local x-axis and rotates the player game game object around it's y-axis.
                Vector2 input = _inputActions.Player.Look.ReadValue<Vector2>();

                _lookAtTransformPitchAngle = Mathf.Clamp(_lookAtTransformPitchAngle + (-input.y * 60f * Time.deltaTime), AIM_ROTATION_PITCH_MIN_ANGLE, AIM_ROTATION_PITCH_MAX_ANGLE);
                _lookAtTransform.localRotation = Quaternion.Euler(_lookAtTransformPitchAngle, 0f, 0f);




                transform.Rotate(0f, input.x * AIM_ROTATION_YAW_SPEED * Time.deltaTime, 0f);
            }
        }
       

        
    }

    private void MovePlayer(ref Vector3 movementInput)
    {
        
        if(!_anim.GetBool("attack") && !_anim.GetBool("dodging"))
        {
            CalculateMovementInputRelativeToCamera(ref movementInput);

            movementInput = movementInput * _currentMovmentSpeed;

        }

    }

    private void ApplyGravity(ref Vector3 movementInput)
    {
        if (!_controller.isGrounded)
            movementInput.y = -9.8f;
    }

 

    //Currently called when the player hurts an enemy.
    public IEnumerator ShakeCameraAndSlowDownTime()
    {
        for(int i = 0; i < 3; i++)
        {
            _freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = _cameraMaxAmp;
            _freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = _cameraMaxFreq;
        }

        Time.timeScale = _minTimeScale;

        yield return new WaitForSecondsRealtime(.2f);

        for (int i = 0; i < 3; i++)
        {
            _freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0f;
            _freeLookCam.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0f;
        }

        Time.timeScale = 1f;
    }


    protected override void PlayFootstepSound(AnimationEvent evt)
    {
        base.PlayFootstepSound(evt);
        float currentVelocity = _anim.GetFloat("velocity");

        if (currentVelocity > 0f)
            _soundStimuli.MakeNoise(currentVelocity / _currentMovmentSpeed);   
    }

    #endregion

}
