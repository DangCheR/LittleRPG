
using QFramework;

namespace LittleRPG
{
    public class CheckUpdateCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<IHotUpdateSystem>().CheckUpdateAndDownload("RemoteRes");
        }
    }
}