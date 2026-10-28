using UnityEngine;

// placeholder, auto-passes
// Replace with minigame later
public class FishingMinigame : MonoBehaviour
{
    public bool IsRunning { get; private set; }

    public void Begin(
        FishData fishData,
        FishingStats fishingStats,
        System.Func<bool> reelHeld,
        System.Action<bool> finished)
    {
        Debug.Log($"[Placeholder Minigame] Auto-catching {fishData.fishName}.");

        IsRunning = false;
        finished?.Invoke(true);
    }

    public void Cancel()
    {
        IsRunning = false;
    }
}