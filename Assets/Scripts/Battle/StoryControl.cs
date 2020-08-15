using UnityEngine;
using UnityEngine.UI;

namespace Battle {
    public class StoryControl : MonoBehaviour {
        private Image _panel;
        private Text _text;
        private RawImage _leftImg, _rightImg;
        public static StoryControl Instance = null;
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(gameObject);
                return;
            }
            _panel = GetComponent<Image>();
            _text = transform.GetChild(0).GetComponent<Text>();
            _leftImg = transform.GetChild(1).GetComponent<RawImage>();
            _rightImg = transform.GetChild(2).GetComponent<RawImage>();
        }
        public void SetText(string text) {
            _text.text = text;
        }
        public void SetLeftImage(Texture img) {
            _leftImg.texture = img;
            _leftImg.enabled = true;
        }
        public void SetRightImage(Texture img) {
            _rightImg.texture = img;
            _rightImg.enabled = true;
        }
        public void ClearImage() {
            ClearLeftImage();
            ClearRightImage();
        }
        public void ClearLeftImage() {
            _leftImg.enabled = false;
            _leftImg.texture = null;
        }
        public void ClearRightImage() {
            _rightImg.enabled = false;
            _rightImg.texture = null;
        }
        public void HidePanel() {
            _panel.enabled = false;
            _text.enabled = false;
        }
        public void ShowPanel() {
            _panel.enabled = true;
            _text.enabled = true;
        }
    }
}
