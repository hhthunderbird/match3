using UnityEngine;

public enum SpriteValue { UM, DOIS, TRES, QUATRO, CINCO, SEIS, SETE }

public class SpriteManager : MonoBehaviour {

    public static SpriteManager instance;

    public Sprite[] sprites;

    void Start() {
        if(instance == null) {
            instance = this;
        }
    }

    public static Sprite GetSprite(SpriteValue value) {
        return instance.sprites[(int)value];
    }
}
