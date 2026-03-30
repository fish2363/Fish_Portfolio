using UnityEngine;

public enum StewState
{
    None,
    Set,
    Stock,
    Fish,
    Radish,
    Spices,
    Welsh,
    Ready,
    Cold
}

public class StewManager : MonoBehaviour
{
    public StewController firstFireplace { get; private set; }
    public StewController secondFireplace { get; private set; }

    public Coroutine CoolCoroutine;

    public void SetFireplace(StewController[] controllers)
    {
        firstFireplace = controllers[0];
        secondFireplace = controllers[1];
    }
}
