using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GameJam.UI
{
    /// <summary>
    /// Mueve la flecha del menú hacia el botón sobre el que esté el ratón.
    /// Detecta el hover comparando la posición del ratón (convertida a coordenadas
    /// de mundo) con los bounds del SpriteRenderer de cada botón.
    /// </summary>
    public class MenuArrowFollow : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("SpriteRenderers de los botones del menú (orden libre).")]
        [SerializeField] private SpriteRenderer[] buttons;
        [Tooltip("Índice del botón sobre el que arranca la flecha.")]
        [SerializeField] private int defaultIndex = 0;

        [Header("Movimiento")]
        [Tooltip("Fuerza la separación lateral de la flecha respecto al centro del botón (0 = mantener la inicial).")]
        [SerializeField] private float sideOffset = 0f;

        [Header("Al hacer clic")]
        [Tooltip("Parpadeos por segundo de la flecha mientras confirma el clic.")]
        [SerializeField] private float confirmBlinksPerSecond = 8f;
        [Tooltip("Segundos de parpadeo rápido antes de cambiar de escena.")]
        [SerializeField] private float confirmDuration = 1.4f;
        [Tooltip("Escena destino de cada botón (mismo orden que 'buttons'). Vacío = salir de la aplicación.")]
        [SerializeField] private string[] sceneDestinations;

        [Header("Fundido de salida")]
        [Tooltip("Duración del fundido a negro antes de cambiar de escena (0 = sin fundido).")]
        [SerializeField] private float fadeOutDuration = 0.8f;

        private bool confirming; // evita reaccionar a más clics durante la transición

        private Camera cam;
        private int currentIndex = -1;
        private Vector3 offset; // desplazamiento de la flecha respecto al botón seleccionado

        private void Awake()
        {
            cam = Camera.main;
            // Fallback por si la cámara no está etiquetada como MainCamera
            if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        }

        private void Start()
        {
            // Conserva el emplazamiento inicial de la flecha respecto a su botón por defecto,
            // así el arte queda donde el autor lo colocó aunque se mueva el botón.
            if (buttons == null || buttons.Length == 0) return;

            int idx = Mathf.Clamp(defaultIndex, 0, buttons.Length - 1);
            if (!buttons[idx]) return;

            currentIndex = idx;
            offset = transform.position - buttons[idx].transform.position;
            if (!Mathf.Approximately(sideOffset, 0f))
            {
                offset.x = sideOffset;
            }
        }

        private void Update()
        {
            if (buttons == null || buttons.Length == 0 || cam == null) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            // Ratón en pantalla -> mundo (cámara ortográfica: se proyecta al plano z=0)
            Vector2 screen = mouse.position.ReadValue();
            Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));

            int hovered = HoveredButton(mouseWorld);
            if (hovered >= 0)
            {
                currentIndex = hovered;
            }

            if (currentIndex >= 0 && buttons[currentIndex])
            {
                // Aparece directamente sobre el botón seleccionado, sin deslizarse
                transform.position = buttons[currentIndex].transform.position + offset;
            }

            // Clic confirmado sobre un botón: parpadeo rápido y transición
            if (!confirming && hovered >= 0 && mouse.leftButton.wasPressedThisFrame)
            {
                StartCoroutine(ConfirmSelection(hovered));
            }
        }

        // Parpadea rápido 'confirmDuration' segundos y luego carga la escena o cierra el juego
        private IEnumerator ConfirmSelection(int index)
        {
            confirming = true;
            var blink = GetComponent<Blink>();
            if (blink) blink.BlinkFast(confirmDuration, confirmBlinksPerSecond);

            yield return new WaitForSeconds(confirmDuration);

            // Fundido a negro antes de cambiar de escena (si hay overlay configurado)
            if (fadeOutDuration > 0f)
            {
                var fade = Object.FindFirstObjectByType<FadeIn>();
                if (fade) yield return StartCoroutine(fade.FadeOut(fadeOutDuration));
            }

            string destination = sceneDestinations != null && index < sceneDestinations.Length
                ? sceneDestinations[index]
                : null;

            if (!string.IsNullOrEmpty(destination))
            {
                SceneManager.LoadScene(destination);
            }
            else
            {
                Application.Quit();
            }
        }

        // Devuelve el índice del botón que contiene el punto, o -1 si ninguno
        private int HoveredButton(Vector3 point)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] && buttons[i].bounds.Contains(point)) return i;
            }
            return -1;
        }
    }
}
