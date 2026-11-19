using Project.CameraSystem;
using Project.Character;
using Project.Combat;
using Project.Core;
using Project.Economy;
using Project.Lane;
using Project.Shared;
using Project.Stats;
using Project.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Bootstrap
{
    /// Builds the vertical-slice test scene entirely from code (hero, dummy target, lane
    /// with towers + minion waves, camera, joystick/attack-button HUD, LogicTicker) so the
    /// project is playable without any manual Editor scene wiring. UI/Physics construction
    /// here is review-only, not unit-tested (needs real UnityEngine.UI/EventSystems/Physics,
    /// which the .NET shim does not provide).
    public class GlobalLoader : MonoBehaviour
    {
        [SerializeField] private string _heroStatsResourcePath = "DefaultHeroStats";

        private void Awake()
        {
            SpawnLogicTicker();
            SpawnEventSystem();

            CharacterBrain hero = SpawnHero();
            SpawnDummyTarget();
            SpawnLane();
            SpawnCamera(hero.transform);
            SpawnHud(hero);
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

        private CharacterBrain SpawnHero()
        {
            BaseStats baseStats = LoadHeroStats();

            var hero = new GameObject("Hero");
            CharacterStats stats = hero.AddComponent<CharacterStats>();
            stats.Initialize(baseStats);

            Targetable targetable = hero.AddComponent<Targetable>();
            targetable.Initialize(Team.Blue, TargetType.Hero);
            hero.AddComponent<SphereCollider>().radius = 0.5f;

            hero.AddComponent<HeroProgression>();
            hero.AddComponent<Inventory>();

            CharacterBrain brain = hero.AddComponent<CharacterBrain>();
            hero.AddComponent<AutoAttackController>();
            hero.AddComponent<DebugKeyboardInput>();
            hero.AddComponent<DebugStatsDisplay>();
            return brain;
        }

        private void SpawnDummyTarget()
        {
            BaseStats baseStats = LoadHeroStats();

            var dummy = new GameObject("Dummy Target");
            dummy.transform.position = new Vector3(4f, 0f, 0f);

            CharacterStats stats = dummy.AddComponent<CharacterStats>();
            stats.Initialize(baseStats);

            Targetable targetable = dummy.AddComponent<Targetable>();
            targetable.Initialize(Team.Red, TargetType.Minion);
            dummy.AddComponent<SphereCollider>().radius = 0.5f;
        }

        /// One lane, two towers, minion waves marching between them — reuses the same
        /// BaseStats asset as Hero/Dummy (no separate MinionData/TowerData needed yet).
        private void SpawnLane()
        {
            var blueTowerPosition = new Vector3(-10f, 0f, 0f);
            var redTowerPosition = new Vector3(10f, 0f, 0f);

            SpawnTower(Team.Blue, blueTowerPosition);
            SpawnTower(Team.Red, redTowerPosition);

            var bluePath = new LanePath(new Vector3(-8f, 0f, 0f), new Vector3(8f, 0f, 0f));
            var redPath = new LanePath(new Vector3(8f, 0f, 0f), new Vector3(-8f, 0f, 0f));

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

            go.AddComponent<TowerBrain>();
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

        private void SpawnHud(CharacterBrain hero)
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            VirtualJoystick joystick = CreateJoystick(canvasGO.transform);
            AttackButton attackButton = CreateAttackButton(canvasGO.transform);
            AttackButton buyButton = CreateBuyButton(canvasGO.transform);

            joystick.OnDirectionChanged += direction => hero.MoveDirection = new Vector3(direction.x, 0f, direction.y);

            var autoAttack = hero.GetComponent<AutoAttackController>();
            attackButton.OnPressed += autoAttack.RequestAttack;

            Inventory inventory = hero.GetComponent<Inventory>();
            ItemSO testItem = CreateTestItem();
            buyButton.OnPressed += () => inventory.BuyItem(testItem);
        }

        /// Stands in for a real shop UI (Giai đoạn sau): proves Inventory/HeroProgression
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
