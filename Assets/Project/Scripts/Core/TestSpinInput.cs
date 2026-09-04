using UnityEngine;
using SlotGame.Events;

public class TestSpinInput : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO spinEvent;

    private void Update()
    {

           

    }

    public void Test()
    {
        spinEvent?.RaiseEvent();
    }
}