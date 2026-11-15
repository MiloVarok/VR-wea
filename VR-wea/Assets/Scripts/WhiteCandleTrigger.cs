using UnityEngine;

public class WhiteCandleTrigger : MonoBehaviour
{
    public GameObject candleFlame;
    public RitualManager ritualManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lighter") && ritualManager != null)
        {
            ritualManager.IgniteWhiteCandle(candleFlame);
        }
    }
}
