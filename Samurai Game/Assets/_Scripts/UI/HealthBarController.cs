using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("References:")]
    [SerializeField] private Slider lerpSlider;
    [SerializeField] private Slider instantSlider;

    [Header("Lerp Slider Settings:")]
    [SerializeField] private float lerpDuration;

    public void SetMaxHealth(int health)
    {
        StopCoroutine(nameof(LerpHealth));

        lerpSlider.maxValue = health;
        lerpSlider.value = health;

        instantSlider.maxValue = health;
        instantSlider.value = health;
    }

    public void SetHealth(int health)
    {
        instantSlider.value = health;

        StartCoroutine(LerpHealth(health));
    }

    private IEnumerator LerpHealth(int targetHealth)
    {
        float elapsedTime = 0f;
        float startHealth = lerpSlider.value;

        while (elapsedTime < lerpDuration)
        {
            lerpSlider.value = Mathf.Lerp(startHealth, targetHealth, elapsedTime / lerpDuration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        lerpSlider.value = targetHealth;
    }
}
