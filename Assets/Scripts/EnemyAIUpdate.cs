using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyAIUpdate : MonoBehaviour
{
    public enum AIState
    {
        Idle,
        Patrol,
        Investigation,
        Pursuit
    }

    [Header("State")]
    [SerializeField] AIState currentState = AIState.Idle;

    [Header("References")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform targetRoot;
    [SerializeField] Transform targetAimPoint;
    [SerializeField] Transform eyePoint;

    [Header("Line Of Sight")]
    [SerializeField] float viewDistance = 25f;
    [SerializeField, Range(0f, 180f)] float viewAngle = 180f;
    [SerializeField] float losCheckInterval = 0.12f;
    [SerializeField] LayerMask obstructionMask = ~0;

    [Header("Pursuit")]
    [SerializeField] float pursuitSpeed = 4.5f;
    [SerializeField] float pursuitStoppingDistance = 1.5f;
    [SerializeField] float loseSightMemoryTime = 1.25f;
    [SerializeField] float arriveAtLastSeenRadius = 1.25f;

    [Header("Investigation (Bridge)")]
    [SerializeField] float investigationSpeed = 3.5f;
    [SerializeField] float investigationStoppingDistance = 0.25f;

    [Header("Events")]
    [SerializeField] UnityEvent EnterPursuit;
    [SerializeField] UnityEvent ExitPursuit;
    [SerializeField] UnityEvent EnterInvestigation;
    [SerializeField] UnityEvent ExitInvestigation;

    public bool HasLineOfSight { get; private set; }
    public Vector3 LastSeenPosition { get; private set; }
    public float LastSeenTime { get; private set; }

    float nextLosTime;
    float viewDistanceSqr;
    float cosHalfFov;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        viewDistanceSqr = viewDistance * viewDistance;
        cosHalfFov = Mathf.Cos((viewAngle * 0.5f) * Mathf.Deg2Rad);

        if (eyePoint == null) eyePoint = transform;
        if (targetAimPoint == null && targetRoot != null) targetAimPoint = targetRoot;

        LastSeenPosition = transform.position;
        LastSeenTime = -9999f;
    }

    void OnValidate()
    {
        viewDistance = Mathf.Max(0.1f, viewDistance);
        losCheckInterval = Mathf.Max(0.01f, losCheckInterval);
        loseSightMemoryTime = Mathf.Max(0f, loseSightMemoryTime);
        arriveAtLastSeenRadius = Mathf.Max(0.01f, arriveAtLastSeenRadius);
    }

    void Update()
    {
        if (targetRoot == null || agent == null) return;
        if (targetAimPoint == null) targetAimPoint = targetRoot;

        if (Time.time >= nextLosTime)
        {
            nextLosTime = Time.time + losCheckInterval;
            HasLineOfSight = CheckLineOfSight();

            if (HasLineOfSight)
            {
                LastSeenPosition = targetAimPoint.position;
                LastSeenTime = Time.time;
            }
        }

        switch (currentState)
        {
            case AIState.Pursuit:
                TickPursuit();
                break;

            case AIState.Investigation:
                TickInvestigation();
                break;

            case AIState.Patrol:
                TickPatrol();
                break;

            case AIState.Idle:
                TickIdle();
                break;
        }

        if (HasLineOfSight && currentState != AIState.Pursuit)
        {
            SetState(AIState.Pursuit);
        }
    }

    bool CheckLineOfSight()
    {
        Vector3 eyePos = eyePoint.position;
        Vector3 targetPos = targetAimPoint.position;

        Vector3 toTarget = targetPos - eyePos;
        float distSqr = toTarget.sqrMagnitude;

        if (distSqr > viewDistanceSqr) return false;

        Vector3 dir = toTarget;
        float mag = dir.magnitude;
        if (mag <= 0.0001f) return true;
        dir /= mag;

        float dot = Vector3.Dot(transform.forward, dir);
        if (viewAngle < 180f)
        {
            if (dot < cosHalfFov) return false;
        }
        else
        {
            if (dot < 0f) return false;
        }

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, mag, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            Transform h = hit.transform;
            if (h == targetRoot) return true;
            if (h.IsChildOf(targetRoot)) return true;
            if (targetRoot.IsChildOf(h)) return true;
            return false;
        }

        return true;
    }

    void TickPursuit()
    {
        agent.speed = pursuitSpeed;
        agent.stoppingDistance = pursuitStoppingDistance;

        if (HasLineOfSight)
        {
            agent.SetDestination(targetRoot.position);
            return;
        }

        float timeSinceSeen = Time.time - LastSeenTime;

        if (timeSinceSeen <= loseSightMemoryTime)
        {
            agent.SetDestination(LastSeenPosition);
            return;
        }

        SetState(AIState.Investigation);
    }

    void TickInvestigation()
    {
        agent.speed = investigationSpeed;
        agent.stoppingDistance = investigationStoppingDistance;

        agent.SetDestination(LastSeenPosition);

        float dist = Vector3.Distance(transform.position, LastSeenPosition);
        if (dist <= arriveAtLastSeenRadius)
        {
            SetState(AIState.Patrol);
        }
    }

    void TickPatrol()
    {
    }

    void TickIdle()
    {
    }

    void SetState(AIState newState)
    {
        if (newState == currentState) return;

        if (currentState == AIState.Pursuit) ExitPursuit?.Invoke();
        if (currentState == AIState.Investigation) ExitInvestigation?.Invoke();

        currentState = newState;

        if (currentState == AIState.Pursuit) EnterPursuit?.Invoke();
        if (currentState == AIState.Investigation) EnterInvestigation?.Invoke();
    }

    public void ForceSetTarget(Transform newTargetRoot, Transform newAimPoint = null)
    {
        targetRoot = newTargetRoot;
        targetAimPoint = newAimPoint != null ? newAimPoint : newTargetRoot;
    }

    public void ForceEnterPursuit()
    {
        if (targetRoot == null) return;
        LastSeenPosition = (targetAimPoint != null ? targetAimPoint.position : targetRoot.position);
        LastSeenTime = Time.time;
        SetState(AIState.Pursuit);
    }

    public void ForceEnterInvestigation(Vector3 investigatePosition)
    {
        LastSeenPosition = investigatePosition;
        LastSeenTime = Time.time;
        SetState(AIState.Investigation);
    }

    public AIState GetCurrentState()
    {
        return currentState;
    }
}
