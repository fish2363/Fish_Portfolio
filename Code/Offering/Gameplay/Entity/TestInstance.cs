using UnityEngine;

public class TestInstance : MonoBehaviour
{
    public static TestInstance Instance;
    public Transform cameraTranform;
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }
}
