using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Battle {
    public class EditorModeControl : MonoBehaviour {
        public int currentItemType = 0;
        public int currentId = 0;
        public GameObject editorPanel;
        public GameObject editorPanelContent;
        public GameObject button;
        private Camera _camera;
        private void Start() {
            if (!BattleControl.Instance.isEditorMode) {
                return;
            }
            _camera = Camera.main;
            for (int i = 0; i < MapControl.Instance.terrains.Length; i++) {
                var p = Instantiate(button, new Vector3(0, 0, 0), Quaternion.identity, editorPanelContent.transform);
                p.GetComponent<EditorModeTerrainButton>().type = 0;
                p.GetComponent<EditorModeTerrainButton>().terrainId = i;
                p.GetComponent<Image>().sprite = MapControl.Instance.terrains[i].icon;
                var id = i;
                p.GetComponent<Button>().onClick.AddListener(() => {
                    currentItemType = 0;
                    currentId = id;
                });
            }
            for (int i = 0; i < MapControl.Instance.embellishments.Length; i++) {
                var p = Instantiate(button, new Vector3(0, 0, 0), Quaternion.identity, editorPanelContent.transform);
                p.GetComponent<EditorModeTerrainButton>().type = 1;
                p.GetComponent<EditorModeTerrainButton>().terrainId = i;
                p.GetComponent<Image>().sprite = MapControl.Instance.embellishments[i].icon;
                var id = i;
                p.GetComponent<Button>().onClick.AddListener(() => {
                    currentItemType = 1;
                    currentId = id;
                });
            }
            StartCoroutine(EditorMode());
        }
        private IEnumerator EditorMode() {
            while (this != null) {
                if (Input.GetMouseButton((int)MouseButton.LeftMouse) && CameraControl.IsInClickRange(Input.mousePosition)) {
                    var x = Physics2D.RaycastAll(_camera.ScreenPointToRay(Input.mousePosition).origin, Vector3.back);
                    if (x.Length < 1) {
                        yield return null;
                    }
                    var p = x[0];
                    if (p.transform.GetComponent<TileAttributes>() == null) {
                        yield return null;
                    }
                    if (currentItemType == 0) {
                        if (currentId != p.transform.GetComponent<TileAttributes>().terrain) {
                            p.transform.GetComponent<TileAttributes>().ChangeTerrainTo(currentId);
                        }
                    } else {
                        if (currentId != p.transform.GetComponent<TileAttributes>().embellishment) {
                            p.transform.GetComponent<TileAttributes>().ChangeEmbellishmentTo(currentId);
                        }
                    }
                }
                yield return null;
            }
        }
    }
}
