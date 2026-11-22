using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace GameJam.UI
{
    /// <summary>
    /// Controlador de la flecha del menú: ratón + teclado.
    /// El ratón mueve la flecha al botón bajo el cursor y el clic lo confirma.
    /// Con el teclado, W/S (o flechas) mueven la selección y Enter confirma,
    /// ejecutando exactamente la misma acción que el clic.
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

        [Header("Teclado")]
        [Tooltip("Tecla para subir la selección.")]
        [SerializeField] private Key navUpKey = Key.W;
        [Tooltip("Tecla alternativa para subir la selección.")]
        [SerializeField] private Key navUpAltKey = Key.UpArrow;
        [Tooltip("Tecla para bajar la selección.")]
        [SerializeField] private Key navDownKey = Key.S;
        [Tooltip("Tecla alternativa para bajar la selección.")]
        [SerializeField] private Key navDownAltKey = Key.DownArrow;
        [Tooltip("Tecla para confirmar el botón seleccionado (misma acción que el clic).")]
        [SerializeField] private Key confirmKey = Key.Enter;

        private bool confirming; // evita reaccionar a más clics durante la transición

        private Camera cam;
        private int currentIndex = -1;
        private Vector3 offset; // desplazamiento de la flecha respecto al botón seleccionado
        private Vector2 lastMousePos; // para no pisar la selección de teclado con un ratón quieto

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

            // Punto de partida del ratón: evita que el primer frame cuente como movimiento
            var mouse = Mouse.current;
            if (mouse != null) lastMousePos = mouse.position.ReadValue();
        }

        private void Update()
        {
            if (buttons == null || buttons.Length == 0 || cam == null) return;

            if (!confirming) HandleKeyboard();

            var mouse = Mouse.current;
            int hovered = -1;

            if (mouse != null)
            {
                // Ratón en pantalla -> mundo (cámara ortográfica: se proyecta al plano z=0)
                Vector2 screen = mouse.position.ReadValue();
                bool mouseMoved = (screen - lastMousePos).sqrMagnitude > 0.01f;
                lastMousePos = screen;

                Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
                hovered = HoveredButton(mouseWorld);

                // Solo el ratón que se mueve cambia la selección: así el teclado no queda pisado
                if (!confirming && mouseMoved && hovered >= 0)
                {
                    currentIndex = hovered;
                }
            }

            UpdateArrowPosition();

            // Clic confirmado sobre un botón: parpadeo rápido y transición
            if (!confirming && hovered >= 0 && mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                currentIndex = hovered;
                StartCoroutine(ConfirmSelection(hovered));
            }
        }

        // W/S o flechas mueven la selección; Enter confirma (misma ruta que el clic)
        private void HandleKeyboard()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (WasPressed(keyboard, navUpKey) || WasPressed(keyboard, navUpAltKey))
            {
                MoveSelection(-1);
            }
            else if (WasPressed(keyboard, navDownKey) || WasPressed(keyboard, navDownAltKey))
            {
                MoveSelection(1);
            }

            bool confirmPressed = WasPressed(keyboard, confirmKey) ||
                                  (confirmKey != Key.NumpadEnter && WasPressed(keyboard, Key.NumpadEnter));

            if (confirmPressed && currentIndex >= 0 && currentIndex < buttons.Length)
            {
                StartCoroutine(ConfirmSelection(currentIndex));
            }
        }

        // Cambia el índice seleccionado con vuelta circular
        private void MoveSelection(int delta)
        {
            int count = buttons.Length;
            currentIndex = ((currentIndex + delta) % count + count) % count;
        }

        private static bool WasPressed(Keyboard keyboard, Key key)
        {
            return key != Key.None && keyboard[key].wasPressedThisFrame;
        }

        private void UpdateArrowPosition()
        {
            if (currentIndex >= 0 && currentIndex < buttons.Length && buttons[currentIndex])
            {
                // Aparece directamente sobre el botón seleccionado, sin deslizarse
                transform.position = buttons[currentIndex].transform.position + offset;
            }
        }

        // Parpadea rápido 'confirmDuration' segundos y luego carga la escena o cierra el juego
        private IEnumerator ConfirmSelection(int index)
        {
            confirming = true;
            var blink = GetComponent<Blink>();
            if (blink) blink.BlinkFast(confirmDuration, confirmBlinksPerSecond);

            yield return new WaitForSeconds(confirmDuration);

            string destination = sceneDestinations != null && index < sceneDestinations.Length
                ? sceneDestinations[index]
                : null;

            // Sin destino: cerrar directamente, sin fundido ni esperas
            if (string.IsNullOrEmpty(destination))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                yield break;
            }

            // Fundido a negro solo al cambiar de escena
            if (fadeOutDuration > 0f)
            {
                var fade = Object.FindFirstObjectByType<FadeIn>();
                if (fade) yield return StartCoroutine(fade.FadeOut(fadeOutDuration));
            }

            SceneManager.LoadScene(destination);
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
