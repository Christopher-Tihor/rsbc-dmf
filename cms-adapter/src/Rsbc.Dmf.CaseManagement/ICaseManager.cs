﻿using Rsbc.Dmf.Dynamics.Microsoft.Dynamics.CRM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rsbc.Dmf.CaseManagement
{

    public interface ICaseManager
    {
        Task<CaseSearchReply> CaseSearch(CaseSearchRequest request);

        Task<CreateStatusReply> CreateLegacyCaseComment(LegacyComment request);

        Task<CreateStatusReply> CreateLegacyCaseDocument(LegacyDocument request);

        Task<bool> DeleteComment(string commentId);

        Task<bool> DeactivateLegacyDocument(string documentId);

        Task<bool> DeleteLegacyDocument(string documentId);

        Task<IEnumerable<LegacyComment>> GetDriverLegacyComments(string driverLicenseNumber, bool allComments);

        Task<IEnumerable<CaseDetail>> GetCases(Guid driverId, ActiveStatus activeStatus);

        Task<CaseDetail> GetCaseDetail(string caseId);

        Task<CaseDetail> GetMostRecentCaseDetail(string driverLicenseNumber);

        Task<LegacyComment> GetComment(string commentId);

        Task<IEnumerable<LegacyComment>> GetCaseLegacyComments(string caseId, bool allComments, OriginRestrictions orginRestrictions);

        Task<IEnumerable<LegacyDocument>> GetCaseLegacyDocuments(string caseId);

        Task<IEnumerable<LegacyComment>> GetDriverLegacyComments(string caseId, bool allComments, OriginRestrictions orginRestrictions);

        Task<IEnumerable<LegacyDocument>> GetDriverLegacyDocuments(string driverLicenseNumber);

        Task<IEnumerable<LegacyDocument>> GetDriverLegacyDocuments(Guid driverId);

        Task<LegacyDocument> GetLegacyDocument(string documentId);

        Task<ResultStatusReply> CreateBringForward(BringForwardRequest request);

        Task<IEnumerable<Driver>> GetDriverByLicenseNumber(string licensenumber);

        Task<IEnumerable<Driver>> GetDrivers();

        Task<CaseSearchReply> LegacyCandidateSearch(LegacyCandidateSearchRequest request);

        /// <summary>
        /// Create a Legacy Candidate
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Guid of the created case</returns>
        /// 
        Task<Guid?> GetNewestCaseIdForDriver(LegacyCandidateSearchRequest request);
        Task LegacyCandidateCreate(LegacyCandidateSearchRequest request, DateTimeOffset? birthDate, DateTimeOffset? effectiveDate);

        Task LegacyCandidateCreate(LegacyCandidateSearchRequest request, DateTimeOffset? birthDate, DateTimeOffset? effectiveDate, string source);

        Task MarkMedicalUpdatesSent(List<string> ids);

        Task<ResultStatusReply> MarkMedicalUpdateError(IcbcErrorRequest request);

        Task<SetCaseFlagsReply> SetCaseFlags(string dmerIdentifier, bool isCleanPass, List<Flag> flags, Microsoft.Extensions.Logging.ILogger logger = null);

        Task SetCasePractitionerClinic(string caseId, string practitionerId, string clinicId);

        Task<List<Flag>> GetAllFlags();

        Task<CaseSearchReply> GetUnsentMedicalPass();

        Task<CaseSearchReply> GetUnsentMedicalAdjudication();

        Task AddDocumentUrlToCaseIfNotExist(string dmerIdentifier, string fileKey, Int64 fileSize);

        DateTimeOffset GetDpsProcessingDate();

        Task UpdateNonComplyDocuments();

        Task ResolveCaseStatusUpdates();

        Task<IEnumerable<PdfDocument>> GetPdfDocuments();

        Task<PdfDocumentReply> UpdatePdfDocumentStatus(PdfDocument pdfDocumentRequest);

        Task<Guid> CreatePdfDocument(PdfDocument pdfDocumentRequest);

        Task<ResultStatusReply> UpdateDriver(Driver driver);

        Task<ResultStatusReply> UpdateBirthDate(UpdateDriverRequest driverRequest);

        Task SetCaseResolveDate(string caseId, DateTimeOffset resolvedDate);

        Task<bool> SetCaseStatus(string caseId, bool caseStatus);

        Task<bool> SetCleanPassFlag(string caseId, bool cleanPassStatus);

        Task<ResultStatusReply> UpdateCleanPassFlag(string caseId);

        Task<ResultStatusReply> UpdateManualPassFlag(string caseId);

        Task<bool> SetManualPassFlag(string caseId, bool manualPassStatus);

        Task SwitchTo8Dl();

        Task MakeFakeDls();

        Task<ResultStatusReply> CreateDriver(CreateDriverRequest request);

        Task<ResultStatusReply> CreateCase(CreateCaseRequest request);

        Task<CreateStatusReply> CreateUnsolicitedCaseDocument(LegacyDocument newDocument);

        Task<CreateStatusReply> CreateDocumentOnDriver(LegacyDocument newDocument);

        Task<CreateStatusReply> CreateCaseDocument(LegacyDocument newDocument);
        Task<CreateStatusReply> CreateICBCDocumentEnvelope(LegacyDocument newDocument);

        Task<CreateStatusReply> CreateICBCMedicalCandidateComment(LegacyComment request);

    }
}
