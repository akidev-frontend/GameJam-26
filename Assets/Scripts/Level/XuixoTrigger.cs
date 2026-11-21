using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameJam.Level
{
    /// <summary>
    /// Trigger colocado sobre xuixo_plat_0 (Level1). Cuando el Player entra en
    /// la hitbox (BoxCollider2D en modo trigger) ejecuta una serie de
    /// acontecimientos: diálogo opcional, activar/desactivar objetos y un
    /// UnityEvent que se puede cablear desde el Inspector.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class XuixoTrigger : MonoBehaviour
    {
        [Header("Referencias opcionales")]
        [SerializeField] private GameJam.UI.DialogueSystem dialogue;
        [SerializeField, TextArea] private string[] dialogueLines =
        {
            "¡Regla rota!",
            "A partir de ahora todo cambia.",
        };

        [Header("Objetos")]
        [SerializeField] private GameObject[] activateOnTouch;
        [SerializeField] private GameObject[] deactivateOnTouch;

        [Header("Evento")]
        [Tooltip("Cableá aquí los acontecimientos extra del nivel (cámara, reglas, sonido, etc.).")]
        public UnityEvent onPlayerTouch;

        private bool triggered;

        private void Awake()
        {
            // Si no se asignó diálogo desde el Inspector, busca uno en la escena.
            if (!dialogue)
                dialogue = UnityEngine.Object.FindFirstObjectByType<GameJam.UI.DialogueSystem>();
        }

        private void Reset()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col) col.isTrigger = true;
        }

        private void OnValidate()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (!other.GetComponent<GameJam.Player.PlayerController2D>()) return;

            triggered = true;

            if (dialogue != null)
                dialogue.Play(dialogueLines);

            foreach (var go in activateOnTouch)
                if (go) go.SetActive(true);
            foreach (var go in deactivateOnTouch)
                if (go) go.SetActive(false);

            onPlayerTouch.Invoke();
        }
    }
}
