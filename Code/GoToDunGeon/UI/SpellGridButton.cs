using UnityEngine;
using UnityEngine.UI;

public class SpellGridButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image roonImage;

    [Header("애니메이션")]
    [SerializeField] private AnimationClip[] elementAnimations; // 원소별 애니메이션 배열
    
    public Button thisButton { get; private set; }
    private Animator animator;

    private void Awake()
    {
        thisButton = GetComponent<Button>();
        animator = GetComponent<Animator>();
    }

    public void SetChangeImage(Sprite newIcon, Color roonColor)
    {
        // Debug.Log("교체");
        iconImage.sprite = newIcon;
        roonImage.color = roonColor;
    }

    /// <summary>
    /// 인덱스에 맞는 애니메이션을 Animator에 설정
    /// </summary>
    public void SetAnimationByIndex(int index)
    {
        if (elementAnimations == null || elementAnimations.Length == 0) return;
        if (index < 0 || index >= elementAnimations.Length) return;
        if (animator == null) return;

        AnimationClip targetClip = elementAnimations[index];
        if (targetClip != null)
        {
            // RuntimeAnimatorController에 애니메이션 설정
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                AnimatorOverrideController overrideController = new AnimatorOverrideController(controller);
                overrideController[controller.animationClips[0]] = targetClip;
                animator.runtimeAnimatorController = overrideController;
            }
        }
    }
}
