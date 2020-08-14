using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle {
    public class MiniMap : MonoBehaviour, IPointerClickHandler {
        public void OnPointerClick(PointerEventData eventData) {
            Debug.Log(eventData.pressEventCamera.name);
        }
    }
}
