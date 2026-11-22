using System.Collections;
using UnityEngine;

namespace GameJam.UI
{
    /// <summary>
    /// Diálogo automático con efecto máquina de escribir sobre un TextMesh.
    /// Escribe cada línea carácter a carácter, la mantiene unos segundos
    /// y pasa sola a la siguiente. No requiere input del jugador.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class DialogueSystem : MonoBehaviour
    {
        [Header("Textos")]
        [Tooltip("Líneas del diálogo, en orden. Usa saltos de línea manuales si hace falta.")]
        [SerializeField, TextArea] private string[] lines =
        {
            "Bienvenido a Reglas Rotas...",
            "Aquí las reglas cambian en cada nivel.",
        };

        [Header("Ajustes")]
        [Tooltip("Segundos de espera antes de empezar a escribir.")]
        [SerializeField] private float startDelay = 1.2f;
        [Tooltip("Caracteres escritos por segundo.")]
        [SerializeField] private float charsPerSecond = 30f;
        [Tooltip("Segundos que se queda cada línea completa antes de la siguiente.")]
        [SerializeField] private float lineHoldSeconds = 1.5f;
        [Tooltip("Si está activo, el diálogo se repite en bucle.")]
        [SerializeField] private bool loop;
        [Tooltip("Si está desactivado, no se reproduce al iniciar (se lanza manualmente con Play).")]
        [SerializeField] private bool playOnStart = true;

        private TextMesh targetText;
        private Coroutine routine;

        private void Awake()
        {
            if (!targetText) targetText = GetComponent<TextMesh>();
            if (targetText) targetText.text = string.Empty;
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        /// <summary>Inicia el diálogo configurado en el inspector.</summary>
        public void Play()
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(PlayRoutine());
        }

        /// <summary>Inicia el diálogo con líneas nuevas.</summary>
        public void Play(string[] newLines)
        {
            if (newLines != null && newLines.Length > 0) lines = newLines;
            Play();
        }

        /// <summary>Detiene la secuencia y vacía el texto inmediatamente.</summary>
        public void Vaciar()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
            if (targetText) targetText.text = string.Empty;
        }

        private IEnumerator PlayRoutine()
        {
            if (!targetText) yield break;

            targetText.text = string.Empty;
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            do
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    yield return TypeLine(lines[i] ?? string.Empty);
                    yield return new WaitForSeconds(lineHoldSeconds);
                }
            }
            while (loop);
        }

        private IEnumerator TypeLine(string line)
        {
            targetText.text = string.Empty;
            float cps = Mathf.Max(1f, charsPerSecond);
            float accumulated = 0f;
            int shown = 0;

            while (shown < line.Length)
            {
                accumulated += Time.deltaTime * cps;
                int count = Mathf.Min(line.Length, Mathf.FloorToInt(accumulated));
                if (count != shown)
                {
                    shown = count;
                    targetText.text = line.Substring(0, shown);
                }
                yield return null;
            }
        }
    }
}
