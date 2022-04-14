using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventDispatch : MonoBehaviour
{
    public delegate void AnimationEvent();

    public List<AnimationEvent> animationEvents = new List<AnimationEvent>();

    public void Dispatch(int index)
    {
        if(index >= 0 && index < animationEvents.Count)
        {
            animationEvents[index]?.Invoke();
        }
    }
}
