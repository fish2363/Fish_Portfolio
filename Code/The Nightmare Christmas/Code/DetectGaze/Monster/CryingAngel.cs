using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CryingAngel : MonoBehaviour, IDetectGaze
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CircularSector circularSector;

    public Transform player;
    [SerializeField] private float deathRadius = 2f;

    private bool isStoppedByGaze = false;
    private bool isSoundPlaying = false;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (circularSector != null) circularSector.enabled = true;
    }

    private void Start()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    public void OnGazeDetected(Transform observer)
    {
        agent.isStopped = true;
        isStoppedByGaze = true;

        if (!isSoundPlaying)
        {
            AudioManager.Instance.PlaySound2D("ComeOn", 0, true, SoundType.VfX);
            isSoundPlaying = true;
        }
    }

    public void OnGazeLost()
    {
        if (isSoundPlaying)
        {
            AudioManager.Instance.StopLoopSound("ComeOn");
            isSoundPlaying = false;
        }

        isStoppedByGaze = false;
        agent.isStopped = false;

        if (player != null)
        {
            agent.SetDestination(player.position);
            CheckKillPlayer();
        }
    }

    private void CheckKillPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) <= deathRadius)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null && playerScript.deathObj != null)
            {
                playerScript.deathObj.SetActive(true);
            }

            AudioManager.Instance.PlaySound2D("Scary", 0, false, SoundType.VfX);
            StartCoroutine(DeathSceneRoutine());
        }
    }

    private IEnumerator DeathSceneRoutine()
    {
        yield return new WaitForSecondsRealtime(1.4f);
        SceneChangeManager.Instance.DeathScene();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, deathRadius);
    }
}