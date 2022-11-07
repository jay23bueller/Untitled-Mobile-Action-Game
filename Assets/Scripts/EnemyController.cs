using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;

public class EnemyController : Controller
{
    private NavMeshAgent agent;
    [SerializeField]
    private Transform[] waypoints;
    private int waypointIdx;
    private bool DestinationSet;
    private EnemyVision vision;
  

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
    private bool HasLineOfSight()
    {
        return vision.HasLineOfSight();
    }

    [Task]
    private bool IsChasingPlayer()
    {
        return vision.IsChasingPlayer();
    }

    [Task]
    private void WindupAttack()
    {
        anim.Play("Base Layer.Windup");
        ThisTask.Succeed();
    }

    [Task]
    private void Attack()
    {
        anim.Play("Base Layer.Slash");
        ThisTask.Succeed();
    }

    [Task]
    private bool IsNearPlayer()
    {
        if (vision.playerTransform != null && Vector3.Distance(vision.playerTransform.position, transform.position) < 2f)
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
    private void WaitUnlessPlayerInSight(float duration)
    {

        if(ThisTask.isStarting)
        {
            PlayerInSightTimer inSightTimer = new PlayerInSightTimer();
            inSightTimer.startTime = Time.time;
            inSightTimer.duration = duration;
            ThisTask.data = inSightTimer;
        }

        PlayerInSightTimer pT = ThisTask.GetData<PlayerInSightTimer>();

        float elapsedTime = (Time.time - pT.startTime) / duration;
        if(vision.HasLineOfSight())
        {
            ThisTask.Succeed();
        }
        else if (elapsedTime >= 1.0f)
        {
            ThisTask.Fail();
        }


    }

    private struct PlayerInSightTimer
    {
        public float startTime;
        public float duration;
    }
}
