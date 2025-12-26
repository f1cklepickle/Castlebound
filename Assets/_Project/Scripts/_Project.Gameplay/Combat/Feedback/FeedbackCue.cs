using UnityEngine;

public struct FeedbackCue
{
    public FeedbackCueType Type;
    public Vector3 Position;

    public FeedbackCue(FeedbackCueType type, Vector3 position)
    {
        Type = type;
        Position = position;
    }
}
