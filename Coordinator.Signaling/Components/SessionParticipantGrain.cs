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

        public Task ForwardIceCandidate(Guid destination, string payload)
        {
            return (_session ?? throw new InvalidOperationException("Participant hasn't joined any sessions"))
                .ForwardIceCandidate(destination, this.GetPrimaryKey(), payload);
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

        public Task OnParticipantIceCandidate(Guid origin, string payload)
        {
            return GetClient().OnIceCandidate(origin, payload);
        }

        public Task OnParticipantJoin(Guid id)
        {
            // TODO: pass participant description to client
            return GetClient().OnParticipantJoin(id, null);
        }

        public Task OnParticipantLeave(Guid id)
        {
            return GetClient().OnParticipantLeave(id);
        }

        public Task OnParticipantRtcAnswer(Guid origin, string sdpAnswer)
        {
            return GetClient().OnRtcAnswer(origin, sdpAnswer);
        }

        public Task OnParticipantRtcOffer(Guid origin, string sdpOffer)
        {
            return GetClient().OnRtcOffer(origin, sdpOffer);
        }

        public Task<Guid[]> GetSessionParticipants()
        {
            return _session.GetParticipants();
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