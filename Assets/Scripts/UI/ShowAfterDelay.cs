using System.Collections;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>
    /// Oculta el SpriteRenderer al arrancar y lo muestra tras un retardo.
    /// Útil para mensajes que deben aparecer unos segundos después de entrar al nivel.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShowAfterDelay : MonoBehaviour
    {
        [Header("Ajustes")]
        [Tooltip("Segundos de espera antes de mostrar el sprite.")]
        [SerializeField] private float delay = 1f;
        [Tooltip("Duración del fundido de invisible a opaco al aparecer. 0 = aparece sin fundido.")]
        [SerializeField, Min(0f)] private float fadeDuration = 0f;

        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;
        }

        private void Start()
        {
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (!sr) yield break;

            if (fadeDuration <= 0f)
            {
                sr.enabled = true;
                yield break;
            }

            // Fundido de invisible a opaco.
            var color = sr.color;
            color.a = 0f;
            sr.color = color;
            sr.enabled = true;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                color.a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / fadeDuration));
                sr.color = color;
                yield return null;
            }
            color.a = 1f;
            sr.color = color;
        }
    }
}
