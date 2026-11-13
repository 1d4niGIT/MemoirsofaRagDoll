using UnityEngine;

namespace RagDoll.Player
{
    /// <summary>
    /// Calculo del apuntado tipo honda. Es una clase C# normal (no MonoBehaviour): se crea con
    /// "new" y recibe su configuracion por constructor. No lee input ni dispara nada: solo
    /// convierte "donde esta Mino" y "donde esta el puntero" en direccion y fuerza.
    ///
    /// El arrastre se mide DESDE EL CENTRO DE MINO hasta el puntero:
    /// - Direccion: contraria al arrastre (puntero a la izquierda, el alfiler sale a la derecha).
    /// - Fuerza: de 0 a 1 segun la distancia, hasta el radio maximo (la cupula).
    ///   Con el puntero sobre Mino la fuerza es ~0.
    ///
    /// Como el ancla es Mino, la camara puede moverse sin que cambie el apuntado.
    /// </summary>
    public class SlingshotAim
    {
        private readonly float maxDragDistance;

        public bool IsAiming { get; private set; }
        public float Power01 { get; private set; }
        public Vector2 LaunchDirection { get; private set; }

        /// <summary>Arrastre limitado a la cupula: va desde Mino hasta el puntero (para dibujar la linea).</summary>
        public Vector2 DragOffset { get; private set; }

        /// <param name="maxDragDistance">Radio maximo de arrastre, en unidades del mundo (la cupula).</param>
        public SlingshotAim(float maxDragDistance)
        {
            this.maxDragDistance = Mathf.Max(0.1f, maxDragDistance);
        }

        public void Begin()
        {
            IsAiming = true;
            Clear();
        }

        public void UpdateAim(Vector2 anchorWorld, Vector2 pointerWorld)
        {
            if (!IsAiming) return;

            Vector2 drag = pointerWorld - anchorWorld;

            DragOffset = Vector2.ClampMagnitude(drag, maxDragDistance);
            Power01 = DragOffset.magnitude / maxDragDistance;
            LaunchDirection = drag.sqrMagnitude > 0.0001f ? -drag.normalized : Vector2.zero;
        }

        public void End()
        {
            IsAiming = false;
            Clear();
        }

        private void Clear()
        {
            Power01 = 0f;
            LaunchDirection = Vector2.zero;
            DragOffset = Vector2.zero;
        }
    }
}
