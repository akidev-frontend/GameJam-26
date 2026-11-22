using UnityEngine;
using UnityEngine.SceneManagement;
using GameJam.UI;

namespace GameJam.Level
{
    /// <summary>
    /// Xuixo comestible. Bueno: suma 1 al contador global de xiuxos comidos, el
    /// jugador celebra (RataXuxo con el xuixo en la mano) y, al acabar, espera
    /// unos decimales, fundido a negro y nivel siguiente.
    /// Malo: mata (animación de muerte + respawn en el SpawnPoint), no suma y no
    /// avanza de nivel.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class XuxoPickup : MonoBehaviour
    {
        [Tooltip("Si está activo, el xuixo desaparece al comérselo.")]
        [SerializeField] private bool desaparecerAlComer = true;

        [Header("Xuixo malo")]
        [Tooltip("Marca este xuixo como malo: al tocarlo mueres y reapareces en el SpawnPoint. También se detecta si el objeto se llama '*malo*'.")]
        [SerializeField] private bool esXuxoMalo;

        [Tooltip("Punto de reaparición del xuixo malo; vacío = busca 'SpawnPoint'.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("Duración de la animación de muerte antes de reaparecer.")]
        [SerializeField] private float deathDuration = 0.9f;

        [Tooltip("Segundos de control bloqueado tras reaparecer.")]
        [SerializeField] private float respawnFreeze = 0.1f;

        [Header("Transición al siguiente nivel")]
        [Tooltip("Duración de la animación de comer (clip RataXuxo: 3 frames a 6 fps = 0,5 s).")]
        [SerializeField] private float eatAnimationDuration = 0.5f;
        [Tooltip("Segundos de espera tras acabar la animación antes del fundido.")]
        [SerializeField] private float waitAfterAnimation = 0.4f;
        [Tooltip("Duración del fundido a negro en segundos.")]
        [SerializeField] private float fadeDuration = 0.8f;
        [Tooltip("Escena a cargar (por nombre). Vacía = siguiente escena en el orden de build.")]
        [SerializeField] private string nextScene = "";

        private bool eaten;

        /// <summary>True si este xuixo es de los que matan.</summary>
        public bool EsMalo => esXuxoMalo || gameObject.name.ToLowerInvariant().Contains("malo");

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
        }

        private void Awake()
        {
            if (spawnPoint == null)
                spawnPoint = GameObject.Find("SpawnPoint")?.transform;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (eaten) return;
            var controller = other.GetComponent<GameJam.Player.PlayerController2D>();
            if (controller == null) return;

            if (EsMalo)
            {
                // Xuixo malo: muerte + respawn. No suma y no avanza de nivel.
                var animator = other.GetComponentInParent<GameJam.Player.PlayerAnimator2D>();
                StartCoroutine(PlayerDeathFlow.Run(
                    other.transform, controller, animator, other.attachedRigidbody,
                    spawnPoint ? spawnPoint.position : Vector3.zero,
                    deathDuration, respawnFreeze));

                // Se consume también (si no, morirías en bucle sobre el spawn).
                if (desaparecerAlComer) OcultarXuixo();
                return;
            }

            eaten = true;
            GameJam.UI.XuixoCounterText.NotifyEaten();

            // Congelar al player donde esté (incluso en el aire) mientras come:
            // sin control y sin física (ni gravedad ni movimiento).
            var playerRb = other.attachedRigidbody;
            if (controller) controller.InputScale = 0f;
            if (playerRb)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.simulated = false;
            }

            var playerAnimator = other.GetComponentInParent<GameJam.Player.PlayerAnimator2D>();
            if (playerAnimator) playerAnimator.PlayXuxo();

            // Ojo: no usar SetActive(false) aquí, mataría la corrutina de abajo.
            if (desaparecerAlComer) OcultarXuixo();

            StartCoroutine(AdvanceLevel());
        }

        // Ocultar el xuixo apagando renderer y collider (sin desactivar el GO,
        // para no matar corrutinas en curso).
        private void OcultarXuixo()
        {
            foreach (var r in GetComponentsInChildren<SpriteRenderer>()) r.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        }

        private System.Collections.IEnumerator AdvanceLevel()
        {
            // Animación de comer + margen pedido (~0,4 s)
            float wait = eatAnimationDuration + waitAfterAnimation;
            if (wait > 0f) yield return new WaitForSeconds(wait);

            // Fundido a negro con el FadeIn del CanvasFade de la escena
            var fader = FindFirstObjectByType<FadeIn>();
            if (fader != null)
                yield return fader.FadeOut(fadeDuration);

            if (!string.IsNullOrEmpty(nextScene))
            {
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                int next = gameObject.scene.buildIndex + 1;
                if (next >= 0 && next < SceneManager.sceneCountInBuildSettings)
                    SceneManager.LoadScene(next);
                else
                    SceneManager.LoadScene("MainMenu");
            }
        }
    }
}

