using System.Collections.Generic;
using Project.Stats;
using UnityEngine;

namespace Project.Economy
{
    /// Pairs each purchase with its own token (not the ItemSO asset itself) as the
    /// StatModifier source — buying two of the same item and selling one would otherwise
    /// remove both copies' modifiers, since they'd share the same asset reference.
    [RequireComponent(typeof(CharacterStats))]
    public class Inventory : MonoBehaviour
    {
        private const int MaxSlots = 6;

        private readonly List<(ItemSO Item, object Token)> _slots = new List<(ItemSO Item, object Token)>();

        private CharacterStats _stats;
        private HeroProgression _progression;

        public int Count => _slots.Count;
        public bool IsFull => _slots.Count >= MaxSlots;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _progression = GetComponent<HeroProgression>();
        }

        public bool BuyItem(ItemSO item)
        {
            if (item == null || IsFull) return false;
            if (_progression == null || !_progression.SpendGold(item.Price)) return false;

            var token = new object();
            _slots.Add((item, token));
            ApplyStatBonuses(item, token);
            return true;
        }

        /// Sells the first matching copy (if duplicates are equipped) at a flat sell-back
        /// ratio; standard MOBA convention to discourage buy/sell abuse.
        public bool SellItem(ItemSO item)
        {
            int index = _slots.FindIndex(slot => slot.Item == item);
            if (index < 0) return false;

            object token = _slots[index].Token;
            _slots.RemoveAt(index);
            RemoveStatBonuses(item, token);
            _progression?.AddGold(item.Price * 0.7f);
            return true;
        }

        public IEnumerable<ItemSO> GetOnHitItems()
        {
            foreach ((ItemSO item, _) in _slots)
            {
                if (item.HasOnHitEffect) yield return item;
            }
        }

        private void ApplyStatBonuses(ItemSO item, object token)
        {
            foreach (ItemStatBonus bonus in item.StatBonuses)
            {
                _stats.GetStat(bonus.Stat).AddModifier(new StatModifier(bonus.Value, bonus.ModType, token));
            }
        }

        private void RemoveStatBonuses(ItemSO item, object token)
        {
            foreach (ItemStatBonus bonus in item.StatBonuses)
            {
                _stats.GetStat(bonus.Stat).RemoveModifiersFromSource(token);
            }
        }
    }
}
