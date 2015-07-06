using System;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreManager : MonoBehaviour {

    public Transform listTransform;
    Text[] players = new Text[10];

    private CellMovement[] cells;

    void Awake() {
        for (int i = 0; i < 10; ++i) {
            players[i] = listTransform.GetChild(i).GetComponent<Text>();
        }
    }

    /*
	void Update () {
        cells = FindObjectsOfType<CellPlayerNonAuthoritative>();
        Array.Sort<CellPlayerNonAuthoritative>(cells, (x, y) => y.theSize.GetSize().CompareTo(x.theSize.GetSize()));

        for (int i = 0; i < 10; ++i) {
            players[i].text = (i < cells.Length) ? (i+1).ToString() + ". " + cells[i].name : "";
        }
	}
    */
}
