namespace RagDoll.Core
{
    /// <summary>
    /// Informa si quien la implementa esta agachado. Igual que ISwingState: las zonas y escenas
    /// solo conocen esta interfaz, no a CrouchAbility ni al jugador.
    /// </summary>
    public interface ICrouchState
    {
        bool IsCrouching { get; }
    }
}
