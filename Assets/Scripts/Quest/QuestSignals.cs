using System;

/// <summary>
/// Session-only quest completion flags (not persisted).
/// </summary>
public static class QuestSignals
{
    public static bool BlueDaoVisited { get; private set; }
    public static bool BoundaryLoreComplete { get; private set; }

    public static event Action OnChanged;

    public static void MarkBlueDaoVisited()
    {
        if (BlueDaoVisited)
        {
            return;
        }

        BlueDaoVisited = true;
        OnChanged?.Invoke();
    }

    public static void MarkBoundaryLoreComplete()
    {
        if (BoundaryLoreComplete)
        {
            return;
        }

        BoundaryLoreComplete = true;
        OnChanged?.Invoke();
    }

    public static void ResetSession()
    {
        BlueDaoVisited = false;
        BoundaryLoreComplete = false;
    }
}
