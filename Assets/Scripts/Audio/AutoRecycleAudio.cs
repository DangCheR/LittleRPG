using System;
using UnityEngine.Pool;
using UnityEngine;

using QFramework;
/// <summary>
/// 该脚本需挂载在预制件上，用于音频播放器的回收
/// </summary>
public class AutoRecycleAudio : MonoBehaviour
{
    //播放音频
    public AudioSource mSource { get; private set; }
    private IObjectPool<AutoRecycleAudio> mPool;

    // 缓存model，监听用
    private IAudioModel mAudioModel;

    // 传入positon就是世界音效
    public void Play(AudioClip clip, Vector3 position = default)
    {
        if (clip == null)
        {
            Recycle();
            return;
        }

        mSource.clip = clip;
        
        // 0 是 2D，1 是 3D
        mSource.spatialBlend = position == default ? 1.0f : 0.0f; 
        
        if (position == default) 
        {
            transform.position = position;
        }

        // 播放前，手动刷一次最终音量，确保没问题
        mSource.volume = mAudioModel.SoundEffectsVolume.Value;
        mSource.Play();

        // 延时回收
        Invoke(nameof(Recycle), clip.length + 0.1f);
    }

    private void Recycle()
    {
        mPool.Release(this);
    }

    // 初始化监听事件
    public void Init(IObjectPool<AutoRecycleAudio> pool, IAudioModel _audioModel)
    {
        mPool = pool;
        mAudioModel = _audioModel;

        mSource = gameObject.AddComponent<AudioSource>();
        mSource.rolloffMode = AudioRolloffMode.Linear;
        mSource.maxDistance = 50f;

        mAudioModel.SoundEffectsVolume.RegisterWithInitValue(OnGlobalVolumeChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);

        mAudioModel.IsSoundEffectsOpen.RegisterWithInitValue(OnGlobalMuteChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    // 全局音量改变时的回调
    private void OnGlobalVolumeChanged(float globalVolume)
    {
        mSource.volume = globalVolume;
    }

    // 全局静音改变时的回调
    private void OnGlobalMuteChanged(bool isOpen)
    {
        mSource.mute = !isOpen;
    }
}