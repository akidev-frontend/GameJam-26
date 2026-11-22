using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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
        [Tooltip("Segundos de espera antes de empezar a escribir, además del fundido de entrada de la escena.")]
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

        [Header("Cuadro de fondo")]
        [Tooltip("Cuadro que va detrás del texto. Si está vacío se coge el primer SpriteRenderer hijo (el 'texto_0').")]
        [SerializeField] private SpriteRenderer background;
        [Tooltip("Margen entre el borde izquierdo del cuadro y el inicio del texto (unidades de mundo). " +
                 "Evita que las frases se peguen o se salgan del borde aunque redimensiones el cuadro.")]
        [SerializeField] private float textLeftMargin = 1.5f;

        [Header("Fundido de entrada")]
        [Tooltip("Si está activo, el diálogo espera a que termine el fundido de entrada de la escena (FadeIn) y luego aparece.")]
        [SerializeField] private bool waitForSceneFade = true;
        [Tooltip("Segundos que tardan en aparecer el cuadro y el texto al empezar el diálogo. 0 = aparecen de golpe.")]
        [SerializeField] private float fadeInDuration = 0.3f;

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

        [Header("Evento")]
        [Tooltip("Se dispara cuando la secuencia completa termina (tras el hold de la última línea). No se dispara si loop está activo ni si se corta con Vaciar().")]
        public UnityEvent onFinish;

        private void Awake()
        {
            if (!targetText) targetText = GetComponent<TextMesh>();
            if (targetText) targetText.text = string.Empty;

            if (background == null) background = GetComponentInChildren<SpriteRenderer>(true);
            ColocarCuadro();
        }

        /// <summary>
        /// Alinea el borde izquierdo del cuadro a <see cref="textLeftMargin"/>
        /// unidades del inicio del texto. El texto está anclado a la izquierda en
        /// el origen del objeto, así que el cuadro se desplaza a partir de ahí.
        /// Se calcula con los bounds en mundo: funciona con cualquier escala.
        /// </summary>
        private void ColocarCuadro()
        {
            if (background == null || background.sprite == null) return;

            Bounds bounds = background.bounds;
            float izquierdaDeseada = transform.position.x - textLeftMargin;
            float izquierdaActual = bounds.center.x - bounds.extents.x;
            float delta = izquierdaDeseada - izquierdaActual;
            if (Mathf.Abs(delta) < 0.0001f) return;

            background.transform.position += new Vector3(delta, 0f, 0f);
        }

        /// <summary>Vuelve a colocar el cuadro (por si se cambia el margen en runtime).</summary>
        public void RefrescarCuadro()
        {
            if (background == null) background = GetComponentInChildren<SpriteRenderer>(true);
            ColocarCuadro();
        }

        /// <summary>Aplica la misma transparencia al texto y al cuadro de fondo.</summary>
        private void SetAlpha(float alpha)
        {
            if (targetText != null)
            {
                Color c = targetText.color;
                c.a = alpha;
                targetText.color = c;
            }

            if (background != null)
            {
                Color c = background.color;
                c.a = alpha;
                background.color = c;
            }
        }

        /// <summary>Fundido de entrada del cuadro + texto al empezar el diálogo.</summary>
        private IEnumerator FadeInRoutine()
        {
            float t = 0f;
            float duracion = Mathf.Max(0.01f, fadeInDuration);
            while (t < duracion)
            {
                t += Time.deltaTime;
                // SmoothStep: entra y sale suave, como el fundido de la escena
                SetAlpha(Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracion)));
                yield return null;
            }
            SetAlpha(1f);
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

            // El cuadro y el texto empiezan invisibles y aparecen con un fundido,
            // para no dar el salto al terminar el fundido de entrada de la escena.
            SetAlpha(fadeInDuration > 0f ? 0f : 1f);

            // Encadena con el fundido de la escena: en cuanto el negro se va,
            // el diálogo aparece (sin el hueco de espera de antes).
            if (waitForSceneFade)
            {
                var sceneFade = FindFirstObjectByType<FadeIn>();
                if (sceneFade != null)
                {
                    float restante = sceneFade.FadeRemaining;
                    if (restante > 0f) yield return new WaitForSeconds(restante);
                }
            }

            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);
            if (fadeInDuration > 0f) yield return FadeInRoutine();

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

                // Secuencia completa (sin loop): notificar el final.
                if (!loop) onFinish?.Invoke();
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
