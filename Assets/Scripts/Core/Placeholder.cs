using UnityEngine;
public class Placeholder : MonoBehaviour {
    public Vector3 position;
    public GameObject mark;
    private void Awake() {
        mark = transform.GetChild(0).gameObject;
        mark.SetActive(false);
    }
}