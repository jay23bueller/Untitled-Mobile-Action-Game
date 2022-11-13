using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;

public class EnemyController : Controller
{
    #region Variables

    [SerializeField]
    private float _distanceToPlayer = 2f;
    private NavMeshAgent _agent;
    [SerializeField]
    private Transform[] _waypoints;
    private int _waypointIdx;
    private bool _destinationSet;
    private EnemyPerception _perception;
    private bool _seeking;




    #endregion

    #region Methods

    private void Awake()
    {
        OnAwake();

    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _movementSpeed;
        _agent.destination = _waypoints[_waypointIdx].position;
        _perception = GetComponentInChildren<EnemyPerception>();
    }


    private void Update()
    {
        _anim.SetFloat("velocity", _agent.velocity.magnitude);
    }

    private void OnDrawGizmos()
    {
        if (_seeking)
            Gizmos.DrawSphere(_perception.DetectedPosition, 2f);
    }

    private void ResumeMovement()
    {
        _anim.Play(_animToId["Base Layer.Idle_Walk_Run"]);
        _agent.isStopped = false;
    }

    protected override void Stun()
    {
        base.Stun();
        _agent.isStopped = true;
    }
    #endregion

    #region Tasks

    [Task]
    private bool PlayerWasSeen;


    [Task]
    private void FindWaypoint()
    {
        _waypointIdx = (_waypointIdx + 1) % _waypoints.Length;

        ThisTask.Succeed();

    }

    [Task]
    private bool IsDestinationSet()
    {
        return _destinationSet;
    }

    [Task]
    private void SetDestination()
    {
        
        _agent.SetDestination(_waypoints[_waypointIdx].position);
        
        _destinationSet = true;
        ThisTask.Succeed();
        
    }

    [Task]
    private void ResetDestination()
    {
        _destinationSet = false;
        ThisTask.Succeed();
    }

    [Task]
    private bool HasArrivedAtDestination()
    {
        
        if(Vector3.Distance(_agent.transform.position, _waypoints[_waypointIdx].position) < .2f)
            return true;

        ThisTask.debugInfo = "Is going to place?";
        
        return false;

    }


    [Task]
    private void ChasePlayer()
    {
        
        if(_perception.HasLineOfSight())
        {
            _agent.SetDestination(_perception.DetectedPosition);
            _agent.speed = 6f;
            _destinationSet = false;
            _perception.ChasingPlayer = true;
            ThisTask.debugInfo = "Seeking player in sight";

            if (Vector3.Distance(_agent.transform.position, _agent.destination) < _distanceToPlayer)
            {
                Debug.Log("at the place");
                _perception.ChasingPlayer = false;
                ThisTask.Succeed();
            }

        }
        else
        {
            _perception.ChasingPlayer = false;
            ThisTask.Fail();
        }

  

    }

    [Task]
    private bool WasPlayerSeenLastFrame()
    {
        ThisTask.debugInfo = "" + _perception.GetPlayerWasSeenLastFrame();
        return _perception.GetPlayerWasSeenLastFrame();
    }

    [Task]
    private bool WasPlayerHeard()
    {
        return _perception.PlayerWasHeard;
    }

    [Task]
    private void GoToLastKnownPlayerPositionWhileSeekingPlayer()
    {
        if(ThisTask.isStarting)
        {
            _seeking = true;
            _agent.SetDestination(_perception.DetectedPosition);
        } 
        else if( (Vector3.Distance(transform.position, _agent.destination) < _distanceToPlayer) || _perception.PlayerIsInSight || _perception.PlayerWasHeard)
        {
            _seeking = false;
            ThisTask.Succeed();
        }
    }

    [Task]
    private bool HasLineOfSight()
    {
        PlayerWasSeen = _perception.HasLineOfSight();
        return PlayerWasSeen;
    }

    [Task]
    private bool IsChasingPlayer()
    {
        return _perception.ChasingPlayer;
    }

    [Task]
    private void WindupAttack()
    {
        _anim.CrossFade(_animToId["Base Layer.Windup"],.2f);
        ThisTask.Succeed();
    }

    [Task]
    private void Attack()
    {
        _anim.CrossFade(_animToId["Base Layer.Slash"], .2f);
        ThisTask.Succeed();
    }

    [Task]
    private bool IsNearPlayer()
    {
        ThisTask.debugInfo = _perception.PlayerIsInSight != false ? "Distance to player" + Vector3.Distance(transform.position, _perception.DetectedPosition) : "Player not set";
        if (_perception.PlayerIsInSight && Vector3.Distance(_perception.DetectedPosition, transform.position) < _distanceToPlayer)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            return true;
        }
        else if(_agent.isStopped)
        {        
            ResumeMovement();
        }
            

        return false;
    }

    [Task]
    private void LookAtPlayerWhileWindingUp(float duration)
    {
        if(ThisTask.isStarting)
        {
            PTaskTimer windupTimer = new PTaskTimer(Time.time, duration);
            ThisTask.data = windupTimer;
        }

        PTaskTimer wT = ThisTask.GetData<PTaskTimer>();

        if (wT.GetElapsedTime() >= 1.0f)
            ThisTask.Succeed();


        if(_perception.HasLineOfSight())
        {
            transform.LookAt(_perception.DetectedPosition, Vector3.up);
        }
    }

    [Task]
    private void WaitUnlessPlayerSensed(float duration)
    {

        if(ThisTask.isStarting)
        {
            PTaskTimer inSightTimer = new PTaskTimer(Time.time, duration);
            ThisTask.data = inSightTimer;
        }

        PTaskTimer pT = ThisTask.GetData<PTaskTimer>();

        float elapsedTime = pT.GetElapsedTime();
        if(_perception.HasLineOfSight() || _perception.PlayerWasHeard)
        {
            ThisTask.Succeed();
        }
        else if (elapsedTime >= 1.0f)
        {
            ThisTask.Fail();
        }


    }

    private struct PTaskTimer
    {
        public PTaskTimer(float startTime, float duration)
        {
            this.startTime = startTime;
            this.duration = duration;
        }

        public float GetElapsedTime()
        {

            return (Time.time - startTime)/duration;
        }
        private float startTime;
        private float duration;
    }
    #endregion
}
