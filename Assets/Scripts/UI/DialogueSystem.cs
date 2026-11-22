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
        [Tooltip("Duración específica por línea (en orden). Si una línea no tiene valor, se usa lineHoldSeconds.")]
        [SerializeField] private float[] lineDurations;
        [Tooltip("Ancho máximo por línea en caracteres. Las frases más largas se parten en espacios (salto de línea automático). 0 = sin límite.")]
        [SerializeField] private int maxCharsPerLine = 40;
        [Tooltip("Si está activo, el diálogo se repite en bucle.")]
        [SerializeField] private bool loop;
        [Tooltip("Si está desactivado, no se reproduce al iniciar (se lanza manualmente con Play).")]
        [SerializeField] private bool playOnStart = true;

        [Header("Audio")]
        [Tooltip("Audio opcional que suena cuando empieza a escribirse el texto; se corta con Vaciar().")]
        [SerializeField] private AudioSource voiceSource;
        [Tooltip("Elemento (1 = primera línea) en el que empieza a sonar el audio, justo al comenzar a teclearse esa línea.")]
        [SerializeField, Min(1)] private int voiceStartLine = 1;

        private TextMesh targetText;
        private Coroutine routine;
        private bool typing;

        /// <summary>True mientras se está escribiendo la línea actual carácter a carácter.</summary>
        public bool IsTyping => typing;

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

        /// <summary>Inicia el diálogo con líneas nuevas y su duración por línea.</summary>
        public void Play(string[] newLines, float[] durations)
        {
            if (durations != null) lineDurations = durations;
            Play(newLines);
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
            if (voiceSource) voiceSource.Stop();
        }

        private IEnumerator PlayRoutine()
        {
            if (!targetText) yield break;

            targetText.text = string.Empty;
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            // El audio arranca al empezar a teclearse la línea elegida.
            int voiceStartIndex = Mathf.Clamp(voiceStartLine - 1, 0, Mathf.Max(0, lines.Length - 1));
            bool voicePlayed = false;

            do
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!voicePlayed && i == voiceStartIndex)
                    {
                        voicePlayed = true;
                        if (voiceSource) voiceSource.Play();
                    }

                    yield return TypeLine(lines[i] ?? string.Empty);

                    float hold = (lineDurations != null && i < lineDurations.Length)
                        ? lineDurations[i]
                        : lineHoldSeconds;
                    yield return new WaitForSeconds(hold);
                }
            }
            while (loop);
        }

        private IEnumerator TypeLine(string line)
        {
            line = AjustarAncho(line);
            targetText.text = string.Empty;
            float cps = Mathf.Max(1f, charsPerSecond);
            float accumulated = 0f;
            int shown = 0;
            typing = true;

            try
            {
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
            finally
            {
                typing = false;
            }
        }

        /// <summary>
        /// Parte las frases demasiado largas en varias líneas, cortando por
        /// espacios para no partir palabras. Respeta saltos de línea existentes.
        /// </summary>
        private string AjustarAncho(string line)
        {
            if (maxCharsPerLine <= 0 || string.IsNullOrEmpty(line)) return line;

            var resultado = new System.Text.StringBuilder();
            foreach (var parrafo in line.Split('\n'))
            {
                var palabras = parrafo.Split(' ');
                int largo = 0;
                for (int i = 0; i < palabras.Length; i++)
                {
                    var palabra = palabras[i];
                    int extra = (largo > 0) ? 1 : 0; // el espacio que la precede

                    if (largo + extra + palabra.Length > maxCharsPerLine && largo > 0)
                    {
                        resultado.Append('\n');
                        largo = 0;
                        extra = 0;
                    }
                    else if (i > 0 && largo > 0)
                    {
                        resultado.Append(' ');
                    }

                    resultado.Append(palabra);
                    largo += extra + palabra.Length;
                }
                resultado.Append('\n');
            }
            // Quitar el salto final sobrante de cada párrafo procesado.
            return resultado.ToString().TrimEnd('\n');
        }
    }
}
