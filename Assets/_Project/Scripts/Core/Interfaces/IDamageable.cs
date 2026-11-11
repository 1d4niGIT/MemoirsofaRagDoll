namespace RagDoll.Core
{
    /// <summary>
    /// Todo lo que pueda recibir dano (jugador, y a futuro enemigos o cosas destruibles).
    /// Las amenazas (tijeras, cactus, guillotina) solo conocen esta interfaz,
    /// nunca la clase PlayerHealth (D de SOLID).
    /// </summary>
    public interface IDamageable
    {
        /// <summary>False cuando se quedo sin hilos/relleno.</summary>
        bool IsAlive { get; }

        /// <summary>Aplica dano. La implementacion decide que hacer (restar vida, invulnerabilidad, etc.).</summary>
        void TakeDamage(int amount);
    }
}
