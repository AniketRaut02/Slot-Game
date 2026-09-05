using UnityEngine;
using SlotGame.Events;

namespace SlotGame.Presentation
{
    public class LeverAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator leverAnimator;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO spinRequestedEvent;

        // Caching the string as a hash for clean, performant animator calls
        private readonly int pulledHash = Animator.StringToHash("pulled");

        private void OnEnable()
        {
            if (spinRequestedEvent != null)
            {
                spinRequestedEvent.OnEventRaised += PlayPullAnimation;
            }
        }

        private void OnDisable()
        {
            if (spinRequestedEvent != null)
            {
                spinRequestedEvent.OnEventRaised -= PlayPullAnimation;
            }
        }

        private void PlayPullAnimation()
        {
            if (leverAnimator != null)
            {
                leverAnimator.SetTrigger(pulledHash);
            }
        }
    }
}