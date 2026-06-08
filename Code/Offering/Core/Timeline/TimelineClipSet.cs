using System;
using UnityEngine.Timeline;

[Serializable]
public class TimelineClipSet
{
    public TimelineType type;
    public TimelineAsset asset;
    public bool savedInJson = true;
}