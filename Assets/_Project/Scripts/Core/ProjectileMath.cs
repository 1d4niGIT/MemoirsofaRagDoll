using UnityEngine;

namespace RagDoll.Core
{
    /// <summary>
    /// Calculo de trayectorias parabolicas. Es una clase estatica pura (sin estado, sin MonoBehaviour):
    /// solo recibe numeros y devuelve una posicion, asi se puede usar desde cualquier sistema.
    /// </summary>
    public static class ProjectileMath
    {
        /// <summary>
        /// Posicion de un proyectil en el instante "time" (segundos desde el lanzamiento).
        ///
        /// La formula escolar es: p = p0 + v*t + 0.5*g*t^2.
        /// Pero el motor de fisica 2D de Unity integra por pasos fijos (Euler semi-implicito:
        /// primero actualiza la velocidad y luego la posicion), lo que suma un termino extra:
        /// 0.5*g*dt*t. Lo incluimos para que la guia dibujada coincida con el recorrido REAL
        /// del alfiler, no solo con la teoria.
        /// </summary>
        /// <param name="gravity">Aceleracion de la gravedad ya multiplicada por el Gravity Scale (ej. Physics2D.gravity * 1).</param>
        public static Vector2 PositionAt(Vector2 origin, Vector2 velocity, Vector2 gravity, float time)
        {
            float dt = Time.fixedDeltaTime;
            return origin + velocity * time + gravity * (0.5f * time * time + 0.5f * dt * time);
        }
    }
}
