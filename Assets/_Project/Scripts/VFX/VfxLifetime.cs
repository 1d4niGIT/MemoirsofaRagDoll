using UnityEngine;

namespace RagDoll.VFX
{
    /// <summary>
    /// Destruye el efecto cuando termina su animacion. Va en el prefab del VFX.
    /// "lifetime" debe ser igual o un poco mayor que la duracion del clip (ej. 7 frames a 12 fps = 0.58 s).
    /// </summary>
    public class VfxLifetime : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.6f;

        private void OnEnable() => Destroy(gameObject, lifetime);
    }
}
