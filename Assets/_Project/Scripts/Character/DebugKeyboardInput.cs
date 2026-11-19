using UnityEngine;

namespace Project.Character
{
    /// Phase 1 placeholder input for manual testing; replaced by the virtual joystick in Phase 2.
    [RequireComponent(typeof(CharacterBrain))]
    public class DebugKeyboardInput : MonoBehaviour
    {
        private CharacterBrain _brain;

        private void Awake()
        {
            _brain = GetComponent<CharacterBrain>();
        }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            _brain.MoveDirection = new Vector3(h, 0f, v);
        }
    }
}
