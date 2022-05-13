﻿using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pssg.DocumentStorageAdapter;
using Rsbc.Dmf.CaseManagement.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Rsbc.Dmf.LegacyAdapter.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class DriversController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DriversController> _logger;


        private readonly CaseManager.CaseManagerClient _cmsAdapterClient;
        private readonly DocumentStorageAdapter.DocumentStorageAdapterClient _documentStorageAdapterClient;

        public DriversController(ILogger<DriversController> logger, IConfiguration configuration, CaseManager.CaseManagerClient cmsAdapterClient)
        {
            _configuration = configuration;
            _cmsAdapterClient = cmsAdapterClient;
            _logger = logger;
        }

       

        /// <summary>
        /// DoesCaseExist
        /// </summary>
        /// <param name="driversLicense"></param>
        /// <param name="surcode"></param>
        /// <returns>True if the case exists</returns>
        // GET: /Drivers/Exist
        [HttpGet("Exist")]
        public ActionResult DoesCaseExist(string driversLicense, string surcode)
        {
            bool result = false;
            // get the case                                                
            return Json (result);
        }

        /// <summary>
        /// Get Comments for a case
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        // GET: /Drivers/Exist
        [HttpGet("{driversLicense}/Comments")]
        [ProducesResponseType(typeof(List<ViewModels.Comment>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]        
        public ActionResult GetComments([FromRoute] string driversLicense)
        
        {            
            // call the back end

            var reply = _cmsAdapterClient.GetDriverComments( new GetDriverCommentsRequest () { DriverLicenceNumber = driversLicense } );

            if (reply.ResultStatus == CaseManagement.Service.ResultStatus.Success)
            {
                // get the comments
                List<ViewModels.Comment> result = new List<ViewModels.Comment>();

                foreach (var item in reply.Items)
                {
                    // todo - get the driver details from ICBC, get the MedicalIssueDate from Dynamics
                    ViewModels.Driver driver = new ViewModels.Driver()
                    {
                        LicenseNumber = driversLicense,
                        Flag51 = false, LastName = "LASTNAME", LoadedFromICBC = false, MedicalIssueDate = DateTimeOffset.Now 
                    };

                    result.Add(new ViewModels.Comment
                    {
                        CaseId = item.CaseId,
                        CommentDate = item.CommentDate.ToDateTimeOffset(),
                        CommentId = item.CommentId,
                        CommentText = item.CommentText,
                        CommentTypeCode = item.CommentTypeCode,
                        Driver = driver,
                        SequenceNumber = item.SequenceNumber,
                        UserId = item.UserId
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
        [HttpPost("{driversLicense}/Comments")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public ActionResult CreateCommentForDriver([FromRoute] string driversLicense, [FromBody] ViewModels.Comment comment )
        {
            // add the comment

            var driver = new CaseManagement.Service.Driver()
            {
                DriverLicenceNumber = driversLicense                

            };

            if (comment.Driver != null)
            {
                driver.Surname = comment.Driver.LastName;
            }

            var result = _cmsAdapterClient.CreateLegacyCaseComment(new LegacyComment()
            {
                CaseId = comment.CaseId,
                CommentText = comment.CommentText,
                CommentTypeCode = comment.CommentTypeCode,
                SequenceNumber = comment.SequenceNumber,
                UserId = comment.UserId,
                CommentDate = Timestamp.FromDateTimeOffset(comment.CommentDate),
                Driver = driver

            });

            if (result.ResultStatus == CaseManagement.Service.ResultStatus.Success)
            {
                var actionName = nameof(CreateCommentForDriver);
                var routeValues = new
                {
                    driversLicence = driversLicense
                };

                return CreatedAtAction(actionName, routeValues, comment);
            }
            else
            {
                return StatusCode(500);
            }
        }


        [HttpGet("{driversLicense}/Documents")]
        public ActionResult GetDocuments([FromRoute] string driversLicense)
        {
            bool result = false;
            // get the comments
            return Ok(); //return Json(result);
        }


        /// <summary>
        /// Add a document to a case
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="driversLicense"></param>
        /// <param name="surcode"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("{caseId}/Documents")]
        // allow large uploads
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateCaseDocuments([FromRoute] string caseId, [FromForm] string driversLicense, [FromForm] string surcode,
            [FromForm] IFormFile file)
        {
            return Ok();
        }

    }
}
