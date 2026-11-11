using UnityEngine;

namespace RagDoll.Core
{
    /// <summary>
    /// Entidades de lana donde el jugador puede esconderse (ovejas, monton de ropa, peluches).
    /// HideAbility trabaja contra esta interfaz, por eso agregar un escondite nuevo
    /// no requiere modificar la habilidad (O de SOLID).
    /// </summary>
    public interface IHideable
    {
        /// <summary>True si ya hay alguien escondido aqui.</summary>
        bool IsOccupied { get; }

        /// <summary>Posicion donde se coloca al jugador mientras esta escondido.</summary>
        Transform HidePoint { get; }

        /// <summary>Indica si este escondite acepta al que lo intenta usar.</summary>
        bool CanHide(GameObject hider);

        /// <summary>El jugador entra al escondite.</summary>
        void Enter(GameObject hider);

        /// <summary>El jugador sale del escondite.</summary>
        void Exit(GameObject hider);
    }
}
