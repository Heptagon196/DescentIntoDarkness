using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle {
    public class TileAttributes : MonoBehaviour{
        public int terrain = 0;
        public int embellishment = 0;
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
    }
}
