using UnityEngine;

namespace MainMenu {
    public class EditorModeSwitch : MonoBehaviour {
        public static bool IsEditorMode = false;
        private static EditorModeSwitch Instance = null;
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }
    }
}
