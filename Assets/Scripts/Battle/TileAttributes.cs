using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Battle {
    public class TileAttributes : MonoBehaviour {
        public int terrain = 0;
        public int embellishment = 0;
        [HideInInspector] public GameObject stayingUnit;
        [HideInInspector] public int x, y;
        private bool _loop = false;
        private int _loopCount = 0;
        private SpriteRenderer _spriteRenderer;
        public void ChangeTerrainTo(int id) {
            GetComponent<TileAttributes>().terrain = id;
            GetComponent<SpriteRenderer>().sprite = id == -1 ? null : MapControl.Instance.GetRandomSprite(MapControl.Instance.terrains[id].sprites);
            var color = GetComponent<SpriteRenderer>().color;
            color.a = MapControl.Instance.terrains[id].transparency;
            GetComponent<SpriteRenderer>().color = color;
        }
        public void ChangeEmbellishmentTo(int id) {
            GetComponent<TileAttributes>().embellishment = id;
            _loop = MapControl.Instance.embellishments[id].loop;
            _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (_loop) {
                _spriteRenderer.sprite = MapControl.Instance.embellishments[id].sprites[0];
            } else {
                _spriteRenderer.sprite = id == -1 ? null : MapControl.Instance.GetRandomSprite(MapControl.Instance.embellishments[id].sprites);
            }
        }
        private void FixedUpdate() {
            if (_loop) {
                _loopCount = (_loopCount + 1) % MapControl.Instance.embellishments[embellishment].sprites.Length;
                _spriteRenderer.sprite = MapControl.Instance.embellishments[embellishment].sprites[_loopCount];
            }
        }
        private string ToChinese(TerrainType type) {
            if (type == TerrainType.Desert) {
                return "沙漠地带";
            } else if (type == TerrainType.Forest) {
                return "森林地带";
            } else if (type == TerrainType.Mountain) {
                return "丘陵地带";
            } else if (type == TerrainType.Plain) {
                return "平原地带";
            } else if (type == TerrainType.Swamp) {
                return "沼泽地带";
            } else if (type == TerrainType.Water) {
                return "水域";
            }
            return "无";
        }
        public string GetDescription() {
            string d = MapControl.Instance.terrains[terrain].chineseName + "/" + MapControl.Instance.embellishments[embellishment].chineseName + "\n" + ToChinese(GetTerrainType());
            #if UNITY_EDITOR
            d += "\n" + x + " " + y;
            #endif
            return d;
        }
        private int _frameCount = 0;
        public void OnMouseEnter() {
            _frameCount = 0;
            transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = true;
        }
        public void OnMouseOver() {
            _frameCount++;
            if (!CameraControl.IsInClickRange(Input.mousePosition)) {
                return;
            }
            if (Input.GetMouseButtonDown((int) MouseButton.RightMouse) && !BattleControl.Instance.isSelectingTarget) {
                // summon a zombie on right click
                // for test only
                SummonUnitHere(true, "僵尸", "僵尸");
            }
            if (_frameCount == 100 || (Input.GetMouseButtonDown((int)MouseButton.LeftMouse) && !BattleControl.Instance.isSelectingTarget)) {
                TerrainIndicatorControl.Instance.ShowText(GetDescription());
            }
        }
        public void OnMouseExit() {
            _frameCount = 0;
            transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = false;
            TerrainIndicatorControl.Instance.ClearText();
        }
        public void TurnOnGray() {
            transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = true;
        }
        public void TurnOffGray() {
            transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = false;
        }
        public TerrainType GetTerrainType() {
            if (MapControl.Instance.embellishments[embellishment].type == TerrainType.None) {
                return MapControl.Instance.terrains[terrain].type;
            } else {
                return MapControl.Instance.embellishments[embellishment].type;
            }
        }
        public void SummonUnitHere(bool controlledByPlayer, string branchName, string displayName) {
            GameObject p;
            if (controlledByPlayer) {
                p = Instantiate(BattleControl.Instance.playerUnitTemplate);
            } else {
                p = Instantiate(BattleControl.Instance.enemyUnitTemplate);
            }
            p.GetComponent<Unit>().branch = branchName;
            p.GetComponent<Unit>().displayName = displayName;
            p.GetComponent<Unit>().pos = new Tuple<int, int>(y, x);
            p.transform.position = MapControl.GetTilePositionOnScreen(x, y);
            stayingUnit = p;
            if (controlledByPlayer) {
                BattleControl.Instance.player.Add(p.GetComponent<Unit>());
            } else {
                BattleControl.Instance.enemy.Add(p.GetComponent<Unit>());
            }
        }
        public void DisableFlag() {
            transform.GetChild(4).gameObject.SetActive(false);
            transform.GetChild(5).gameObject.SetActive(false);
        }
        public void EnableFlag(bool player) {
            if (player) {
                transform.GetChild(4).gameObject.SetActive(true);
            } else {
                transform.GetChild(5).gameObject.SetActive(true);
            }
        }
    }
}
