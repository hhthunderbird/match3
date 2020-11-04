using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Piece : MonoBehaviour, IPointerClickHandler {

    public delegate void OnSelected(Piece p);
    public static event OnSelected onSelected;

    public delegate void OnArrival(Piece p);
    public static event OnArrival onArrival;

    float moveDuration;

    public Transform t;
    public bool isSelected;
    public bool isSelectable;

    public bool isMoving { get; protected set; }

    GameObject selection;
    Image img_piece;

    Vector3 startPos;
    Vector3 target;

    [SerializeField]
    private int index;
    public int Index {
        get { return index; }
        set {
            index = value;
            x = value % Grid.size;
            y = value / Grid.size;
}
    }
    public int x { get; protected set; }
    public int y { get; protected set; }

    public int Value { get; protected set; }

    RectTransform recttransform;
    float currentTime;

    void OnEnable() {
        GameManager.onUpdate += UpdatePiece;
    }

    void OnDisable() {
        GameManager.onUpdate -= UpdatePiece;
    }

    void Awake() {
        t = transform;
        t.position = new Vector3(0, Screen.height, 0);
        recttransform = GetComponent<RectTransform>();
        selection = t.GetChild(0).gameObject;
        selection.SetActive(false);
        img_piece = GetComponent<Image>();
    }

    void Start() {
        Randomize();
    }

    public void Randomize() {
        Value = Random.Range(0, Grid.size);
        img_piece.sprite = SpriteManager.GetSprite((SpriteValue)Value);
    }

    public void SetSize(float f) {
        recttransform.sizeDelta = new Vector2(f, f);
    }

    public void MoveTo(Vector3 to, float _duration = 0) {
        currentTime = 0;
        moveDuration = _duration;
        startPos = t.position;
        target = to;
        isMoving = true;
    }

    public void Appear(Vector3 to, float _duration = 0) {
        currentTime = 0;
        moveDuration = _duration;
        startPos = new Vector3(to.x, Screen.height, 0);
        target = to;
        isMoving = true;
    }

    void UpdatePiece() {
        if (isMoving) {
            if (moveDuration > 0) {
                currentTime += Time.deltaTime;
                float time = currentTime / moveDuration;
                time = time * time * time * (time * (6f * time - 15f) + 10f);
                t.position = Vector3.Lerp(startPos, target, time);

                if (time >= 1) {
                    isMoving = false;
                    t.position = target;
                    onArrival?.Invoke(this);
                    Index = t.GetSiblingIndex();
                }
            } else {
                isMoving = false;
                t.position = target;
                onArrival?.Invoke(this);
                Index = t.GetSiblingIndex();
            }
        }
    }


    public void OnPointerClick(PointerEventData eventData) {
        Select(!isSelected);
    }

    void Select(bool b) {
        if (!isSelectable) {
            isSelected = b;
            if (isSelected) {
                onSelected?.Invoke(this);
            }
        } else {
            //SWAP
            onSelected?.Invoke(this);
        }
    }

    public void Clear() {
        isSelectable = false;
        isSelected = false;
        selection.SetActive(false);
    }

    public void MarkAsAdjacent(bool b) {
        if (!b) {
            Clear();
        }
        isSelectable = b;
        selection.SetActive(b);
    }
}