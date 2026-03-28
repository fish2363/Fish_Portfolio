using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SoundMonster : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioInput audioInput;
    [SerializeField] private FollowRadius followRadiusCollider;

    [SerializeField] private float foundRadius = 10f;
    [SerializeField] private float deadRadius = 2f;
    [SerializeField] private LayerMask targetLayer;

    private Dictionary<string, Transform> soundDictionary = new Dictionary<string, Transform>();
    private Transform currentTarget;

    public bool isPlayerFollow { get; private set; }
    private bool isMovingToPoint;

    private Collider[] hitColliders = new Collider[5];

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (audioInput != null) audioInput._BigSound += OnTargetChange;
        if (followRadiusCollider != null) followRadiusCollider.OnLessPlayer += OnPlayerLost;
    }

    private void OnDestroy()
    {
        if (audioInput != null) audioInput._BigSound -= OnTargetChange;
        if (followRadiusCollider != null) followRadiusCollider.OnLessPlayer -= OnPlayerLost;
    }

    public void AddTransform(Transform targetTransform)
    {
        if (!soundDictionary.ContainsKey(targetTransform.name))
        {
            soundDictionary.Add(targetTransform.name, targetTransform);
        }
        else
        {
            soundDictionary[targetTransform.name] = targetTransform;
        }
    }

    private void OnPlayerLost()
    {
        if (isPlayerFollow)
        {
            animator.SetFloat("Velocity", 0.2f);
            agent.isStopped = true;
            isPlayerFollow = false;
        }
    }

    public void OnTargetChange(string targetName)
    {
        if (isPlayerFollow || !soundDictionary.ContainsKey(targetName)) return;

        currentTarget = soundDictionary[targetName];
        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
        isMovingToPoint = true;
    }

    private void Update()
    {
        CheckDeadZone();

        if (isMovingToPoint && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.pathPending)
            {
                isMovingToPoint = false;
                animator.SetFloat("Velocity", 0.2f);
            }
        }

        if (isMovingToPoint)
        {
            SearchForPlayer();
        }
    }

    private void SearchForPlayer()
    {
        animator.SetFloat("Velocity", 0.5f);

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, foundRadius, hitColliders, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].CompareTag("Player")) 
            {
                if (soundDictionary.TryGetValue(hitColliders[i].name, out Transform playerTransform))
                {
                    agent.SetDestination(playerTransform.position);
                    isPlayerFollow = true;
                    animator.SetFloat("Velocity", 1f);
                    break;
                }
            }
        }
    }

    private void CheckDeadZone()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, deadRadius, hitColliders, targetLayer);

        for (int i = 0; i < hitCount; i++)
        {
            if (hitColliders[i].CompareTag("Player"))
            {
                Player playerScript = hitColliders[i].GetComponentInParent<Player>();
                if (playerScript != null && playerScript.soundMonsterDeathObj != null)
                {
                    playerScript.soundMonsterDeathObj.SetActive(true);
                }
                StartCoroutine(DeathSceneRoutine());
                break;
            }
        }
    }

    private IEnumerator DeathSceneRoutine()
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneChangeManager.Instance.DeathScene();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, foundRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deadRadius);
    }
}