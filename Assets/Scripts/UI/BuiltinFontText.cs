using UnityEngine;

namespace GameJam.UI
{
    /// <summary>
    /// Asigna la fuente built-in a un TextMesh (sin fuente no se renderiza nada).
    /// Se ejecuta también en el editor para previsualizar el texto en la escena.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TextMesh))]
    public class BuiltinFontText : MonoBehaviour
    {
        private void OnEnable()
        {
            var tm = GetComponent<TextMesh>();
            var renderer = GetComponent<MeshRenderer>();
            if (tm.font == null)
                tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // El material del renderer debe ser siempre el de la fuente,
            // si no, el texto no se pinta (queda con el material por defecto).
            if (tm.font != null && renderer.sharedMaterial != tm.font.material)
                renderer.sharedMaterial = tm.font.material;
        }
    }
}