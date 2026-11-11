using UnityEngine;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Primera amenaza: tijeras que alternan entre un tiempo seguro (abiertas) y un tiempo
    /// peligroso (cerrandose). Solo hacen dano en la fase peligrosa.
    /// Es polimorfismo aplicado a gameplay: la logica de dano viene de HazardBase y esta clase
    /// solo cambia CUANDO puede golpear.
    /// </summary>
    public class ScissorsHazard : HazardBase
    {
        [SerializeField] private float safeTime = 1.5f;
        [SerializeField] private float dangerTime = 0.6f;

        [Header("Ayuda visual de prueba (opcional)")]
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Color dangerColor = Color.red;

        private bool isDangerous;
        private float nextToggleTime;
        private Color baseColor = Color.white;

        private void Start()
        {
            if (sprite != null) baseColor = sprite.color;
            SetDangerous(false);
        }

        private void Update()
        {
            if (Time.time >= nextToggleTime)
                SetDangerous(!isDangerous);
        }

        private void SetDangerous(bool value)
        {
            isDangerous = value;
            nextToggleTime = Time.time + (value ? dangerTime : safeTime);

            if (sprite != null)
                sprite.color = value ? dangerColor : baseColor;
        }

        protected override bool CanHit() => isDangerous;
    }
}
