namespace RagDoll.Core
{
    /// <summary>
    /// Contrato de las habilidades del jugador (saltar, planear, agacharse, balancearse, esconderse).
    /// PlayerController guarda una lista de IAbility y las recorre sin saber cuales son:
    /// agregar una habilidad nueva = crear una clase nueva, sin tocar PlayerController (O de SOLID).
    /// </summary>
    public interface IAbility
    {
        /// <summary>True mientras la habilidad esta en uso.</summary>
        bool IsActive { get; }

        /// <summary>Se llama en Update: aqui la habilidad lee el input y decide si se activa.</summary>
        void HandleInput(IInputProvider input);

        /// <summary>Se llama en FixedUpdate: aqui va la logica de fisicas 2D.</summary>
        void FixedTick();

        /// <summary>Corta la habilidad de inmediato (por ejemplo, al recibir dano o morir).</summary>
        void Cancel();
    }
}
