// JuiceManager.cs
using UnityEngine;

public enum JuiceState
{
    None,
    Set,
    Filling,
    Paused,
    Endable,
    End,
    Pilled,
    Over,
    Submit
}

public enum PowerColorType
{
    Default,
    Red,
    Green
}

public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance;

    private void Awake() => Instance = this;
    public bool IsFillingOn { get; set; }
    public JuiceController juiceCtrl { get; private set; }

    private readonly Color[] buttonPalette =
    {
        Color.white,
        new Color(1f, 60/255f, 60/255f),
        new Color(155/255f, 1f, 0f)
    };

    public Coroutine btnCoroutine;   // 호환 유지
    public Coroutine juiceCoroutine; // 호환 유지

    // ✅ 노티파이로 변경
    public NotifyValue<int> JuiceCount = new NotifyValue<int>(0);

    // delta 만큼 증감 (예: +1, -1)
    public void AddJuiceCount(int delta)
    {
        JuiceCount.Value += delta;
    }

    // 특정 값으로 강제 세팅
    public void SetJuiceCount(int value)
    {
        JuiceCount.Value = value;
    }

    public void SetJuiceCtrl(JuiceController controller) => juiceCtrl = controller;

    public Color GetPowerColor(PowerColorType type)
    {
        int i = (int)type;
        if (i < 0 || i >= buttonPalette.Length) return Color.white;
        return buttonPalette[i];
    }
}
