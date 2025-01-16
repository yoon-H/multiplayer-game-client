using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public int targetFrameRate;
    public string version = "1.0.0";
    public int latency = 2;

    [Header("# Player Info")]
    public uint playerId;
    public string deviceId;
    public string userId;
    public string gameId;
    public uint score = 0;

    [Header("# Game Object")]
    public PoolManager pool;
    public NetworkManager networkManager;
    public RoomManager roomManager;
    public Player player;
    public GameObject hud;
    public GameObject GameStartUI;
    public GameObject Lobby;
    public Tiles Tiles;

    void Awake() {
        instance = this;
        Application.targetFrameRate = targetFrameRate;
        playerId = (uint)Random.Range(0, 4);
    }

    public void MoveToLobby()
    {
        NetworkManager.instance.SendGetGameSessionsPacket();

        GameStartUI.SetActive(false);
        Lobby.SetActive(true);
    }

    public void ReturnToLobby()
    {
        // 마지막 위치 보내기
        player.ExitGame();

        // 다른 사용자들 맵에서 지우기
        pool.RemoveAll();

        // 로비 새로 고침
        NetworkManager.instance.SendGetGameSessionsPacket();

        player.gameObject.SetActive(false);
        Tiles.ResetTiles();
        hud.SetActive(false);
        Lobby.SetActive(true);
        isLive = false;
    }

    public void GameStart() {
        player.deviceId = deviceId;
        player.gameObject.SetActive(true);
        hud.SetActive(true);
        Lobby.SetActive(false);
        isLive = true;
    }

    public void GameOver() {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine() {
        isLive = false;
        yield return new WaitForSeconds(0.5f);
    }

    public void GameRetry() {
        SceneManager.LoadScene(0);
    }

    public void GameQuit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        if (!isLive) {
            return;
        }
        gameTime += Time.deltaTime;
    }
}
