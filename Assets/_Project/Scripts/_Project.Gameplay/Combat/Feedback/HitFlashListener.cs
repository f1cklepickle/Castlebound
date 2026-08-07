using System.Collections;
using Castlebound.Gameplay.AI;
using UnityEngine;

public class HitFlashListener : MonoBehaviour
{
    [SerializeField] FeedbackEventChannel feedbackChannel;
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] float flashDurationSeconds = 0.1f;
    [SerializeField] EnemyStaggerReceiver staggerReceiver;
    [SerializeField] Color staggerColor = new Color(1f, 0.85f, 0f, 1f);

    Color originalColor;
    Coroutine flashRoutine;
    bool isDamageFlashActive;

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
            feedbackChannel.SubscribeTarget(gameObject.GetInstanceID(), OnFeedbackRaised);
    }

    void OnDisable()
    {
        if (feedbackChannel != null)
            feedbackChannel.UnsubscribeTarget(gameObject.GetInstanceID(), OnFeedbackRaised);

        ResetFlash();
    }

    void LateUpdate()
    {
        ApplyCurrentColor();
    }

    void OnFeedbackRaised(FeedbackCue cue)
    {
        if (cue.Type != FeedbackCueType.PlayerHitEnemy)
            return;

        if (targetRenderer == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        isDamageFlashActive = true;
        ApplyCurrentColor();
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        yield return new WaitForSeconds(flashDurationSeconds);

        isDamageFlashActive = false;
        flashRoutine = null;
        ApplyCurrentColor();
    }

    void ResetFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        isDamageFlashActive = false;
        if (targetRenderer != null)
            targetRenderer.color = originalColor;
    }

    private void ApplyCurrentColor()
    {
        if (targetRenderer == null)
            return;

        if (isDamageFlashActive)
        {
            targetRenderer.color = flashColor;
            return;
        }

        targetRenderer.color = staggerReceiver != null && staggerReceiver.IsActionLocked
            ? staggerColor
            : originalColor;
    }
}
