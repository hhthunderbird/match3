using UnityEngine;

public class SFXManager : MonoBehaviour {

    private static SFXManager instance;
    
    public AudioClip clear;
    public AudioClip select;
    public AudioClip swap;
    AudioSource source;

    private void Awake() {
        if(instance == null) {
            instance = this;
        }
        source = GetComponent<AudioSource>();
    }


    void Start() {

    }

    public static void PlayClear() {
        instance.source.PlayOneShot(instance.clear);
    }
    public static void PlaySelect() {
        instance.source.PlayOneShot(instance.select);
    }

    public static void PlaySwap() {
        instance.source.PlayOneShot(instance.swap);
    }
}
