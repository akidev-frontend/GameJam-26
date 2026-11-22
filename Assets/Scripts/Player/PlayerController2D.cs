using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam.Player
{
    /// <summary>
    /// Movimiento y salto del jugador (plataformeo 2D) con game feel mejorado:
    /// - Aceleración distinta al arrancar, frenar y girar.
    /// - Salto variable: soltar la tecla corta la velocidad vertical.
    /// - Gravedad extra al caer y "hang time" en el apex del salto.
    /// - Coyote time y jump buffer para que el control se sienta justo.
    /// Pensado para el sistema de reglas: el nivel puede modificar InputScale
    /// (p. ej. -1 para invertir los controles en el nivel 2).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 7f;
        [Tooltip("Aceleración en el suelo al arrancar.")]
        [SerializeField] private float groundAccel = 90f;
        [Tooltip("Deceleración al soltar el input; si es alta, el personaje para en seco.")]
        [SerializeField] private float groundDecel = 70f;
        [Tooltip("Aceleración extra al cambiar de dirección: hace el giro más responsivo.")]
        [SerializeField] private float turnAccel = 120f;
        [Tooltip("Fracción de aceleración disponible en el aire (la velocidad máxima no cambia).")]
        [SerializeField, Range(0f, 1f)] private float airControl = 0.65f;

        [Header("Salto")]
        [SerializeField] private float jumpForce = 12f;
        [Tooltip("Fracción de velocidad vertical que se conserva al soltar el salto antes de tiempo.")]
        [SerializeField, Range(0.1f, 1f)] private float jumpCutMultiplier = 0.45f;
        [Tooltip("Tiempo tras dejar el suelo en el que aun puedes saltar.")]
        [SerializeField] private float coyoteTime = 0.1f;
        [Tooltip("Ventana en la que un salto pulsado antes de aterrizar se ejecuta al aterrizar.")]
        [SerializeField] private float jumpBuffer = 0.1f;

        [Header("Gravedad ajustada")]
        [Tooltip("Multiplicador de gravedad al caer: hace la caída más pesada y rápida.")]
        [SerializeField] private float fallGravityMultiplier = 1.8f;
        [Tooltip("Multiplicador de gravedad cerca del apex: da tiempo de reacción en el punto más alto.")]
        [SerializeField] private float apexGravityMultiplier = 0.55f;
        [Tooltip("Velocidad vertical (|vy|) por debajo de la cual consideramos que estás en el apex.")]
        [SerializeField] private float apexThreshold = 0.8f;
        [Tooltip("Velocidad de caída máxima; evita atravesar el suelo en caídas largas.")]
        [SerializeField] private float maxFallSpeed = 22f;

        [Header("Deteccion de suelo")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundMask;

        [Header("Regla: modificador de controles")]
        [Tooltip("Multiplica el input horizontal. Nivel 2 lo pone a -1 para invertir los controles.")]
        [SerializeField] private float inputScale = 1f;

        [Header("Sonido")]
        [Tooltip("AudioSource 2D del jugador para sus efectos.")]
        [SerializeField] private AudioSource sfxSource;
        [Tooltip("Clip que suena cada vez que el personaje muere.")]
        [SerializeField] private AudioClip deathSfx;

        public bool IsGrounded { get; private set; }

        /// <summary>True mientras dura la muerte: sin física ni controles.</summary>
        public bool IsDead { get; private set; }

        public float InputScale
        {
            get => inputScale;
            set => inputScale = value;
        }

        /// <summary>
        /// Congela al jugador para morir: deja de tener físicas (ni gravedad, ni
        /// velocidad, ni empujes) y se ignoran los controles hasta ReviveAt().
        /// Devuelve false si ya estaba muerto, para no encadenar muertes duplicadas.
        /// </summary>
        public bool BeginDeath()
        {
            if (IsDead) return false;

            IsDead = true;
            inputAxis = 0f;
            jumpHeld = false;
            jumpQueued = false;
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            jumpCutAvailable = false;
            IsGrounded = false;

            if (rb != null)
            {
                // Parar en seco y dejar de simular: el cuerpo no cae ni recibe empujes
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }
            return true;
        }

        /// <summary>
        /// Reaparece en <paramref name="position"/> y devuelve el control tras
        /// <paramref name="controlFreeze"/> segundos. Reactiva la física limpia.
        /// </summary>
        public void ReviveAt(Vector3 position, float controlFreeze = 0f)
        {
            if (!IsDead) return;

            jumpQueued = false;
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            jumpCutAvailable = false;
            inputAxis = 0f;
            jumpHeld = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.gravityScale = baseGravityScale;
                transform.position = position;
                rb.simulated = true;
                rb.position = position;
            }
            else
            {
                transform.position = position;
            }

            IsDead = false;
            controlLockUntil = Time.time + Mathf.Max(0f, controlFreeze);
        }

        /// <summary>
        /// Reproduce el sonido de muerte. Lo llama PlayerDeathFlow en cada muerte.
        /// </summary>
        public void PlayDeathSound()
        {
            if (deathSfx == null) return;

            if (sfxSource != null)
                sfxSource.PlayOneShot(deathSfx);
            else
                AudioSource.PlayClipAtPoint(deathSfx, transform.position);
        }

        private Rigidbody2D rb;
        private float baseGravityScale;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private bool jumpQueued;
        private bool jumpCutAvailable;

        // Estado de input leído en Update y consumido en FixedUpdate
        private float inputAxis;
        private bool jumpHeld;

        // Bloqueo temporal de control al reaparecer (el player ya tiene física).
        private float controlLockUntil;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            baseGravityScale = rb.gravityScale;
        }

        private void Update()
        {
            if (IsDead) return;

            HandleInput();
            UpdateJumpTimers();
        }

        private void FixedUpdate()
        {
            if (IsDead) return;

            CheckGround();
            ApplyGravity();
            ApplyMovement();
            TryJump();
        }

        private void HandleInput()
        {
            // Tras reaparecer, unos instantes sin control (el cuerpo ya es dinámico)
            if (Time.time < controlLockUntil)
            {
                inputAxis = 0f;
                jumpHeld = false;
                return;
            }

            var kb = Keyboard.current;
            if (kb == null)
            {
                inputAxis = 0f;
                jumpHeld = false;
                return;
            }

            inputAxis = 0f;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) inputAxis -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) inputAxis += 1f;

            jumpHeld = kb.spaceKey.isPressed || kb.wKey.isPressed;
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

        // Gravedad dinámica: corta el salto si sueltas, flota en el apex y cae más rápido
        private void ApplyGravity()
        {
            float vy = rb.linearVelocity.y;

            if (IsGrounded && vy <= 0.01f)
            {
                rb.gravityScale = baseGravityScale;
                return;
            }

            float multiplier;
            if (vy > apexThreshold && !jumpHeld)
                multiplier = fallGravityMultiplier;   // subiendo sin mantener: corta el salto
            else if (Mathf.Abs(vy) <= apexThreshold)
                multiplier = apexGravityMultiplier;   // hang time en el apex
            else if (vy < 0f)
                multiplier = fallGravityMultiplier;   // cayendo: caída más pesada
            else
                multiplier = 1f;                      // subiendo con el botón pulsado

            rb.gravityScale = baseGravityScale * multiplier;

            // Limita la velocidad de caída para evitar tunelling y caídas injustas
            if (rb.linearVelocity.y < -maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }

        private void ApplyMovement()
        {
            float targetSpeed = inputAxis * inputScale * moveSpeed;
            float current = rb.linearVelocity.x;
            float diff = targetSpeed - current;

            float accel;
            if (Mathf.Abs(targetSpeed) < 0.01f)
            {
                // Sin input: frenar (en el aire se frena más despacio)
                accel = IsGrounded ? groundDecel : groundDecel * airControl;
            }
            else if (diff * current < -0.01f)
            {
                // Cambiando de dirección: el giro es el más rápido de todos
                accel = turnAccel * (IsGrounded ? 1f : airControl);
            }
            else
            {
                accel = (IsGrounded ? groundAccel : groundAccel * airControl);
            }

            float newX = Mathf.MoveTowards(current, targetSpeed, accel * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }

        private void TryJump()
        {
            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                coyoteCounter = 0f;
                jumpBufferCounter = 0f;
                jumpCutAvailable = true;
            }
            else if (jumpCutAvailable && !jumpHeld && rb.linearVelocity.y > 0f)
            {
                // Salto variable: al soltar se conserva solo parte de la velocidad
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                jumpCutAvailable = false;
            }
        }

        private void CheckGround()
        {
            Vector2 origin = groundCheck ? (Vector2)groundCheck.position : (Vector2)transform.position;
            IsGrounded = Physics2D.OverlapCircle(origin, groundCheckRadius, groundMask);
        }
    }
}
