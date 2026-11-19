using Project.CameraSystem;
using Project.Character;
using Project.Combat;
using Project.Core;
using Project.Economy;
using Project.Lane;
using Project.Shared;
using Project.Skills;
using Project.Stats;
using Project.UI;
using Project.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Bootstrap
{
    /// Builds the full vertical-slice match entirely from code — two Heroes (player +
    /// SimpleHeroAI bot), a lane with two Towers, two Nexuses, minion waves, a shop-stand-in,
    /// Skillshot casting, placeholder "Model/Animation" visuals, and the win-condition
    /// MatchManager — so the project is playable without any manual Editor scene wiring.
    /// UI/Physics/rendering construction here is review-only, not unit-tested (needs real
    /// UnityEngine.UI/EventSystems/Physics/rendering, which the .NET shim does not provide).
    public class GlobalLoader : MonoBehaviour
    {
        private const float LaneHalfLength = 8f;
        private const float TowerDistance = 10f;
        private const float NexusDistance = 14f;

        [SerializeField] private string _heroStatsResourcePath = "DefaultHeroStats";

        private void Awake()
        {
            SpawnLogicTicker();
            SpawnEventSystem();

            (CharacterBrain playerHero, SkillInstance fireball) = SpawnPlayerHero();
            SpawnHeroBot();
            SpawnLane();
            CharacterStats blueNexus = SpawnNexus(Team.Blue, new Vector3(-NexusDistance, 0f, 0f));
            CharacterStats redNexus = SpawnNexus(Team.Red, new Vector3(NexusDistance, 0f, 0f));
            SpawnMatchManager(blueNexus, redNexus);

            SpawnCamera(playerHero.transform);
            SpawnHud(playerHero, fireball);
        }

        private static void SpawnLogicTicker()
        {
            var tickerGO = new GameObject("LogicTicker");
            tickerGO.AddComponent<LogicTicker>();
        }

        private static void SpawnEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        /// Shared setup for both the player's Hero and the enemy Hero Bot — only the input
        /// source (DebugKeyboardInput+UI vs. SimpleHeroAI) differs between them.
        private (CharacterBrain brain, SkillInstance fireball) SpawnHeroUnit(string name, Team team, Vector3 position, Color color)
        {
            BaseStats baseStats = LoadHeroStats();

            var hero = new GameObject(name);
            hero.transform.position = position;

            CharacterStats stats = hero.AddComponent<CharacterStats>();
            stats.Initialize(baseStats);

            Targetable targetable = hero.AddComponent<Targetable>();
            targetable.Initialize(team, TargetType.Hero);
            hero.AddComponent<SphereCollider>().radius = 0.5f;

            hero.AddComponent<HeroProgression>();
            hero.AddComponent<Inventory>();

            CharacterBrain brain = hero.AddComponent<CharacterBrain>();
            hero.AddComponent<AutoAttackController>();
            hero.AddComponent<ProjectileLauncher>();

            CharacterView view = hero.AddComponent<CharacterView>();
            view.Configure(stats, PrimitiveType.Capsule, color);
            brain.OnAttackPerformed += view.PlayAttackPulse;

            SkillInstance fireball = AddFireballSkill(brain);

            return (brain, fireball);
        }

        private (CharacterBrain brain, SkillInstance fireball) SpawnPlayerHero()
        {
            (CharacterBrain brain, SkillInstance fireball) = SpawnHeroUnit("Hero (Player)", Team.Blue, new Vector3(-6f, 0f, 0f), Color.blue);
            brain.gameObject.AddComponent<DebugKeyboardInput>();
            brain.gameObject.AddComponent<DebugStatsDisplay>();
            return (brain, fireball);
        }

        private void SpawnHeroBot()
        {
            (CharacterBrain brain, _) = SpawnHeroUnit("Hero (Bot)", Team.Red, new Vector3(6f, 0f, 0f), Color.red);

            SimpleHeroAI ai = brain.gameObject.AddComponent<SimpleHeroAI>();
            ai.SetPath(new LanePath(new Vector3(6f, 0f, 0f), new Vector3(-6f, 0f, 0f)));
        }

        /// A single Skillshot skill so the player has something to aim with the SkillAimButton
        /// — proving the Projectile pipeline end to end, not a real kit (that's a future pass).
        private static SkillInstance AddFireballSkill(CharacterBrain brain)
        {
            var fireball = ScriptableObject.CreateInstance<SkillSO>();
            fireball.SkillName = "Fireball";
            fireball.ManaCost = 40f;
            fireball.Cooldown = 4f;
            fireball.ProjectileSpeed = 12f;
            fireball.ProjectileRange = 10f;
            fireball.TargetingType = SkillTargetingType.Skillshot;
            fireball.DamageType = DamageType.Magic;
            fireball.Effects.Add(new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 60f });

            return brain.AddSkill(fireball);
        }

        /// One lane, two Towers, minion waves marching between them — reuses the same
        /// BaseStats asset as the Heroes (no separate MinionData/TowerData yet).
        private void SpawnLane()
        {
            SpawnTower(Team.Blue, new Vector3(-TowerDistance, 0f, 0f));
            SpawnTower(Team.Red, new Vector3(TowerDistance, 0f, 0f));

            var bluePath = new LanePath(new Vector3(-LaneHalfLength, 0f, 0f), new Vector3(LaneHalfLength, 0f, 0f));
            var redPath = new LanePath(new Vector3(LaneHalfLength, 0f, 0f), new Vector3(-LaneHalfLength, 0f, 0f));

            var spawnerGO = new GameObject("LaneSpawner");
            LaneSpawner spawner = spawnerGO.AddComponent<LaneSpawner>();
            spawner.Configure(LoadHeroStats(), bluePath, redPath);
        }

        private void SpawnTower(Team team, Vector3 position)
        {
            var go = new GameObject($"Tower_{team}");
            go.transform.position = position;

            CharacterStats stats = go.AddComponent<CharacterStats>();
            stats.Initialize(LoadHeroStats());

            Targetable targetable = go.AddComponent<Targetable>();
            targetable.Initialize(team, TargetType.Tower);
            go.AddComponent<SphereCollider>().radius = 1f;

            TowerBrain brain = go.AddComponent<TowerBrain>();

            CharacterView view = go.AddComponent<CharacterView>();
            view.Configure(stats, PrimitiveType.Cylinder, team == Team.Blue ? Color.blue : Color.red);
            brain.OnAttackPerformed += view.PlayAttackPulse;
        }

        /// High-HP, passive (no TowerBrain — a Nexus does not attack) — destroying the
        /// enemy one ends the match (see MatchManager).
        private CharacterStats SpawnNexus(Team team, Vector3 position)
        {
            var go = new GameObject($"Nexus_{team}");
            go.transform.position = position;

            var nexusStats = ScriptableObject.CreateInstance<BaseStats>();
            nexusStats.baseHealth = 3000f;
            nexusStats.baseArmor = 30f;

            CharacterStats stats = go.AddComponent<CharacterStats>();
            stats.Initialize(nexusStats);

            Targetable targetable = go.AddComponent<Targetable>();
            targetable.Initialize(team, TargetType.Nexus);
            go.AddComponent<SphereCollider>().radius = 1.5f;

            CharacterView view = go.AddComponent<CharacterView>();
            view.Configure(stats, PrimitiveType.Sphere, team == Team.Blue ? Color.blue : Color.red);

            return stats;
        }

        private static void SpawnMatchManager(CharacterStats blueNexus, CharacterStats redNexus)
        {
            var go = new GameObject("MatchManager");
            MatchManager manager = go.AddComponent<MatchManager>();
            manager.Configure(blueNexus, redNexus);
        }

        private BaseStats LoadHeroStats()
        {
            BaseStats baseStats = Resources.Load<BaseStats>(_heroStatsResourcePath);
            if (baseStats == null)
            {
                Debug.LogError($"GlobalLoader could not find BaseStats at Resources/{_heroStatsResourcePath}.");
            }

            return baseStats;
        }

        private static void SpawnCamera(Transform target)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.AddComponent<Camera>();
            TopDownCamera topDown = camGO.AddComponent<TopDownCamera>();
            topDown.SetTarget(target);
        }

        private void SpawnHud(CharacterBrain hero, SkillInstance fireball)
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            VirtualJoystick joystick = CreateJoystick(canvasGO.transform);
            AttackButton attackButton = CreateAttackButton(canvasGO.transform);
            AttackButton buyButton = CreateBuyButton(canvasGO.transform);
            SkillAimButton skillButton = CreateSkillAimButton(canvasGO.transform);

            joystick.OnDirectionChanged += direction => hero.MoveDirection = new Vector3(direction.x, 0f, direction.y);

            var autoAttack = hero.GetComponent<AutoAttackController>();
            attackButton.OnPressed += autoAttack.RequestAttack;

            Inventory inventory = hero.GetComponent<Inventory>();
            ItemSO testItem = CreateTestItem();
            buyButton.OnPressed += () => inventory.BuyItem(testItem);

            skillButton.OnSkillTriggered += direction =>
                hero.TryCastSkill(fireball, new Vector3(direction.x, 0f, direction.y));
        }

        /// Stands in for a real shop UI (future pass): proves Inventory/HeroProgression
        /// wiring works end to end without needing a shop panel yet.
        private static ItemSO CreateTestItem()
        {
            var item = ScriptableObject.CreateInstance<ItemSO>();
            item.ItemName = "Test Blade";
            item.Price = 300f;
            item.StatBonuses.Add(new ItemStatBonus { Stat = StatType.AttackDamage, Value = 15f, ModType = ModifierType.Flat });
            return item;
        }

        private static AttackButton CreateBuyButton(Transform canvasParent)
        {
            var rect = CreateUIRect("Buy Button", canvasParent, new Vector2(1f, 1f), new Vector2(-120f, -150f), new Vector2(140f, 100f));
            rect.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 0.8f);
            return rect.gameObject.AddComponent<AttackButton>();
        }

        private static VirtualJoystick CreateJoystick(Transform canvasParent)
        {
            var background = CreateUIRect("Joystick Background", canvasParent, new Vector2(0f, 0f), new Vector2(180f, 180f), new Vector2(220f, 220f));
            background.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);

            var handle = CreateUIRect("Joystick Handle", background.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90f, 90f));
            handle.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.6f);

            VirtualJoystick joystick = background.gameObject.AddComponent<VirtualJoystick>();
            joystick.Configure(background, handle, handleRange: 80f);
            return joystick;
        }

        private static AttackButton CreateAttackButton(Transform canvasParent)
        {
            var rect = CreateUIRect("Attack Button", canvasParent, new Vector2(1f, 0f), new Vector2(-120f, 150f), new Vector2(140f, 140f));
            rect.gameObject.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            return rect.gameObject.AddComponent<AttackButton>();
        }

        private static SkillAimButton CreateSkillAimButton(Transform canvasParent)
        {
            var rect = CreateUIRect("Skill Button", canvasParent, new Vector2(1f, 0f), new Vector2(-280f, 150f), new Vector2(120f, 120f));
            rect.gameObject.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.9f, 0.8f);
            SkillAimButton button = rect.gameObject.AddComponent<SkillAimButton>();
            button.Configure(rect);
            return button;
        }

        private static RectTransform CreateUIRect(string name, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
