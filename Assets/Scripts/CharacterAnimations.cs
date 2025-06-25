using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class CharacterAnimations : MonoBehaviour
{
    // Types of animations
    public enum AnimationType
    {
        FadeIn,
        FadeOut,
    }

    private Dictionary<GameObject, Coroutine> runningAnimations = new Dictionary<GameObject, Coroutine>();

    // Public method for other scripts to call to play an animation
    public void PlayAnimation(GameObject obj, AnimationType animation, float duration = 1f, Action onComplete = null)
    {
        Debug.Log($"Playing animation {animation.ToString()} for {duration} seconds on {obj.gameObject.GetComponent<Image>().sprite.name}");

        bool shouldCompleteFadeIn = animation == AnimationType.FadeIn;

        // If an animation is already playing on obj, don't start another
        if (runningAnimations.ContainsKey(obj))
        {
            Debug.LogWarning($"Animation already running on {obj.name}. Stopping it and starting new animation {animation.ToString()}.");
            StopExistingAnimation(obj, animation == AnimationType.FadeIn);
        }

        Coroutine newCoroutine = null;

        switch (animation)
        {
            case AnimationType.FadeIn:
                newCoroutine = StartCoroutine(FadeInCoroutine(obj, duration, onComplete));
                break;
            case AnimationType.FadeOut:
                newCoroutine = StartCoroutine(FadeOutCoroutine(obj, duration, onComplete));
                break;
        }

        // Set current animation
        if (newCoroutine != null)
        {
            runningAnimations[obj] = newCoroutine;
        }
    }


    // Stop any animation currently running
    private void StopExistingAnimation(GameObject obj, bool completeFadeIn)
    {
        if (runningAnimations.TryGetValue(obj, out Coroutine runningCoroutine))
        {
            StopCoroutine(runningCoroutine);
            runningAnimations.Remove(obj);

            // Finalize the state immediately
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = completeFadeIn ? 1f : 0f;
                if (!completeFadeIn)
                    obj.SetActive(false);
            }
        }
    }

    // Called at the end of each animation coroutine
    private void FinishAnimation(GameObject obj, Action onComplete)
    {
        runningAnimations.Remove(obj);
        onComplete?.Invoke();
    }



    // The coroutine for fading in
    private IEnumerator FadeInCoroutine(GameObject obj, float duration, Action onComplete)
    {
        // Enable the GameObject
        obj.SetActive(true);

        // Get or add a CanvasGroup component on the parent object
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }

        // Start fully transparent
        cg.alpha = 0f;

        // Fade in
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        // Ensure it's fully opaque at the end
        cg.alpha = 1f;

        // Animation is done
        FinishAnimation(obj, onComplete);
    }


    // The coroutine for fading out
    private IEnumerator FadeOutCoroutine(GameObject obj, float duration, Action onComplete)
    {
        // Get or add a CanvasGroup component on the parent object
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }

        // Fade out
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }

        // Ensure it's fully invisible at the end (and nonnegative)
        cg.alpha = 0f;

        // Animation is done
        FinishAnimation(obj, onComplete);
    }
}
