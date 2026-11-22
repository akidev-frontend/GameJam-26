using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GameJam.EditorTools
{
    /// <summary>
    /// Configuración de una sola vez para la animación de muerte:
    /// crea el clip RataMuerte desde la hoja muerte.png y añade el estado
    /// "Muerte" al PlayerRata.controller (sin tocar el estado Walk).
    /// Se reproduce con Animator.Play("Muerte") desde PlayerAnimator2D.
    /// </summary>
    public static class MuerteAnimationSetup
    {
        private const string SheetPath = "Assets/Art/Sprites/muerte.png";
        private const string ClipPath = "Assets/Animations/Player/RataMuerte.anim";
        private const string ControllerPath = "Assets/Animators/PlayerRata.controller";

        private const string XuxoSheetPath = "Assets/Art/Sprites/Rata/rata_xuxo.png";
        private const string XuxoClipPath = "Assets/Animations/Player/RataXuxo.anim";

        [MenuItem("Tools/MCP/Setup Muerte Animation")]
        public static void Setup()
        {
            // 1. Frames de la hoja de sprites (sub-assets _0.._7, ordenados por nombre)
            var sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();
            if (sprites.Length == 0)
            {
                Debug.LogError("[MuerteAnimationSetup] No hay sprites en " + SheetPath);
                return;
            }

            // 2. Clip de muerte: intercambio de sprites a 12 fps, SIN bucle
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath) != null)
                AssetDatabase.DeleteAsset(ClipPath);

            var clip = new AnimationClip { frameRate = 12 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            float step = 1f / clip.frameRate;
            // Última clave extra para sostener el último frame un tic completo
            var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
            for (int i = 0; i < sprites.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i * step, value = sprites[i] };
            keys[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length * step, value = sprites[sprites.Length - 1] };

            AnimationUtility.SetObjectReferenceCurve(
                clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            AssetDatabase.CreateAsset(clip, ClipPath);

            // 3. Añadir el estado "Muerte" al controller existente (Walk queda intacto)
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError("[MuerteAnimationSetup] No se encontró " + ControllerPath);
                return;
            }
            var stateMachine = controller.layers[0].stateMachine;
            var muerte = stateMachine.states.FirstOrDefault(s => s.state.name == "Muerte").state;
            if (muerte == null)
                muerte = stateMachine.AddState("Muerte");
            muerte.motion = clip;
            stateMachine.defaultState = stateMachine.states.First(s => s.state.name == "Walk").state;

            AssetDatabase.SaveAssets();
            Debug.Log($"[MuerteAnimationSetup] OK: {sprites.Length} frames, clip {clip.length:F2}s, estado 'Muerte' añadido a {controller.name}.");
        }

        /// <summary>
        /// Animación de celebración al comer un xuixo: clip RataXuxo (3 frames
        /// de rata_xuxo.png a 6 fps, sin bucle) y estado "Xuxo" en el controller.
        /// Se reproduce con Animator.Play("Xuxo") desde PlayerAnimator2D.PlayXuxo().
        /// </summary>
        [MenuItem("Tools/MCP/Setup RataXuxo Animation")]
        public static void SetupXuxo()
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(XuxoSheetPath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();
            if (sprites.Length == 0)
            {
                Debug.LogError("[MuerteAnimationSetup] No hay sprites en " + XuxoSheetPath);
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(XuxoClipPath) != null)
                AssetDatabase.DeleteAsset(XuxoClipPath);

            var clip = new AnimationClip { frameRate = 6 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            float step = 1f / clip.frameRate;
            var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
            for (int i = 0; i < sprites.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i * step, value = sprites[i] };
            keys[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length * step, value = sprites[sprites.Length - 1] };

            AnimationUtility.SetObjectReferenceCurve(
                clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            AssetDatabase.CreateAsset(clip, XuxoClipPath);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError("[MuerteAnimationSetup] No se encontró " + ControllerPath);
                return;
            }
            var stateMachine = controller.layers[0].stateMachine;
            var xuxo = stateMachine.states.FirstOrDefault(s => s.state.name == "Xuxo").state;
            if (xuxo == null)
                xuxo = stateMachine.AddState("Xuxo");
            xuxo.motion = clip;

            AssetDatabase.SaveAssets();
            Debug.Log($"[MuerteAnimationSetup] OK Xuxo: {sprites.Length} frames, clip {clip.length:F2}s, estado 'Xuxo' añadido a {controller.name}.");
        }

    }
}
