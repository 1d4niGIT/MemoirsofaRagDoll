using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected int CurrentHealth;
    protected int MaxHealth;
    private BaseStats stats;
    private void Awake()
    {
        stats = new BaseStats(100, 5.0f, 2.5f, 0f, "Enemy");
        MaxHealth = stats.Health;
        CurrentHealth = MaxHealth;
    }
    protected virtual void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        Debug.Log("me golpearon");
        if (CurrentHealth <= 0)
        {
            OnDie();
        }
    }
    protected virtual void Attack()
    {

        Debug.Log("atacando");
    }
    protected virtual void Move()
    {
        Debug.Log("moviendome");
    }
    protected abstract void SpecialAttack(int value);
    protected virtual void OnDie()
    {
        Debug.Log("mori");
    }

}
