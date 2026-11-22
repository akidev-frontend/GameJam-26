using UnityEngine;

namespace GameJam.Player
{
    /// <summary>
    /// Presentación del jugador: reproduce la animación de caminar cuando se mueve,
    /// congela el primer frame al parar y gira el sprite según la dirección.
    /// Solo presentación: la lógica vive en PlayerController2D.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator2D : MonoBehaviour
    {
        [Header("Ajustes")]
        [SerializeField] private float speedThreshold = 0.05f;
        [Tooltip("¿El arte original mira a la derecha? Si no, se invierte el giro.")]
        [SerializeField] private bool facesRight = true;

        [Header("Xuixo sostenido")]
        [Tooltip("Sprite del xuixo que se muestra en la mano durante la animación de comer.")]
        [SerializeField] private Sprite xuixoHeldSprite;
        [Tooltip("Posición local del xuixo respecto al jugador.")]
        [SerializeField] private Vector2 xuixoOffset = new Vector3(0.05f, 0.02f);

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        // Mientras dura la animación de muerte, el walk/idle no toca el Animator.
        private bool dying;
        // Mientras celebra/come el xuixo, tampoco.
        private bool eating;
        private SpriteRenderer xuixoHeld;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            EnsureXuixoHeld();
        }

        private void Update()
        {
            if (animator.runtimeAnimatorController == null) return;
            if (dying) return;

            // Fin de la animación de comer: ocultar el xuixo y volver a Walk
            if (eating)
            {
                var eatState = animator.GetCurrentAnimatorStateInfo(0);
                if (eatState.IsName("Xuxo") && eatState.normalizedTime >= 1f)
                {
                    eating = false;
                    SetXuixoHeldVisible(false);
                    animator.Play("Walk", 0, 0f);
                }
                return;
            }

            float vx = rb.linearVelocity.x;
            bool moving = Mathf.Abs(vx) > speedThreshold;

            // Sin clip de idle: pausamos la animación y volvemos al primer frame
            animator.speed = moving ? 1f : 0f;
            if (!moving && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0f)
                animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 0f);

            // Giro según la dirección del movimiento
            if (moving)
                spriteRenderer.flipX = facesRight ? vx < 0f : vx > 0f;
        }

        /// <summary>
        /// Reproduce la animación de muerte (estado "Muerte" del controller).
        /// Llamarlo al morir; al reaparecer llamar OnRespawned().
        /// </summary>
        public void PlayDeath()
        {
            if (animator == null) return;
            dying = true;
            animator.speed = 1f; // por si estaba pausado (idle congela el speed)
            animator.Play("Muerte", 0, 0f);
        }

        /// <summary>Vuelve al estado Walk tras reaparecer.</summary>
        public void OnRespawned()
        {
            dying = false;
            SetXuixoHeldVisible(false);
            if (animator != null && animator.runtimeAnimatorController != null)
                animator.Play("Walk", 0, 0f);
        }

        /// <summary>
        /// Reproduce la animación de comer xuixo (estado "Xuxo") y muestra el
        /// xuixo sostenido durante ella.
        /// </summary>
        public void PlayXuxo()
        {
            if (animator == null) return;
            EnsureXuixoHeld();
            eating = true;
            SetXuixoHeldVisible(true);
            animator.speed = 1f;
            animator.Play("Xuxo", 0, 0f);
        }

        // Crea (una vez) el SpriteRenderer hijo con el xuixo en la mano.
        private void EnsureXuixoHeld()
        {
            if (xuixoHeld != null || xuixoHeldSprite == null) return;

            var held = new GameObject("XuixoEnMano");
            held.transform.SetParent(transform, false);
            held.transform.localPosition = xuixoOffset;
            held.transform.localScale = Vector3.one * 0.1f;
            xuixoHeld = held.AddComponent<SpriteRenderer>();
            xuixoHeld.sprite = xuixoHeldSprite;
            xuixoHeld.sortingOrder = 15; // por delante del jugador
            xuixoHeld.enabled = false;
        }

        private void SetXuixoHeldVisible(bool visible)
        {
            if (xuixoHeld != null) xuixoHeld.enabled = visible;
        }
    }
}
