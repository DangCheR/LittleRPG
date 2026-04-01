using QFramework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

public interface IAudioModel : IModel
{
    BindableProperty<bool> IsSoundEffectsOpen { get; }
    BindableProperty<bool> IsBGMOpen { get; }
    BindableProperty<float> SoundEffectsVolume { get; }
    BindableProperty<float> BGMVolume { get; }
}

public class AudioModel : AbstractModel, IAudioModel
{
    public BindableProperty<bool> IsSoundEffectsOpen { get; } = new BindableProperty<bool>(true);
    public BindableProperty<bool> IsBGMOpen { get; } = new BindableProperty<bool>(true);
    public BindableProperty<float> SoundEffectsVolume { get; } = new BindableProperty<float>(1.0f);
    public BindableProperty<float> BGMVolume { get; } = new BindableProperty<float>(1.0f);

    protected override void OnInit()
    {
        // 这里可以接入 PlayerPrefs，读取玩家本地保存的设置
        // IsBGMOpen.Value = PlayerPrefs.GetInt("BGMOpen", 1) == 1;
    }
}
// 音效播放仅接收事件
// 主动触发音量大小是设置的事情
interface IAudioSystem : ISystem
{
    void PlayBGM(AudioClip clip);

    // UI音效
    void Play2DSFX(AudioClip clip);

    // 世界音效
    void Play3DSFX(AudioClip clip, Vector3 v);
}


class AudioSystem : AbstractSystem, IAudioSystem
{
    private AudioSource mBGMSource;

    private IObjectPool<AutoRecycleAudio> mSourcePool;    // 定义一个对象池，用于存储音效播放器
    private Transform mPoolRoot; //对象池根节点

    protected override void OnInit()
    {
        // 1. 动态创建一个全局不销毁的物体，用来放播放器
        var audioGO = new GameObject("GlobalAudioSystem");
        Object.DontDestroyOnLoad(audioGO);
        mBGMSource = audioGO.AddComponent<AudioSource>();

        mBGMSource.loop = true; // BGM默认循环

        // 获取数据模型
        var audioModel = this.GetModel<IAudioModel>();

        // 监听BGM数据变化
        audioModel.IsBGMOpen.RegisterWithInitValue(isOpen => mBGMSource.mute = !isOpen);
        audioModel.BGMVolume.RegisterWithInitValue(v => mBGMSource.volume = v);


        mPoolRoot = new GameObject("SFX_Pool").transform;
        Object.DontDestroyOnLoad(mPoolRoot.gameObject);

        // 使用 Unity 自带的对象池
        mSourcePool = new ObjectPool<AutoRecycleAudio>(
            createFunc: () =>
            {
                var go = new GameObject("3D_SFX");
                go.transform.SetParent(mPoolRoot);
                var audio = go.AddComponent<AutoRecycleAudio>();
                audio.Init(mSourcePool, audioModel); 
                return audio;
            },
            actionOnGet: obj => obj.gameObject.SetActive(true), // get时
            actionOnRelease: obj => obj.gameObject.SetActive(false), //回收时
            defaultCapacity: 10,
            maxSize: 50
        );
    }

    public void PlayBGM(AudioClip clip)
    {
        if (mBGMSource.clip == clip) return; // 防止重复播放相同的BGM
        mBGMSource.clip = clip;
        mBGMSource.Play();
    }

    public void Play2DSFX(AudioClip clip)
    {
        if (clip == null) return;
        var audioPlayer = mSourcePool.Get();
        audioPlayer.Play(clip);
    }

    // 播放3D音效
    public void Play3DSFX(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        // 从池子里拿一个播放器，在指定位置播放
        var audioPlayer = mSourcePool.Get();
        audioPlayer.Play(clip, position);
    }
}

// 对应的 Command
public class Play3DSFXCommand : AbstractCommand
{
    private AudioClip mClip;
    private Vector3 mPosition;

    public Play3DSFXCommand(AudioClip clip, Vector3 pos)
    {
        mClip = clip;
        mPosition = pos;
    }

    protected override void OnExecute()
    {
        this.GetSystem<IAudioSystem>().Play3DSFX(mClip, mPosition);
    }
}

// 播放音效Command
public class Play2DSFXCommand : AbstractCommand
{
    private AudioClip mClip;

    public Play2DSFXCommand(AudioClip clip)
    {
        mClip = clip;
    }
    protected override void OnExecute()
    {
        this.GetSystem<IAudioSystem>().Play2DSFX(mClip);
    }
}

class AudioController : MonoBehaviour, IController
{
    Button SoundEffectsBtn;
    Button BGMBtn;

    AudioClip test;

    public Slider SoundEffectsVolumeSlider; // 音效音量大小
    public Slider BGMVolumeSlider; // BGM音量大小

    private void Start()
    {
        SoundEffectsBtn = transform.Find("SoundEffectsBtn").gameObject.GetComponent<Button>();
        BGMBtn = transform.Find("BGMBtn").gameObject.GetComponent<Button>();
        SoundEffectsVolumeSlider = transform.Find("SoundEffectsVolumeSlider").gameObject.GetComponent<Slider>();
        BGMVolumeSlider = transform.Find("BGMVolumeSlider").gameObject.GetComponent<Slider>();

        var audioModel = this.GetModel<AudioModel>();

        //从模型中加载数据到运行时数据
        SoundEffectsVolumeSlider.value = audioModel.SoundEffectsVolume.Value;
        BGMVolumeSlider.value = audioModel.BGMVolume.Value;

        //绑定事件
        SoundEffectsBtn.onClick.AddListener(() =>
        {
            audioModel.IsSoundEffectsOpen.Value = !audioModel.IsSoundEffectsOpen.Value;
            // 修改静音键图标
        });

        // BGM静音事件
        BGMBtn.onClick.AddListener(() =>
        {
            audioModel.IsBGMOpen.Value = !audioModel.IsBGMOpen.Value;
            // 修改静音键图标
        });
        // BGM音量更改事件
        BGMVolumeSlider.onValueChanged.AddListener((float v) =>
        {
            audioModel.BGMVolume.Value = v;
        });

        // 音效音量更改事件
        SoundEffectsVolumeSlider.onValueChanged.AddListener((float v) =>
        {
            audioModel.SoundEffectsVolume.Value = v;
        });

        this.RegisterEvent<EnemyDieEvent>(e =>
        {
            this.SendCommand(new Play3DSFXCommand(test,new Vector3(1,1,1)));
            // 播放音效
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

    }

    public IArchitecture GetArchitecture()
    {
        return LittleRPGArchitecture.Interface;
    }
}
