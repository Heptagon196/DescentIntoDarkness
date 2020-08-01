using System;
using UnityEngine;

namespace Battle {
    public class CameraControl : MonoBehaviour {
        public float speed = 0.1f;
        public float edge = 0.1f;
        private Camera _camera;
        private static float scale;
        private float screenWidthInWorld;
        private float screenHeightInWorld;
        public static bool IsInClickRange(Vector3 pos) {
            return pos.x >= 0 && pos.x <= (Screen.width - 300) && pos.y >= 0 && pos.y <= (Screen.height - 50);
        }
        private void Awake() {
            _camera = Camera.main;
            scale = _camera.WorldToScreenPoint(new Vector3(1, 0, 0)).x - _camera.WorldToScreenPoint(new Vector3(0, 0, 0)).x;
            screenWidthInWorld = Screen.width / scale;
            screenHeightInWorld = Screen.height / scale;
        }
        private float LimitTo(float f, float l, float r, float w) {
            return Mathf.Max(Mathf.Min(f, r - w / 2f), l + w / 2f);
        }
        private void FixedUpdate() {
            var pos = Input.mousePosition;
            var campos = _camera.transform.position;
            Vector2 s = new Vector2(0, 0);
            if (pos.x < 0 || pos.y < 0 || pos.x > Screen.width || pos.y > Screen.height) {
                return;
            }
            if (pos.x < Screen.width * edge) {
                s.x = -speed;
            } else if (pos.x > Screen.width * (1 - edge)) {
                s.x = speed;
            }
            if (pos.y < Screen.height * edge) {
                s.y = -speed;
            } else if (pos.y > Screen.height * (1 - edge)) {
                s.y = speed;
            }
            if (Mathf.Abs(s.x) > 0.001f && Mathf.Abs(s.y) > 0.001f) {
                s.x /= Mathf.Sqrt(2);
                s.y /= Mathf.Sqrt(2);
            }
            _camera.transform.position = new Vector3(LimitTo(campos.x + s.x, 0, MapControl.Instance.width * 0.54f - 0.36f + 300 / scale, screenWidthInWorld), LimitTo(campos.y + s.y, 0, MapControl.Instance.height * 0.72f - 0.36f + 50 / scale, screenHeightInWorld), campos.z);
        }
    }
}
