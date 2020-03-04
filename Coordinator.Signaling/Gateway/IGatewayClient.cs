using System;
using System.Threading.Tasks;
using Coordinator.Signaling.Abstractions;

namespace Coordinator.Signaling.Gateway
{
    public interface IGatewayClient
    {
        Task OnParticipantLeave(Guid id);

        Task OnParticipantJoin(Guid id, SessionParticipantDescription description);

        Task OnIceCandidate(Guid origin, string payload);

        Task OnRtcAnswer(Guid origin, string sdpAnswer);

        Task OnRtcOffer(Guid origin, string sdpOffer);
    }
}