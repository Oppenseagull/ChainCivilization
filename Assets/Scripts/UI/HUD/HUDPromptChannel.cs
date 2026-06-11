using System.Collections.Generic;

/// <summary>
/// Bottom-center interaction prompt channel. Interact scripts publish; Game HUD displays.
/// </summary>
public static class HUDPromptChannel
{
    struct PromptEntry
    {
        public string Line1;
        public string Line2;
        public float Priority;
    }

    static readonly Dictionary<object, PromptEntry> _entries = new Dictionary<object, PromptEntry>();

    public static void Set(object owner, string line1, string line2 = null, float priority = 0f)
    {
        if (owner == null || string.IsNullOrEmpty(line1))
        {
            return;
        }

        _entries[owner] = new PromptEntry
        {
            Line1 = line1,
            Line2 = line2,
            Priority = priority
        };
    }

    public static void Clear(object owner)
    {
        if (owner == null)
        {
            return;
        }

        _entries.Remove(owner);
    }

    public static bool TryGetDisplay(out string line1, out string line2)
    {
        line1 = null;
        line2 = null;

        if (_entries.Count == 0)
        {
            return false;
        }

        object bestOwner = null;
        PromptEntry best = default;
        bool found = false;

        foreach (KeyValuePair<object, PromptEntry> pair in _entries)
        {
            if (!found || pair.Value.Priority > best.Priority)
            {
                found = true;
                bestOwner = pair.Key;
                best = pair.Value;
            }
        }

        if (!found)
        {
            return false;
        }

        line1 = best.Line1;
        line2 = best.Line2;
        return true;
    }

    public static void ClearAll()
    {
        _entries.Clear();
    }
}
