using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameJam.UI
{
    /// <summary>
    /// Punto de partida del menú de inicio (el compañero de UI puede ampliarlo).
    /// Engancha automáticamente los botones "JugarButton" y "SalirButton" si existen,
    /// pero también expone métodos públicos por si prefiere cablearse desde el inspector.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private string firstLevelScene = "Level1";

        [SerializeField] private Button jugarButton;
        [SerializeField] private Button salirButton;

        private void Awake()
        {
            AsignarFuentePorDefecto();

            // Auto-cableado de los botones si no se asignaron en el inspector
            if (jugarButton == null) jugarButton = GameObject.Find("JugarButton")?.GetComponent<Button>();
            if (salirButton == null) salirButton = GameObject.Find("SalirButton")?.GetComponent<Button>();

            if (jugarButton != null) jugarButton.onClick.AddListener(Jugar);
            if (salirButton != null) salirButton.onClick.AddListener(Salir);
        }

        private void OnDestroy()
        {
            if (jugarButton != null) jugarButton.onClick.RemoveListener(Jugar);
            if (salirButton != null) salirButton.onClick.RemoveListener(Salir);
        }

        // El proyecto aún no tiene fuentes importadas: asigna la fuente built-in
        // para que los textos legacy se rendericen sin configurar nada.
        // Cuando se importe una fuente (p. ej. pixel art), se puede reemplazar en el inspector.
        private void AsignarFuentePorDefecto()
        {
            foreach (var texto in GetComponentsInChildren<Text>(true))
            {
                if (texto.font == null)
                    texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        // Llamado por el botón "Jugar"
        public void Jugar()
        {
            SceneManager.LoadScene(firstLevelScene);
        }

        // Llamado por el botón "Salir"
        public void Salir()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
