using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class Animations : MonoBehaviour
{
    /// <summary>
    /// Types of animations
    /// </summary>
    public enum AnimationType
    {
        FadeIn,
        FadeOut,
        Shake
    }

    private Dictionary<GameObject, Coroutine> runningAnimations = new Dictionary<GameObject, Coroutine>();

    /// <summary>
    /// Public method for other scripts to call to play an animation
    /// </summary>
    /// <param name="obj">GameObject the animation will run on</param>
    /// <param name="animation">Type of animation to play</param>
    /// <param name="duration">Duration of animation in seconds</param>
    /// <param name="onComplete">Action/method to run when animation is done</param>
    public void PlayAnimation(GameObject obj, AnimationType animation, float duration, Action onComplete)
    {
        Debug.Log($"Playing animation {animation.ToString()} for {duration} seconds on {obj.gameObject.GetComponent<Image>().sprite.name}");

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
            case AnimationType.Shake:
                newCoroutine = StartCoroutine(ShakeCoroutine(obj, duration, onComplete));
                break;
        }

        // Set current animation
        if (newCoroutine != null)
        {
            runningAnimations[obj] = newCoroutine;
        }
    }


    /// <summary>
    /// Stop any animation currently running
    /// </summary>
    /// <param name="obj">GameObject to stop the exiting animation on</param>
    /// <param name="completeAnimation">If the animation has been completed</param>
    private void StopExistingAnimation(GameObject obj, bool completeAnimation)
    {
        if (runningAnimations.TryGetValue(obj, out Coroutine runningCoroutine))
        {
            StopCoroutine(runningCoroutine);
            runningAnimations.Remove(obj);

            // Finalize the state immediately
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = completeAnimation ? 1f : 0f;
                if (!completeAnimation)
                    obj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Called at the end of each animation coroutine
    /// </summary>
    /// <param name="obj">GameObject the animation was ran on</param>
    /// <param name="onComplete">Action/method to run when animation is done</param>
    private void FinishAnimation(GameObject obj, Action onComplete)
    {
        runningAnimations.Remove(obj);
        onComplete?.Invoke();
    }



    /// <summary>
    /// The coroutine for fading in
    /// </summary>
    /// <param name="obj">GameObject the animation will be run on</param>
    /// <param name="duration">Duration of animation in seconds</param>
    /// <param name="onComplete">Action/method to run when animation is done</param>
    /// <returns>An IEnumerator used by Unity to run the coroutine</returns>
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


    /// <summary>
    /// The coroutine for fading out
    /// </summary>
    /// <param name="obj">GameObject the animation will be run on</param>
    /// <param name="duration">Duration of animation in seconds</param>
    /// <param name="onComplete">Action/method to run when animation is done</param>
    /// <returns>An IEnumerator used by Unity to run the coroutine</returns>
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

        // Disable object
        obj.SetActive(false);

        // Animation is done
        FinishAnimation(obj, onComplete);
    }

    private IEnumerator ShakeCoroutine(GameObject obj, float duration, Action onComplete)
    {
        obj.SetActive(true);

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = obj.AddComponent<CanvasGroup>();
        }

        // Make sure it's visible
        cg.alpha = 1f;

        // Store original position so we can return to it
        Vector3 originalPos = obj.transform.localPosition;

        float elapsed = 0f;
        float shakeMagnitude = 5f;  // how far it shakes
        float shakeFrequency = 40f; // how fast it shakes

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Shake X position using sine wave
            float xOffset = Mathf.Sin(elapsed * shakeFrequency) * shakeMagnitude;
            obj.transform.localPosition = originalPos + new Vector3(xOffset, 0f, 0f);

            yield return null;
        }

        // Reset position
        obj.transform.localPosition = originalPos;

        FinishAnimation(obj, onComplete);
    }

}
