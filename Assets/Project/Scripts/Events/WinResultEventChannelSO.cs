using System;
using UnityEngine;
using SlotGame.Core;

namespace SlotGame.Events
{
    [CreateAssetMenu(fileName = "NewWinResultEvent", menuName = "Slot Game/Events/Win Result Event")]
    public class WinResultEventChannelSO : ScriptableObject
    {
        public Action<WinResult> OnEventRaised;
        public void RaiseEvent(WinResult result) => OnEventRaised?.Invoke(result);
    }
}