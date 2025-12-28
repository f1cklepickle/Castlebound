using System.Collections;
using UnityEngine;

public class HitFlashListener : MonoBehaviour
{
    [SerializeField] FeedbackEventChannel feedbackChannel;
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] float flashDurationSeconds = 0.1f;

    Color originalColor;
    Coroutine flashRoutine;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            originalColor = targetRenderer.color;
    }

    void OnEnable()
    {
        if (feedbackChannel != null)
            feedbackChannel.Raised += OnFeedbackRaised;
    }

    void OnDisable()
    {
        if (feedbackChannel != null)
            feedbackChannel.Raised -= OnFeedbackRaised;
    }

    void OnFeedbackRaised(FeedbackCue cue)
    {
        if (cue.Type != FeedbackCueType.PlayerHitEnemy)
            return;

        if (targetRenderer == null)
            return;

        if (cue.TargetInstanceId != 0 && cue.TargetInstanceId != gameObject.GetInstanceID())
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        if (targetRenderer != null)
            targetRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDurationSeconds);

        if (targetRenderer != null)
            targetRenderer.color = originalColor;

        flashRoutine = null;
    }
}
