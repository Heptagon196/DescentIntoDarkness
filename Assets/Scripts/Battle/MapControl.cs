using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

namespace Battle {
    [Serializable]
    public enum TerrainType {
        Plain, Forest, Water, Desert, Swamp, Mountain, None
    }
    [Serializable] public class SpriteGroup {
        public string name;
        public string chineseName;
        public int order;
        public float transparency = 1;
        public Sprite icon;
        public Sprite[] sprites;
        public Sprite[] spread;
        public TerrainType type;
    }
    public class MapControl : MonoBehaviour {
        [HideInInspector] public GameObject[,] map = new GameObject[100, 100];
        public static MapControl Instance = null;
        public GameObject tileObj;
        public int width = 30;
        public int height = 30;
        public SpriteGroup[] terrains;
        public SpriteGroup[] embellishments;
        public GameObject tilesContainer;
        [HideInInspector] public Dictionary<Tuple<int, int>, List<Tuple<int, int>>> G = new Dictionary<Tuple<int, int>, List<Tuple<int, int>>>();
        private readonly Dictionary<string, int> _terrainId = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _embellishmentId = new Dictionary<string, int>();
        private Random rand = new Random(114514);
        private readonly int[,] dx = new int[2, 6] {{0, 1, -1, 0, 1, -1}, {0, 1, -1, 0, 1, -1}};
        private readonly int[,] dy = new int[2, 6] {{1, 0, 0, -1, -1, -1}, {1, 1, 1, -1, 0, 0}};
        private void Start() {
            if (Instance == null) {
                Instance = this;
            } else if (this != Instance) {
                Destroy(this);
                return;
            }
            for (int i = 0; i < terrains.Length; i++) {
                _terrainId[terrains[i].name] = i;
            }
            for (int i = 0; i < embellishments.Length; i++) {
                _embellishmentId[embellishments[i].name] = i;
            }
            GenerateGraph();
            LoadMap("1.map");
        }
        private void GenerateGraph() {
            for (int row = 0; row < height; row++) {
                for (int column = 0; column < width; column++) {
                    var p = new List<Tuple<int, int>>();
                    for (int i = 0; i < 6; i++) {
                        int px = column + dx[column % 2, i];
                        int py = row + dy[column % 2, i];
                        if (px >= 0 && px < width && py >= 0 && py < height) {
                            p.Add(new Tuple<int, int>(py, px));
                        }
                    }
                    G[new Tuple<int, int>(row, column)] = p;
                }
            }
        }
        public static Vector2 GetTilePositionOnScreen(int x, int y) {
            return new Vector2(x * 0.54f, y * 0.72f + (x % 2 == 0 ? 0 : 0.36f));
        }
        public Sprite GetRandomSprite(Sprite[] s) {
            if (s == null || s.Length == 0) {
                return null;
            }
            return s[rand.Next(0, s.Length)];
        }
        private void ModifyTile(GameObject tile, string terrain, string embellishment) {
            if (!string.IsNullOrEmpty(terrain)) {
                tile.GetComponent<TileAttributes>().ChangeTerrainTo(_terrainId[terrain]);
            }
            if (!string.IsNullOrEmpty(embellishment)) {
                tile.GetComponent<TileAttributes>().ChangeEmbellishmentTo(_embellishmentId[embellishment]);
            }
        }
        public void LoadMap(string fileName) {
            var s = new StreamReader(fileName);
            var l = s.ReadLine();
            if (string.IsNullOrEmpty(l)) {
                return;
            }
            rand = new Random(114514);
            width = Int32.Parse((l.Split(' '))[0]);
            height = Int32.Parse((l.Split(' '))[1]);
            var row = 0;
            while (!string.IsNullOrEmpty(l = s.ReadLine())) {
                var p = l.Split(' ');
                for (int column = 0; column < p.Length; column++) {
                    if (map[row, column] == null) {
                        map[row, column] = Instantiate(tileObj, GetTilePositionOnScreen(column, row), Quaternion.identity);
                        map[row, column].transform.SetParent(tilesContainer.transform);
                        map[row, column].name += row + "-" + column;
                        map[row, column].GetComponent<TileAttributes>().x = column;
                        map[row, column].GetComponent<TileAttributes>().y = row;
                    }
                    if (p[column] == "Water") {
                        continue;
                    }
                    var x = p[column].Split('^');
                    if (x.Length == 2) {
                        ModifyTile(map[row, column], x[0], x[1]);
                    } else {
                        ModifyTile(map[row, column], x[0], "");
                    }
                }
                row++;
            }
            s.Close();
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    UpdateTransition(i, j);
                }
            }
        }
        public void SaveMap(string fileName) {
            var s = new StreamWriter(fileName, false);
            s.WriteLine(width + " " + height);
            for (int row = 0; row < height; row++) {
                for (int column = 0; column < width; column++) {
                    var x = map[row, column].GetComponent<TileAttributes>();
                    if (x.terrain != -1) {
                        s.Write(terrains[x.terrain].name);
                    } else {
                        s.Write("Water");
                    }
                    if (x.embellishment != -1 && x.embellishment != 0) {
                        s.Write("^" + embellishments[x.embellishment].name);
                    }
                    s.Write(" ");
                }
                s.WriteLine();
            }
            s.Close();
        }
        public void QuitGame() {
            Application.Quit();
        }
        private int GetOrder(int x, int y) {
            return terrains[map[y, x].GetComponent<TileAttributes>().terrain].order;
        }
        public void UpdateTransition(int x, int y) {
            for (int i = 0; i < 6; i++) {
                map[y, x].transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().sprite = null;
            }
            for (int i = 0; i < 6; i++) {
                int px = x + dx[x % 2, i];
                int py = y + dy[x % 2, i];
                if (px >= 0 && px < width && py >= 0 && py < height) {
                    if (GetOrder(px, py) > GetOrder(x, y)) {
                        map[y, x].transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().sprite = terrains[map[py, px].GetComponent<TileAttributes>().terrain].spread[i];
                        var color = map[y, x].transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().color;
                        color.a = terrains[map[py, px].GetComponent<TileAttributes>().terrain].transparency;
                        map[y, x].transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().color = color;
                    }
                }
            }
        }
        private void Update() {
            if (Input.GetMouseButtonUp((int) MouseButton.LeftMouse) && CameraControl.IsInClickRange(Input.mousePosition)) {
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        UpdateTransition(i, j);
                    }
                }
            }
        }
    }
}
