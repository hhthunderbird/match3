using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour {

    public delegate void OnScore();
    public static event OnScore onScore;

    public delegate void OnStart();
    public static event OnStart onStart;

    public Transform gridParent;
    public GameObject prefab;
    public float moveDuration;
    public float swapDuration;
    public float fillDelay;

    public enum AdjMode { ORTOGONAL, DIAGONAL, FULL }

    Stack<Piece> outPieces;

    public static int size;
    float cellSize;

    Dictionary<int, Piece> map;

    Piece selectedPiece;

    SortedSet<int> blank;

    WaitForSeconds one;
    WaitForSeconds dotFive;
    WaitForSeconds dotZeroFive;

    private void OnEnable() {
        Piece.onSelected += SetSelected;
        Piece.onArrival += OnArrival;
    }

    private void OnDisable() {
        Piece.onSelected -= SetSelected;
        Piece.onArrival -= OnArrival;
    }

    private void Awake() {
        size = 6;
    }

    IEnumerator Start() {
        blank = new SortedSet<int>();
        //pieces = new Piece[size * size];
        map = new Dictionary<int, Piece>();


        outPieces = new Stack<Piece>();
        one = new WaitForSeconds(1f);
        dotFive= new WaitForSeconds(0.5f);
        dotZeroFive = new WaitForSeconds(0.05f);

        GridLayoutGroup gridLayout = GetComponentInChildren<GridLayoutGroup>();
        gridLayout.enabled = true;
        gridLayout.cellSize = new Vector2(Screen.width / (float)size, Screen.width / (float)size);
        cellSize = gridLayout.cellSize.x;
        gridLayout.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.width);
        yield return one;
        gridLayout.enabled = false;

        for (int i = 0; i < (size * size); i++) {
            Piece p = Instantiate(prefab, gridParent).GetComponent<Piece>();
            p.Appear(IndexPosition(i), 0.2f);
            p.SetSize(gridLayout.cellSize.x);
            yield return dotZeroFive;
        }
        
        Invoke("SetStart", moveDuration);
    }

    void SetStart() {
        onStart?.Invoke();
        StartCoroutine("EvaluateCaller");
    }


    Vector3 IndexPosition(int i) {
        int x = (i % size);
        int y = (i / size);
        return new Vector3(x * cellSize, y * cellSize, 0);
    }


    Piece GetPieceAt(int index) {
        map.TryGetValue(index, out Piece p);
        return p;
    }

    void MarkAdjacents(Piece p) {
        List<Piece> adjs = Adjacents(p, AdjMode.ORTOGONAL);
        foreach (Piece item in adjs) {
            item.MarkAsAdjacent(true);
        }
    }

    List<Piece> Adjacents(Piece p, AdjMode m, bool ofTheSame = false) {
        List<Piece> ps = new List<Piece>();

        foreach (var item in map) {
            int x = item.Value.Index % size;
            int y = item.Value.Index / size;

            bool select =
                (x == p.x && y == p.y + 1) && ((m == AdjMode.ORTOGONAL && m != AdjMode.DIAGONAL) || m == AdjMode.FULL) || //top
                (x == p.x && y == p.y - 1) && ((m == AdjMode.ORTOGONAL && m != AdjMode.DIAGONAL) || m == AdjMode.FULL) || //down
                (x == p.x + 1 && y == p.y) && ((m == AdjMode.ORTOGONAL && m != AdjMode.DIAGONAL) || m == AdjMode.FULL) || //right
                (x == p.x - 1 && y == p.y) && ((m == AdjMode.ORTOGONAL && m != AdjMode.DIAGONAL) || m == AdjMode.FULL) || //left

                (y == p.y + 1 && x == p.x + 1) && ((m != AdjMode.ORTOGONAL && m == AdjMode.DIAGONAL) || m == AdjMode.FULL) || //topright
                (y == p.y + 1 && x == p.x - 1) && ((m != AdjMode.ORTOGONAL && m == AdjMode.DIAGONAL) || m == AdjMode.FULL) || //topleft
                (y == p.y - 1 && x == p.x + 1) && ((m != AdjMode.ORTOGONAL && m == AdjMode.DIAGONAL) || m == AdjMode.FULL) || //downright
                (y == p.y - 1 && x == p.x - 1) && ((m != AdjMode.ORTOGONAL && m == AdjMode.DIAGONAL) || m == AdjMode.FULL); //downleft

            if (select) {
                if (ofTheSame) {
                    if (item.Value.Value == p.Value) {
                        ps.Add(item.Value);
                    }
                } else {
                    ps.Add(item.Value);
                }
            }
        }
        return ps;
    }

    void SetSelected(Piece p) {
        if (!p.isSelectable) {
            p.isSelected = true;
            if (p.isSelected) {
                SFXManager.PlaySelect();
                Clear();
                selectedPiece = p;
                MarkAdjacents(p);
            } else {
                Clear();
            }
        } else {
            //SWAP
            Swap(p);
        }
    }

    void OnArrival(Piece p) {
        int x = (int)(p.t.position.x / cellSize);
        int y = (int)(p.t.position.y / cellSize);

        int idx = size * y + x;

        map[idx] = p;

        if (gridParent.childCount >= idx) {
            p.t.SetSiblingIndex(idx);
        }
        p.name = idx.ToString();

        StopCoroutine("EvaluateCaller");
        StartCoroutine("EvaluateCaller");
    }

    public void Clear() {
        foreach (KeyValuePair<int, Piece> item in map) {
            item.Value.Clear();
        }
    }

    public void Swap(Piece p) {
        SFXManager.PlaySwap();
        Clear();

        int pIndex = p.Index;
        p.Index = selectedPiece.Index;
        selectedPiece.Index = pIndex;

        map[selectedPiece.Index] = selectedPiece;
        map[p.Index] = p;

        p.MoveTo(IndexPosition(p.Index), moveDuration);
        selectedPiece.MoveTo(IndexPosition(selectedPiece.Index), moveDuration);

        StopCoroutine("EvaluateCaller");
        StartCoroutine("EvaluateCaller");
    }

    IEnumerator EvaluateCaller() {
        yield return dotFive;
        Evaluate();
    }

    void Evaluate() {
        blank.Clear();
        for (int i = 0; i < (size * size); i++) {
            int c = i % size;
            int l = i / size;

            if (c < size - 2) {
                Piece p1 = map.ElementAt(i).Value;
                Piece p2 = map.ElementAt(i + 1).Value;
                Piece p3 = map.ElementAt(i + 2).Value;

                if ((p1.Value == p2.Value) && (p2.Value == p3.Value)) {
                    blank.Add(i);
                    blank.Add(i + 1);
                    blank.Add(i + 2);
                }
            }
            //por coluna
            if (l <= 3) {
                Piece p1 = map.ElementAt(i).Value;
                Piece p2 = map.ElementAt(i + 6).Value;
                Piece p3 = map.ElementAt(i + 12).Value;

                if ((p1.Value == p2.Value) && (p2.Value == p3.Value)) {
                    blank.Add(i);
                    blank.Add(i + 6);
                    blank.Add(i + 12);
                }
            }
        }


        if (blank.Count > 0) {
            foreach (var item in blank) {
                Remove(map[item]);
                onScore?.Invoke();
                SFXManager.PlayClear();
            }

            Invoke("Fill", fillDelay);
        } else {
            foreach (var item in map) {
                IsThereAMove(item.Value);
            }

            foreach (var item in map) {
                if (IsThereAMove(item.Value)) {
                    return;
                }
            }
            Shuffle();
        }
    }

    void Fill() {
        for (int i = 0; i < (size * size); i++) {
            if (map.ElementAt(i).Value == null) {
                Piece p = null;
                int upperIndex = i;

                bool fromPool = false;

                while (p == null) {
                    upperIndex += 6;

                    //GET FROM POOL
                    if (upperIndex > (size * size) - 1) {
                        fromPool = true;
                        p = outPieces.Pop();
                        p.t.position = new Vector3(IndexPosition(i).x, Screen.height, 0);
                        p.Randomize();
                        continue;
                    }
                    p = GetPieceAt(upperIndex);
                }
                if (!fromPool) {
                    map[upperIndex] = null;
                }
                map[i] = p;
                p.Index = i;
            }
        }

        foreach (var item in map) {
            Piece p = item.Value;
            p.MoveTo(IndexPosition(p.Index), moveDuration);
        }
    }

    bool IsThereAMove(Piece first) {
        List<Piece> adjs = Adjacents(first, AdjMode.FULL, ofTheSame: true);

        foreach (Piece second in adjs) {
            int x = second.Index % size;
            int y = second.Index / size;

            var adjOfAdj = Adjacents(second, AdjMode.FULL, ofTheSame: true);


            if (second.x == first.x) { //ortogonal horizontal
                if (adjOfAdj.Where(o => o.x != first.x).Count() > 0) {
                    return true;
                }
            } else if (second.y == first.y) { //ortogonal vertical
                if (adjOfAdj.Where(o => o.y != first.y).Count() > 0) {
                    return true;
                }
            }

            if (second.x != first.x && second.y != first.y) { //diagonal
                if (second.x > first.x && second.y > first.y) { //top right

                    if (adjOfAdj.Where(o => (o.x >= first.x) || (o.y >= first.y) && (o.x < second.x + 1 && o.y < second.y + 1)).Count() > 0) {
                        return true;
                    }
                } else if (second.x > first.x && second.y > first.y) { //down right

                    if (adjOfAdj.Where(o => (o.x >= first.x) || (o.y <= first.y) && (o.x < second.x + 1 && o.y > second.y - 1)).Count() > 0) {
                        return true;
                    }
                } else if (second.x > first.x && second.y > first.y) { //down left
                    if (adjOfAdj.Where(o => (o.x <= first.x) || (o.y <= first.y) && (o.x > second.x - 1 && o.y > second.y - 1)).Count() > 0) {
                        return true;
                    }
                } else if (second.x > first.x && second.y > first.y) { //top left
                    if (adjOfAdj.Where(o => (o.x <= first.x) || (o.y >= first.y) && (o.x > second.x - 1 && o.y < second.y + 1)).Count() > 0) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void Shuffle() {
        foreach (KeyValuePair<int, Piece> item in map) {
            item.Value.Randomize();
        }
    }

    void Remove(Piece p) {
        p.t.position = Vector3.up * Screen.height;
        outPieces.Push(p);
        map[p.Index] = null;
        p.Index = -1;
    }
}