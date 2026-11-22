using System.Collections;
using UnityEngine;

namespace GameJam.Level
{
    /// <summary>
    /// Flujo compartido de muerte + respawn: congela al jugador (control y
    /// física), reproduce la animación de muerte, espera, teletransporta al
    /// spawn y devuelve el control. Lo usan Hazard (helados) y XuxoPickup
    /// (xuixo malo).
    /// </summary>
    public static class PlayerDeathFlow
    {
        public static IEnumerator Run(
            Transform victim,
            GameJam.Player.PlayerController2D controller,
            GameJam.Player.PlayerAnimator2D animator,
            Rigidbody2D rb,
            Vector3 target,
            float deathDuration,
            float respawnFreeze)
        {
            // Si ya está muerto (p. ej. dos hazards a la vez) no encadenamos otra
            // muerte: antes podía quedar con los controles bloqueados para siempre.
            if (controller != null)
            {
                if (!controller.BeginDeath())
                    yield break;
            }
            else if (rb != null)
            {
                // Sin controller, al menos que no siga cayendo ni deslizándose
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            if (animator) animator.PlayDeath();
            if (controller) controller.PlayDeathSound();

            if (deathDuration > 0f)
                yield return new WaitForSeconds(deathDuration);

            if (controller != null)
            {
                // Teletransporta, reactiva la física limpia y deja un freeze de control
                controller.ReviveAt(target, respawnFreeze);
            }
            else
            {
                if (rb != null)
                {
                    rb.position = target;
                    rb.linearVelocity = Vector2.zero;
                    rb.simulated = true;
                }
                if (victim) victim.position = target;
            }

            if (animator) animator.OnRespawned();
        }
    }
}
