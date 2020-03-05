using System;
using System.Threading.Tasks;
using Coordinator.Signaling.Abstractions;

namespace Coordinator.Signaling.Gateway
{
    public interface IGatewayClient
    {
        Task OnParticipantLeave(Guid id);

        Task OnParticipantJoin(Guid id, SessionParticipantDescription description);

        Task OnParticipantIceCandidate(Guid origin, string payload);

        Task OnParticipantRtcAnswer(Guid origin, string sdpAnswer);

        Task OnParticipantRtcOffer(Guid origin, string sdpOffer);
    }
}