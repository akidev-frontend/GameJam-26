using UnityEngine;

namespace GameJam.UI
{
    /// <summary>
    /// Contador global de trozos de xiuxo comidos. Se muestra arriba a la
    /// izquierda con la misma tipografía que el resto de textos (TextMesh).
    /// El contador persiste entre escenas durante la partida (estático) y
    /// sube 1 cada vez que el jugador come un xuixo (ver XuxoPickup).
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class XuixoCounterText : MonoBehaviour
    {
        [Header("Ajustes")]
        [Tooltip("Total de trozos de xiuxo = número de niveles jugables (7 levels - 1, el Level1 no cuenta).")]
        [SerializeField] private int totalPieces = 6;

        [Tooltip("Márgenes (0-1) respecto a la esquina superior izquierda de la cámara.")]
        [SerializeField] private float marginX = 0.04f;
        [SerializeField] private float marginY = 0.95f;

        [Tooltip("Cartel de fondo. Si está vacío se busca 'PosterContadorXuxo'. Con cartel, el texto se ancla a él.")]
        [SerializeField] private Transform poster;
        [Tooltip("Margen horizontal (unidades de mundo) desde el borde izquierdo del cartel.")]
        [SerializeField] private float posterPadding = 0.25f;

        // Contador de xiuxos comidos; sobrevive a los cambios de escena.
        private static int eaten;
        // Último nivel visto; si volvemos atrás (o reiniciamos la partida),
        // el contador vuelve a 0.
        private static int lastSceneIndex = -1;

        private static readonly System.Collections.Generic.List<XuixoCounterText> instances =
            new System.Collections.Generic.List<XuixoCounterText>();

        private Camera mainCamera;
        private TextMesh textMesh;
        private SpriteRenderer posterRenderer;

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
            mainCamera = Camera.main;

            // El texto se pega al cartel para quedar siempre encima de él
            // (el cartel es unlit: se ve también en niveles a oscuras).
            if (poster == null)
                poster = GameObject.Find("PosterContadorXuxo")?.transform;
            if (poster != null)
                posterRenderer = poster.GetComponent<SpriteRenderer>();

            // Si volvemos a un nivel igual o anterior al último visto (reinicio
            // de partida, volver al menú y empezar de nuevo...), el contador
            // se resetea. Ojo: los statics persisten entre sesiones de Play en
            // el editor, por eso esto es necesario también aquí.
            int sceneIndex = gameObject.scene.buildIndex;
            if (lastSceneIndex >= 0 && sceneIndex <= lastSceneIndex)
                eaten = 0;
            lastSceneIndex = sceneIndex;

            // El nivel N corresponde a N-2 xiuxos ya comidos al entrar; evita
            // que reiniciar un nivel o saltarse uno deje el contador por detrás.
            int baseline = Mathf.Max(0, sceneIndex - 2);
            if (eaten < baseline) eaten = baseline;

            Refresh();
        }

        private void OnEnable() => instances.Add(this);
        private void OnDisable() => instances.Remove(this);

        private void LateUpdate()
        {
            // Si hay cartel, el texto se alinea dentro de él. El cartel no lleva
            // script: queda fijo, así que el HUD tampoco se mueve con la cámara.
            if (poster)
            {
                float leftEdge = posterRenderer
                    ? poster.position.x - posterRenderer.bounds.extents.x
                    : poster.position.x;
                transform.position = new Vector3(leftEdge + posterPadding, poster.position.y, transform.position.z);
                return;
            }

            // Sin cartel: anclado a la esquina superior izquierda de la cámara.
            if (!mainCamera) mainCamera = Camera.main;
            if (!mainCamera) return;

            float dist = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
            Vector3 corner = mainCamera.ViewportToWorldPoint(new Vector3(marginX, marginY, dist));
            transform.position = new Vector3(corner.x, corner.y, transform.position.z);
        }

        /// <summary>Registra xiuxos comidos y refresca todos los textos vivos.</summary>
        public static void NotifyEaten(int amount = 1)
        {
            eaten += amount;
            foreach (var instance in instances)
                instance.Refresh();
        }

        /// <summary>Reinicia el contador (p. ej. al volver al menú principal).</summary>
        public static void ResetCounter() => eaten = 0;

        private void Refresh()
        {
            if (textMesh != null)
                textMesh.text = $"trozos de xiuxo comidos: {eaten}/{totalPieces}";
        }
    }
}
