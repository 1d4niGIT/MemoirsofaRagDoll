namespace RagDoll.Core
{
    /// <summary>
    /// Todo lo que puede morir de golpe, ignorando vida e invulnerabilidad
    /// (por ejemplo, Mino al quedarse sin hilo en pleno balanceo).
    /// Es distinto de IDamageable: dano se puede esquivar o amortiguar, esto no.
    /// </summary>
    public interface IKillable
    {
        void Kill();
    }
}
