using UnityEngine;

public class FlashlightItem : MonoBehaviour, IUseItem
{
    [SerializeField] private Light flashlight;
    private bool isOn = false;

    private void Start()
    {
        if (flashlight == null) flashlight = GetComponentInChildren<Light>();
        if (flashlight != null) flashlight.enabled = false;
    }

    public void Use(ItemContainer handController)
    {
        AudioManager.Instance.PlaySound2D("UseF", 0, false, SoundType.VfX);
        isOn = !isOn;

        if (flashlight != null)
        {
            flashlight.enabled = isOn;
        }
    }
}