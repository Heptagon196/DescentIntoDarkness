using System;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Battle {
    [Serializable] public enum CursorType {
        Normal, Attack, Move, Select, None
    }
    public class CursorIconControl : MonoBehaviour {
        public Texture2D normal;
        public Texture2D attack;
        public Texture2D move;
        public Texture2D select;
        public CursorType currentType = CursorType.None;
        public static CursorIconControl Instance = null;
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
                return;
            }
            ChangeToCursorTo(CursorType.Normal);
        }
        public void ChangeToCursorTo(CursorType type) {
            if (type == currentType) {
                return;
            }
            if (type == CursorType.Normal) {
                Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
            } else if (type == CursorType.Attack) {
                Cursor.SetCursor(attack, Vector2.zero, CursorMode.Auto);
            } else if (type == CursorType.Move) {
                Cursor.SetCursor(move, Vector2.zero, CursorMode.Auto);
            } else if (type == CursorType.Select) {
                Cursor.SetCursor(@select, Vector2.zero, CursorMode.Auto);
            }
            currentType = type;
        }
        public void FixedUpdate() {
            var x = Physics2D.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Vector3.back);
            if (x.Length == 1) {
                if (BattleControl.Instance.isSelectingTarget) {
                    ChangeToCursorTo(CursorType.Move);
                } else {
                    ChangeToCursorTo(CursorType.Normal);
                }
            } else if (BattleControl.Instance.isSelectingTarget) {
                foreach (var i in x) {
                    if (i.transform.GetComponent<Unit>() != null && i.transform.GetComponent<Unit>().controlledByPlayer == false) {
                        ChangeToCursorTo(CursorType.Attack);
                        goto END;
                    }
                }
                ChangeToCursorTo(CursorType.Normal);
                END:;
            }
        }
    }
}
