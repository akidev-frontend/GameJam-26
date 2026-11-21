using UnityEngine;

namespace GameJam.Level
{
    /// <summary>
    /// Levitación sencilla: oscila el objeto arriba y abajo con una onda sinusoidal.
    /// Si el objeto tiene Rigidbody2D (idealmente Kinematic) lo mueve con MovePosition
    /// para que la física lo siga; si no, mueve el Transform directamente.
    /// </summary>
    public class Levitate : MonoBehaviour
    {
        [Header("Ajustes")]
        [SerializeField] private float amplitude = 0.15f;
        [Tooltip("Ciclos completos (arriba+abajo) por segundo.")]
        [SerializeField] private float frequency = 1.2f;
        [Tooltip("Desfase para que varios objetos no se muevan al unísono.")]
        [SerializeField, Range(0f, 1f)] private float phase = 0f;
        [Tooltip("Añade una fase aleatoria al iniciar para desincronizar copias del mismo prefab.")]
        [SerializeField] private bool randomizePhase = true;

        private Vector2 basePosition;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            basePosition = rb ? rb.position : (Vector2)transform.position;
            if (randomizePhase)
                phase = Random.value;
        }

        private void Update()
        {
            if (rb) return; // lo mueve la física en FixedUpdate
            transform.position = Evaluated(Time.time);
        }

        private void FixedUpdate()
        {
            if (!rb) return;
            rb.MovePosition(Evaluated(Time.time));
        }

        // Posición en función del tiempo: seno centrado en la posición inicial
        private Vector2 Evaluated(float time)
        {
            float offset = Mathf.Sin((time * frequency + phase) * Mathf.PI * 2f) * amplitude;
            return basePosition + Vector2.up * offset;
        }
    }
}
