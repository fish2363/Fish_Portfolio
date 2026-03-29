using UnityEngine;

public class DeadLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Putris"))
        {
            FindAnyObjectByType<MainUIEffect>().SetScore();
            GameManager.Instance.GameOver();
        }
    }
}
