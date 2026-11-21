using UnityEngine;

namespace GameJam.UI
{
    /// <summary>
    /// Parpadeo para selectores de menú: alterna la visibilidad del SpriteRenderer
    /// con una onda cuadrada. Empieza siempre visible y sincronizado desde el inicio.
    /// </summary>
    public class Blink : MonoBehaviour
    {
        [Header("Ajustes")]
        [Tooltip("Parpadeos completos (visible+oculto) por segundo.")]
        [SerializeField] private float blinksPerSecond = 2f;
        [Tooltip("Fracción del ciclo que pasa visible (0.5 = mitad y mitad).")]
        [SerializeField, Range(0.05f, 0.95f)] private float dutyCycle = 0.5f;

        private SpriteRenderer sr;

        // Estado del parpadeo rápido temporal (confirmación de clic)
        private float fastUntil = -1f;
        private float fastBps;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            // Al (re)activarse empieza el ciclo visible
            SetVisible(true);
        }

        private void OnDisable()
        {
            SetVisible(true);
        }

        /// <summary>
        /// Fuerza un parpadeo más rápido durante 'seconds' segundos
        /// (se usa como feedback al confirmar una opción del menú).
        /// </summary>
        public void BlinkFast(float seconds, float blinksPerSecondFast)
        {
            fastUntil = Time.time + seconds;
            fastBps = blinksPerSecondFast;
            SetVisible(true);
        }

        private void Update()
        {
            float bps = Time.time < fastUntil ? fastBps : blinksPerSecond;
            float t = (Time.time * bps) % 1f;
            SetVisible(t < dutyCycle);
        }

        private void SetVisible(bool visible)
        {
            if (sr) sr.enabled = visible;
        }
    }
}
