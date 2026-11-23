using System.Collections;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>
    /// Transición de entrada: cubre la pantalla de negro y se desvanece al cargar la escena.
    /// Colócalo sobre un Canvas (Screen Space Overlay) con un Image negro a pantalla completa.
    /// Al terminar, se desactiva el overlay para que no interfiera.
    /// </summary>
    [ExecuteAlways]
    public class FadeIn : MonoBehaviour
    {
        [Header("Ajustes")]
        [Tooltip("Duración del fundido de entrada en segundos.")]
        [SerializeField] private float duration = 0.8f;
        [Tooltip("CanvasGroup cuyo alpha se anima (el del propio Canvas si se deja vacío).")]
        [SerializeField] private CanvasGroup canvasGroup;

        // Momento (Time.time) en el que arranca el fundido, para saber cuánto queda.
        private float fadeStartTime;

        // En el editor mantenemos el overlay transparente para que no
        // tape la escena en la vista de Scene (en play lo controla la corrutina).
        private void OnEnable()
        {
            if (Application.isPlaying) return;
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void Awake()
        {
            // Marca de inicio para que otros sistemas (p. ej. DialogueSystem)
            // sepan cuánto queda de fundido sin depender del orden de Start().
            fadeStartTime = Time.time;
        }

        private void Start()
        {
            if (Application.isPlaying) StartCoroutine(Fade());
        }

        /// <summary>Segundos que le quedan al fundido de entrada (0 si ya terminó).</summary>
        public float FadeRemaining =>
            Application.isPlaying ? Mathf.Max(0f, fadeStartTime + duration - Time.time) : 0f;

        /// <summary>True mientras la pantalla aún se está aclarando al entrar en la escena.</summary>
        public bool IsFading => FadeRemaining > 0f;

        private IEnumerator Fade()
        {
            if (!canvasGroup) yield break;

            // Arranca cubriendo toda la pantalla
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                // SmoothStep para un fundido más suave (arranca y termina con menos brusquedad)
                canvasGroup.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                yield return null;
            }

            // Al acabar dejamos el overlay en alpha 0 (sin desactivarlo,
            // para poder reutilizarlo para el fundido de salida)
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        // Fundido de salida: de transparente a negro y bloquea el input mientras dura.
        public IEnumerator FadeOut(float fadeDuration)
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) yield break;

            canvasGroup.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }
}
