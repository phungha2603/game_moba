using System.Collections.Generic;
using Project.Skills;
using UnityEngine;

namespace Project.Economy
{
    [CreateAssetMenu(menuName = "Project/Economy/Item", fileName = "NewItem")]
    public class ItemSO : ScriptableObject
    {
        public string ItemName = "New Item";
        public float Price = 500f;
        public List<ItemStatBonus> StatBonuses = new List<ItemStatBonus>();

        [Header("On-Hit Passive (optional, single effect, no ICD/proc-chance yet)")]
        public bool HasOnHitEffect;
        public SkillEffectData OnHitEffect;
    }
}
