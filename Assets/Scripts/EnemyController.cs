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
    private EnemyVision vision;

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
        agent.destination = waypoints[waypointIdx].position;
        vision = GetComponentInChildren<EnemyVision>();
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
        
        return false;

    }


    [Task]
    private void ChasePlayer()
    {
        if(vision.playerTransform != null)
        {
            agent.SetDestination(vision.playerTransform.position);
            agent.speed = 6f;
            DestinationSet = false;
            vision.chasingPlayer = true;

            ThisTask.Succeed();
        }

        ThisTask.Fail();

    }

    [Task]
    private bool WasPlayerSeenLastFrame()
    {
        return vision.GetPlayerWasSeenLastFrame();
    }

    [Task]
    private void GoToLastKnownPlayerPositionWhileSeekingPlayer()
    {
        if(ThisTask.isStarting)
        {
            agent.SetDestination(vision.PlayerPosition);
        } 
        else if( Vector3.Distance(transform.position, agent.destination) < distanceToPlayer)
        {
            ThisTask.Succeed();
        }
    }


    [Task]
    private bool HasLineOfSight()
    {
        bool seen = vision.HasLineOfSight();
        playerWasSeen = seen;
        return seen;
    }

    [Task]
    private bool IsChasingPlayer()
    {
        return vision.IsChasingPlayer();
    }

    [Task]
    private void WindupAttack()
    {
        anim.CrossFade("Base Layer.Windup",.2f);
        //anim.Play("Base Layer.Windup");
        ThisTask.Succeed();
    }

    [Task]
    private void Attack()
    {
        anim.CrossFade("Base Layer.Slash", .2f);
        //anim.Play("Base Layer.Slash");
        ThisTask.Succeed();
    }

    [Task]
    private bool IsNearPlayer()
    {
        if (vision.playerTransform != null && Vector3.Distance(vision.playerTransform.position, transform.position) < distanceToPlayer)
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
        anim.Play("Base Layer.Idle");
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


        if(vision.playerIsInSight)
        {
            transform.LookAt(vision.playerTransform.position, Vector3.up);
        }
    }

    [Task]
    private void WaitUnlessPlayerInSight(float duration)
    {

        if(ThisTask.isStarting)
        {
            PTaskTimer inSightTimer = new PTaskTimer(Time.time, duration);
            ThisTask.data = inSightTimer;
        }

        PTaskTimer pT = ThisTask.GetData<PTaskTimer>();

        float elapsedTime = pT.GetElapsedTime();
        if(vision.HasLineOfSight())
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
