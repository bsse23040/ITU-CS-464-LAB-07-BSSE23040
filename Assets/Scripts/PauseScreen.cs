using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The pause menu is an overlay: the game stays visible and dimmed behind it, and Time.timeScale is 0
// until the player resumes. The dark layer fades in while the panel pops in with a small overshoot.
public class PauseScreen : UIScreen
{
    [SerializeField] Image dim;
    [SerializeField] RectTransform panel;
    [SerializeField] TMP_Text subtitle;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] RectTransform[] buttons;
    [SerializeField] Button resume;
    [SerializeField] Button restart;
    [SerializeField] Button quit;
    [SerializeField] GameplayScreen gameplay;
    [SerializeField] float dimAlpha = 0.72f;

    protected override void Awake()
    {
        base.Awake();
        resume.onClick.AddListener(Resume);
        restart.onClick.AddListener(Restart);
        quit.onClick.AddListener(Quit);
    }

    public override void OnOpen()
    {
        Time.timeScale = 0f;
        subtitle.text = GameSession.LevelLabel;
        scoreText.text = gameplay.Score.ToString("N0");

        var c = dim.color;
        c.a = 0f;
        dim.color = c;
        c.a = dimAlpha;
        Tween.Tint(dim, c, 0.25f, Ease.OutQuad);

        panel.localScale = Vector3.one * 0.82f;
        Tween.Scale(panel, 1f, 0.38f, Ease.OutBack);

        for (int i = 0; i < buttons.Length; i++)
            UIAnim.PopIn(buttons[i], 0.12f + i * 0.06f, 0.9f);
    }

    void Resume()
    {
        Time.timeScale = 1f;
        ScreenManager.Instance.GoBack();
    }

    void Restart()
    {
        Time.timeScale = 1f;
        gameplay.StartRun();
        ScreenManager.Instance.GoBack();
    }

    void Quit()
    {
        Time.timeScale = 1f;
        ScreenManager.Instance.OpenFresh(ScreenId.MainMenu);
    }

    public override bool HandleBack()
    {
        Resume();
        return true;
    }
}
