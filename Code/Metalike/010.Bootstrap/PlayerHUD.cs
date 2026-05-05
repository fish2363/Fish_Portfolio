using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    private async void Start()
    {
        await UIManager.Instance.Show<CharacterBarView>();
        //await UIManager.Instance.Show<WeaponHudView>();
    }
}
