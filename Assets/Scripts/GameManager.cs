using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public enum GameStates {
        Login,
        Playing,
        Paused
    }

    [SerializeField]
    private GameStates state = GameStates.Playing;
    public static GameStates State {
        get { return instance.state; }
        private set { instance.state = value; }
    }

    public Canvas pauseCanvas;

	void Awake() {
        instance = this;
	}

    public static void ChangeState(GameStates newState) {
        if (instance.state == newState)
            return;

        // Before change
        switch (instance.state) {
            case GameStates.Login:
                break;
            case GameStates.Playing:
                break;
            case GameStates.Paused:
                instance.pauseCanvas.gameObject.SetActive(false);
                break;
        }

        instance.state = newState;

        // After change
        switch (instance.state) {
            case GameStates.Login:
                break;
            case GameStates.Playing:
                break;
            case GameStates.Paused:
                instance.pauseCanvas.gameObject.SetActive(true);
                break;
        }
    }

    public void Pause() {
        ChangeState(GameStates.Paused);
    }

    public void Unpause() {
        ChangeState(GameStates.Playing);
    }

    void Update() {
        switch (state) {
            case GameStates.Login:
                break;
            case GameStates.Playing:
                if (Input.GetKeyDown(KeyCode.Escape))
                    ChangeState(GameStates.Paused);
                break;
            case GameStates.Paused:
                break;
        }
    }
}