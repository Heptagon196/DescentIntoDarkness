using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Battle {
    public class BattlePanelControl : MonoBehaviour {
        public static BattlePanelControl Instance = null;
        private Button firstButton, secondButton, thirdButton;
        private Text l, r;
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
            }
        }
        private void Start() {
            l = transform.GetChild(0).GetChild(1).GetComponent<Text>();
            r = transform.GetChild(0).GetChild(2).GetComponent<Text>();
            firstButton = transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Button>();
            secondButton = transform.GetChild(0).GetChild(3).GetChild(1).GetComponent<Button>();
            thirdButton = transform.GetChild(0).GetChild(3).GetChild(2).GetComponent<Button>();
        }
        private void AddMessage(Text t, Unit u) {
            t.text = "";
            foreach (var i in BattleControl.Instance.unitType[u.branch].skills) {
                t.text += i.name + "\n" +
                          BattleControl.Instance.GetChinese(i.range) + " " + BattleControl.Instance.GetChinese(i.type) + "\n" +
                          i.damagePerHit + " X " + i.attacksPerTurn + "\n\n";
            }
        }
        public void AddLeftMessage(Unit u) {
            AddMessage(l, u);
        }
        public void AddRightMessage(Unit u) {
            AddMessage(r, u);
        }
        public void ShowPanel() {
            transform.GetChild(0).gameObject.SetActive(true);
        }
        public void HidePanel() {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        public void SetFirstListener(UnityAction call) {
            firstButton.onClick.RemoveAllListeners();
            firstButton.onClick.AddListener(call);
        }
        public void SetSecondListener(UnityAction call) {
            secondButton.onClick.RemoveAllListeners();
            secondButton.onClick.AddListener(call);
        }
        public void SetThirdListener(UnityAction call) {
            thirdButton.onClick.RemoveAllListeners();
            thirdButton.onClick.AddListener(call);
        }
    }
}
