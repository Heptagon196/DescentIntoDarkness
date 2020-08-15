using Battle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu {
    public class MenuControl : MonoBehaviour {
        public void StartTutorial() {
            EditorModeSwitch.IsEditorMode = false;
            SceneManager.LoadScene("Scenes/Battle");
        }
        public void StartEditor() {
            EditorModeSwitch.IsEditorMode = true;
            SceneManager.LoadScene("Scenes/Battle");
        }
        public void Exit() {
            Application.Quit();
        }
    }
}
