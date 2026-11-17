namespace RagDoll.Core
{
    /// <summary>
    /// Todo lo que tiene hilo y se lo pueden gastar a la fuerza (ej. una aguja enemiga).
    /// Las amenazas solo conocen esta interfaz, no a ThreadSupply (D de SOLID).
    /// </summary>
    public interface IThreadDrainable
    {
        void Drain(float amount);
    }
}
