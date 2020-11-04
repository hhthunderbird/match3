using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour {

    public delegate void OnFinish();
    public static event OnFinish onFinish;

    public TextMeshProUGUI text_time;

    private static TimeManager instance;

    public float timeLimit;
    float time;
    public bool go;

    char[] numbers;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    void Start() {
        numbers = new char[4];
    }

    void OnEnable() {
        GameManager.onUpdate += UpdateTimer;
        Grid.onStart += SetStart;
    }

    void OnDisable() {
        GameManager.onUpdate -= UpdateTimer;
        Grid.onStart -= SetStart;
    }

    public static void SetLimit(float f) {
        instance.timeLimit = f;
    }

    void SetStart() {
        go = true;
    }

    // Update is called once per frame
    void UpdateTimer() {
        if (go) {
            time += Time.deltaTime;

            if(time >= timeLimit) {
                go = false;
                onFinish?.Invoke();
            }
                
            int minutes = (int)(time / 60f);
            numbers[0] = (char)(48 + (minutes % 10));
            numbers[1] = ':';

            int seconds = (int)(time - minutes * 60);
            numbers[2] = (char)(48 + seconds * 0.1f);
            numbers[3] = (char)(48 + seconds % 10);
            text_time.SetCharArray(numbers);
        }
    }
}
