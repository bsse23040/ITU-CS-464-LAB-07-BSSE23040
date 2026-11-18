using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A very small stand-in for the real game, so the pause menu and the results screen have something to
// belong to. Rocks fall, the ship follows your finger, and the score goes up with time.
// It uses scaled time, so Time.timeScale = 0 (the pause menu) really stops it.
public class GameplayScreen : UIScreen
{
    [SerializeField] RectTransform playArea;
    [SerializeField] RectTransform ship;
    [SerializeField] Image shipImage;
    [SerializeField] Sprite[] shipSprites;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] RectTransform[] rocks;
    [SerializeField] Button pauseButton;
    [SerializeField] Button finishButton;
    [SerializeField] float scorePerSecond = 40f;

    float time;
    float[] rockSpeeds;
    int shownScore = -1;

    public int Score => Mathf.FloorToInt(time * scorePerSecond);

    protected override void Awake()
    {
        base.Awake();
        pauseButton.onClick.AddListener(OpenPause);
        finishButton.onClick.AddListener(Finish);
    }

    public override void OnOpen()
    {
        // coming back from the pause menu is not a new run
        if (GameSession.freshRun) StartRun();
        GameSession.freshRun = false;
    }

    public void StartRun()
    {
        time = 0f;
        shownScore = -1;
        shipImage.sprite = shipSprites[PlayerData.EquippedShip];
        levelText.text = GameSession.LevelLabel;
        ship.anchoredPosition = new Vector2(0f, ship.anchoredPosition.y);

        rockSpeeds = new float[rocks.Length];
        for (int i = 0; i < rocks.Length; i++) ResetRock(i, Random.Range(0f, 1400f));
    }

    void ResetRock(int i, float extraHeight)
    {
        float halfWidth = playArea.rect.width / 2f - 70f;
        rocks[i].anchoredPosition = new Vector2(Random.Range(-halfWidth, halfWidth), 120f + extraHeight);
        rockSpeeds[i] = Random.Range(420f, 760f);
    }

    void Update()
    {
        if (rockSpeeds == null) return;
        float dt = Time.deltaTime;
        time += dt;

        int score = Score;
        if (score != shownScore)
        {
            shownScore = score;
            scoreText.text = score.ToString("N0");
        }

        // the ship follows a finger (or the held mouse button) and sways gently when nothing touches the screen
        float targetX = Mathf.Sin(time * 1.1f) * 150f;
        if (Input.GetMouseButton(0))
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(playArea, Input.mousePosition, null, out local);
            float halfWidth = playArea.rect.width / 2f - 90f;
            targetX = Mathf.Clamp(local.x, -halfWidth, halfWidth);
        }
        Vector2 p = ship.anchoredPosition;
        // the Steering setting decides how quickly the ship follows the finger
        float follow = Mathf.Lerp(5f, 18f, PlayerData.Steering);
        float newX = Mathf.Lerp(p.x, targetX, 1f - Mathf.Exp(-follow * dt));
        ship.anchoredPosition = new Vector2(newX, p.y);
        ship.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp((p.x - targetX) * 0.06f, -14f, 14f));

        float height = playArea.rect.height;
        for (int i = 0; i < rocks.Length; i++)
        {
            Vector2 r = rocks[i].anchoredPosition;
            r.y -= rockSpeeds[i] * dt;
            rocks[i].anchoredPosition = r;
            rocks[i].localRotation = Quaternion.Euler(0f, 0f, rocks[i].localEulerAngles.z + 60f * dt);
            if (r.y < -height - 120f) ResetRock(i, 0f);
        }
    }

    void OpenPause()
    {
        ScreenManager.Instance.Open(ScreenId.Pause);
    }

    void Finish()
    {
        GameSession.Finish(Score);
        ScreenManager.Instance.Open(ScreenId.Results);
    }

    public override bool HandleBack()
    {
        OpenPause();
        return true;
    }

    // a phone that goes to the home screen should not keep running the level
    void OnApplicationPause(bool paused)
    {
        if (paused && ScreenManager.Instance != null && ScreenManager.Instance.Current == ScreenId.Gameplay)
            OpenPause();
    }
}
