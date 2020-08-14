using System;
using System.Collections;
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
        public bool controlledByPlayer = true;
        public static Text Description = null;
        private string _lastAnimation;
        private SpriteRenderer _spriteRenderer;
        private int _spriteIndex = 0;
        private UnitType _type;
        private void Awake() {
            pos = new Tuple<int, int>(y, x);
            transform.position = MapControl.GetTilePositionOnScreen(x, y);
        }
        private void Start() {
            if (Description == null) {
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
                }
                _lastAnimation = currentAnimation;
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
        private string GetChinese(UnitAttribute attr) {
            if (attr == UnitAttribute.Chaos) {
                return "混沌";
            } else if (attr == UnitAttribute.Neutral) {
                return "中立";
            } else if (attr == UnitAttribute.Order) {
                return "秩序";
            }
            return "无";
        }
        private string GetChinese(UnitSkillRange range) {
            if (range == UnitSkillRange.CloseCombat) {
                return "近战";
            } else if (range == UnitSkillRange.RangedAttack) {
                return "远程";
            }
            return "无";
        }
        private string GetChinese(UnitSkillType type) {
            if (type == UnitSkillType.Magical) {
                return "魔法";
            } else if (type == UnitSkillType.Physical) {
                return "物理";
            }
            return "无";
        }
        public string GetDescription() {
            string basic = "";
            if (displayName != branch) {
                basic += displayName + " (" + branch + ")\n\n";
            } else {
                basic += displayName + "\n\n";
            }
            basic += GetChinese(_type.attribute) + " " + _type.raceName + "\n\n" +
                     "HP: " + health + " / " + _type.maxHealth;
            if (controlledByPlayer) {
                basic += "\n\n";
                foreach (var i in _type.skills) {
                    basic += i.name + "(" + GetChinese(i.range) + ", " + GetChinese(i.type) + ")" + "\n" + i.damagePerHit + " X " + i.attacksPerTurn + "\n";
                }
                return basic;
            } else {
                return basic;
            }
        }
    }
}
