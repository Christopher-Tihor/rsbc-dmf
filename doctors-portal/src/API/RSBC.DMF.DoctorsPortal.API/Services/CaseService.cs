﻿using Rsbc.Dmf.CaseManagement.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSBC.DMF.DoctorsPortal.API.Services
{
    public interface ICaseQueryService
    {
        Task<IEnumerable<DmerCaseListItem>> SearchCases(CaseSearchQuery query);
    }

    public class CaseSearchQuery
    {
        public string ByTitle { get; set; }
        public string ByDriverLicense { get; set; }
        public IEnumerable<string> ByStatus { get; set; } = Array.Empty<string>();
    }

    public class DmerCaseListItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string PatientName { get; set; }
        public string DriverLicense { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Status { get; set; }
    }

    public class CaseService : ICaseQueryService
    {
        private readonly CaseManager.CaseManagerClient caseManager;
        private readonly IUserService userService;

        public CaseService(CaseManager.CaseManagerClient caseManager, IUserService userService)
        {
            this.caseManager = caseManager;
            this.userService = userService;
        }

        public async Task<IEnumerable<DmerCaseListItem>> SearchCases(CaseSearchQuery query)
        {
            var userContext = await userService.GetCurrentUserContext();
            var currentClinic = userContext.CurrentClinicAssignment;

            //must search within a specific clinic
            if (currentClinic == null) return Array.Empty<DmerCaseListItem>();

            var searchRequest = new SearchRequest
            {
                Title = query.ByTitle ?? string.Empty,
                DriverLicenseNumber = query.ByDriverLicense ?? string.Empty,
                ClinicId = currentClinic.ClinicId
            };
            searchRequest.Statuses.Add(query.ByStatus);

            var results = (await caseManager.SearchAsync(searchRequest)).Items;

            return results.Select(c => new DmerCaseListItem
            {
                Id = c.CaseId,
                Title = c.Title,
                CreatedBy = c.CreatedBy,
                CreatedOn = c.CreatedOn.ToDateTime(),
                ModifiedBy = c.ModifiedBy,
                ModifiedOn = c.ModifiedOn.ToDateTime(),
                PatientName = c.DriverName,
                DriverLicense = c.DriverLicenseNumber,
            });
        }
    }
}