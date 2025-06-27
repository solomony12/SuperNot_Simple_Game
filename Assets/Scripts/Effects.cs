using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Effects : MonoBehaviour
{
    /// <summary>
    /// Types of effects
    /// </summary>
    public enum EffectType
    {
        Default,
        Dim,
        Affection
    }

    private Image effectImage;
    private Mask mask;

    /// <summary>
    /// Public method for other scripts to call to play an effect
    /// </summary>
    /// <param name="obj">The mask/image the effect goes on</param>
    /// <param name="effect">Type of effect to play</param>
    public void PlayEffect(GameObject effectImageGameObject, EffectType effect)
    {
        Debug.Log($"Playing effect {effect}.");

        mask = effectImageGameObject.GetComponent<Mask>();
        effectImage = effectImageGameObject.GetComponent<Image>();

        Color colorToApply = Color.white;

        switch (effect)
        {
            case EffectType.Default:
                colorToApply = ApplyDefaultEffect();
                break;
            case EffectType.Dim:
                colorToApply = ApplyDimEffect();
                break;
            case EffectType.Affection:
                colorToApply = ApplyAffectionEffect();
                break;
        }

        ApplyColorAndAlphaToChildren(colorToApply);
    }

    private void ApplyColorAndAlphaToChildren(Color maskColor)
    {
        if (effectImage == null) return;

        Image[] childImages = effectImage.GetComponentsInChildren<Image>();

        foreach (var img in childImages)
        {
            // Skip the mask itself
            if (img == effectImage) continue;

            // Reset to original before applying
            Color original = Color.white;

            // Lerp between original and maskColor based on mask alpha
            Color tinted = Color.Lerp(original, maskColor, maskColor.a);

            // Preserve the original alpha
            img.color = new Color(tinted.r, tinted.g, tinted.b, original.a);
        }
    }

    private Color ApplyDefaultEffect()
    {
        effectImage.color = Color.white;
        mask.enabled = true;
        return Color.white;
    }

    private Color ApplyDimEffect()
    {
        Color navyBlueMidAlpha = new Color(0f, 0f, 0.5f, 0.6f);
        effectImage.color = navyBlueMidAlpha;
        mask.enabled = true;
        return navyBlueMidAlpha;
    }

    private Color ApplyAffectionEffect()
    {
        Color pinkLoveLightAlpha = new Color(1f, 0.41f, 0.71f, 0.3f);
        effectImage.color = pinkLoveLightAlpha;
        mask.enabled = false;
        return pinkLoveLightAlpha;
    }

}
