using MEHoloClient.Entities;

namespace DataMesh.AR.Network
{
    public interface IMessageHandler
    {
        void DealMessage(SyncProto proto);
    }
}