namespace RagDoll.Hazards
{
    /// <summary>
    /// Aguja peligrosa. El golpe (dano + empujon) viene de KnockbackHazard; su diferencia con la
    /// penca es el valor de Damage en el Inspector (mayor). Con el sistema de hilo, el dano gasta
    /// hilo primero y solo baja el relleno cuando Mino ya no tiene hilo (lo decide PlayerHealth).
    /// </summary>
    public class NeedleHazard : KnockbackHazard { }
}