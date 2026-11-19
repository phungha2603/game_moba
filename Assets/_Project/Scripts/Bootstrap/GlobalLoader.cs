using Project.CameraSystem;
using Project.Character;
using Project.Combat;
using Project.Core;
using Project.Shared;
using Project.Stats;
using Project.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Bootstrap
{
    /// Builds the Phase 1+2 test scene entirely from code (hero, dummy target, camera,
    /// joystick/attack-button HUD, LogicTicker) so the project is playable without any
    /// manual Editor scene wiring. UI construction here is review-only, not unit-tested
    /// (needs real UnityEngine.UI/EventSystems/Physics, which the .NET shim does not provide).
    public class GlobalLoader : MonoBehaviour
    {
        [SerializeField] private string _heroStatsResourcePath = "DefaultHeroStats";

        private void Awake()
        {
            SpawnLogicTicker();
            SpawnEventSystem();

            CharacterBrain hero = SpawnHero();
            SpawnDummyTarget();
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

            joystick.OnDirectionChanged += direction => hero.MoveDirection = new Vector3(direction.x, 0f, direction.y);

            var autoAttack = hero.GetComponent<AutoAttackController>();
            attackButton.OnPressed += autoAttack.RequestAttack;
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
