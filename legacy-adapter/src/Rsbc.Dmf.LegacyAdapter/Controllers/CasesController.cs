﻿using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pssg.DocumentStorageAdapter;
using Pssg.Interfaces;
using Pssg.Interfaces.Icbc.Models;
using Rsbc.Dmf.CaseManagement.Service;
using Rsbc.Dmf.LegacyAdapter.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rsbc.Dmf.LegacyAdapter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class CasesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CasesController> _logger;


        private readonly CaseManager.CaseManagerClient _cmsAdapterClient;
        private readonly DocumentStorageAdapter.DocumentStorageAdapterClient _documentStorageAdapterClient;
        private readonly IIcbcClient _icbcClient;
        private readonly IMemoryCache _cache;

        public CasesController(ILogger<CasesController> logger, IConfiguration configuration, CaseManager.CaseManagerClient cmsAdapterClient, DocumentStorageAdapter.DocumentStorageAdapterClient documentStorageAdapterClient, IIcbcClient icbcClient,
            IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _configuration = configuration;
            _cmsAdapterClient = cmsAdapterClient;
            _documentStorageAdapterClient = documentStorageAdapterClient;
            _logger = logger;
            _icbcClient = icbcClient;
        }

        [HttpGet("Exist")]
        public ActionResult DoesCaseExist([Required] string licenseNumber, [Required] string surcode)
        {
            // trim out any spaces, force upper case.

            surcode = surcode.ToUpper();

            Regex rgx = new Regex("[^A-Z]");
            surcode = rgx.Replace(surcode, "");

            surcode = surcode.Trim();
            if (surcode.Length > 3)
            {
                surcode = surcode.Substring(0, 3);
            }

            string result = null;

            licenseNumber = _icbcClient.NormalizeDl(licenseNumber, _configuration);


            CLNT driver = null;
            if (licenseNumber != null && licenseNumber != "00000000" && !_cache.TryGetValue(licenseNumber, out driver))
            {
                // get the history from ICBC
                driver = _icbcClient.GetDriverHistory(licenseNumber);
                // Key not in cache, so get data.
                //cacheEntry = DateTime.Now;
                if (driver != null)
                {
                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromHours(6));

                    // Save data in cache.
                    _cache.Set(licenseNumber, driver, cacheEntryOptions);
                }

            }

            if (driver != null && !string.IsNullOrEmpty(driver.INAM?.SURN))
            {
                // first check that it matches ICBC

                string surname = driver.INAM?.SURN;

                surname = surname.ToUpper();
                surname = rgx.Replace(surname, "");

                surname = surname.Trim();

                if (surname.Length > 3)
                {
                    surname = surname.Substring(0, 3);
                }

                if (surname == surcode)  // proceed if the surcode matches.
                {
                    // ensure the surcode matches.
                    _cmsAdapterClient.UpdateDriver(new CaseManagement.Service.Driver
                    {
                        DriverLicenseNumber = licenseNumber,
                        BirthDate = Timestamp.FromDateTimeOffset(driver.BIDT ?? DateTime.Now),
                        GivenName = driver.INAM?.GIV1 ?? string.Empty,
                        Surname = driver.INAM?.SURN ?? string.Empty
                    });

                    result = _cmsAdapterClient.GetCaseId(licenseNumber, driver.INAM?.SURN);

                    if (result == null)
                    {
                        if (String.IsNullOrEmpty(_configuration["BYPASS_CASE_CREATION"])) // create it
                        {
                            try
                            {

                                {
                                    //
                                    LegacyCandidateRequest legacyCandidateRequest = new LegacyCandidateRequest
                                    {
                                        LicenseNumber = licenseNumber,
                                        EffectiveDate = Timestamp.FromDateTimeOffset(DateTimeOffset.Now),
                                        Surname = driver.INAM?.SURN ?? string.Empty,
                                        BirthDate = Timestamp.FromDateTimeOffset(driver.BIDT ?? DateTime.Now),
                                    };
                                    var legacyResult = _cmsAdapterClient.ProcessLegacyCandidate(legacyCandidateRequest);
                                    if (legacyResult.ResultStatus == CaseManagement.Service.ResultStatus.Success)
                                    {
                                        result = _cmsAdapterClient.GetCaseId(licenseNumber, driver.INAM?.SURN);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _logger.LogInformation(e, "Error getting driver.");
                            }
                        }
                        else
                        {
                            if (driver != null)
                            {
                                result = Guid.Empty.ToString();
                            }
                        }
                    }
                }
            }
            else  // fallback, just check Dynamics.
            {
                result = _cmsAdapterClient.GetCaseId(licenseNumber, surcode);
            }

            return Json(result);
        }

        private void IcbcCreate(string licenseNumber, bool legacyOnly, CaseManagement.Service.Driver driver)
        {
            try
            {
                CLNT icbcDriver = null;

                if (!_cache.TryGetValue(licenseNumber, out icbcDriver))
                {
                    // get the history from ICBC
                    icbcDriver = _icbcClient.GetDriverHistory(licenseNumber);
                    // Key not in cache, so get data.
                    //cacheEntry = DateTime.Now;
                    if (icbcDriver != null)
                    {
                        // Set cache options.
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            // Keep in cache for this time, reset time if accessed.
                            .SetSlidingExpiration(TimeSpan.FromHours(6));

                        // Save data in cache.
                        _cache.Set(licenseNumber, icbcDriver, cacheEntryOptions);
                    }

                }
                if (icbcDriver != null && icbcDriver.INAM?.SURN != null)
                {
                    if (driver != null) // update driver if it exists
                    {
                        driver.Surname = icbcDriver.INAM?.SURN ?? string.Empty;
                        driver.GivenName = icbcDriver.INAM?.GIV1 ?? string.Empty;
                        driver.BirthDate = Timestamp.FromDateTimeOffset(icbcDriver.BIDT ?? DateTime.Now);
                    }
                    if (legacyOnly)
                    {
                        LegacyCandidateRequest legacyCandidateRequest = new LegacyCandidateRequest
                        {
                            LicenseNumber = licenseNumber,
                            EffectiveDate = Timestamp.FromDateTimeOffset(DateTimeOffset.Now),
                            Surname = icbcDriver.INAM?.SURN ?? string.Empty,
                            BirthDate = Timestamp.FromDateTimeOffset(icbcDriver.BIDT ?? DateTime.Now)
                        };
                        _cmsAdapterClient.ProcessLegacyCandidate(legacyCandidateRequest);
                    }
                    else
                    {
                        CreateDriverRequest createDriverRequest = new CreateDriverRequest()
                        {
                            DriverLicenseNumber = licenseNumber,
                            Surname = icbcDriver.INAM?.SURN ?? string.Empty,
                            GivenName = icbcDriver.INAM?.GIV1 ?? string.Empty,
                            BirthDate = Timestamp.FromDateTimeOffset(icbcDriver.BIDT ?? DateTime.Now)
                        };

                        // attempt to get the driver from ICBC.

                        _cmsAdapterClient.CreateDriver(createDriverRequest);

                    }

                }
                else
                {
                    _logger.LogError("ICBC ERROR - Unable to get driver from ICBC");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Error getting driver.");
            }
        }

        [HttpGet("ExistByDl")]
        public ActionResult DoesCaseExistByDl([Required] string licenseNumber)
        {
            licenseNumber = _icbcClient.NormalizeDl(licenseNumber, _configuration);

            string caseId = _cmsAdapterClient.GetCaseId(licenseNumber);

            if (caseId == null) // create it
            {
                if (String.IsNullOrEmpty(_configuration["BYPASS_CASE_CREATION"])) // create it
                {
                    IcbcCreate(licenseNumber, true, null);
                    caseId = _cmsAdapterClient.GetCaseId(licenseNumber);
                }
                else
                {
                    caseId = Guid.Empty.ToString();
                }
            }

            return Json(caseId);
        }
     
        [HttpGet("{caseId}")]
        [ProducesResponseType(typeof(ViewModels.CaseDetail), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [ActionName("GetCase")]
        public ActionResult GetCase([Required][FromRoute] string caseId)
        {
            var result = new ViewModels.CaseDetail();

            if (!string.IsNullOrEmpty(caseId) && caseId != Guid.Empty.ToString())
            {
                var c = _cmsAdapterClient.GetCaseDetail(new CaseIdRequest { CaseId = caseId });
                if (c != null && c.ResultStatus == CaseManagement.Service.ResultStatus.Success)
                {
                    string caseType = "Unsolicited";

                    if (c.Item.CaseType == "DMER")
                    {
                        caseType = "Solicited";
                    }

                    result.CaseId = c.Item.CaseId;

                    string title = "";
                    int dashPos = c.Item.Title.LastIndexOf("-");
                    if (dashPos != -1)
                    {
                        title = c.Item.Title.Substring(dashPos + 1).Trim();
                    }
                    else
                    {
                        title = Math.Abs(c.Item.CaseSequence).ToString();
                    }


                    result.Title = title;
                    result.IdCode = c.Item.IdCode;
                    result.OpenedDate = c.Item.OpenedDate.ToDateTimeOffset();
                    result.CaseType = caseType;
                    result.DmerType = c.Item.DmerType;
                    result.Status = c.Item.Status;
                    result.AssigneeTitle = c.Item.AssigneeTitle;
                    result.LastActivityDate = c.Item.LastActivityDate.ToDateTimeOffset();
                    result.LatestDecision = c.Item.LatestDecision;
                    result.DecisionForClass = c.Item.DecisionForClass;
                    result.DecisionDate = c.Item.DecisionDate.ToDateTimeOffset();
                    result.CaseSequence = (int)Math.Abs(c.Item.CaseSequence);

                    result.DpsProcessingDate = c.Item.DpsProcessingDate.ToDateTimeOffset();

                    result.Comments = GetCommentsForCase(caseId, c.Item.DriverId, OriginRestrictions.SystemOnly, true).OrderByDescending(x => x.CommentDate).ToList();
                }

                // set to null if no decision has been made.
                if (result.DecisionDate == DateTimeOffset.MinValue)
                {
                    result.DecisionDate = null;
                }
            }
            return Json(result);
        }



        private List<Comment> GetCommentsForCase(string caseId, string driverId, OriginRestrictions originRestrictions, bool maskAuthor)
        {
            List<ViewModels.Comment> result = new List<ViewModels.Comment>();
            var reply = _cmsAdapterClient.GetComments(new CommentsRequest() { CaseId = caseId, DriverId = driverId, OriginRestrictions = originRestrictions });

            if (reply.ResultStatus == CaseManagement.Service.ResultStatus.Success)
            {
                // get the comments


                foreach (var item in reply.Items)
                {
                    // todo - get the driver details from ICBC, get the MedicalIssueDate from Dynamics
                    ViewModels.Driver driver = new ViewModels.Driver()
                    {
                        LicenseNumber = item.Driver.DriverLicenseNumber,
                        Flag51 = false,
                        LastName = item.Driver.Surname,
                        LoadedFromICBC = false,
                        MedicalIssueDate = DateTimeOffset.Now
                    };

                    var comment = new ViewModels.Comment
                    {
                        CaseId = item.CaseId,
                        CommentDate = item.CommentDate.ToDateTimeOffset(),
                        CommentId = item.CommentId,
                        CommentText = item.CommentText,
                        CommentTypeCode = item.CommentTypeCode,
                        Driver = driver,
                        SequenceNumber = Math.Abs(item.SequenceNumber),
                        UserId = string.IsNullOrEmpty(item.SignatureName) ? item.UserId : item.SignatureName  // 24-01-12 default to signature name, fallback to UserID if not present.
                    };

                    result.Add(comment);
                }

            }
            return result;
        }

        /// <summary>
        /// Get Comments for a case
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        // GET: /Cases/Exist
        [HttpGet("{caseId}/Comments")]
        [ProducesResponseType(typeof(List<ViewModels.Comment>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public ActionResult GetComments([FromRoute] string caseId)

        {
            // call the back end

            var reply = _cmsAdapterClient.GetComments(new CommentsRequest() { CaseId = caseId, OriginRestrictions = OriginRestrictions.UserOnly });

            if (reply.ResultStatus == CaseManagement.Service.ResultStatus.Success)
            {
                // get the comments
                List<ViewModels.Comment> result = new List<ViewModels.Comment>();

                foreach (var item in reply.Items)
                {
                    // todo - get the driver details from ICBC, get the MedicalIssueDate from Dynamics
                    ViewModels.Driver driver = new ViewModels.Driver()
                    {
                        LicenseNumber = item.Driver.DriverLicenseNumber,
                        Flag51 = false,
                        LastName = item.Driver.Surname,
                        LoadedFromICBC = false,
                        MedicalIssueDate = DateTimeOffset.Now
                    };

                    result.Add(new ViewModels.Comment
                    {
                        CaseId = item.CaseId,
                        CommentDate = item.CommentDate.ToDateTimeOffset(),
                        CommentId = item.CommentId,
                        CommentText = item.CommentText,
                        CommentTypeCode = item.CommentTypeCode,
                        Driver = driver,
                        SequenceNumber = Math.Abs(item.SequenceNumber),
                        UserId = string.IsNullOrEmpty(item.SignatureName) ? item.UserId : item.SignatureName
                    });
                }
                return Json(result);
            }
            else
            {
                return StatusCode(500);
            }
            /*
            result.Add (new ViewModels.Comment() { CaseId = Guid.NewGuid().ToString(), CommentText = "SAMPLE TEXT", CommentTypeCode="W",  CommentDate = DateTime.Now, CommentId = Guid.NewGuid().ToString(),
                Driver = new ViewModels.Driver() { Flag51 = false, LastName = "LASTNAME", LicenseNumber = "01234567", LoadedFromICBC = false, MedicalIssueDate = DateTimeOffset.Now }, 
                SequenceNumber = 0, UserId = "TESTUSER" });
            */
        }

        /// <summary>
        /// Add a comment to a case
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost("{caseId}/Comments")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public ActionResult CreateComments([FromRoute] string caseId, [FromBody] ViewModels.Comment comment)
        {

            throw new NotImplementedException();
        }

        /// <summary>
        /// Get Documents
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpGet("{caseId}/Documents")]
        [ProducesResponseType(typeof(List<ViewModels.Document>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public ActionResult GetDocuments([FromRoute] string caseId)
        {
            // call the back end

            var reply = _cmsAdapterClient.GetCaseDocuments(new CaseIdRequest() { CaseId = caseId });

            if (reply.ResultStatus == CaseManagement.Service.ResultStatus.Success)
            {
                // get the comments
                List<ViewModels.Document> result = new List<ViewModels.Document>();

                foreach (var item in reply.Items)
                {
                    // todo - get the driver details from ICBC, get the MedicalIssueDate from Dynamics
                    ViewModels.Driver driver = new ViewModels.Driver()
                    {
                        LicenseNumber = item.Driver.DriverLicenseNumber,
                        Flag51 = false,
                        LastName = item.Driver.Surname,
                        LoadedFromICBC = false,
                        MedicalIssueDate = DateTimeOffset.Now
                    };

                    var resultDocument = new ViewModels.Document
                    {
                        CaseId = item.CaseId,
                        DocumentId = item.DocumentId,
                        Driver = driver,
                        SequenceNumber = item.SequenceNumber,
                        UserId = item.UserId

                    };

                    if (item.FaxReceivedDate != null)
                    {
                        resultDocument.FaxReceivedDate = item.FaxReceivedDate.ToDateTimeOffset();
                    }

                    if (item.ImportDate != null)
                    {
                        resultDocument.ImportDate = item.ImportDate.ToDateTimeOffset();
                    }

                    result.Add(resultDocument);
                }
                return Json(result);
            }
            else
            {
                return StatusCode(500, reply.ErrorDetail);
            }
            /*
            result.Add (new ViewModels.Comment() { CaseId = Guid.NewGuid().ToString(), CommentText = "SAMPLE TEXT", CommentTypeCode="W",  CommentDate = DateTime.Now, CommentId = Guid.NewGuid().ToString(),
                Driver = new ViewModels.Driver() { Flag51 = false, LastName = "LASTNAME", LicenseNumber = "01234567", LoadedFromICBC = false, MedicalIssueDate = DateTimeOffset.Now }, 
                SequenceNumber = 0, UserId = "TESTUSER" });
            */
        }

        /// <summary>
        /// Add Case Document
        /// </summary>
        /// <param name="caseId">Pass Guid.empty to bypass case creation.</param>
        /// <param name="skipDpsProcessing"></param>
        /// <param name="licenseNumber"></param>
        /// <param name="batchId"></param>
        /// <param name="faxReceivedDate"></param>
        /// <param name="importDate"></param>
        /// <param name="importID"></param>
        /// <param name="originatingNumber"></param>
        /// <param name="documentPages"></param>
        /// <param name="documentType"></param>
        /// <param name="documentTypeCode"></param>
        /// <param name="validationMethod"></param>
        /// <param name="validationPrevious"></param>
        /// <param name="file"></param>
        /// <param name="priority"></param>
        /// <param name="assign"></param>
        /// <param name="submittalStatus"></param>
        /// <param name="surcode"></param>
        /// <param name="envelopeId"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        [HttpPost("{caseId}/Documents")]
        // allow large uploads
        [DisableRequestSizeLimit]
        public async Task<IActionResult> AddCaseDocument([FromRoute] string caseId,  // GUID
            [FromForm][Required] string driversLicense,  // Driver -> DL            
            [FromForm] string batchId,         // add to document entity
            [FromForm] string faxReceivedDateString,  // dfp_faxreceivedate
            [FromForm] string importDateString,  // dfp_dpsprocessingdate
            [FromForm] string importID, // add to document entity
            [FromForm] string originatingNumber, // dfp_faxnumber
            [FromForm] int? documentId, // maps to dfp_attachmentnumber
            [FromForm] int? documentPages, // add to document entity
            [FromForm] string documentType, // dfp_documenttypeid
            [FromForm] string documentTypeCode, // "form type" in DPS; the code that will be used for a lookup.
            [FromForm] string validationMethod, // add to document entity
            [FromForm] string validationPrevious, // add to document entity
            [FromForm] IFormFile file,
            [FromForm] string priority,
            [FromForm] string assign = null,
            [FromForm] string submittalStatus = null,
            [FromForm] string surcode = null,         // Driver -> Lastname
            [FromForm] string envelopeId = null
            )
        {
            string surname = surcode;
            var actionName = nameof(AddCaseDocument);
            var routeValues = new
            {
                driversLicence = driversLicense
            };

            if (documentType == null)
            {
                documentType = documentTypeCode;
            }

            if (!string.IsNullOrEmpty(driversLicense))
            {
                string licenseNumber = _icbcClient.NormalizeDl(driversLicense, _configuration);

                if (!string.IsNullOrEmpty(_configuration["VERBOSE_LOGS"]))
                {
                    Log.Information($"{faxReceivedDateString} {importDateString}");
                }

                    DateTimeOffset faxReceivedDate = DocumentUtils.ParseDpsDate(faxReceivedDateString);
                DateTimeOffset importDate = DocumentUtils.ParseDpsDate(importDateString);
                if (!string.IsNullOrEmpty(_configuration["VERBOSE_LOGS"]))
                {
                    var debugObject = new
                    {
                        driversLicense = licenseNumber,
                        batchId = batchId,
                        faxReceivedDate = faxReceivedDate,
                        importDate = importDate,
                        importID = importID,
                        originatingNumber = originatingNumber,
                        documentPages = documentPages,
                        documentType = documentType,
                        documentTypeCode = documentTypeCode,
                        validationMethod = validationMethod,
                        validationPrevious = validationPrevious,
                        priority = priority,
                        assign = assign,
                        submittalStatus = submittalStatus,
                        surcode = surcode,
                        documentId = documentId,
                    };

                    Log.Information(JsonConvert.SerializeObject(debugObject));
                }


                if (priority == null)
                {
                    priority = "Regular";
                }

                var driver = new CaseManagement.Service.Driver()
                {
                    DriverLicenseNumber = licenseNumber,
                    Address = new Address()
                    {
                        City = String.Empty,
                        Line1 = String.Empty,
                        Line2 = String.Empty,
                        Postal = String.Empty
                    },
                    BirthDate = Timestamp.FromDateTimeOffset(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)),
                    GivenName = String.Empty,
                    Height = 0.0,
                    Id = String.Empty,
                    Middlename = String.Empty,
                    Name = String.Empty,
                    Seck = String.Empty,
                    Sex = String.Empty,
                    Surname = surcode ?? String.Empty,
                    Weight = 0.0
                };


                var driverRequest = new DriverLicenseRequest() { DriverLicenseNumber = licenseNumber };
                var driverReply = _cmsAdapterClient.GetDriver(driverRequest);

                string driverId = "";

                if (driverReply.ResultStatus == CaseManagement.Service.ResultStatus.Success && driverReply.Items != null && driverReply.Items.Count > 0)
                {
                    driverId = driverReply.Items.FirstOrDefault()?.Id;
                }
                else
                {
                    // create the driver.

                    IcbcCreate(licenseNumber, false, driver);

                    driverReply = _cmsAdapterClient.GetDriver(driverRequest);
                    if (driverReply.ResultStatus == CaseManagement.Service.ResultStatus.Success && driverReply.Items != null && driverReply.Items.Count > 0)
                    {
                        var returnedDriver = driverReply.Items.FirstOrDefault();
                        driverId = returnedDriver?.Id;
                        surname = returnedDriver?.Surname;
                    }

                }

                driver.Id = driverId;

                if (faxReceivedDate == null)
                {
                    faxReceivedDate = DateTimeOffset.Now;
                }

                if (importDate == null)
                {
                    importDate = DateTimeOffset.Now;
                }

                if (driverId != null)
                {
                    // Check if the document is classified or not classified
                    // add the document
                    var ms = new MemoryStream();
                    if (file != null)
                    {
                        file.OpenReadStream().CopyTo(ms);
                    }
                    else
                    {
                        DebugUtils.SaveDebug("AddCaseDocument", "File is empty");
                    }

                    var data = ms.ToArray();
                    string fileName = file?.FileName ?? "UnknownFile.pdf";

                    // ensure there are no invalid characters.

                    fileName = DocumentUtils.SanitizeKeyFilename(fileName);

                    string fileImportDateString = importDate.ToString("yyyyMMddHHmmss");
                    string fileKey = DocumentUtils.SanitizeKeyFilename($"D{fileImportDateString}-{fileName}");

                    UploadFileRequest pdfData = new UploadFileRequest()
                    {
                        ContentType = DocumentUtils.GetMimeType(fileName),
                        Data = ByteString.CopyFrom(data),
                        EntityName = "dfp_driver",
                        FileName = fileKey,
                        FolderName = driverId,
                    };
                    var fileReply = _documentStorageAdapterClient.UploadFile(pdfData);


                    if (fileReply.ResultStatus != Pssg.DocumentStorageAdapter.ResultStatus.Success
                        || string.IsNullOrEmpty(fileReply.FileName)) // do not proceed if the URL is empty
                    {
                        return StatusCode(500, $"S3 Error - Filename is '{fileReply.FileName}', error is '{fileReply.ErrorDetail}'");
                    }

                    string legacyDocumentType = documentTypeCode ?? documentType ?? String.Empty;

                    string documentSubmittalStatus = "Uploaded";

                    string origin = "DPS/KOFAX";

                    // New workflow for the DPS mitigation 
                    if (!String.IsNullOrEmpty(_configuration["FORCE_RECEIVED"]))
                    {
                        documentSubmittalStatus = "Received";
                        origin = "Migration";
                    }

                    string extension = System.IO.Path.GetExtension(fileReply.FileName);
                    string filenameOverride = $"{documentType}-{driversLicense}-{surname}{extension}";                     

                    var document = new LegacyDocument()
                    {
                        BatchId = batchId ?? string.Empty,
                        DocumentPages = documentPages ?? 1,
                        DocumentType = legacyDocumentType,
                        DocumentTypeCode = documentTypeCode ?? legacyDocumentType,
                        DocumentUrl = fileReply.FileName,
                        CaseId = caseId ?? string.Empty,
                        FaxReceivedDate = Timestamp.FromDateTimeOffset(faxReceivedDate),
                        ImportDate = Timestamp.FromDateTimeOffset(importDate),
                        ImportId = importID ?? string.Empty,
                        FilenameOverride = filenameOverride ?? string.Empty,
                        OriginatingNumber = originatingNumber ?? string.Empty,
                        Driver = driver,
                        ValidationMethod = validationMethod ?? string.Empty,
                        ValidationPrevious = validationPrevious ?? string.Empty,
                        Priority = priority ?? string.Empty,
                        Owner = "Client Services" ?? string.Empty,
                        SubmittalStatus = documentSubmittalStatus ?? string.Empty,
                        Queue = assign ?? string.Empty,
                        DpsDocumentId = documentId ?? -1,
                        Origin = origin ?? string.Empty,
                    };

                    CreateStatusReply result;

                    string[] documentTypeCodes = {
                            "080",
                            "081",
                            "110",
                            "120",
                            "121",
                            "122",
                            "123",
                            "124",
                            "250",
                            "210",
                            "125",
                            "212",
                            "320",
                            "211",
                            "213",
                            "214",
                            "180" // 2024-03-01 Added letter out, as all letter outs received from DPS are remedial.
                            };

                    var documentTypeindex = Array.IndexOf(documentTypeCodes, documentTypeCode);

                    if (documentTypeCode != null && documentTypeindex != -1)
                    {
                        var findDocumentTypeCode = documentTypeCodes.ElementAt(documentTypeindex);

                        if (documentTypeCode != findDocumentTypeCode)
                        {
                            Log.Error("Unable to find the document type code");

                        }
                        else
                        {
                            //  If document is remedial Type 

                            var remedialDocument = new LegacyDocument()
                            {
                                BatchId = batchId ?? string.Empty,
                                DocumentPages = documentPages ?? 1,
                                DocumentType = legacyDocumentType,
                                DocumentTypeCode = documentTypeCode ?? legacyDocumentType,
                                DocumentUrl = fileReply.FileName,
                                CaseId = caseId ?? string.Empty,
                                FaxReceivedDate = Timestamp.FromDateTimeOffset(faxReceivedDate),
                                ImportDate = Timestamp.FromDateTimeOffset(importDate),
                                ImportId = importID ?? string.Empty,
                                FilenameOverride = filenameOverride ?? string.Empty,
                                OriginatingNumber = originatingNumber ?? string.Empty,
                                Driver = driver,
                                ValidationMethod = validationMethod ?? string.Empty,
                                ValidationPrevious = validationPrevious ?? string.Empty,
                                Priority = "Regular",
                                Owner = "Client Services",
                                SubmittalStatus = "Received",
                                Queue = assign ?? string.Empty,
                            };

                            // 24/4/17 Set submittal status to "Issued" for letter out.
                            if (documentTypeCode == "180")
                            {
                                remedialDocument.SubmittalStatus = "Issued";  
                            }

                            var documentAttached = _cmsAdapterClient.CreateDocumentOnDriver(remedialDocument);
                            return CreatedAtAction(actionName, routeValues, remedialDocument);
                        }

                    }
                    // Path 2 : Check if the document is classified
                    else if (documentType != null && documentType == legacyDocumentType)
                    {
                        // Add the document to a driver instead of a case
                        result = _cmsAdapterClient.CreateDocumentOnDriver(document);
                        return CreatedAtAction(actionName, routeValues, document);
                    }

                    // Path 3: If the document is unclassified
                    // Attach the documents to the driver 00000000 

                    else if (documentType != null || documentType == "Unclassified")
                    {
                        var documentAttached = _cmsAdapterClient.CreateDocumentOnDriver(document);
                        return CreatedAtAction(actionName, routeValues, document);
                    }
                }
            }
            return Ok();
        }
    }
}
