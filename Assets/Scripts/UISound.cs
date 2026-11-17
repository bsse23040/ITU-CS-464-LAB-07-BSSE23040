using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The sounds are made in code (there are no audio files): a short blip for every button
// and a soft looping pad for the music. The two sliders in the settings control them.
public class UISound : MonoBehaviour
{
    static UISound instance;

    AudioSource blipSource;
    AudioSource padSource;
    AudioClip blip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        var go = new GameObject("UI Sound");
        instance = go.AddComponent<UISound>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        blipSource = gameObject.AddComponent<AudioSource>();
        padSource = gameObject.AddComponent<AudioSource>();
        blip = MakeBlip();
        padSource.clip = MakePad();
        padSource.loop = true;
        padSource.Play();
        Refresh();
    }

    // anything that is clicked makes a blip. It listens to the pointer for the whole UI at once.
    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || EventSystem.current == null) return;
        var data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var hits = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(data, hits);
        foreach (var hit in hits)
        {
            var button = hit.gameObject.GetComponentInParent<Button>();
            if (button != null && button.interactable)
            {
                blipSource.pitch = Random.Range(0.97f, 1.05f);
                blipSource.PlayOneShot(blip, PlayerData.Sfx);
                return;
            }
        }
    }

    public static void Refresh()
    {
        if (instance == null) return;
        instance.padSource.volume = PlayerData.Music * 0.18f;
    }

    static AudioClip MakeBlip()
    {
        const int rate = 22050;
        int samples = rate / 14;
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / rate;
            float fade = 1f - (float)i / samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * fade * fade * 0.5f;
        }
        var clip = AudioClip.Create("blip", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // four sine waves that make a calm chord, with a slow swell. Every frequency fits a whole number
    // of cycles into the eight seconds, so the loop has no click where it restarts.
    static AudioClip MakePad()
    {
        const int rate = 22050;
        const int seconds = 8;
        var data = new float[rate * seconds];
        float[] hz = { 110f, 165f, 220f, 277.5f };
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / rate;
            float sum = 0f;
            for (int k = 0; k < hz.Length; k++)
                sum += Mathf.Sin(2f * Mathf.PI * hz[k] * t) * (k == 0 ? 0.5f : 0.3f);
            float swell = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
            data[i] = sum * swell * 0.4f;
        }
        var clip = AudioClip.Create("pad", data.Length, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
