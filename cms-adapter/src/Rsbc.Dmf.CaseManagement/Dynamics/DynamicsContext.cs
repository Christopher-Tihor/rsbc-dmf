﻿using Microsoft.Extensions.Logging;
using Microsoft.OData.Client;
using Rsbc.Dmf.Dynamics.Microsoft.Dynamics.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rsbc.Dmf.CaseManagement.Dynamics
{
    internal class DynamicsContext : Rsbc.Dmf.Dynamics.Microsoft.Dynamics.CRM.System
    {
        public Func<Task<string>> _tokenFactory;
        public DynamicsContext(Uri serviceRoot, Uri url, Func<Task<string>> tokenFactory, ILogger<DynamicsContext> logger) : base(serviceRoot)
        {
            _tokenFactory = tokenFactory;
            this.HttpRequestTransportMode = HttpRequestTransportMode.HttpClient;
            this.SaveChangesDefaultOptions = SaveChangesOptions.BatchWithSingleChangeset;  // Set to SaveChangesOptions.None to troubleshoot query issues
            MergeOption = MergeOption.OverwriteChanges;

            this.EntityParameterSendOption = EntityParameterSendOption.SendOnlySetProperties;

            Func<Uri, Uri> formatUri = requestUri => requestUri.IsAbsoluteUri
                    ? new Uri(url, (url.AbsolutePath == "/" ? string.Empty : url.AbsolutePath) + requestUri.AbsolutePath + requestUri.Query)
                    : new Uri(serviceRoot, (url.AbsolutePath == "/" ? string.Empty : url.AbsolutePath) + serviceRoot.AbsolutePath + requestUri.ToString());

            BuildingRequest += (sender, args) =>
            {
                args.Headers.Add("Authorization", $"Bearer {tokenFactory().GetAwaiter().GetResult()}");
                args.RequestUri = formatUri(args.RequestUri);
            };

            Configurations.RequestPipeline.OnEntryStarting((arg) =>
            {                
                // do not send reference properties and null values to Dynamics
                arg.Entry.Properties = arg.Entry.Properties.Where((prop) => !prop.Name.StartsWith('_') && prop.Value != null);
            });

            Format.UseJson();
            
        }

        public void DetachAll()
        {
            foreach (var descriptor in this.EntityTracker.Entities)
            {
                this.Detach(descriptor.Entity);
            }
            foreach (var link in this.EntityTracker.Links)
            {
                this.DetachLink(link.Source, link.SourceProperty, link.Target);
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllPagesAsync<TEntity>(IQueryable<TEntity> query) => (await ((DataServiceQuery<TEntity>)query).GetAllPagesAsync());

        public void ActivateObject(object entity, int activeStatusValue) =>
            ModifyEntityStatus(this, entity, (int)EntityState.Active, activeStatusValue);

        public void DeactivateObject(object entity, int inactiveStatusValue) =>
            ModifyEntityStatus(this, entity, (int)EntityState.Inactive, inactiveStatusValue);

        private static void ModifyEntityStatus(DynamicsContext context, object entity, int state, int status)
        {
            var entityType = entity.GetType();
            var statusProp = entityType.GetProperty("statuscode");
            var stateProp = entityType.GetProperty("statecode");

            if (statusProp == null) throw new InvalidOperationException($"statuscode property not found in type {entityType.FullName}");
            if (stateProp == null) throw new InvalidOperationException($"stateProp property not found in type {entityType.FullName}");

            statusProp.SetValue(entity, status);
            if (state >= 0) stateProp.SetValue(entity, state);

            context.UpdateObject(entity);
        }
    }

    // sync with EntityState enum in Rsbc.Dmf.CaseManagement.Service\Protos\cmsAdapter.proto
    public enum EntityState
    {
        Active = 0,
        Inactive = 1,
        Cancelled = 2,
        //Pending = 3?
    }
}