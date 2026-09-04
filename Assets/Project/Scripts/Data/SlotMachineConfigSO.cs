using UnityEngine;

namespace SlotGame.Data
{
    [CreateAssetMenu(fileName = "SlotMachineConfig", menuName = "Slot Game/Data/Machine Config")]
    public class SlotMachineConfigSO : ScriptableObject
    {
        [Header("Grid Configuration")]
        [SerializeField, Min(1)] private int reelCount = 3;
        [SerializeField, Min(1)] private int visibleRows = 3;

        [Header("Spin Timings")]
        [SerializeField, Min(0.1f)] private float minSpinDuration = 2.0f;
        [SerializeField, Min(0.1f)] private float maxSpinDuration = 3.0f;
        [SerializeField, Min(0f)] private float perReelStopStagger = 0.15f;

        [Header("Economy")]
        [SerializeField, Min(1)] private int minBet = 10;
        [SerializeField, Min(1)] private int maxBet = 100;
        [SerializeField, Min(1)] private int defaultBet = 10;

        public int ReelCount => reelCount;
        public int VisibleRows => visibleRows;
        public float MinSpinDuration => minSpinDuration;
        public float MaxSpinDuration => maxSpinDuration;
        public float PerReelStopStagger => perReelStopStagger;
        public int MinBet => minBet;
        public int MaxBet => maxBet;
        public int DefaultBet => defaultBet;
    }
}