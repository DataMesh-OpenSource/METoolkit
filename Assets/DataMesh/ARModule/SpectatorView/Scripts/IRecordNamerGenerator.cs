using System;

namespace DataMesh.AR.SpectatorView
{

    public interface IRecordNamerGenerator
    {
        /// <summary>
        /// 为录制视频生成一个名字，此名字不包含扩展名
        /// </summary>
        /// <returns></returns>
        string GetName();
    }

    /// <summary>
    /// 默认起名器，使用时间戳
    /// </summary>
    public class RecordNameGeneratorDefault : IRecordNamerGenerator
    {
        public string GetName()
        {
            DateTime dt = DateTime.Now;
            return String.Format("{0:yyyyMMddTHHmmss}", dt);
        }
    }

}