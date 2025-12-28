using System.Collections;
using UnityEngine;

public class BarrierHitShake : MonoBehaviour
{
    [SerializeField] FeedbackEventChannel feedbackChannel;
    [SerializeField] float durationSeconds = 0.15f;
    [SerializeField] float intensity = 0.05f;

    Vector3 originalLocalPos;
    Coroutine shakeRoutine;

    void Awake()
    {
        originalLocalPos = transform.localPosition;
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
        if (cue.Type != FeedbackCueType.EnemyHitBarrier)
            return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < durationSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);
            transform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }
}
