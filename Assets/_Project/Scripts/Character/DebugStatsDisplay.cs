using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Character
{
    /// Phase 1 placeholder HP/Mana readout; replaced by the real HUD in Phase 6.
    [RequireComponent(typeof(CharacterStats))]
    public class DebugStatsDisplay : MonoBehaviour
    {
        private CharacterStats _stats;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
        }

        private void OnGUI()
        {
            if (Camera.main == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            if (screenPos.z < 0f) return;

            string text = $"HP {_stats.GetValue(StatType.Health):0} / MP {_stats.GetValue(StatType.Mana):0}";
            GUI.Label(new Rect(screenPos.x - 50f, Screen.height - screenPos.y, 150f, 20f), text);
        }
    }
}
