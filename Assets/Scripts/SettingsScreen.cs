using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Steering, vibration, notifications and graphics quality. Every change is saved straight away,
// there is no "apply" button to forget. Resetting progress needs two taps so it cannot happen by accident.
public class SettingsScreen : UIScreen
{
    [SerializeField] Slider steering;
    [SerializeField] TMP_Text steeringValue;
    [SerializeField] ToggleSwitch vibration;
    [SerializeField] ToggleSwitch notifications;
    [SerializeField] SegmentedControl quality;
    [SerializeField] StyledButton reset;
    [SerializeField] Image resetBorder;
    [SerializeField] RectTransform[] popItems;

    bool armed;
    int armTicket;

    protected override void Awake()
    {
        base.Awake();
        steering.onValueChanged.AddListener(v =>
        {
            PlayerData.Steering = v;
            steeringValue.text = Mathf.RoundToInt(v * 100f) + "%";
        });
        vibration.Toggled += on =>
        {
            PlayerData.Vibration = on;
            if (on) Handheld.Vibrate();   // does nothing on a computer, buzzes on a phone
        };
        notifications.Toggled += on => PlayerData.Notifications = on;
        quality.Changed += i =>
        {
            PlayerData.Quality = i;
            ApplyQuality();
        };
        reset.Button.onClick.AddListener(OnReset);
    }

    public override void OnOpen()
    {
        Load();
        Disarm();
        for (int i = 0; i < popItems.Length; i++)
            UIAnim.PopIn(popItems[i], 0.05f + i * 0.06f);
    }

    // put the controls on the saved values without firing the change events
    void Load()
    {
        steering.SetValueWithoutNotify(PlayerData.Steering);
        steeringValue.text = Mathf.RoundToInt(PlayerData.Steering * 100f) + "%";
        vibration.Set(PlayerData.Vibration, false);
        notifications.Set(PlayerData.Notifications, false);
        quality.Select(Mathf.Clamp(PlayerData.Quality, 0, 2), false);
    }

    static void ApplyQuality()
    {
        int levels = QualitySettings.names.Length;
        if (levels > 0) QualitySettings.SetQualityLevel(Mathf.Min(PlayerData.Quality, levels - 1), true);
    }

    void OnReset()
    {
        if (!armed)
        {
            armed = true;
            reset.SetLabel("TAP AGAIN TO CONFIRM");
            Tween.Tint(resetBorder, new Color(resetBorder.color.r, resetBorder.color.g, resetBorder.color.b, 1f), 0.2f);
            int mine = ++armTicket;
            // if nothing happens for three seconds the button relaxes again
            Tween.Value(reset, "arm", 3f, Ease.Linear, k => { }, () => { if (mine == armTicket) Disarm(); });
            return;
        }

        PlayerData.ResetProgress();
        Load();
        Disarm();
        Toast.Show("Progress reset");
    }

    void Disarm()
    {
        armed = false;
        armTicket++;
        reset.SetLabel("RESET PROGRESS");
        var c = resetBorder.color;
        c.a = 0f;
        resetBorder.color = c;
    }
}
