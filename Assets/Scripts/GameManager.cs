using UnityEngine;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour {
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
    public Canvas highscoreCanvas;

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
                instance.highscoreCanvas.gameObject.SetActive(false);
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
                instance.highscoreCanvas.gameObject.SetActive(true);
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

    [Command]
    public void CmdRespawnFood() {
        if (NetworkServer.active)
            Networking.instance.RespawnFood();
    }
}