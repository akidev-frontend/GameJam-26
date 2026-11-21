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

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (animator.runtimeAnimatorController == null) return;

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
    }
}
