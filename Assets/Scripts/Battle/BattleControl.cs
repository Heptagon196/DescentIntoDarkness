using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MainMenu;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

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
        public bool loop;
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
        public static BattleControl Instance = null;
        public UnitType[] unitTypeList;
        [HideInInspector] public Dictionary<string, UnitType> unitType = new Dictionary<string, UnitType>();
        public List<Unit> player;
        public List<Unit> enemy;
        public Image timeSchedule;
        public Image worldColor;
        public Sprite[] timePoints;
        public Color[] timePointsColor;
        public int timeCount = 0;
        [HideInInspector] public bool isSelectingTarget = false;
        private void InitSprites() {
            foreach (var i in unitTypeList) {
                unitType[i.unitName] = i;
                foreach (var j in unitType[i.unitName].spritesList) {
                    unitType[i.unitName].sprites[j.name] = j;
                }
            }
        }
        public string GetChinese(UnitAttribute attr) {
            if (attr == UnitAttribute.Chaos) {
                return "混沌";
            } else if (attr == UnitAttribute.Neutral) {
                return "中立";
            } else if (attr == UnitAttribute.Order) {
                return "秩序";
            }
            return "无";
        }
        public string GetChinese(UnitSkillRange range) {
            if (range == UnitSkillRange.CloseCombat) {
                return "近战";
            } else if (range == UnitSkillRange.RangedAttack) {
                return "远程";
            }
            return "无";
        }
        public string GetChinese(UnitSkillType type) {
            if (type == UnitSkillType.Magical) {
                return "魔法";
            } else if (type == UnitSkillType.Physical) {
                return "物理";
            }
            return "无";
        }
        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
                return;
            }
            InitSprites();
            var canvas = GameObject.Find("Canvas");
            if (!EditorModeSwitch.IsEditorMode) {
                canvas.transform.GetChild(2).gameObject.SetActive(true);
                canvas.transform.GetChild(1).gameObject.SetActive(false);
                StartCoroutine(MainGame());
                StartCoroutine(RunStory("1.cfg"));
            } else {
                canvas.transform.GetChild(2).gameObject.SetActive(false);
                canvas.transform.GetChild(1).gameObject.SetActive(true);
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
        public void PauseGame() {
            CameraControl.GamePaused = true;
        }
        public void ContinueGame() {
            CameraControl.GamePaused = false;
        }
        private IEnumerator StartBattle(Unit p, Unit e) {
            PauseGame();
            BattlePanelControl.Instance.AddLeftMessage(p);
            BattlePanelControl.Instance.AddRightMessage(e);
            int choice = -1;
            BattlePanelControl.Instance.SetFirstListener(() => { choice = 0; });
            BattlePanelControl.Instance.SetSecondListener(() => { choice = 1; });
            BattlePanelControl.Instance.SetThirdListener(() => { choice = 2; });
            BattlePanelControl.Instance.ShowPanel();
            while (choice == -1) {
                yield return null;
            }
            BattlePanelControl.Instance.HidePanel();
            if (choice == 0 || choice == 1) {
                Unit.Description.text = "";
                p.remainAttacks = 0;
                UnitSkill pskill = null, eskill = null;
                int pleft = 0, eleft = 0;
                if (unitType[p.branch].skills.Length > choice) {
                    pskill = unitType[p.branch].skills[choice];
                    pleft = pskill.attacksPerTurn;
                }
                if (unitType[e.branch].skills.Length > choice) {
                    eskill = unitType[e.branch].skills[choice];
                    eleft = eskill.attacksPerTurn;
                }
                while (pleft != 0 || eleft != 0) {
                    if (pleft != 0) {
                        pleft--;
                        if (pskill != null) {
                            p.currentAnimation = pskill.name;
                            e.currentAnimation = "defend";
                            yield return new WaitForSeconds(0.5f);
                            e.health -= pskill.damagePerHit;
                        }
                        if (e.health <= 0) {
                            e.currentAnimation = "die";
                            yield return new WaitForSeconds(2f);
                            break;
                        }
                        if (p.health <= 0) {
                            p.currentAnimation = "die";
                            yield return new WaitForSeconds(2f);
                            break;
                        }
                    }
                    if (eleft != 0) {
                        eleft--;
                        if (eskill != null) {
                            e.currentAnimation = eskill.name;
                            p.currentAnimation = "defend";
                            yield return new WaitForSeconds(0.5f);
                            p.health -= eskill.damagePerHit;
                        }
                        if (e.health <= 0) {
                            e.currentAnimation = "die";
                            yield return new WaitForSeconds(2f);
                            break;
                        }
                        if (p.health <= 0) {
                            p.currentAnimation = "die";
                            yield return new WaitForSeconds(2f);
                            break;
                        }
                    }
                    yield return null;
                }
                if (p.health > 0) {
                    p.currentAnimation = "idle";
                } else {
                    Destroy(p.gameObject);
                }
                if (e.health > 0) {
                    e.currentAnimation = "idle";
                } else {
                    Destroy(e.gameObject);
                }
            }
            ContinueGame();
            yield return null;
        }
        private bool skip = false;
        private IEnumerator MainGame() {
            while (this != null) {
                // player's turn
                foreach (var i in player) {
                    if (i != null) {
                        i.remainSteps = unitType[i.branch].speed.stepCount;
                        i.remainAttacks = 1;
                    }
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
                        var e = (x[0].transform.GetComponent<Unit>() == null ? x[1] : x[0]).transform.GetComponent<Unit>();
                        bool adjacent = false;
                        foreach (var i in MapControl.Instance.G[p.transform.GetComponent<Unit>().pos]) {
                            if (Equals(i, e.pos)) {
                                adjacent = true;
                                break;
                            }
                        }
                        if (adjacent && p.transform.GetComponent<Unit>().remainAttacks == 1) {
                            yield return StartBattle(p.transform.GetComponent<Unit>(), e);
                        }
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
                timeCount = (timeCount + 1) % timePoints.Length;
                timeSchedule.sprite = timePoints[timeCount];
                worldColor.color = timePointsColor[timeCount];
                yield return null;
            }
        }
        public void SkipPlayerTurn() {
            skip = true;
        }
        string Convert(string s) {
            string ans = "";
            for (int i = 0; i < s.Length; i++) {
                if (i != s.Length - 1 && s[i] == '\\') {
                    if (s[i + 1] == 'n') {
                        ans += '\n';
                    } else if (s[i + 1] == 't') {
                        ans += '\t';
                    } else {
                        ans += s[i + 1];
                    }
                    i++;
                } else {
                    ans += s[i];
                }
            }
            return ans;
        }
        private IEnumerator RunStory(string filename) {
            var s = new StreamReader(filename);
            string l;
            while (!string.IsNullOrEmpty(l = s.ReadLine())) {
                string[] args = l.Split(' ');
                if (args[0] == "pause") {
                    PauseGame();
                } else if (args[0] == "continue") {
                    ContinueGame();
                } else if (args[0] == "story") {
                    if (args[1] == "start") {
                        StoryControl.Instance.ShowPanel();
                    } else if (args[1] == "end") {
                        StoryControl.Instance.HidePanel();
                    }
                } else if (args[0] == "t") {
                    string msg = "";
                    for (int i = 1; i < args.Length; i++) {
                        msg = msg + args[i] + " ";
                    }
                    StoryControl.Instance.SetText(Convert(msg));
                    while (!Input.GetMouseButtonDown((int) MouseButton.LeftMouse)) {
                        yield return null;
                    }
                } else if (args[0] == "pic") {
                    if (args[1] == "close") {
                        if (args[2] == "left") {
                            StoryControl.Instance.ClearLeftImage();
                        } else if (args[2] == "right") {
                            StoryControl.Instance.ClearRightImage();
                        } else if (args[2] == "all") {
                            StoryControl.Instance.ClearImage();
                        }
                    } else {
                        UnityWebRequest r = UnityWebRequestTexture.GetTexture("file://" + Path.GetFullPath(args[2]));
                        yield return r.SendWebRequest();
                        Texture t = DownloadHandlerTexture.GetContent(r);
                        if (args[1] == "left") {
                            StoryControl.Instance.SetLeftImage(t);
                        } else if (args[1] == "right") {
                            StoryControl.Instance.SetRightImage(t);
                        }
                    }
                } else if (args[0] == "msg") {
                    string msg = "";
                    for (int i = 1; i < args.Length; i++) {
                        msg = msg + args[i] + " ";
                    }
                    MessageBox.Instance.ShowMessage(Convert(msg));
                    while (!Input.GetMouseButtonDown((int) MouseButton.LeftMouse)) {
                        yield return null;
                    }
                    MessageBox.Instance.CloseMessage();
                } else if (args[0] == "until") {
                    if (args[1] == "death") {
                        List<GameObject> obj = new List<GameObject>();
                        foreach (var i in player) {
                            for (int j = 2; j < args.Length; j++) {
                                if (i != null && i.displayName == args[j]) {
                                    obj.Add(i.gameObject);
                                }
                            }
                        }
                        foreach (var i in enemy) {
                            for (int j = 2; j < args.Length; j++) {
                                if (i != null && i.displayName == args[j]) {
                                    obj.Add(i.gameObject);
                                }
                            }
                        }
                        while (true) {
                            foreach (var i in obj) {
                                if (i == null) {
                                    yield return null;
                                    goto BREAKOUT;
                                }
                            }
                            yield return null;
                        }
                        BREAKOUT:;
                    }
                } else if (args[0] == "jump") {
                    if (args.Length >= 3 && args[2] == "death") {
                        bool jump = true;
                        foreach (var i in player) {
                            for (int j = 3; j < args.Length; j++) {
                                if (i != null && i.displayName == args[j]) {
                                    jump = false;
                                }
                            }
                        }
                        foreach (var i in enemy) {
                            for (int j = 3; j < args.Length; j++) {
                                if (i != null && i.displayName == args[j]) {
                                    jump = false;
                                }
                            }
                        }
                        if (jump) {
                            int c = Int32.Parse(args[1]);
                            for (int i = 0; i < c; i++) {
                                s.ReadLine();
                            }
                        }
                    } else {
                        int c = Int32.Parse(args[1]);
                        for (int i = 0; i < c; i++) {
                            s.ReadLine();
                        }
                    }
                }
                yield return null;
            }
        }
    }
}
