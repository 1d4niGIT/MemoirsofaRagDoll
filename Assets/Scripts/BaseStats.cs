using UnityEngine;

public class BaseStats
{
    private int health;
    private float speed;
    private float cruchSpeed;
    private float levelOfAnxiety;
    private string name;

    public BaseStats(int _health, float _speed, float _cruchSpeed, float _levelOfAnxiety, string _name)
    {
        this.health = _health;
        this.speed = _speed;
        this.cruchSpeed = _cruchSpeed;
        this.levelOfAnxiety = _levelOfAnxiety;
        this.name = _name;
    }

    public int Health => health;
    public float Speed => speed;
    public float CruchSpeed => cruchSpeed;
    public float LevelOfAnxiety => levelOfAnxiety;
    public string Name => name;
}
