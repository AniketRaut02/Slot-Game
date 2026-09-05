using System;
using UnityEngine;

namespace SlotGame.Events
{
    [CreateAssetMenu(fileName = "NewVoidEvent", menuName = "Slot Game/Events/Void Event")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public Action OnEventRaised;
        public void RaiseEvent() => OnEventRaised?.Invoke();
    }

}