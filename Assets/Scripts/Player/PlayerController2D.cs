using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Player
{
    /// <summary>
    /// Movimiento y salto del jugador (plataformeo 2D).
    /// Incluye coyote time y jump buffer para que el control se sienta justo.
    /// Pensado para el sistema de reglas: el nivel puede modificar InputScale
    /// (p. ej. -1 para invertir los controles en el nivel 2).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float acceleration = 60f;
        [Tooltip("Multiplicador de control en el aire (0-1).")]
        [SerializeField, Range(0f, 1f)] private float airControl = 0.8f;

        [Header("Salto")]
        [SerializeField] private float jumpForce = 12f;
        [Tooltip("Tiempo tras dejar el suelo en el que aun puedes saltar.")]
        [SerializeField] private float coyoteTime = 0.1f;
        [Tooltip("Ventana en la que un salto pulsado antes de aterrizar se ejecuta al aterrizar.")]
        [SerializeField] private float jumpBuffer = 0.1f;

        [Header("Deteccion de suelo")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundMask;

        [Header("Regla: modificador de controles")]
        [Tooltip("Multiplica el input horizontal. Nivel 2 lo pone a -1 para invertir los controles.")]
        [SerializeField] private float inputScale = 1f;

        public bool IsGrounded { get; private set; }
        public float InputScale
        {
            get => inputScale;
            set => inputScale = value;
        }

        private Rigidbody2D rb;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private bool jumpQueued;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            HandleInput();
            UpdateJumpTimers();
            TryJump();
        }

        private void FixedUpdate()
        {
            CheckGround();
        }

        private void HandleInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float axis = 0f;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) axis -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) axis += 1f;

            float targetSpeed = axis * inputScale * moveSpeed * (IsGrounded ? 1f : airControl);
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.deltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

            if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
                jumpQueued = true;
        }

        private void UpdateJumpTimers()
        {
            coyoteCounter = IsGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;

            if (jumpQueued)
            {
                jumpBufferCounter = jumpBuffer;
                jumpQueued = false;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        private void TryJump()
        {
            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                coyoteCounter = 0f;
                jumpBufferCounter = 0f;
            }
        }

        private void CheckGround()
        {
            Vector2 origin = groundCheck ? (Vector2)groundCheck.position : (Vector2)transform.position;
            IsGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundMask);
        }
    }
}