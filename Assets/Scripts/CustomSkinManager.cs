using UnityEngine;
using System.Collections.Generic;

public class CustomSkinManager : MonoBehaviour {
    static public CustomSkinManager instance;

    [SerializeField] private List<string> skinNames;
    [SerializeField] private List<Sprite> skinSprites;
    //private Dictionary<string, Sprite> skinDict;

    void Awake() {
        instance = this;
    }

    public static Sprite GetCustomSkin(string name) {
        for (int i = 0; i < instance.skinNames.Count; ++i)
            if (instance.skinNames[i] == name)
                return instance.skinSprites[i];
        return null;
    }
}
