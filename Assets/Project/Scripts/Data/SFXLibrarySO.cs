using UnityEngine;

namespace SlotGame.Audio
{
    [CreateAssetMenu(fileName = "NewSFXLibrary", menuName = "Slot Game/Audio/SFX Library")]
    public class SFXLibrarySO : ScriptableObject
    {
        [Header("Music")]
        public AudioClip baseMusic;
        public AudioClip bonusMusic;

        [Header("Sound Effects")]
        public AudioClip spinLoop;
        public AudioClip reelStop;
        public AudioClip winJingle;
        public AudioClip buttonClick;
        public AudioClip leverPull;
        public AudioClip coinTick;
    }
}