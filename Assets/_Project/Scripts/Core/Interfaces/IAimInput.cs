using UnityEngine;

namespace RagDoll.Core
{
    /// <summary>
    /// Entrada de apuntado tipo honda (clic, arrastrar, soltar). Es una interfaz APARTE de
    /// IInputProvider a proposito (I de SOLID): solo las habilidades que apuntan la necesitan.
    /// PlayerInput implementa las dos interfaces.
    /// </summary>
    public interface IAimInput
    {
        /// <summary>True solo en el frame en que se presiona el boton de apuntar (clic).</summary>
        bool AimPressed { get; }

        /// <summary>True mientras se mantiene el boton.</summary>
        bool AimHeld { get; }

        /// <summary>True solo en el frame en que se suelta el boton.</summary>
        bool AimReleased { get; }

        /// <summary>Posicion del puntero en pixeles de pantalla.</summary>
        Vector2 PointerScreenPosition { get; }
    }
}
