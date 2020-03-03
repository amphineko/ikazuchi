using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Coordinator.Signaling.Abstractions;
using Coordinator.Signaling.Abstractions.Components;
using Coordinator.Signaling.Gateway;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace Coordinator.Signaling.Components
{
    public class SessionParticipantGrain : Grain, ISessionParticipant
    {
        private readonly IHubContext<GatewayController, IGatewayClient> _hubContext;

        private SessionParticipantDescription _description;

        private ISession _session;

        public SessionParticipantGrain(IHubContext<GatewayController, IGatewayClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task<string> GetConnectionId()
        {
            return Task.FromResult(_description.GatewayConnectionId);
        }

        public Task SetConnectionId(string connectionId)
        {
            _description.GatewayConnectionId = connectionId;
            return Task.CompletedTask;
        }

        public Task ForwardRtcAnswer(Guid destination, string sdpAnswer)
        {
            return (_session ?? throw new InvalidOperationException("Participant hasn't joined any sessions"))
                .ForwardRtcAnswer(destination, this.GetPrimaryKey(), sdpAnswer);
        }

        public Task ForwardRtcOffer(Guid destination, string sdpOffer)
        {
            return (_session ?? throw new InvalidOperationException("Participant hasn't joined any sessions"))
                .ForwardRtcOffer(destination, this.GetPrimaryKey(), sdpOffer);
        }

        public async Task JoinSession(Guid sessionId)
        {
            await LeaveCurrentSession();

            // TODO: permission check
            _session = GrainFactory.GetGrain<ISession>(sessionId);
            await _session.AddParticipant(this);
        }

        public async Task<bool> LeaveCurrentSession()
        {
            if (_session == null)
                return false;

            await _session.RemoveParticipant(this);
            _session = null;

            return true;
        }

        public Task PushRtcAnswerToClient(Guid origin, string sdpAnswer)
        {
            return GetClient().ReceiveRtcAnswer(origin, sdpAnswer);
        }

        public Task PushRtcOfferToClient(Guid origin, string sdpOffer)
        {
            return GetClient().ReceiveRtcOffer(origin, sdpOffer);
        }

        public override Task OnActivateAsync()
        {
            _description = new SessionParticipantDescription();
            return base.OnActivateAsync();
        }

        private IGatewayClient GetClient()
        {
            Debug.Assert(_description.GatewayConnectionId != null, "Unexpected not connected participant");

            return _hubContext.Clients.Client(_description.GatewayConnectionId);
        }
    }
}