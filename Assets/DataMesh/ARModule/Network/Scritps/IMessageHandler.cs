using MEHoloClient.Proto;

namespace DataMesh.AR.Network
{
    public interface IMessageHandler
    {
        void DealMessage(SyncProto proto);
    }
}