using System.Collections;
using UnityEngine;

namespace Battle {
    public class Flag : MonoBehaviour {
        public Sprite[] sprites;
        public int delta = 10;
        private int _index = 0;
        private int p = 0;
        private SpriteRenderer _spriteRenderer;
        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        private void FixedUpdate() {
            p++;
            if (p == delta) {
                p = 0;
                _spriteRenderer.sprite = sprites[_index];
                _index = (_index + 1) % sprites.Length;
            }
        }
    }
}
