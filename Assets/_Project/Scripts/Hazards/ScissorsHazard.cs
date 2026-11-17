using UnityEngine;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Primera amenaza: tijeras que alternan entre un tiempo seguro (abiertas) y un tiempo
    /// peligroso (cerrandose). Solo hacen dano en la fase peligrosa.
    /// Es polimorfismo aplicado a gameplay: el dano y el empujon vienen de KnockbackHazard y esta
    /// clase solo cambia CUANDO puede golpear.
    /// </summary>
    public class ScissorsHazard : KnockbackHazard
    {
        [SerializeField] private float safeTime = 1.5f;
        [SerializeField] private float dangerTime = 0.6f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Sprite openSprite;     // tijera abierta (fase segura)
        [SerializeField] private Sprite closedSprite;   // tijera cerrada (fase peligrosa)

        [Header("Ayuda visual de prueba (opcional)")]
        [SerializeField] private bool tintWhenDangerous = false;
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

            if (sprite == null) return;

            // Cambia el dibujo segun la fase (si se asignaron los dos sprites).
            if (openSprite != null && closedSprite != null)
                sprite.sprite = value ? closedSprite : openSprite;

            if (tintWhenDangerous)
                sprite.color = value ? dangerColor : baseColor;
        }

        // Solo golpea en la fase peligrosa Y respetando el intervalo de KnockbackHazard.
        protected override bool CanHit() => isDangerous && base.CanHit();
    }
}