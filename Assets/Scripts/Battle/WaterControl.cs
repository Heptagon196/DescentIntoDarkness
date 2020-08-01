using System;
using System.Collections;
using UnityEngine;

namespace Battle {
    public class WaterControl : MonoBehaviour {
        public Sprite[] waters;
        private int _count = 0;
        private SpriteRenderer _spriteRenderer;
        private void Start() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            StartCoroutine(SpriteLoop());
        }
        private IEnumerator SpriteLoop() {
            while (this != null) {
                _spriteRenderer.sprite = waters[_count++];
                _count %= waters.Length;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
