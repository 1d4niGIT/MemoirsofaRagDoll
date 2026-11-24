namespace RagDoll.Core
{
    /// <summary>
    /// Quien puede mostrar que recibio un golpe SIN moverse de verdad (sacudida visual).
    /// La amenaza solo conoce esta interfaz, no al jugador ni a su sprite.
    /// </summary>
    public interface IHitReactable
    {
        void ReactToHit();
    }
}
