using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action OnAnimationEndTrigger;

    private void AnimationEnd()
    {
        OnAnimationEndTrigger?.Invoke();
    }
    private void CameraImpulse()
    {
    }
    private void TestSkillEventHandle()
    {
    }
}
