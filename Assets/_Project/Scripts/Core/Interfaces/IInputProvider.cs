namespace RagDoll.Core
{
    /// <summary>
    /// Contrato de entrada del jugador. PlayerController y las habilidades dependen de esta interfaz,
    /// no del teclado, asi puedes cambiar a mando, input falso para pruebas o IA (D de SOLID).
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>Movimiento horizontal: -1 (izquierda), 0 (quieto), 1 (derecha).</summary>
        float Horizontal { get; }

        /// <summary>Eje vertical: -1 (abajo, S), 0 (nada), 1 (arriba, W). Sirve para recoger o soltar el hilo.</summary>
        float Vertical { get; }

        /// <summary>True solo en el frame en que se presiona saltar.</summary>
        bool JumpPressed { get; }

        /// <summary>True mientras se mantiene saltar (sirve para planear).</summary>
        bool JumpHeld { get; }

        /// <summary>True mientras se mantiene agacharse.</summary>
        bool CrouchHeld { get; }

        /// <summary>True solo en el frame en que se presiona interactuar (esconderse).</summary>
        bool InteractPressed { get; }
    }
}

