using UnityEngine;

public struct FeedbackCue
{
    public FeedbackCueType Type;
    public Vector3 Position;
    public int TargetInstanceId;

    public FeedbackCue(FeedbackCueType type, Vector3 position, int targetInstanceId = 0)
    {
        Type = type;
        Position = position;
        TargetInstanceId = targetInstanceId;
    }
}
