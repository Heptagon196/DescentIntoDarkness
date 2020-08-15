using System;
using System.Collections;
using MainMenu;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Battle {
    public class Unit : MonoBehaviour {
        public string displayName;
        public string branch;
        public int health;
        public string currentAnimation;
        public int x, y;
        public Tuple<int, int> pos = new Tuple<int, int>(0, 0);
        public int remainSteps;
        [HideInInspector] public int remainAttacks = 1;
        public bool controlledByPlayer = true;
        public static Text Description = null;
        private string _lastAnimation;
        private SpriteRenderer _spriteRenderer;
        private int _spriteIndex = 0;
        private UnitType _type;
        private GameObject _healthBar;
        private void Awake() {
            pos = new Tuple<int, int>(y, x);
            transform.position = MapControl.GetTilePositionOnScreen(x, y);
            _healthBar = transform.GetChild(0).gameObject;
        }
        private void Start() {
            if (!EditorModeSwitch.IsEditorMode && Description == null) {
                Description = GameObject.Find("UnitDescription").GetComponent<Text>();
            }
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (controlledByPlayer) {
                BattleControl.Instance.player.Add(this);
            } else {
                BattleControl.Instance.enemy.Add(this);
            }
            _type = BattleControl.Instance.unitType[branch];
            remainSteps = _type.speed.stepCount;
            health = _type.maxHealth;
            StartCoroutine(SpriteControl());
        }
        private IEnumerator SpriteControl() {
            yield return null;
            MapControl.Instance.map[y, x].GetComponent<TileAttributes>().stayingUnit = gameObject;
            while (this != null) {
                var animationSprites = _type.sprites[currentAnimation];
                if (currentAnimation != _lastAnimation) {
                    _spriteIndex = 0;
                    _lastAnimation = currentAnimation;
                } else if (_spriteIndex == 0 && !animationSprites.loop) {
                    if (currentAnimation != "die") {
                        currentAnimation = "idle";
                    }
                    yield return null;
                    continue;
                }
                _spriteRenderer.sprite = animationSprites.sprites[_spriteIndex];
                _spriteIndex = (_spriteIndex + 1) % animationSprites.sprites.Length;
                yield return new WaitForSeconds(animationSprites.speed);
            }
        }
        private void OnMouseEnter() {
            MapControl.Instance.map[pos.Item1, pos.Item2].GetComponent<TileAttributes>().OnMouseEnter();
        }
        private void OnMouseOver() {
            if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse) && CameraControl.IsInClickRange(Input.mousePosition)) {
                Description.text = GetDescription();
            }
            MapControl.Instance.map[pos.Item1, pos.Item2].GetComponent<TileAttributes>().OnMouseOver();
        }
        private void OnMouseExit() {
            MapControl.Instance.map[pos.Item1, pos.Item2].GetComponent<TileAttributes>().OnMouseExit();
        }
        public IEnumerator MoveTo(Vector3 dest, GameObject destObj) {
            MapControl.Instance.map[this.pos.Item1, this.pos.Item2].GetComponent<TileAttributes>().stayingUnit = null;
            this.pos = new Tuple<int, int>(destObj.GetComponent<TileAttributes>().y, destObj.transform.GetComponent<TileAttributes>().x);
            MapControl.Instance.map[this.pos.Item1, this.pos.Item2].GetComponent<TileAttributes>().stayingUnit = gameObject;
            Vector3 pos = transform.position;
            var dis = Mathf.Sqrt(Mathf.Pow(dest.x - pos.x, 2) + Mathf.Pow(dest.y - pos.y, 2));
            int s = Convert.ToInt32(dis * 12);
            for (int i = 0; i < s; i++) {
                transform.position = pos + (dest - pos) / s * i;
                yield return new WaitForSeconds(0.02f);
            }
            transform.position = dest;
        }
        public string GetDescription() {
            string basic = "";
            if (displayName != branch) {
                basic += displayName + " (" + branch + ")\n\n";
            } else {
                basic += displayName + "\n\n";
            }
            basic += BattleControl.Instance.GetChinese(_type.attribute) + " " + _type.raceName + "\n\n" +
                     "HP: " + health + " / " + _type.maxHealth;
            if (controlledByPlayer) {
                basic += "\n\n";
                foreach (var i in _type.skills) {
                    basic += i.name + "(" + BattleControl.Instance.GetChinese(i.range) + ", " + BattleControl.Instance.GetChinese(i.type) + ")" + "\n" + i.damagePerHit + " X " + i.attacksPerTurn + "\n";
                }
                return basic;
            } else {
                return basic;
            }
        }
        public void SetHealthBar(float p) {
            if (p < 0) {
                p = 0;
            }
            _healthBar.transform.localPosition = new Vector3((p - 1f) * 2f / 9f, 0.25f);
            _healthBar.GetComponent<SpriteRenderer>().size = new Vector2(p * 2, 0.3f);
        }
        private void FixedUpdate() {
            if (!EditorModeSwitch.IsEditorMode) {
                SetHealthBar((float) health / (float) _type.maxHealth);
            }
        }
    }
}
