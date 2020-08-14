using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Battle {
    [Serializable] public enum UnitSkillRange {
        CloseCombat, RangedAttack
    }
    [Serializable] public enum UnitSkillType {
        Physical, Magical
    }
    [Serializable] public enum UnitAttribute {
        Order, Neutral, Chaos
    }
    [Serializable] public class UnitMoveSpeed {
        public int stepCount = 5;
        public int plain = 1;
        public int forest = 1;
        public int water = 1;
        public int desert = 1;
        public int swamp = 1;
        public int mountain = 1;
    }
    [Serializable] public class UnitSkill {
        public string name;
        public int damagePerHit;
        public int attacksPerTurn;
        public UnitSkillRange range;
        public UnitSkillType type;
    }
    [Serializable] public class AnimationSprites {
        public string name;
        public float speed;
        public Sprite[] sprites;
    }
    [Serializable] public class UnitType {
        public string unitName;
        public int maxHealth;
        public UnitMoveSpeed speed;
        public UnitAttribute attribute;
        public string raceName;
        public UnitSkill[] skills;
        public AnimationSprites[] spritesList;
        [HideInInspector] public Dictionary<string, AnimationSprites> sprites = new Dictionary<string, AnimationSprites>();
    }
    public class BattleControl : MonoBehaviour {
        public bool isEditorMode = false;
        public static BattleControl Instance = null;
        public UnitType[] unitTypeList;
        [HideInInspector] public Dictionary<string, UnitType> unitType = new Dictionary<string, UnitType>();
        public List<Unit> player;
        public List<Unit> enemy;
        [HideInInspector] public bool isSelectingTarget = false;
        private void InitSprites() {
            foreach (var i in unitTypeList) {
                unitType[i.unitName] = i;
                foreach (var j in unitType[i.unitName].spritesList) {
                    unitType[i.unitName].sprites[j.name] = j;
                }
            }
        }
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
                return;
            }
            InitSprites();
            if (!isEditorMode) {
                StartCoroutine(MainGame());
            }
        }
        private Dictionary<Tuple<int, int>, int> vis;
        private int GetStep(Tuple<int, int> p, UnitMoveSpeed s) {
            foreach (var i in MapControl.Instance.G[p]) {
                var x = MapControl.Instance.map[i.Item1, i.Item2].GetComponent<TileAttributes>().stayingUnit;
                if (x != null && x.GetComponent<Unit>().controlledByPlayer == false) {
                    // cannot pass a block that is near to an enemy
                    return 114514;
                }
            }
            var t = MapControl.Instance.map[p.Item1, p.Item2].GetComponent<TileAttributes>().GetTerrainType();
            if (t == TerrainType.Desert) {
                return s.desert;
            } else if (t == TerrainType.Forest) {
                return s.forest;
            } else if (t == TerrainType.Mountain) {
                return s.mountain;
            } else if (t == TerrainType.Plain) {
                return s.plain;
            } else if (t == TerrainType.Swamp) {
                return s.swamp;
            } else if (t == TerrainType.Water) {
                return s.water;
            }
            return 1;
        }
        private void Search(Tuple<int, int> p, int d, UnitMoveSpeed s) {
            var x = MapControl.Instance.map[p.Item1, p.Item2].GetComponent<TileAttributes>().stayingUnit;
            if (x != null && x.GetComponent<Unit>().controlledByPlayer == false) {
                return;
            }
            if (!vis.ContainsKey(p)) {
                vis[p] = d;
            } else {
                vis[p] = Math.Max(vis[p], d);
            }
            MapControl.Instance.map[p.Item1, p.Item2].GetComponent<TileAttributes>().TurnOffGray();
            if (d <= 0) {
                return;
            }
            foreach (var i in MapControl.Instance.G[p]) {
                Search(i, d - GetStep(i, s), s);
            }
        }
        private bool skip = false;
        private IEnumerator MainGame() {
            while (this != null) {
                // player's turn
                foreach (var i in player) {
                    i.remainSteps = unitType[i.branch].speed.stepCount;
                }
                while (!skip) {
                    while (!skip && !(Input.GetMouseButtonDown((int) MouseButton.LeftMouse) && CameraControl.IsInClickRange(Input.mousePosition))) {
                        yield return null;
                    }
                    if (skip) {
                        break;
                    }
                    var x = Physics2D.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Vector3.back);
                    if (x.Length < 2) {
                        yield return null;
                        continue;
                    }
                    var p = x[1].transform.GetComponent<Unit>() == null ? x[0] : x[1];
                    isSelectingTarget = true;
                    if (!p.transform.GetComponent<Unit>().controlledByPlayer) {
                        isSelectingTarget = false;
                        yield return null;
                        continue;
                    }
                    /*
                    if (p.transform.GetComponent<Unit>().remainSteps == 0) {
                        yield return null;
                        continue;
                    }
                    */
                    MapControl.Instance.tilesContainer.BroadcastMessage("TurnOnGray", SendMessageOptions.DontRequireReceiver);
                    vis = new Dictionary<Tuple<int, int>, int>();
                    Search(p.transform.GetComponent<Unit>().pos, p.transform.GetComponent<Unit>().remainSteps, unitType[p.transform.GetComponent<Unit>().branch].speed);
                    while (!skip && Input.GetMouseButtonDown((int) MouseButton.LeftMouse)) {
                        yield return null;
                    }
                    while (!skip && !(Input.GetMouseButtonDown((int) MouseButton.LeftMouse) && CameraControl.IsInClickRange(Input.mousePosition))) {
                        yield return null;
                    }
                    if (skip) {
                        MapControl.Instance.tilesContainer.BroadcastMessage("TurnOffGray", SendMessageOptions.DontRequireReceiver);
                        isSelectingTarget = false;
                        break;
                    }
                    RaycastHit2D d;
                    x = Physics2D.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Vector3.back);
                    if (x.Length != 1) {
                        MapControl.Instance.tilesContainer.BroadcastMessage("TurnOffGray", SendMessageOptions.DontRequireReceiver);
                        isSelectingTarget = false;
                        yield return null;
                        continue;
                    }
                    d = x[0];
                    if (!vis.ContainsKey(new Tuple<int, int>(d.transform.GetComponent<TileAttributes>().y, d.transform.GetComponent<TileAttributes>().x))) {
                        MapControl.Instance.tilesContainer.BroadcastMessage("TurnOffGray", SendMessageOptions.DontRequireReceiver);
                        isSelectingTarget = false;
                        yield return null;
                        continue;
                    }
                    yield return p.transform.GetComponent<Unit>().MoveTo(MapControl.GetTilePositionOnScreen(d.transform.GetComponent<TileAttributes>().x, d.transform.GetComponent<TileAttributes>().y), d.transform.gameObject);
                    p.transform.GetComponent<Unit>().remainSteps = vis[p.transform.GetComponent<Unit>().pos];
                    MapControl.Instance.tilesContainer.BroadcastMessage("TurnOffGray", SendMessageOptions.DontRequireReceiver);
                    isSelectingTarget = false;
                    yield return null;
                }
                skip = false;
                yield return null;
                // TODO: Enemy's turn
            }
        }
        public void SkipPlayerTurn() {
            skip = true;
        }
    }
}
