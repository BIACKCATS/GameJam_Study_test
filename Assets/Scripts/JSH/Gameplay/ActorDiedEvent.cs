using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Global death notice (score, kill count, wave progress, quest...).
    /// Usage: EventBus&lt;ActorDiedEvent&gt;.Subscribe(OnActorDied);
    /// </summary>
    public readonly struct ActorDiedEvent
    {
        public readonly Health Victim;
        public readonly GameObject Killer;

        public ActorDiedEvent(Health victim, GameObject killer)
        {
            Victim = victim;
            Killer = killer;
        }
    }
}
