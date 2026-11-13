using UnityEngine;
using RagDoll.Player;

namespace RagDoll.Pickups
{
    /// <summary>
    /// Ovillo de lana o madeja de pabilo: devuelve hilo al tocarlo.
    /// Es UN solo script; la diferencia entre un ovillo y una madeja es solo el valor de "Amount"
    /// (ejemplo: ovillo = 30, madeja = 60) y el arte.
    /// Si Mino tiene el hilo lleno, el objeto no se gasta y queda para despues.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ThreadPickup : MonoBehaviour
    {
        [SerializeField] private float amount = 30f;

        // Deja el collider como trigger al agregar el componente en el editor.
        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other) => TryCollect(other);

        // Si Mino estaba lleno al tocarlo, lo recoge cuando gaste hilo sin tener que salir y volver a entrar.
        private void OnTriggerStay2D(Collider2D other) => TryCollect(other);

        private void TryCollect(Collider2D other)
        {
            ThreadSupply supply = other.GetComponentInParent<ThreadSupply>();
            if (supply == null) return;

            if (supply.Add(amount))
                gameObject.SetActive(false);
        }
    }
}
