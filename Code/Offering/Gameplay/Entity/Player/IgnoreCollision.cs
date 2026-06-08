using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [SerializeField] private Collider thisCollider;
    [SerializeField] private Collider[] colliderToIgnore;
    void Start()
    {
        foreach(Collider otherCollider in colliderToIgnore)
        {
            Physics.IgnoreCollision(thisCollider, otherCollider, true);
        }
    }
}
