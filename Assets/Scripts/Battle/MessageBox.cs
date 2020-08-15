using UnityEngine;
using UnityEngine.UI;

namespace Battle {
    public class MessageBox : MonoBehaviour {
        public static MessageBox Instance = null;
        private Image _img;
        private Text _text;
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(gameObject);
                return;
            }
            _img = GetComponent<Image>();
            _text = transform.GetChild(0).GetComponent<Text>();
        }
        public void ShowMessage(string msg) {
            _text.text = msg;
            _img.enabled = true;
            _text.enabled = true;
        }
        public void CloseMessage() {
            _img.enabled = false;
            _text.enabled = false;
        }
    }
}
