using QFramework;
using UnityEngine;

/// <summary>
/// 用于使世界UI统一朝向摄像机
/// </summary>
public class FaceToCameraSystem : IController
{
    private Camera _main_camera;
    protected void OnInit()
    {
        _main_camera = Camera.main ?? null;
        if(!_main_camera) throw new System.Exception("我靠？相机不见了？");
    }

    public IArchitecture GetArchitecture()
    {
        return LittleRPGArchitecture.Interface;
    }
}

// 当物体存在UI需要朝向摄像机时挂载
// 例如血条
interface IUIFaceToCamera{
    void FaceToCamera(UnityEngine.Vector3 tar);
}