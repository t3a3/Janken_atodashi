using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("オーディオミキサー")]
    [SerializeField]
    AudioMixer mixer;

    [Header("オーディオソース")]
    [SerializeField]
    AudioSource bgmAudioSource;
    [SerializeField]
    GameObject systemSE_AudioSourceObj;
    AudioSource[] systemSE_AudioSources;
    [SerializeField]
    GameObject gamingSE_AudioSourceObj;
    AudioSource[] gamingSE_AudioSources;

    [Header("スライダー類")]
    [SerializeField]
    Slider bgmSlider;
    [SerializeField]
    Slider seSlider;

    private void Awake()
    {
        //
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        //systemSEオブジェクトに付いているオーディオソースを全て取得
        systemSE_AudioSources =systemSE_AudioSourceObj.GetComponents<AudioSource>();
        //systemSEオブジェクトに付いているオーディオソースを全て取得
        gamingSE_AudioSources = systemSE_AudioSourceObj.GetComponents<AudioSource>();
        //----------
    }
    void Start()
    {
        bgmSlider.onValueChanged.AddListener(BGM_SliderOnValueChange);
        seSlider.onValueChanged.AddListener(SE_SliderOnValueChange);

        float bgmvalue = PlayerPrefs.GetFloat("BGM", 1);
        float sevalue= PlayerPrefs.GetFloat("SE", 1);
        bgmAudioSource.volume = bgmvalue;
        bgmSlider.value = bgmvalue;
        foreach (AudioSource audioSource in systemSE_AudioSources)
        {
            audioSource.volume = sevalue; 
        }
        seSlider.value = sevalue;
    }

    /// <summary>
	/// BGMスライドバー値の変更イベント
	/// </summary>
	/// <param name="value">スライドバーの値(自動的に引数に値が入る)</param>
	public void BGM_SliderOnValueChange(float value)
    {
        bgmAudioSource.volume = value;
    }

    /// <summary>
	/// SEスライドバー値の変更イベント
	/// </summary>
	/// <param name="value">スライドバーの値(自動的に引数に値が入る)</param>
	public void SE_SliderOnValueChange(float value)
    {
        foreach (AudioSource audioSource in systemSE_AudioSources)
        {
            audioSource.volume = value;
        }
        foreach (AudioSource audioSource in gamingSE_AudioSources)
        {
            audioSource.volume = value;
        }
        PlayerPrefs.SetFloat("SE", value);
    }

    // 現在再生されていないオーディオソースを取得して再生する
    public void PlaySystemSE(AudioClip clip)
    {
        foreach (AudioSource seSource in systemSE_AudioSources)
        {
            if (!seSource.isPlaying)
            {
                seSource.clip = clip;
                seSource.Play();
                return; // 再生が完了したら処理を終了
            }
        }
    }
    // 現在再生されていないオーディオソースを取得して再生する
    public void PlayGamingSE(AudioClip clip)
    {
        foreach (AudioSource seSource in gamingSE_AudioSources)
        {
            if (!seSource.isPlaying)
            {
                seSource.clip = clip;
                seSource.Play();
                return; // 再生が完了したら処理を終了
            }
        }
    }
}
