using System;
using System.Collections;
using UnityEngine;

public class GiftItem : MonoBehaviour, IUseItem
{
    [SerializeField] private ItemSOList itemSOList;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem openParticlePrefab;
    [SerializeField] private Transform particleSpawnPos;

    private ItemContainer currentHand;
    private readonly int OpenHash = Animator.StringToHash("Open");

    public void Use(ItemContainer handController)
    {
        currentHand = handController;
        animator.SetBool(OpenHash, true);
    }

    public void GetRandomItem()
    {
        if (currentHand == null || itemSOList == null || itemSOList.items.Count == 0) return;

        StartCoroutine(GiftParticleRoutine(2f));

        int rand = Random.Range(0, itemSOList.items.Count);
        ItemSO randomItem = itemSOList.items[rand];

        currentHand.isAlreadyGift = false;
        currentHand.OpenGiftAndGetItem(randomItem);
    }

    private IEnumerator GiftParticleRoutine(float destroyTime)
    {
        AudioManager.Instance.PlaySound2D("OpenItem", 0, false, SoundType.VfX);

        Transform spawnPoint = particleSpawnPos != null ? particleSpawnPos : transform;
        ParticleSystem effect = Instantiate(openParticlePrefab, spawnPoint.position, spawnPoint.rotation);

        yield return new WaitForSeconds(destroyTime);
        if (effect != null) Destroy(effect.gameObject);
    }
}