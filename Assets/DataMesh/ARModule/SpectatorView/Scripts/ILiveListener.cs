namespace DataMesh.AR.SpectatorView
{

    public interface ILiveListener
    {
        void OnRecordStart(string fileName);
        void OnRecordStop(string outputPath, string fileName);
    }

}