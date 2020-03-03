using System;
using System.Threading.Tasks;

namespace Coordinator.Signaling.Gateway
{
    public interface IGatewayClient
    {
        Task ReceiveRtcAnswer(Guid origin, string sdpAnswer);

        Task ReceiveRtcOffer(Guid origin, string sdpOffer);
    }
}