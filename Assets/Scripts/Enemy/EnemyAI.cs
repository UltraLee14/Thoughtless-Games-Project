using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    public enum AIState
    {
        Idle,
        Investigate,
        Pursuit
    }

    [Header("References")]
    [SerializeField, InspectorName("View Camera")]
    Camera viewCamera;

    [SerializeField, InspectorName("Player Transform")]
    Transform playerTransform;

    [SerializeField, InspectorName("NavMesh Agent")]
    NavMeshAgent navAgent;

    [Header("IdleStateSettingValues")]
    [SerializeField, InspectorName("Idle Speed")]
    float idleSpeed;

    [SerializeField, InspectorName("Scanning")]
    bool scanning;

    [Header("InvestigateStateSettingValues")]
    [SerializeField, InspectorName("Investigate Speed")]
    float investigateSpeed;

    [SerializeField, InspectorName("Investigate Exit Time")]
    float investigateExitTime;

    [SerializeField, InspectorName("Investigate Active")]
    bool investigateActive;

    [SerializeField, InspectorName("Investigate Start Event")]
    UnityEvent InvestigateStartEvent;

    [SerializeField, InspectorName("Investigate End Event")]
    UnityEvent InvestigateEndEvent;

    [Header("PursuitStateSettingValues")]
    [SerializeField, InspectorName("Pursuit Speed")]
    float pursuitSpeed;

    [SerializeField, InspectorName("Pursuit End Delay")]
    float pursuitEvadeTime;

    [SerializeField, InspectorName("Player In Sight")]
    bool playerInSight;

    [SerializeField, InspectorName("In Pursuit")]
    bool inPursuit;

    [SerializeField, InspectorName("Player In Sight Event")]
    UnityEvent PlayerInSightEvent;

    [SerializeField, InspectorName("Pursuit Start Event")]
    UnityEvent PursuitStartEvent;

    [SerializeField, InspectorName("Pursuit End Event")]
    UnityEvent PursuitEndEvent;

    [Header("Movement Control")]
    [SerializeField, InspectorName("Can Move")]
    bool canMove;

    [Header("Sonar")]
    [SerializeField, InspectorName("Sonar Event")]
    UnityEvent SonarEvent;

    [Header("AI State (Read Only)")]
    [SerializeField, InspectorName("Current AI State"), ReadOnly]
    AIState currentAIState = AIState.Idle;

    Transform currentSeenPlayer;
    Coroutine investigateExitCoroutine;

    float pursuitNoSightTimer;
    bool pursuitEndEventTriggered;

    void Awake()
    {
        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (viewCamera == null)
            return;

        if (scanning)
        {
            float angle = Mathf.Sin(Time.time * Mathf.PI) * 45f;
            Vector3 camAngles = viewCamera.transform.localEulerAngles;
            camAngles.y = angle;
            viewCamera.transform.localEulerAngles = camAngles;
        }

        float viewDistance = viewCamera.farClipPlane;
        float fieldOfView = viewCamera.fieldOfView;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null || players.Length == 0)
        {
            HandleNoPlayerVisible();
            UpdatePursuitEndTimer();
            UpdateMovement();
            return;
        }

        Vector3 eyePos = viewCamera.transform.position;
        Vector3 eyeForward = viewCamera.transform.forward;

        Transform visiblePlayer = null;

        for (int i = 0; i < players.Length; i++)
        {
            Transform player = players[i].transform;

            Vector3 toPlayer = player.position - eyePos;
            float distance = toPlayer.magnitude;
            if (distance > viewDistance)
                continue;

            Vector3 dirToPlayer = toPlayer.normalized;
            float angleToPlayer = Vector3.Angle(eyeForward, dirToPlayer);
            if (angleToPlayer > fieldOfView * 0.5f)
                continue;

            if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    visiblePlayer = hit.collider.transform;
                    break;
                }
            }
        }

        if (visiblePlayer != null)
        {
            HandlePlayerVisible(visiblePlayer);
        }
        else
        {
            HandleNoPlayerVisible();
        }

        UpdatePursuitEndTimer();
        UpdateMovement();
    }

    void HandlePlayerVisible(Transform player)
    {
        if (!playerInSight || currentSeenPlayer != player)
        {
            Debug.Log($"Enemy '{name}' has spotted player '{player.gameObject.name}'.");
            PlayerInSightEvent?.Invoke();
        }

        playerInSight = true;
        currentSeenPlayer = player;
    }

    void HandleNoPlayerVisible()
    {
        if (playerInSight && currentSeenPlayer != null)
        {
            Debug.Log($"Enemy '{name}' has lost line of sight to player '{currentSeenPlayer.gameObject.name}'.");
        }

        playerInSight = false;
        currentSeenPlayer = null;
    }

    void UpdatePursuitEndTimer()
    {
        if (currentAIState == AIState.Pursuit)
        {
            if (!playerInSight)
            {
                pursuitNoSightTimer += Time.deltaTime;

                if (!pursuitEndEventTriggered && pursuitNoSightTimer >= pursuitEvadeTime)
                {
                    inPursuit = false;
                    PursuitEndEvent?.Invoke();
                    pursuitEndEventTriggered = true;
                }
            }
            else
            {
                pursuitNoSightTimer = 0f;
                pursuitEndEventTriggered = false;
            }
        }
        else
        {
            pursuitNoSightTimer = 0f;
            pursuitEndEventTriggered = false;
        }
    }

    public void EnterInvestigate()
    {
        if (investigateExitCoroutine != null)
        {
            StopCoroutine(investigateExitCoroutine);
            investigateExitCoroutine = null;
        }

        if (!investigateActive)
        {
            investigateActive = true;
            InvestigateStartEvent?.Invoke();
        }
    }

    public void ExitInvestigate()
    {
        if (investigateExitCoroutine != null)
        {
            StopCoroutine(investigateExitCoroutine);
        }

        investigateExitCoroutine = StartCoroutine(ExitInvestigateRoutine());
    }

    System.Collections.IEnumerator ExitInvestigateRoutine()
    {
        yield return new WaitForSeconds(investigateExitTime);

        investigateActive = false;
        InvestigateEndEvent?.Invoke();
        investigateExitCoroutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("SonarSphere"))
        {
            SonarEvent?.Invoke();
        }

        if (inPursuit)
            return;

        if (other.isTrigger && other.CompareTag("PlayerDetectionSphere"))
        {
            EnterInvestigate();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (inPursuit)
            return;

        if (other.isTrigger && other.CompareTag("PlayerDetectionSphere"))
        {
            ExitInvestigate();
        }
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        ApplyMovementState();
    }

    float GetCurrentMoveSpeed()
    {
        switch (currentAIState)
        {
            case AIState.Idle:
                return idleSpeed;
            case AIState.Investigate:
                return investigateSpeed;
            case AIState.Pursuit:
                return pursuitSpeed;
            default:
                return investigateSpeed;
        }
    }

    void ApplyMovementState()
    {
        if (navAgent == null)
            return;

        if (canMove)
        {
            navAgent.isStopped = false;
            navAgent.speed = GetCurrentMoveSpeed();
        }
        else
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
            navAgent.speed = 0f;
        }
    }

    void ResetViewCameraYaw()
    {
        if (viewCamera == null)
            return;

        Vector3 angles = viewCamera.transform.localEulerAngles;
        angles.y = 0f;
        viewCamera.transform.localEulerAngles = angles;
    }

    public void SetAIStateIdle()
    {
        currentAIState = AIState.Idle;
        SetCanMove(false);
        scanning = true;
        inPursuit = false;
        ResetViewCameraYaw();
    }

    public void SetAIStateInvestigate()
    {
        currentAIState = AIState.Investigate;
        SetCanMove(true);
        scanning = false;
        inPursuit = false;
        ResetViewCameraYaw();
    }

    public void SetAIStatePursuit()
    {
        bool wasInPursuit = (currentAIState == AIState.Pursuit) || inPursuit;

        currentAIState = AIState.Pursuit;
        SetCanMove(true);
        scanning = false;
        inPursuit = true;
        ResetViewCameraYaw();

        if (!wasInPursuit)
            PursuitStartEvent?.Invoke();
    }

    void UpdateMovement()
    {
        if (navAgent == null || playerTransform == null)
            return;

        if (!canMove)
            return;

        navAgent.speed = GetCurrentMoveSpeed();

        if (navAgent.isStopped)
            navAgent.isStopped = false;

        navAgent.SetDestination(playerTransform.position);
    }
}
