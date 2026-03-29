using UnityEngine;
using QFramework;

// 环境音
public class AmbientAudio : MonoBehaviour, ICanGetModel
{
    private AudioSource mSource;
    private float mBaseVolume = 1.0f; // 这个瀑布自己的基础音量（可能比较吵，设为0.5）

    private void Start()
    {
        mSource = GetComponent<AudioSource>();
        mSource.loop = true;
        mSource.spatialBlend = 1.0f; // 3D声音
        mBaseVolume = mSource.volume; // 记录在 Inspector 里调好的音量

        var audioModel = this.GetModel<IAudioModel>();

        // 监听model的变化
        audioModel.SoundEffectsVolume.RegisterWithInitValue(globalVolume =>
        {
            UpdateVolume(globalVolume, audioModel.IsSoundEffectsOpen.Value);
        }).UnRegisterWhenGameObjectDestroyed(gameObject);

        audioModel.IsSoundEffectsOpen.RegisterWithInitValue(isOpen =>
        {
            UpdateVolume(audioModel.SoundEffectsVolume.Value, isOpen);
        }).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void UpdateVolume(float globalVolume, bool isOpen)
    {
        mSource.mute = !isOpen;
        // 最终音量 = 全局设置音量 * 这个环境音自己的基础音量
        mSource.volume = globalVolume * mBaseVolume;
    }

    public IArchitecture GetArchitecture() => LittleRPGArchitecture.Interface;
}