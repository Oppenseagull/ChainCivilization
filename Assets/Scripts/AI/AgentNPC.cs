using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// NPC proximity detector + E key trigger. Opens AgentUI when near.
/// Attach to the NPC GameObject (must have a Collider or CharacterController).
/// </summary>
public class AgentNPC : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] float interactRadius = 5f;
    [SerializeField] string npcName = "智者";

    Transform _player;
    bool _playerNear;
    bool _wasPlayerNear;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
        }
    }

    void Update()
    {
        if (_player == null)
        {
            return;
        }

        float distance = Vector3.Distance(_player.position, transform.position);
        _playerNear = distance <= interactRadius;

        if (_playerNear && !_wasPlayerNear)
        {
            HUDPromptChannel.Set(this, npcName, "【按 E 与大祭司对话】", -distance);
        }
        else if (!_playerNear && _wasPlayerNear)
        {
            HUDPromptChannel.Clear(this);
            if (AgentUI.IsOpen)
            {
                AgentUI.CloseUI();
            }
        }
        else if (_playerNear && !AgentUI.IsOpen)
        {
            HUDPromptChannel.Set(this, npcName, "【按 E 与大祭司对话】", -distance);
        }

        _wasPlayerNear = _playerNear;

        if (_playerNear && WasEPressedThisFrame() && !AgentUI.IsOpen)
        {
            HUDPromptChannel.Clear(this);
            AgentUI.OpenUI();
        }
    }

    void OnDisable()
    {
        HUDPromptChannel.Clear(this);
    }

    static bool WasEPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.7f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
