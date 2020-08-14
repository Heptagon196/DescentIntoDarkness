using UnityEngine;
using UnityEngine.UI;

namespace Battle {
    public class TerrainIndicatorControl : MonoBehaviour {
        public static TerrainIndicatorControl Instance = null;
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
            }
        }
        private void Update() {
            transform.position = Input.mousePosition;
        }
        public void ShowText(string text) {
            if (text == "") {
                ClearText();
                return;
            }
            GetComponent<Image>().enabled = true;
            GetComponentInChildren<Text>().text = text;
        }
        public void ClearText() {
            GetComponent<Image>().enabled = false;
            GetComponentInChildren<Text>().text = "";
        }
    }
}
