using UnityEngine;

namespace RagDoll.Core
{
    /// <summary>
    /// Todo lo que puede ser empujado por un golpe. Las amenazas solo conocen esta interfaz,
    /// no al jugador (D de SOLID). Un enemigo o un objeto tambien podrian implementarla.
    /// </summary>
    public interface IKnockbackable
    {
        /// <param name="velocity">Velocidad del empujon (direccion y fuerza).</param>
        /// <param name="duration">Segundos que el empujon manda sobre el movimiento.</param>
        void ApplyKnockback(Vector2 velocity, float duration);
    }
}
