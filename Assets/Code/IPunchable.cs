using UnityEngine;

namespace Gnome
{
    public interface IPunchable
    {
        Vector2 Position { get; }
        int Priority { get; }
        float Radius { get; }
        void TakeHit(Vector2 hitDirection);
        bool IsDead { get; }
    }
}