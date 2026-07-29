using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Castlebound/Combat/Feedback Event Channel")]
public class FeedbackEventChannel : ScriptableObject
{
    public event Action<FeedbackCue> Raised;

    readonly Dictionary<int, Action<FeedbackCue>> targetedListeners =
        new Dictionary<int, Action<FeedbackCue>>();

    public void SubscribeTarget(int targetInstanceId, Action<FeedbackCue> listener)
    {
        if (targetInstanceId == 0 || listener == null)
            return;

        targetedListeners.TryGetValue(targetInstanceId, out Action<FeedbackCue> listeners);
        targetedListeners[targetInstanceId] = listeners + listener;
    }

    public void UnsubscribeTarget(int targetInstanceId, Action<FeedbackCue> listener)
    {
        if (targetInstanceId == 0 || listener == null ||
            !targetedListeners.TryGetValue(targetInstanceId, out Action<FeedbackCue> listeners))
            return;

        listeners -= listener;
        if (listeners == null)
            targetedListeners.Remove(targetInstanceId);
        else
            targetedListeners[targetInstanceId] = listeners;
    }

    public void Raise(FeedbackCue cue)
    {
        Raised?.Invoke(cue);

        if (cue.TargetInstanceId != 0 &&
            targetedListeners.TryGetValue(cue.TargetInstanceId, out Action<FeedbackCue> listeners))
        {
            listeners?.Invoke(cue);
        }
    }
}
