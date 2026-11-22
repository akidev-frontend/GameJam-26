using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameJam.UI
{
    /// <summary>
    /// Secuencia de viñetas a pantalla completa (estilo manga) con fundidos:
    /// cada imagen entra desde negro, se mantiene y se va a negro antes de la
    /// siguiente. Al acabar puede volver al menú o cargar otra escena.
    /// Usa un SpriteRenderer que se escala solo para encajar en la cámara.
    /// </summary>
    public class MangaSequence : MonoBehaviour
    {
        [Header("Viñetas")]
        [Tooltip("SpriteRenderer donde se pintan las viñetas. Si está vacío se busca en el propio objeto o en sus hijos.")]
        [SerializeField] private SpriteRenderer target;

        [Tooltip("Viñetas en orden. Las entradas vacías se saltan.")]
        [SerializeField] private Sprite[] slides;

        [Tooltip("Segundos que se ve cada viñeta completa.")]
        [SerializeField] private float holdSeconds = 3f;

        [Tooltip("Duración del fundido de entrada y salida de cada viñeta.")]
        [SerializeField] private float fadeDuration = 0.8f;

        [Tooltip("Segundos de negro entre viñeta y viñeta.")]
        [SerializeField] private float blackBetween = 0.4f;

        [Tooltip("Duración propia por viñeta (opcional). Si es 0 o falta, se usa Hold Seconds.")]
        [SerializeField] private float[] slideDurations;

        [Tooltip("Si está activo, la viñeta llena la pantalla recortándose; si no, se ve entera con bordes negros.")]
        [SerializeField] private bool fillScreen;

        [Header("Final")]
        [Tooltip("Escena a cargar al terminar. Vacío = se queda en negro.")]
        [SerializeField] private string nextScene = "MainMenu";

        [Tooltip("Segundos de negro al terminar antes de cambiar de escena.")]
        [SerializeField] private float endDelay = 1f;

        [Tooltip("Se dispara al acabar la última viñeta.")]
        public UnityEvent onFinish;

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            if (target == null) target = GetComponentInChildren<SpriteRenderer>(true);

            if (target != null)
            {
                // Que se dibuje por encima de todo lo demás
                target.sortingOrder = 1000;
                SetAlpha(0f);
            }
        }

        private void LateUpdate()
        {
            FitToScreen();
        }

        private void Start()
        {
            StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            if (target == null) yield break;

            for (int i = 0; i < slides.Length; i++)
            {
                if (slides[i] == null) continue;

                target.sprite = slides[i];
                FitToScreen();
                yield return Fade(0f, 1f);

                float hold = (slideDurations != null && i < slideDurations.Length && slideDurations[i] > 0f)
                    ? slideDurations[i]
                    : holdSeconds;
                if (hold > 0f) yield return new WaitForSeconds(hold);

                yield return Fade(1f, 0f);
                if (blackBetween > 0f) yield return new WaitForSeconds(blackBetween);
            }

            onFinish?.Invoke();

            if (endDelay > 0f) yield return new WaitForSeconds(endDelay);
            if (!string.IsNullOrEmpty(nextScene)) SceneManager.LoadScene(nextScene);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (fadeDuration <= 0f)
            {
                SetAlpha(to);
                yield break;
            }

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / fadeDuration));
                SetAlpha(Mathf.Lerp(from, to, k));
                yield return null;
            }
            SetAlpha(to);
        }

        /// <summary>Escala y centra la viñeta para que encaje en la cámara.</summary>
        private void FitToScreen()
        {
            if (target == null || target.sprite == null) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            float alto = cam.orthographicSize * 2f;
            float ancho = alto * cam.aspect;

            Vector2 tam = target.sprite.bounds.size;
            if (tam.x <= 0f || tam.y <= 0f) return;

            float sx = ancho / tam.x;
            float sy = alto / tam.y;
            float escala = fillScreen ? Mathf.Max(sx, sy) : Mathf.Min(sx, sy);

            target.transform.localScale = new Vector3(escala, escala, 1f);
            Vector3 p = cam.transform.position;
            target.transform.position = new Vector3(p.x, p.y, 0f);
        }

        private void SetAlpha(float a)
        {
            if (target == null) return;
            Color c = target.color;
            c.a = a;
            target.color = c;
        }
    }
}
