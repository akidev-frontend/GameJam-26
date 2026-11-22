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

        // Contador de xiuxos comidos; sobrevive a los cambios de escena.
        private static int eaten;

        private static readonly System.Collections.Generic.List<XuixoCounterText> instances =
            new System.Collections.Generic.List<XuixoCounterText>();

        private Camera mainCamera;
        private TextMesh textMesh;

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
            mainCamera = Camera.main;

            // El nivel N corresponde a N-2 xiuxos ya comidos al entrar; evita
            // que reiniciar un nivel o saltarse uno deje el contador por detrás.
            int baseline = Mathf.Max(0, gameObject.scene.buildIndex - 2);
            if (eaten < baseline) eaten = baseline;

            Refresh();
        }

        private void OnEnable() => instances.Add(this);
        private void OnDisable() => instances.Remove(this);

        private void LateUpdate()
        {
            // Anclado a la esquina superior izquierda de la cámara, por si esta se mueve.
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
