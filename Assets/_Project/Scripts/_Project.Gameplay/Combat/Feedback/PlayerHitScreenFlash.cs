using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHitScreenFlash : MonoBehaviour
{
    [SerializeField] FeedbackEventChannel feedbackChannel;
    [SerializeField] Image overlayImage;
    [SerializeField] Color flashColor = new Color(1f, 0f, 0f, 0.35f);
    [SerializeField] float flashDurationSeconds = 0.2f;

    Coroutine flashRoutine;

    void Awake()
    {
        if (overlayImage == null)
            overlayImage = GetComponent<Image>();

        if (overlayImage != null)
            overlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
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
        if (cue.Type != FeedbackCueType.PlayerHit)
            return;

        if (overlayImage == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        overlayImage.color = flashColor;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, flashDurationSeconds);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(flashColor.a, 0f, t);
            overlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        overlayImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        flashRoutine = null;
    }
}
