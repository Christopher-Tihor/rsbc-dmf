﻿using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Serilog;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using System.IO;
using Pssg.SharedUtils;
using FileHelpers;
using Rsbc.Dmf.CaseManagement.Service;
using System.Linq;
using Rsbc.Dmf.IcbcAdapter.ViewModels;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net.Http;
using Newtonsoft.Json;
using Google.Protobuf.WellKnownTypes;
using Pssg.Interfaces;
using Newtonsoft.Json.Serialization;
using Pssg.Interfaces.Icbc.Models;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using Grpc.Core;
using Pssg.Interfaces.IcbcModels;
using Org.BouncyCastle.Asn1.X509;

namespace Rsbc.Dmf.IcbcAdapter
{
    public class EnhancedIcbcApiUtils
    {
        
        private IConfiguration _configuration { get; }
        private readonly CaseManager.CaseManagerClient _caseManagerClient;
        private readonly IIcbcClient _icbcClient;
       
       

        public EnhancedIcbcApiUtils(IConfiguration configuration, CaseManager.CaseManagerClient caseManagerClient, IIcbcClient icbcClient )
        {
            _configuration = configuration;
            _caseManagerClient = caseManagerClient;
            _icbcClient = icbcClient;
            
 
        }



        /// <summary>
        /// Hangfire job to check for and send recent items in the queue
        /// </summary>
        [AutomaticRetry(Attempts = 0)]
        public async Task SendMedicalUpdates(PerformContext hangfireContext)
        {
            LogStatement(hangfireContext, "Starting SendMedicalUpdates");

            var unsentItems = _caseManagerClient.GetUnsentMedicalUpdates(new CaseManagement.Service.EmptyRequest());

           

            foreach (var unsentItem in unsentItems.Items)
            {
                var item = GetMedicalUpdateData(unsentItem);

                if (item != null)
                {

                    string responseContent = _icbcClient.SendMedicalUpdate(item);

                    if (responseContent.Contains("SUCCESS"))
                    {
                        // mark it as sent
                        MarkMedicalUpdateSent(hangfireContext, unsentItem.CaseId);                             
                    }
                    else
                    {
                        var bringForwardRequest = new BringForwardRequest
                        {
                            CaseId = unsentItem.CaseId,
                            Subject = "ICBC Error",
                            Description = responseContent,
                            Assignee = string.Empty,
                            Priority = BringForwardPriority.Normal
                            
                        };
                            
                        _caseManagerClient.CreateBringForward(bringForwardRequest);

                         // Mark ICBC error 

                        var icbcError = new IcbcErrorRequest
                        {
                            ErrorMessage = "ICBC Error"
                        };

                        _caseManagerClient.MarkMedicalUpdateError(icbcError);    

                        LogStatement(hangfireContext, $"ICBC ERROR {responseContent}");
                    }                                            
                }
                else
                {
                    LogStatement(hangfireContext, $"Null received from GetMedicalUpdateData for {unsentItem.CaseId} {unsentItem.Driver?.DriverLicenseNumber}");
                }

            }

            LogStatement(hangfireContext, "End of SendMedicalUpdates.");
        }

        private void MarkMedicalUpdateSent (PerformContext hangfireContext, string caseId)
        {            
            var idListRequest = new IdListRequest();
            idListRequest.IdList.Add(caseId);
            var result = _caseManagerClient.MarkMedicalUpdatesSent(idListRequest);

            LogStatement(hangfireContext, $"Mark Medical Update Sent {caseId} status is  {result.ResultStatus} {result.ErrorDetail}");
        }

        public IcbcMedicalUpdate GetMedicalUpdateData (DmerCase item)
        {

            // Start by getting the current status for the given driver.  If the medical disposition matches, do not proceed.
                
            if (item.Driver != null)
            {
                string licenseNumber = item.Driver.DriverLicenseNumber;
                try
                {
                    var driver = _icbcClient.GetDriverHistory(licenseNumber);
                    if (driver != null && driver.INAM?.SURN != null)
                    {
                        var newUpdate = new IcbcMedicalUpdate()
                        {
                            DlNumber = licenseNumber,
                            LastName = driver.INAM.SURN,
                        };

                        var firstDecision = item.Decisions.OrderByDescending(x => x.CreatedOn).FirstOrDefault();

                        if (firstDecision != null)
                        {
                            if (firstDecision.Outcome == DecisionItem.Types.DecisionOutcomeOptions.FitToDrive)
                            {
                                newUpdate.MedicalDisposition = "P";
                            }
                            else
                            {
                                newUpdate.MedicalDisposition = "J";
                            }
                        }
                        else
                        {
                            newUpdate.MedicalDisposition = "J";
                        }

                        // get most recent Medical Issue Date from the driver.

                        DateTimeOffset adjustedDate = GetMedicalIssueDate(driver); // DateUtility.FormatDateOffsetPacific(GetMedicalIssueDate(driver)).Value;

                        newUpdate.MedicalIssueDate = adjustedDate;

                        return newUpdate;
                    }
                    else
                    {
                        Log.Logger.Error("Error getting driver from ICBC.");
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, "Error getting driver from ICBC.");
                }

                
            }
            else
            {
                Log.Logger.Information($"Case {item.CaseId} {item.Title} has no Driver..");
            }
             
            return null;
        }

        /// <summary>
        /// UpdateBirthdateFromIcbc
        /// </summary>
        /// <returns></returns>

        public async Task UpdateBirthdateFromIcbc()
        {

            // Get List of  drivers from dynamics

            var driversReply = _caseManagerClient.GetDrivers(new CaseManagement.Service.EmptyRequest());
                
            foreach(var driver in driversReply.Items)
            {
                var dlNumber = driver.DriverLicenseNumber;

                // Call the tombstone endpoint
                var response = _icbcClient.GetDriverHistory(dlNumber);
                if (response != null && response.BIDT != null)
                {
                    // Compare Dynamics DOB and ICBC DOB
                    if (driver.BirthDate != Timestamp.FromDateTime((DateTime)response.BIDT))
                    {
                        _caseManagerClient.UpdateDriver(new CaseManagement.Service.Driver
                        {
                            DriverLicenseNumber = dlNumber,
                            BirthDate = Timestamp.FromDateTimeOffset(response.BIDT ?? DateTime.Now),
                            GivenName = response.INAM?.GIV1 ?? string.Empty,
                            Surname = response.INAM?.SURN ?? string.Empty
                        });
                    }
                  
                }
            }        
        }


        public DateTime GetMedicalIssueDate(CLNT driver)
        {
            DateTime result = DateTime.MinValue;
            if (driver.DR1MST != null && driver.DR1MST.DR1MEDN != null)
            {
                foreach (var item in driver.DR1MST.DR1MEDN)
                {
                    if (item.MIDT != null && item.MIDT > result)
                    {
                        result = item.MIDT.Value;
                    }
                }
            }
            

            return result;
        }


        public class ClientResult
        {
            public CLNT CLNT { get; set; }
        }

        private void LogStatement(PerformContext hangfireContext, string message)
        {
            if (hangfireContext != null)
            {
                hangfireContext.WriteLine(message);
            }
            // emit to Serilog.
            Log.Logger.Information(message);
        }

    }
}
