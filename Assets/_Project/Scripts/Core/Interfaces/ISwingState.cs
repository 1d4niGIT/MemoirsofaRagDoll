namespace RagDoll.Core
{
    /// <summary>
    /// Dice si el jugador esta colgado en el aire de su hilo. Asi otros sistemas (ej. el desgaste
    /// por falta de hilo) lo consultan sin conocer la clase SwingAbility.
    /// </summary>
    public interface ISwingState
    {
        bool IsSwinging { get; }
    }
}
