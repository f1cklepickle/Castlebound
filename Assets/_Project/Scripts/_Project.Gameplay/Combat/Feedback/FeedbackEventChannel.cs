using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Castlebound/Combat/Feedback Event Channel")]
public class FeedbackEventChannel : ScriptableObject
{
    public event Action<FeedbackCue> Raised;

    public void Raise(FeedbackCue cue)
    {
        Raised?.Invoke(cue);
    }
}
