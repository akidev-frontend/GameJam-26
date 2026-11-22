using UnityEngine;
using UnityEngine.SceneManagement;
using GameJam.UI;

namespace GameJam.Level
{
    /// <summary>
    /// Xuixo comestible: cuando el Player toca el collider (trigger), suma 1 al
    /// contador global de xiuxos comidos, el jugador celebra (animación RataXuxo
    /// con el xuixo en la mano) y, al acabar la animación, espera unos decimales,
    /// hace fundido a negro y carga el nivel siguiente.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class XuxoPickup : MonoBehaviour
    {
        [Tooltip("Si está activo, el xuixo desaparece al comérselo.")]
        [SerializeField] private bool desaparecerAlComer = true;

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

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (eaten) return;
            if (!other.GetComponent<GameJam.Player.PlayerController2D>()) return;

            eaten = true;
            GameJam.UI.XuixoCounterText.NotifyEaten();

            var animator = other.GetComponentInParent<GameJam.Player.PlayerAnimator2D>();
            if (animator) animator.PlayXuxo();

            // Ojo: no usar SetActive(false) aquí, mataría la corrutina de abajo.
            // Ocultar el xuixo apagando renderer y collider.
            if (desaparecerAlComer)
            {
                foreach (var r in GetComponentsInChildren<SpriteRenderer>()) r.enabled = false;
                foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            }

            StartCoroutine(AdvanceLevel());
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

