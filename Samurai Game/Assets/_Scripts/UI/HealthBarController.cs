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

    public void SetHealth(int health, bool isPositiveChange = false)
    {
        if (!isPositiveChange)
        {
            instantSlider.value = health;
        }
        else
        {
            lerpSlider.value = health;
        }

        StartCoroutine(LerpHealth(health, isPositiveChange));
    }

    private IEnumerator LerpHealth(int targetHealth, bool isPositiveChange)
    {
        float elapsedTime = 0.0f;
        float startHealth;

        if (!isPositiveChange)
        {
            startHealth = lerpSlider.value;

            while (elapsedTime < lerpDuration)
            {
                lerpSlider.value = Mathf.Lerp(startHealth, targetHealth, elapsedTime / lerpDuration);
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            lerpSlider.value = targetHealth;
        }
        else
        {
            startHealth = instantSlider.value;

            while (elapsedTime < lerpDuration)
            {
                instantSlider.value = Mathf.Lerp(startHealth, targetHealth, elapsedTime / lerpDuration);
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            instantSlider.value = targetHealth;
        }
    }
}
