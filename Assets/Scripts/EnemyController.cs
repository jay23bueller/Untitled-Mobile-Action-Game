using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;

public class EnemyController : Controller
{
    [SerializeField]
    private float distanceToPlayer = 2f;
    private NavMeshAgent agent;
    [SerializeField]
    private Transform[] waypoints;
    private int waypointIdx;
    private bool DestinationSet;
    private EnemyPerception perception;

    [Task]
    private bool playerWasSeen;
  

    private void Awake()
    {
        OnAwake();

    }

    protected override void OnAwake()
    {
        base.OnAwake();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = movementSpeed;
        agent.destination = waypoints[waypointIdx].position;
        perception = GetComponentInChildren<EnemyPerception>();
    }


    private void Update()
    {
        anim.SetFloat("velocity", agent.velocity.magnitude);
    }


    [Task]
    private void FindWaypoint()
    {
        waypointIdx = (waypointIdx + 1) % waypoints.Length;

        ThisTask.Succeed();

    }

    [Task]
    private bool IsDestinationSet()
    {
        return DestinationSet;
    }

    [Task]
    private void SetDestination()
    {
        
        agent.SetDestination(waypoints[waypointIdx].position);
        
        DestinationSet = true;
        ThisTask.Succeed();
        
    }

    [Task]
    private void ResetDestination()
    {
        DestinationSet = false;
        ThisTask.Succeed();
    }

    [Task]
    private bool HasArrivedAtDestination()
    {
        
        if(Vector3.Distance(agent.transform.position, waypoints[waypointIdx].position) < .2f)
            return true;

        ThisTask.debugInfo = "Is going to place?";
        
        return false;

    }

    protected override void Stun()
    {
        base.Stun();
        agent.isStopped = true;
    }


    [Task]
    private void ChasePlayer()
    {
        
        if(perception.HasLineOfSight())
        {
            agent.SetDestination(perception.DetectedPosition);
            agent.speed = 6f;
            DestinationSet = false;
            perception.chasingPlayer = true;
            ThisTask.debugInfo = "Seeking player in sight";

            if (Vector3.Distance(agent.transform.position, agent.destination) < distanceToPlayer)
            {
                Debug.Log("at the place");
                perception.chasingPlayer = false;
                ThisTask.Succeed();
            }

        }
        else
        {
            perception.chasingPlayer = false;
            ThisTask.Fail();
        }

  

    }

    [Task]
    private bool WasPlayerSeenLastFrame()
    {
        ThisTask.debugInfo = "" + perception.GetPlayerWasSeenLastFrame();
        return perception.GetPlayerWasSeenLastFrame();
    }

    [Task]
    private bool WasPlayerHeard()
    {
        return perception.PlayerWasHeard;
    }

    [Task]
    private void GoToLastKnownPlayerPositionWhileSeekingPlayer()
    {
        if(ThisTask.isStarting)
        {
            seeking = true;
            agent.SetDestination(perception.DetectedPosition);
        } 
        else if( (Vector3.Distance(transform.position, agent.destination) < distanceToPlayer) || perception.PlayerIsInSight || perception.PlayerWasHeard)
        {
            seeking = false;
            ThisTask.Succeed();
        }
    }

    bool seeking;
    private void OnDrawGizmos()
    {
        if (seeking)
            Gizmos.DrawSphere(perception.DetectedPosition, 2f);
    }


    [Task]
    private bool HasLineOfSight()
    {
        bool seen = perception.HasLineOfSight();
        playerWasSeen = seen;
        return seen;
    }

    [Task]
    private bool IsChasingPlayer()
    {
        return perception.IsChasingPlayer();
    }

    [Task]
    private void WindupAttack()
    {
        anim.CrossFade(animNameToId["Base Layer.Windup"],.2f);
        ThisTask.Succeed();
    }

    [Task]
    private void Attack()
    {
        anim.CrossFade(animNameToId["Base Layer.Slash"], .2f);
        ThisTask.Succeed();
    }

    [Task]
    private bool IsNearPlayer()
    {
        ThisTask.debugInfo = perception.PlayerIsInSight != false ? "Distance to player" + Vector3.Distance(transform.position, perception.DetectedPosition) : "Player not set";
        if (perception.PlayerIsInSight && Vector3.Distance(perception.DetectedPosition, transform.position) < distanceToPlayer)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return true;
        }
        else if(agent.isStopped)
        {        
            ResumeMovement();
        }
            

        return false;
    }

    private void ResumeMovement()
    {
        anim.Play(animNameToId["Base Layer.Idle_Walk_Run"]);
        agent.isStopped = false;
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


        if(perception.HasLineOfSight())
        {
            transform.LookAt(perception.DetectedPosition, Vector3.up);
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
        if(perception.HasLineOfSight() || perception.PlayerWasHeard)
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
}
