using System;


public interface IStats
{
    int Health { get; }
    float Speed { get; }
    float CrouchSpeed { get; }
    float LevelOfAnxiety { get; }
    string Name { get; }
}

public interface IDamageable
{
    int MaxHealth { get; }
    int CurrentHealth { get; }
    bool IsDead { get; }

    event Action Died;

    void TakeDamage(int damage);
}

public interface IAttacker
{
    void Attack();
}

public interface ISpecialAttacker
{
    void SpecialAttack(int value);
}

// Todo lo que puede moverse.
public interface IMovable
{
    void Move();
}