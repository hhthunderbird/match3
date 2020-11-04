using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public delegate void OnUpdate();
    public static event OnUpdate onUpdate;

    public TextMeshProUGUI text_score;
    public TextMeshProUGUI text_scoreToBeat;

    public GameObject modal;

    public float roundTimeLimit;

    public int scoreToBeat;
    string scoreToBeatSave = "scoretobeat";

    int score;

    void Start() {
        modal.SetActive(false);
        scoreToBeat = PlayerPrefs.GetInt(scoreToBeatSave) + 10;
        PlayerPrefs.SetInt(scoreToBeatSave, scoreToBeat);
        PlayerPrefs.Save();

        text_scoreToBeat.SetText(scoreToBeat.ToString());

        TimeManager.SetLimit(roundTimeLimit);
    }

    private void OnEnable() {
        Grid.onScore += OnScore;
        TimeManager.onFinish += OnTimeEnds;
    }

    private void OnDisable() {
        Grid.onScore -= OnScore;
        TimeManager.onFinish -= OnTimeEnds;
    }


    void OnScore() {
        score++;
        text_score.SetText(score.ToString());
        modal.SetActive(score >= scoreToBeat);
    }

    void OnTimeEnds() {

    }

    private void Update() {
        onUpdate?.Invoke();
    }

    public void Restart() {
        PlayerPrefs.SetInt(scoreToBeatSave, 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
