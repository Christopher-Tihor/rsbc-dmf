﻿using Microsoft.Extensions.Logging;
using Rsbc.Dmf.CaseManagement.Dynamics;
using Rsbc.Dmf.Dynamics.Microsoft.Dynamics.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rsbc.Dmf.CaseManagement
{
    internal partial class CallbackManager : ICallbackManager
    {
        internal readonly DynamicsContext _dynamicsContext;
        private readonly ILogger<CallbackManager> _logger;

        public CallbackManager(DynamicsContext dynamicsContext, ILogger<CallbackManager> logger)
        {
            _dynamicsContext = dynamicsContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Callback>> GetDriverCallbacks(Guid driverId)
        {
            // get cases and include callbacks
            var cases = _dynamicsContext.incidents
                .Expand(c => c.Incident_Tasks)
                .Where(c => c._dfp_driverid_value == driverId && c.statecode == (int)EntityState.Active);

            // compile a list of callbacks from each case
            var results = new List<Callback>();
            foreach (var @case in cases)
            {
                // skip if tasks is null or has no active task
                if (!@case.Incident_Tasks?.Any(task => task.statecode == (int)EntityState.Active) ?? false)
                    break;

                foreach (var callback in @case.Incident_Tasks.Where(task => task.statecode == (int)EntityState.Active))
                {
                    results.Add(new Callback
                    {
                        Id = callback.activityid ?? Guid.NewGuid(),
                        RequestCallback = callback.scheduledend.GetValueOrDefault(),
                        // TODO find the correct field for Topic, no Dynamics values seem to match this
                        Topic = CallbackTopic.Upload,//Enum.Parse<CallbackTopic>(callback.activitytypecode), e.g. "task"
                        // TODO find the correct field for CallStatus
                        // Dynamics statuscode values are blank, "Pending", and "Completed"
                        CallStatus = (CallbackCallStatus)callback.statuscode,
                        Closed = callback.actualend.GetValueOrDefault()
                    });
                }
            }

            return results;
        }

        public async Task<ResultStatusReply> Cancel(Guid caseId, Guid callbackId)
        {
            var reply = new ResultStatusReply();

            try
            {
                var callback = _dynamicsContext.incidents
                    .Expand(c => c.Incident_Tasks)
                    .Where(c => c.incidentid == caseId && c.statecode == (int)EntityState.Active)
                    .ToList()
                    .First()
                    .Incident_Tasks
                    .Where(cb => cb.activityid == callbackId)
                    .ToList()
                    .First();

                if (callback == null)
                {
                    reply.Success = false;
                    reply.ErrorDetail = "Callback not found.";
                    return reply;
                }

                callback.statecode = (int)EntityState.Cancelled;
                _dynamicsContext.UpdateObject(callback);

                await _dynamicsContext.SaveChangesAsync();
                reply.Success = true;
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, $"{nameof(Cancel)} failed.");
                reply.Success = false;
                reply.ErrorDetail = ex.Message;
            }

            return reply;
        }
    }
}
