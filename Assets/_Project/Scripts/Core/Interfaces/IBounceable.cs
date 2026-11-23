namespace RagDoll.Core
{
    /// <summary>
    /// Todo lo que puede rebotar en una superficie elastica (cama, trampolin...).
    /// La superficie solo conoce esta interfaz, no al jugador (D de SOLID).
    /// </summary>
    public interface IBounceable
    {
        /// <param name="upwardSpeed">Velocidad vertical hacia arriba con la que sale despedido.</param>
        void Bounce(float upwardSpeed);
    }
}
