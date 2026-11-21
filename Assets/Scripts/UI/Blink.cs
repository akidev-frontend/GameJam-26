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

        private void Update()
        {
            float t = (Time.time * blinksPerSecond) % 1f;
            SetVisible(t < dutyCycle);
        }

        private void SetVisible(bool visible)
        {
            if (sr) sr.enabled = visible;
        }
    }
}
