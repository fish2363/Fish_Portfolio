using UnityEngine;
using UnityEngine.InputSystem;

public class FloatingTextSpawner : MonoBehaviour
{
    public GameObject floatingTextPrefab;
    public FloatingTextSettings settings;

    [ContextMenu("TestSpawn")]
    public void TestSpawn()
    {
        Spawn(Vector3.zero, 50);
    }

    public void Spawn(Vector3 position, int damage)
    {
        GameObject ftObj = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        ftObj.GetComponent<FloatingText>().Initialize(damage, settings);
    }
}
