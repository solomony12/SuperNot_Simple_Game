using UnityEngine;
using UnityEngine.SceneManagement;

public class TapParticleSpawn : MonoBehaviour
{
    public GameObject particlePrefab;
    public RectTransform canvasRectTransform;

    private Canvas canvas;

    private void Awake()
    {
        setCanvas();
    }

    void Update()
    {
        // Detect click/tap
        #if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                SpawnEffect(Input.mousePosition);
            }
        #else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                SpawnEffect(Input.GetTouch(0).position);
            }
        #endif
    }

    /// <summary>
    /// Spawns a particle effect
    /// </summary>
    /// <param name="screenPosition">Position clicked on screen</param>
    void SpawnEffect(Vector2 screenPosition)
    {
        if (canvasRectTransform == null || particlePrefab == null)
            return;

        Vector2 localPoint;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPosition, cam, out localPoint))
        {
            // Instantiate under canvas
            GameObject effect = Instantiate(particlePrefab, canvasRectTransform);
            RectTransform effectRect = effect.GetComponent<RectTransform>();
            effectRect.anchoredPosition = localPoint;
            effectRect.localScale = Vector3.one;

            // Disable raycast target to allow clicks through
            var graphic = effect.GetComponent<UnityEngine.UI.Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = false;
            }

            // Play the particle system manually
            UIParticleSystem uiParticle = effect.GetComponent<UIParticleSystem>();
            if (uiParticle != null)
            {
                ParticleSystem ps = uiParticle.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                }
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        setCanvas();
    }

    // Set the canvas game object
    private void setCanvas()
    {
        GameObject canvasObject = GameObject.FindWithTag(ScriptConstants.mainCanvasString);
        if (canvasObject == null)
        {
            Debug.LogError("Canvas not found.");
            return;
        }

        canvasRectTransform = canvasObject.GetComponent<RectTransform>();
        canvas = canvasObject.GetComponent<Canvas>();
    }
}
