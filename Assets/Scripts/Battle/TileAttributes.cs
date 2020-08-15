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
        public void ChangeTerrainTo(int id) {
            GetComponent<TileAttributes>().terrain = id;
            GetComponent<SpriteRenderer>().sprite = id == -1 ? null : MapControl.Instance.GetRandomSprite(MapControl.Instance.terrains[id].sprites);
            var color = GetComponent<SpriteRenderer>().color;
            color.a = MapControl.Instance.terrains[id].transparency;
            GetComponent<SpriteRenderer>().color = color;
        }
        public void ChangeEmbellishmentTo(int id) {
            GetComponent<TileAttributes>().embellishment = id;
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = id == -1 ? null : MapControl.Instance.GetRandomSprite(MapControl.Instance.embellishments[id].sprites);
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
            return MapControl.Instance.terrains[terrain].chineseName + "/" + MapControl.Instance.embellishments[embellishment].chineseName + "\n" + ToChinese(GetTerrainType());
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
    }
}
