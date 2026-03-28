using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class CircularSector : MonoBehaviour
{
    public Transform target;
    public float angleRange = 30f;
    public float radius = 3f;

    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Volume volume;

    private IDetectGaze targetGaze;
    private Vignette vignette;
    private Tween vignetteTween;

    private bool wasInSight = false;

    private void Awake()
    {
        targetGaze = target.GetComponent<IDetectGaze>();
        if (volume != null && volume.profile.TryGet(out Vignette tmpVignette))
        {
            vignette = tmpVignette;
        }
    }

    private void Update()
    {
        bool isInSight = CheckTargetInSight();

        if (isInSight && !wasInSight)
        {
            targetGaze.OnGazeDetected(transform);
            SafeVignette();
            wasInSight = true;
        }
        else if (!isInSight && wasInSight)
        {
            targetGaze.OnGazeLost();
            DangerousVignette();
            wasInSight = false;
        }
    }

    private bool CheckTargetInSight()
    {
        Vector3 dirToTarget = target.position - transform.position;
        float distance = dirToTarget.magnitude;

        if (distance > radius) return false;

        Vector3 normalizedDir = dirToTarget.normalized;
        float dot = Vector3.Dot(normalizedDir, transform.forward);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        if (angle > angleRange / 2f) return false;

        Vector3 rayStart = transform.position + Vector3.up;
        if (Physics.Raycast(rayStart, normalizedDir, distance, obstacleLayer))
            return false;

        return true;
    }

    private void DangerousVignette()
    {
        if (vignette == null) return;

        vignetteTween?.Kill();
        vignetteTween = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.4f, 2f);
    }

    private void SafeVignette()
    {
        if (vignette == null) return;

        vignetteTween?.Kill();
        vignetteTween = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, 2f);
    }
}