using System.Collections.Generic;
using UnityEngine;
using SlotGame.Core;
using SlotGame.Events;
using SlotGame.Reels;

namespace SlotGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private SFXLibrarySO sfxLibrary;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource spinLoopSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Scene References")]
        [SerializeField] private List<ReelController> reels;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO spinRequestedEvent;
        [SerializeField] private WinResultEventChannelSO winEvaluatedEvent;
        [SerializeField] private VoidEventChannelSO bonusTriggeredEvent;
        [SerializeField] private VoidEventChannelSO bonusEndedEvent;

        private int activeSpinningReels = 0;

        private void Start()
        {
            if (sfxLibrary.baseMusic != null)
            {
                musicSource.clip = sfxLibrary.baseMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        private void OnEnable()
        {
            if (spinRequestedEvent != null) spinRequestedEvent.OnEventRaised += HandleSpinRequested;
            if (winEvaluatedEvent != null) winEvaluatedEvent.OnEventRaised += HandleWinEvaluated;
            if (bonusTriggeredEvent != null) bonusTriggeredEvent.OnEventRaised += HandleBonusTriggered;
            if (bonusEndedEvent != null) bonusEndedEvent.OnEventRaised += HandleBonusEnded;

            foreach (ReelController reel in reels)
            {
                reel.OnReelSnapped += HandleReelSnapped;
            }
        }

        private void OnDisable()
        {
            if (spinRequestedEvent != null) spinRequestedEvent.OnEventRaised -= HandleSpinRequested;
            if (winEvaluatedEvent != null) winEvaluatedEvent.OnEventRaised -= HandleWinEvaluated;
            if (bonusTriggeredEvent != null) bonusTriggeredEvent.OnEventRaised -= HandleBonusTriggered;
            if (bonusEndedEvent != null) bonusEndedEvent.OnEventRaised -= HandleBonusEnded;

            foreach (ReelController reel in reels)
            {
                reel.OnReelSnapped -= HandleReelSnapped;
            }
        }

        private void HandleSpinRequested()
        {
            activeSpinningReels = reels.Count;

            if (sfxLibrary.spinLoop != null)
            {
                spinLoopSource.clip = sfxLibrary.spinLoop;
                spinLoopSource.volume = 1f;
                spinLoopSource.loop = true;
                spinLoopSource.Play();
            }
        }

        private void HandleReelSnapped(ReelController reel, Data.SymbolDefinitionSO symbol)
        {
            activeSpinningReels--;

            // Play the thump
            PlayOneShot(sfxLibrary.reelStop);

            // Fade out the spin loop as reels stop
            if (activeSpinningReels <= 0)
            {
                spinLoopSource.Stop();
            }
            else
            {
                spinLoopSource.volume = (float)activeSpinningReels / reels.Count;
            }
        }

        private void HandleWinEvaluated(WinResult result)
        {
            if (result.isWin)
            {
                PlayOneShot(sfxLibrary.winJingle);
            }
        }

        private void HandleBonusTriggered()
        {
            musicSource.clip = sfxLibrary.bonusMusic;
            musicSource.Play();
        }

        private void HandleBonusEnded()
        {
            musicSource.clip = sfxLibrary.baseMusic;
            musicSource.Play();
        }

        // --- Public API for UI and tight loops ---

        public void PlayButtonClick()
        {
            PlayOneShot(sfxLibrary.buttonClick);
        }

        public void PlayCoinTick()
        {
            // using pitch variation so the rapid ticking doesn't sound robotic
            if (sfxLibrary.coinTick != null)
            {
                sfxSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(sfxLibrary.coinTick);
                sfxSource.pitch = 1f; // reset for next standard SFX
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}