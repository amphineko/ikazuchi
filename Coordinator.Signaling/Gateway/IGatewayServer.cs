using System;
using System.Threading.Tasks;

namespace Coordinator.Signaling.Gateway
{
    public interface IGatewayServer
    {
        Task<Guid> GetParticipantId();

        Task LeaveCurrentSession();

        Task JoinSession(Guid sessionId);

        Task SendRtcAnswer(Guid destination, string sdpAnswer);

        Task SendRtcOffer(Guid destination, string sdpOffer);
    }
}