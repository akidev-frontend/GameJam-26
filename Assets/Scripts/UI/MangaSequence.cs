using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameJam.UI
{
    /// <summary>
    /// Página final estilo manga: las viñetas ya colocadas en la escena van
    /// apareciendo una a una con un fundido y se quedan visibles (no se
    /// reemplazan entre ellas). Al terminar, fundido a negro y vuelta al menú.
    /// </summary>
    public class MangaSequence : MonoBehaviour
    {
        [Header("Viñetas")]
        [Tooltip("Viñetas en orden de aparición. Cada una debe estar ya colocada en su parte de la pantalla.")]
        [SerializeField] private SpriteRenderer[] panels;

        [Header("Tiempos")]
        [Tooltip("Segundos de negro antes de que aparezca la primera viñeta.")]
        [SerializeField] private float startDelay = 0.4f;
        [Tooltip("Duración del fundido de entrada de cada viñeta.")]
        [SerializeField] private float fadeInDuration = 0.6f;
        [Tooltip("Espera entre la aparición de una viñeta y la siguiente.")]
        [SerializeField] private float delayBetween = 0.9f;
        [Tooltip("Tiempo que se queda la página completa antes del fundido final.")]
        [SerializeField] private float endHoldSeconds = 2f;
        [Tooltip("Duración del fundido a negro final.")]
        [SerializeField] private float endFadeDuration = 0.8f;

        [Header("Final")]
        [Tooltip("Escena a cargar al terminar. Vacío = se queda en negro.")]
        [SerializeField] private string nextScene = "MainMenu";
        [Tooltip("Segundos de negro antes de cambiar de escena.")]
        [SerializeField] private float endDelay = 0.5f;

        [Tooltip("Se dispara cuando ya se han mostrado todas las viñetas.")]
        public UnityEvent onFinish;

        private void Awake()
        {
            // Todas las viñetas empiezan invisibles sobre el fondo negro
            if (panels == null) return;
            foreach (var panel in panels)
                if (panel != null) SetAlpha(panel, 0f);
        }

        private void Start()
        {
            StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            if (panels == null || panels.Length == 0) yield break;

            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            for (int i = 0; i < panels.Length; i++)
            {
                SpriteRenderer panel = panels[i];
                if (panel == null) continue;

                // Aparece la viñeta y se queda en pantalla
                yield return Fade(panel, 0f, 1f);

                if (i < panels.Length - 1 && delayBetween > 0f)
                    yield return new WaitForSeconds(delayBetween);
            }

            onFinish?.Invoke();

            if (endHoldSeconds > 0f) yield return new WaitForSeconds(endHoldSeconds);

            // Fundido de salida: toda la página se va a negro a la vez
            yield return FadeAll(1f, 0f);

            if (endDelay > 0f) yield return new WaitForSeconds(endDelay);

            if (!string.IsNullOrEmpty(nextScene))
                SceneManager.LoadScene(nextScene);
        }

        private IEnumerator Fade(SpriteRenderer panel, float from, float to)
        {
            if (fadeInDuration <= 0f)
            {
                SetAlpha(panel, to);
                yield break;
            }

            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / fadeInDuration));
                SetAlpha(panel, Mathf.Lerp(from, to, k));
                yield return null;
            }
            SetAlpha(panel, to);
        }

        private IEnumerator FadeAll(float from, float to)
        {
            if (endFadeDuration <= 0f)
            {
                foreach (var panel in panels) if (panel != null) SetAlpha(panel, to);
                yield break;
            }

            float t = 0f;
            while (t < endFadeDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / endFadeDuration));
                float alpha = Mathf.Lerp(from, to, k);
                foreach (var panel in panels) if (panel != null) SetAlpha(panel, alpha);
                yield return null;
            }
            foreach (var panel in panels) if (panel != null) SetAlpha(panel, to);
        }

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}
