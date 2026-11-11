using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hideables
{
    /// <summary>
    /// Escondite generico. Implementa IHideable y lleva la cuenta de quien esta dentro.
    /// Sirve tal cual para el monton de ropa y el monton de peluches (solo cambia el arte).
    /// Los escondites con comportamiento propio (como la oveja) heredan y sobrescriben
    /// OnEntered / OnExited o agregan su propio Update (O de SOLID).
    ///
    /// Requisitos en Unity: un Collider2D (puede ser Trigger) en la capa "Hideable" y, si quieres,
    /// un hijo vacio "HidePoint" colocado donde debe quedar el jugador.
    /// </summary>
    public class HideableBase : MonoBehaviour, IHideable
    {
        [SerializeField] private Transform hidePoint; // opcional: si esta vacio se usa este mismo objeto

        public bool IsOccupied { get; private set; }
        public Transform HidePoint => hidePoint != null ? hidePoint : transform;

        protected GameObject Occupant { get; private set; }

        public virtual bool CanHide(GameObject hider) => !IsOccupied;

        public void Enter(GameObject hider)
        {
            Occupant = hider;
            IsOccupied = true;
            OnEntered(hider);
        }

        public void Exit(GameObject hider)
        {
            if (Occupant != hider) return;

            Occupant = null;
            IsOccupied = false;
            OnExited(hider);
        }

        /// <summary>Gancho para las clases hijas: algo ocurre cuando alguien entra.</summary>
        protected virtual void OnEntered(GameObject hider) { }

        /// <summary>Gancho para las clases hijas: algo ocurre cuando alguien sale.</summary>
        protected virtual void OnExited(GameObject hider) { }
    }
}
