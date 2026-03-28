using UnityEngine;
using TMPro;
using System.Collections;

public class ItemContainer : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private LayerMask interactableLayer;

    public ItemSO currentItem;
    public ItemSO testItem;
    [SerializeField] private float throwPower = 10f;

    [HideInInspector] public SoundMonster soundMonster;
    public bool isAlreadyGift { get; set; }
    public bool isGetPresent { get; set; }

    private GameObject currentHandObj; 
    private Coroutine soundCoroutine;

    private void OnEnable()
    {
        player._inputReader.OnThrow += ThrowItem;
        player.OnClick += UseItem;

        UpdateUI();
    }

    private void OnDisable()
    {
        player._inputReader.OnThrow -= ThrowItem;
        player.OnClick -= UseItem;
    }

    private void UpdateUI()
    {
        if (itemNameText != null)
        {
            itemNameText.text = currentItem != null ? currentItem.itemName : "¸Ç¼Õ";
        }
    }

    public void ChangeItem(ItemSO newItem)
    {
        if (currentHandObj != null)
        {
            Destroy(currentHandObj);
        }

        currentItem = newItem;
        if (currentItem != null && currentItem.itemPrefab != null)
        {
            currentHandObj = Instantiate(currentItem.itemPrefab, transform);
            currentHandObj.transform.localPosition = Vector3.zero;
            currentHandObj.transform.localRotation = Quaternion.identity;
        }

        UpdateUI();
    }

    private void UseItem()
    {
        if (currentItem == null || currentHandObj == null) return;

        if (currentItem.isPuzzleItem)
        {
            if (currentHandObj.TryGetComponent(out IPuzzleItem puzzleItem))
            {
                if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 5f, interactableLayer))
                {
                    puzzleItem.Use(this, hit);
                }
            }
        }
        else if (!currentItem.isProp)
        {
            if (currentHandObj.TryGetComponent(out IUseItem useItem))
            {
                useItem.Use(this);
            }
        }
    }

    public void ThrowItem()
    {
        if (currentItem == null || currentItem.itemPlacePrefab == null) return;

        AudioManager.Instance.PlaySound2D("GetItem", 0, false, SoundType.VfX);
        GameObject dropObj = Instantiate(currentItem.itemPlacePrefab, transform.position, transform.rotation);

        if (!dropObj.TryGetComponent(out Rigidbody rb))
        {
            rb = dropObj.AddComponent<Rigidbody>();
        }
        rb.AddForce(transform.forward * throwPower, ForceMode.Impulse);

        if (soundMonster != null && !soundMonster.isPlayerFollow)
        {
            soundMonster.AddTransform(dropObj.transform);
            if (soundCoroutine != null) StopCoroutine(soundCoroutine);
            soundCoroutine = StartCoroutine(SoundRoutine(dropObj));
        }

        ChangeItem(null);
    }

    private IEnumerator SoundRoutine(GameObject soundPlayObj)
    {
        yield return new WaitForSeconds(1f);
        if (soundPlayObj != null && soundMonster != null)
        {
            soundMonster.TargetChange(soundPlayObj.name);
        }
    }

    public void OpenGiftAndGetItem(ItemSO giftResultItem)
    {
        ChangeItem(giftResultItem);
    }
}