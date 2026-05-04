using EPOOutline;
using UnityEngine;

public class MeshContainer : MonoBehaviour
{
    [field:SerializeField] public Renderer[] CurrentMeshs { get; set; }
    [field:SerializeField] public Outlinable outlinable { get; set; }

    private void Awake()
    {
        if(outlinable != null)
            outlinable.enabled = false;
    }
}
