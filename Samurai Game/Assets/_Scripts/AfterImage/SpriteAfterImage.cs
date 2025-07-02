using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAfterImage : MonoBehaviour
{
    [SerializeField] private float startingAlpha = 0.8f;
    [SerializeField] [Range(0.0f, 1.0f)] private float alphaMultiplier = 0.85f;
    [SerializeField] private float activeDuration = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color colour;
    private float alpha;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // INFO: Modify alpha with alpha multiplier
        alpha *= alphaMultiplier;

        colour = new Color(colour.r, colour.g, colour.b, alpha);
        spriteRenderer.color = colour;
    }

    private void DeactivateAfterImage()
    {
        gameObject.SetActive(false);
    }

    public void Initialise(SpriteRenderer _spriteRenderer)
    {
        if (spriteRenderer == null || _spriteRenderer == null) { return; }

        alpha = startingAlpha;
        colour = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, alpha);

        spriteRenderer.sprite = _spriteRenderer.sprite;
        spriteRenderer.color = colour;
        spriteRenderer.flipX = _spriteRenderer.flipX;
        spriteRenderer.flipY = _spriteRenderer.flipY;
        spriteRenderer.material = _spriteRenderer.material;
        spriteRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
        spriteRenderer.sortingOrder = _spriteRenderer.sortingOrder;
        spriteRenderer.drawMode = _spriteRenderer.drawMode;
        spriteRenderer.size = _spriteRenderer.size;
        spriteRenderer.adaptiveModeThreshold = _spriteRenderer.adaptiveModeThreshold;
        spriteRenderer.tileMode = _spriteRenderer.tileMode;
        spriteRenderer.maskInteraction = _spriteRenderer.maskInteraction;
        spriteRenderer.receiveShadows = _spriteRenderer.receiveShadows;
        spriteRenderer.shadowCastingMode = _spriteRenderer.shadowCastingMode;
        spriteRenderer.enabled = _spriteRenderer.enabled;

        gameObject.SetActive(true);

        // INFO: Deactivate the game object after active duration
        Invoke(nameof(DeactivateAfterImage), activeDuration);
    }
}
