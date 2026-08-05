using Castlebound.Gameplay.Combat;
using UnityEngine;

public class PlayerGuardArcPresenter : MonoBehaviour
{
    [SerializeField] private PlayerDefenseController defenseController;
    [SerializeField] private LineRenderer arcRenderer;
    [SerializeField, Min(2)] private int segmentCount = 16;
    [SerializeField, Min(0f)] private float radius = 1.15f;
    [SerializeField, Min(0f)] private float lineWidth = 0.08f;
    [SerializeField, Min(0f)] private float parryFlashDuration = 0.12f;
    [SerializeField] private Color parryWindowColor = new Color(0.25f, 0.95f, 1f, 0.7f);
    [SerializeField] private Color blockingColor = new Color(0.25f, 0.6f, 1f, 0.55f);
    [SerializeField] private Color parrySuccessColor = new Color(1f, 0.9f, 0.2f, 0.95f);

    private float parryFlashRemaining;
    private Material runtimeMaterial;
    private PlayerDefenseController subscribedDefenseController;

    public Color ParrySuccessColor => parrySuccessColor;

    private void OnEnable()
    {
        EnsureReferences();
        BindDefenseEvents();
        RefreshPresentation();
    }

    private void OnDisable() => UnbindDefenseEvents();

    private void OnDestroy()
    {
        if (runtimeMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(runtimeMaterial);
        else
            DestroyImmediate(runtimeMaterial);
    }

    private void Update()
    {
        if (parryFlashRemaining <= 0f)
            return;

        parryFlashRemaining = Mathf.Max(0f, parryFlashRemaining - Time.deltaTime);
        if (parryFlashRemaining <= 0f)
            ApplyStateColor();
    }

    private void OnValidate()
    {
        segmentCount = Mathf.Max(2, segmentCount);
        radius = Mathf.Max(0f, radius);
        lineWidth = Mathf.Max(0f, lineWidth);
        parryFlashDuration = Mathf.Max(0f, parryFlashDuration);
    }

    public void RefreshPresentation()
    {
        EnsureReferences();
        BindDefenseEvents();
        ConfigureRenderer();
        BuildArc();
        ApplyStateColor();
    }

    private void HandleStateChanged(PlayerDefenseState state)
    {
        parryFlashRemaining = 0f;
        ApplyStateColor();
    }

    private void HandleHitResolved(PlayerHitResult result)
    {
        if (result.Outcome != PlayerHitOutcome.Parried || arcRenderer == null)
            return;

        parryFlashRemaining = parryFlashDuration;
        SetColor(parrySuccessColor);
    }

    private void ConfigureRenderer()
    {
        if (arcRenderer == null)
            return;

        arcRenderer.useWorldSpace = false;
        arcRenderer.loop = false;
        arcRenderer.widthMultiplier = lineWidth;
        arcRenderer.numCapVertices = 2;
        arcRenderer.sortingOrder = 2;
        if (arcRenderer.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
                arcRenderer.sharedMaterial = runtimeMaterial;
            }
        }
    }

    private void BuildArc()
    {
        if (arcRenderer == null)
            return;

        float arcDegrees = defenseController != null ? defenseController.BlockArcDegrees : 120f;
        arcRenderer.positionCount = segmentCount + 1;
        for (int index = 0; index <= segmentCount; index++)
        {
            float progress = index / (float)segmentCount;
            float angle = Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, progress);
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.down;
            arcRenderer.SetPosition(index, direction * radius);
        }
    }

    private void ApplyStateColor()
    {
        if (arcRenderer == null)
            return;

        PlayerDefenseState state = defenseController != null
            ? defenseController.State
            : PlayerDefenseState.Idle;
        arcRenderer.enabled = state == PlayerDefenseState.ParryWindow ||
                              state == PlayerDefenseState.Blocking;

        switch (state)
        {
            case PlayerDefenseState.ParryWindow:
                SetColor(parryWindowColor);
                break;
            case PlayerDefenseState.Blocking:
                SetColor(blockingColor);
                break;
        }
    }

    private void SetColor(Color color)
    {
        arcRenderer.startColor = color;
        arcRenderer.endColor = color;
    }

    private void EnsureReferences()
    {
        if (defenseController == null)
            defenseController = GetComponentInParent<PlayerDefenseController>();
        if (arcRenderer == null)
            arcRenderer = GetComponent<LineRenderer>();
        if (arcRenderer == null)
            arcRenderer = gameObject.AddComponent<LineRenderer>();
    }

    private void BindDefenseEvents()
    {
        if (subscribedDefenseController == defenseController)
            return;

        UnbindDefenseEvents();
        if (defenseController == null)
            return;

        defenseController.StateChanged += HandleStateChanged;
        defenseController.HitResolved += HandleHitResolved;
        subscribedDefenseController = defenseController;
    }

    private void UnbindDefenseEvents()
    {
        if (subscribedDefenseController == null)
            return;

        subscribedDefenseController.StateChanged -= HandleStateChanged;
        subscribedDefenseController.HitResolved -= HandleHitResolved;
        subscribedDefenseController = null;
    }
}
