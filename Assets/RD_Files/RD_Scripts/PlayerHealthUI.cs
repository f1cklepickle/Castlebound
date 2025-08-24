// Assets/Scripts/UI/PlayerHealthUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro; // <-- add this

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] Slider healthSlider;
    [SerializeField] Health playerHealth;
    [SerializeField] TextMeshProUGUI hpText;    // <-- add this (drag your HPText here)

    void Awake()
    {
        if (!healthSlider) healthSlider = GetComponentInChildren<Slider>(true);
        // Optional auto-find if placed as a child:
        if (!hpText) hpText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void OnEnable()
    {
        if (!playerHealth)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) playerHealth = player.GetComponent<Health>();
        }

        if (playerHealth)
        {
            healthSlider.maxValue = playerHealth.Max;
            healthSlider.value = playerHealth.Current;
            UpdateHpText(playerHealth.Current, playerHealth.Max);
            playerHealth.OnHealthChanged += OnHealthChanged;
        }
    }

    void OnDisable()
    {
        if (playerHealth)
            playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(int current, int max)
    {
        if (healthSlider.maxValue != max) healthSlider.maxValue = max;
        healthSlider.value = current;
        UpdateHpText(current, max);
    }

    void UpdateHpText(int current, int max)
    {
        if (!hpText) return;
        hpText.text = $"/{current}";
        // Optional low-HP tint:
        // hpText.color = (current <= max * 0.25f) ? new Color(1f,0.3f,0.3f) : Color.white;
    }
}
