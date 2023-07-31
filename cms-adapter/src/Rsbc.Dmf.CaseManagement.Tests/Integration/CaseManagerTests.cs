﻿using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System;
using NuGet.Frameworks;
using Rsbc.Dmf.CaseManagement.Dynamics;
using System.Reflection.Metadata;
using Rsbc.Dmf.Dynamics.Microsoft.Dynamics.CRM;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Rsbc.Dmf.CaseManagement.Tests.Integration
{
    public class CaseManagerTests : WebAppTestBase
    {
        private readonly ICaseManager caseManager;

          public CaseManagerTests(ITestOutputHelper output) : base(output)
        {
           
            caseManager = services.GetRequiredService<ICaseManager>();

            
        }

        // example of how to get the dynamics context.  Requires special internal access to be fvided from the CaseManager assembly.

        private void DynamicsContextExample ()
        {
            var dynamicsContext = ((CaseManager)caseManager).dynamicsContext;
            // ... now you can use the Dynamics context...
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanSetFlagsAndSearchById()
        {
            var title = "222";
            // first do a search to get this case by title.
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { Title = title })).Items;
            if (queryResults.Count() > 0)
            {
                var dmerCase = queryResults.ShouldHaveSingleItem().ShouldBeAssignableTo<DmerCase>();
                var caseId = dmerCase.Id;

                List<Flag> flags = new List<Flag>()
                {
                    new Flag(){Description  = "testFlag - 1", Id = "flagTestItem1"},
                    new Flag(){Description  = "testFlag - 2", Id = "flagTestItem2"},
                };
                var result = await caseManager.SetCaseFlags(caseId, false, flags, testLogger);
                result.ShouldNotBeNull().Success.ShouldBe(true);

                var actualCase = (await caseManager.CaseSearch(new CaseSearchRequest { CaseId = caseId })).Items.ShouldHaveSingleItem().ShouldBeAssignableTo<DmerCase>();

                actualCase.Flags.Count().ShouldBe(flags.Count);
                foreach (var actualFlag in actualCase.Flags)
                {
                    var expectedFlag = flags.Where(f => f.Id == actualFlag.Id && f.Description == actualFlag.Description).ShouldHaveSingleItem();
                }
            }
        }

        /// <summary>
        /// CanSetCleanPassValue
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = RequiresDynamics)]
        public async Task CanSetCleanPassValue()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];

            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items;
            if (queryResults.Count() > 0)
            {

                queryResults.ShouldNotBeEmpty();
                foreach (var dmerCase in queryResults)
                {
                    dmerCase.ShouldBeAssignableTo<DmerCase>().Driver.DriverLicenseNumber.ShouldBe(driverLicenseNumber);
                    List<Flag> flags = new List<Flag>()
                {
                    new Flag(){Description  = "testFlag - 1", Id = "flagTestItem1"},
                    new Flag(){Description  = "testFlag - 2", Id = "flagTestItem2"},
                };
                    await caseManager.SetCaseFlags(dmerCase.Id, true, flags, testLogger);
                }

            }

        }


        /// <summary>
        /// CanSetCleanPassValue
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = RequiresDynamics)]
        public async Task CanUpdateCleanPassValue()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];

            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items;
            if (queryResults.Count() > 0)
            {
                queryResults.ShouldNotBeEmpty();
                foreach (var dmerCase in queryResults)
                {
                    var caseId = dmerCase.Id;
                    // set the value to true
                    await caseManager.SetCleanPassFlag(caseId, false);

                    // Update Clean Pass Flag

                    await caseManager.UpdateCleanPassFlag(caseId);
                }

                // verify in dynamics wether this is updated

            }

            
        }

        /// <summary>
        /// CanSetManualPassValue
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = RequiresDynamics)]
        public async Task CanSetManualPassValue()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];

            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items;
            if (queryResults.Count() > 0)
            {
                queryResults.ShouldNotBeEmpty();
                foreach (var dmerCase in queryResults)
                {
                    var caseId = dmerCase.Id;
                    // set the value to true
                    await caseManager.SetManualPassFlag(caseId, false);

                    // Update Manaul Pass Flag
                    await caseManager.UpdateManualPassFlag(caseId);
                }

                // verify in dynamics wether this is updated

            }


        }



        /// <summary>
        /// Verify that the Practioner and Clinic set function can be called with the empty string.
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = RequiresDynamics)]
        public async Task CanSetCasePractitionerClinicEmpty()
        {
            var title = "222";
            // first do a search to get this case by title.
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { Title = title })).Items;
            if (queryResults.Count() > 0)
            {
                var dmerCase = queryResults.ShouldHaveSingleItem().ShouldBeAssignableTo<DmerCase>();
                var caseId = dmerCase.Id;

                await caseManager.SetCasePractitionerClinic (caseId, "", "");
            }

        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanQueryCasesByTitle()
        {
            var title = "222";
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { Title = title })).Items;

            if (queryResults.Count() > 0)
            {

                var dmerCase = queryResults.ShouldHaveSingleItem().ShouldBeAssignableTo<DmerCase>();
                dmerCase.Title.ShouldBe(title);

                dmerCase.CreatedBy.ShouldNotBeNullOrEmpty();
                dmerCase.Driver.DriverLicenseNumber.ShouldNotBeNullOrEmpty();
                dmerCase.Driver.Name.ShouldNotBeNullOrEmpty();
            }
        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanQueryCasesByDriverLicense()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];

            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items;

            if (queryResults.Count() > 0)
            {

                queryResults.ShouldNotBeEmpty();
                foreach (var dmerCase in queryResults)
                {
                    dmerCase.ShouldBeAssignableTo<DmerCase>().Driver.DriverLicenseNumber.ShouldBe(driverLicenseNumber);
                }
            }
        }


        /// <summary>
        /// Verify that the Practioner and Clinic set function can be called with the empty string.
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = RequiresDynamics)]
        public async Task CanLegacyCandidateCreate()
        {
            var newDriver = new LegacyCandidateSearchRequest()

            {
                DriverLicenseNumber = "999" + (DateTime.Now.Year % 10).ToString() 
                    + (DateTime.Now.Hour % 10).ToString() + (DateTime.Now.Minute % 10).ToString()
                    + (DateTime.Now.Second % 10).ToString() + (DateTime.Now.Millisecond % 10).ToString(),
                Surname = "TEST",
                SequenceNumber = 1
            };
            DateTime testDate = DateTime.Now;

            await caseManager.LegacyCandidateCreate(newDriver, testDate, testDate);
            await caseManager.LegacyCandidateCreate(newDriver, testDate, DateTime.Now);

            var newCaseId = await caseManager.GetNewestCaseIdForDriver(newDriver);

            Assert.True(newCaseId.HasValue);


        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanQueryCasesByClinicId()
        {
            var title = "222";
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { Title = title })).Items;
            if (queryResults.Count() > 0)
            {
                var testItem = queryResults.First().ShouldBeAssignableTo<DmerCase>();

                var expectedClinicId = testItem.ClinicId;

                queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { ClinicId = expectedClinicId })).Items;

                queryResults.ShouldNotBeEmpty();
                foreach (var dmerCase in queryResults)
                {
                    dmerCase.ShouldBeAssignableTo<DmerCase>().ClinicId.ShouldBe(expectedClinicId);
                }
            }
            
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanGetFlags()
        {

            var queryResults = await caseManager.GetAllFlags();

            queryResults.ShouldNotBeEmpty();
            
        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanDoDpsProcessingDate()
        {
            var queryResults = caseManager.GetDpsProcessingDate();

            Assert.NotEqual (queryResults, DateTimeOffset.MinValue );
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanUpdateNonComplyDocuments()
        {
            await caseManager.UpdateNonComplyDocuments();
        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanUpdateResolveCaseStatus()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];
            // first do a search to get this case by title.
            var queryResults1 = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items;
                
            var queryResults = queryResults1.FirstOrDefault();

            var dmerCase = queryResults.ShouldBeAssignableTo<DmerCase>();
            var caseId = dmerCase.Id;

            // set the Case Resolve Date to get past date
            DateTimeOffset caseResolveDate = DateTimeOffset.UtcNow.AddDays(-500);     

            // Get the case and Set the dfp_caseresolvedate to date in past

            await caseManager.SetCaseResolveDate(caseId, caseResolveDate);

            // Set the case status to false
            
            await caseManager.SetCaseStatus(caseId, false);

            // Act
            await caseManager.ResolveCaseStatusUpdates();

            // Assert

           // Manually verify the case status is set
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanResolveCaseStatusUpdates()
        {
            await caseManager.ResolveCaseStatusUpdates();
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanDeleteComment()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];
            // get the case
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items.FirstOrDefault();

            var dmerCase = queryResults.ShouldBeAssignableTo<DmerCase>();
            var caseId = dmerCase.Id;

            string documentUrl = $"TEST-DOCUMENT-{DateTime.Now.ToFileTimeUtc()}";

            // add a document

            LegacyComment legacyCommentRequest = new LegacyComment
            {
                CaseId = caseId,
                Driver = new Driver { DriverLicenseNumber = driverLicenseNumber },
                SequenceNumber = 1,
                CommentDate = DateTimeOffset.UtcNow,
                CommentText = "AUTOMATED TEST COMMENT",
                CommentTypeCode = "W",
                UserId = "TEST"                
            };

            await caseManager.CreateLegacyCaseComment(legacyCommentRequest);

            // confirm it is present

            var comments = await caseManager.GetCaseLegacyComments(caseId, true, OriginRestrictions.None);

            bool found = false;

            string commentId = null;

            foreach (var comment in comments)
            {
                if (comment.CommentText == legacyCommentRequest.CommentText)
                {
                    found = true;
                    commentId = comment.CommentId;
                    break;
                }
            }

            Assert.True(found);

            // test the get
            var c = await caseManager.GetComment(commentId);
            // delete it            

            await caseManager.DeleteComment(commentId);

            // confirm that it is deleted

            found = false;

            comments = await caseManager.GetCaseLegacyComments(caseId, true, OriginRestrictions.None);

            foreach (var comment in comments)
            {
                if (comment.CommentText == legacyCommentRequest.CommentText)
                {
                    found = true;
                    break;
                }
            }

            Assert.False(found);
        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanDeleteLegacyDocument()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];
            // get the case
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items.FirstOrDefault();

            var dmerCase = queryResults.ShouldBeAssignableTo<DmerCase>();
            var caseId = dmerCase.Id;

            string documentUrl = $"TEST-DOCUMENT-{DateTime.Now.ToFileTimeUtc()}";

            // add a document

            LegacyDocument legacyDocumentRequest = new LegacyDocument { BatchId = "1", 
                CaseId = caseId,
                DocumentType = "Legacy Review",
                DocumentTypeCode = "LegacyReview",
                Driver = new Driver { DriverLicenseNumber = driverLicenseNumber },
                FaxReceivedDate = DateTimeOffset.UtcNow,
                ImportDate = DateTimeOffset.UtcNow,
                FileSize = 10,
                DocumentPages = 1,
                OriginatingNumber = "1",
                SequenceNumber = 1,
                DocumentUrl = documentUrl                
            };

            await caseManager.CreateLegacyCaseDocument(legacyDocumentRequest);

            // confirm it is present

            var docs = await caseManager.GetCaseLegacyDocuments(caseId);

            bool found = false;

            string documentId = null;

            foreach (var doc in docs)
            {
                if (doc.DocumentUrl == documentUrl)
                {
                    found = true;
                    documentId = doc.DocumentId;
                    break;
                }
            }

            Assert.True(found);

            // delete it

            await caseManager.DeleteLegacyDocument(documentId);

            // confirm that it is deleted

            found = false;

            docs = await caseManager.GetCaseLegacyDocuments(caseId);

            foreach (var doc in docs)
            {
                if (doc.DocumentUrl == documentUrl)
                {
                    found = true;                    
                    break;
                }
            }

            Assert.False(found);
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanCreateBringForward()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];
            // first do a search to get this case by title.
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items.FirstOrDefault();

            var dmerCase = queryResults.ShouldBeAssignableTo<DmerCase>();
            var caseId = dmerCase.Id;

            // We need to get a valid case Id to test

            var bringForwardRequest = new CaseManagement.BringForwardRequest()
            {
                CaseId = caseId,
                Assignee = string.Empty,
                Description = "Test Description1",
                Subject = "ICBC Error",
                Priority = (CaseManagement.BringForwardPriority?)BringForwardPriority.Normal
            };
            var result = await caseManager.CreateBringForward(bringForwardRequest);
            result.ShouldNotBeNull();
            Assert.True(result.Success);
        }

        [Fact(Skip = RequiresDynamics)]
        public async Task SwitchTo8Dl()
        {
            await caseManager.SwitchTo8Dl();
        }

        [Fact(Skip = RequiresDynamics)]
        public async Task MakeFakeDls()
        {
            await caseManager.MakeFakeDls();
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanCreateIcbcError()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];
            // first do a search to get this case by title.
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items.FirstOrDefault();

            var dmerCase = queryResults.ShouldBeAssignableTo<DmerCase>();
            var caseId = dmerCase.Id;

            // We need to get a valid case Id to test
            var icbcErrorRequest = new IcbcErrorRequest()
            {
                CaseId = caseId,
                ErrorMessage = "Icbc Error Testing"

            };
            var result = await caseManager.MarkMedicalUpdateError(icbcErrorRequest);
            result.ShouldNotBeNull();
            Assert.True(result.Success);
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanGetUnsentMedicalUpdates()
        {
            var queryResults = await caseManager.GetUnsentMedicalUpdates();
            queryResults.Items.ShouldNotBeEmpty();
        }



        [Fact(Skip = RequiresDynamics)]
        public async Task CanUpdateDriverBirthDate()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];

             var request = new UpdateDriverRequest()
            {
                BirthDate = new DateTime(1994,02,16),
                DriverLicenseNumber = driverLicenseNumber
            };
            // Get the driver

           var result = await caseManager.UpdateBirthDate(request);
            result.ShouldNotBeNull();
            Assert.True(result.Success);
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanGetListOfPdfDocuments()
        {
            
           await caseManager.GetPdfDocuments();

        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanUpdatePdfDocumentStatus()
        {

            var pdfDocumentId = await caseManager.CreatePdfDocument(new PdfDocument { StatusCode = StatusCodeOptionSet.SendToBCMail });
            var request = new PdfDocument()
            {
                
                    PdfDocumentId = pdfDocumentId.ToString(),
                    StatusCode = StatusCodeOptionSet.Sent                
            };
            var result = await caseManager.UpdatePdfDocumentStatus(request);
            result.ShouldNotBeNull();
            Assert.True(result.Success);
        }


        [Fact(Skip = RequiresDynamics)]
        public async Task CanCreateDriver()
        {
            var dynamicsContext = ((CaseManager)caseManager).dynamicsContext;
            // Act : Create the driver
            var request = new CreateDriverRequest()
            {
                DriverLicenseNumber = "01234571",
                BirthDate = DateTimeOffset.UtcNow,
                Surname = "TestUser"

            };

            // check to driver exsists and delete 
            var driverExists = dynamicsContext.dfp_drivers.Expand(c => c.dfp_PersonId).Where(x => x.dfp_licensenumber == request.DriverLicenseNumber).FirstOrDefault();
            

            if (driverExists != null)
            {
                // Delete if driver exsists
               bool result = false;
               dynamicsContext.DeleteObject(driverExists);                
               await dynamicsContext.SaveChangesAsync();
               dynamicsContext.DetachAll();
                result = true;
            }

            
            // Create Driver
            var createDriver = await caseManager.CreateDriver(request);

            // Query dynamics to check if the driver is created

            var createdDriverExists = dynamicsContext.dfp_drivers.Expand(c => c.dfp_PersonId).Where(x => x.dfp_licensenumber == request.DriverLicenseNumber).FirstOrDefault();

            Assert.Equal(createdDriverExists.dfp_licensenumber, request.DriverLicenseNumber);


        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanCreateCase()
        {

            var dynamicsContext = ((CaseManager)caseManager).dynamicsContext;

             // Arrange
            var request = new CreateCaseRequest()
            {
                DriverLicenseNumber = "01234571",
                SequenceNumber = 2,
               
            };

            // check to driver 
            var driverExists = dynamicsContext.dfp_drivers.Expand(c => c.dfp_PersonId).Where(x => x.dfp_licensenumber == request.DriverLicenseNumber).FirstOrDefault();

            if (driverExists != null)
            {

                // check for case 
                var caseQuery = dynamicsContext.incidents.Where(i => i._dfp_driverid_value == driverExists.dfp_driverid).FirstOrDefault();

                if (caseQuery != null)
                {

                    // delete the case
                    bool result = false;
                    dynamicsContext.DeleteObject(caseQuery);
                    await dynamicsContext.SaveChangesAsync();
                    dynamicsContext.DetachAll();
                    result = true;

                }
 
                // Create a case  

                var createCase = await caseManager.CreateCase(request);

                // Search for the case id 

                var searchaseQuery = dynamicsContext.incidents.Where(i => i._dfp_driverid_value == driverExists.dfp_driverid).FirstOrDefault();

                Assert.Equal(searchaseQuery._dfp_driverid_value, driverExists.dfp_driverid);

            }
        }

        [Fact(Skip = RequiresDynamics)]
        public async Task CanCreateDocumentEnvelope()
        {
            var driverLicenseNumber = configuration["ICBC_TEST_DL"];
            // get the case
            var queryResults = (await caseManager.CaseSearch(new CaseSearchRequest { DriverLicenseNumber = driverLicenseNumber })).Items.FirstOrDefault();

            var dmerCase = queryResults.ShouldBeAssignableTo<DmerCase>();
            var caseId = dmerCase.Id;

            string documentUrl = $"DMER-TEST-DOCUMENT-{DateTime.Now.ToFileTimeUtc()}";

            // add a document

            LegacyDocument legacyDocumentRequest = new LegacyDocument
            {
                BatchId = "1",
                CaseId = caseId,
                DocumentType = "DMER",
                DocumentTypeCode = "001",
                Driver = new Driver { DriverLicenseNumber = driverLicenseNumber },
               // FaxReceivedDate = DateTime.MinValue,
                ImportDate = DateTimeOffset.UtcNow,
                FileSize = 10,
                DocumentPages = 1,
                OriginatingNumber = "1",
                SequenceNumber = 1,
                DocumentUrl = documentUrl,
                SubmittalStatus = "Open-Required"
            };

            await caseManager.CreateICBCDocumentEnvelope(legacyDocumentRequest);

            // confirm it is present

            var docs = await caseManager.GetCaseLegacyDocuments(caseId);

            bool found = false;

            string documentId = null;

            foreach (var doc in docs)
            {
                if (doc.DocumentUrl == documentUrl)
                {
                    found = true;
                    documentId = doc.DocumentId;
                    break;
                }
            }

            Assert.False(found);
        }
    }
}