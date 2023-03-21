﻿using pdipadapter.Infrastructure.HttpClients;
using MedicalPortal.API.Features.Endorsement.Model;
using MedicalPortal.API.Features.Endorsement.Services.Interfaces;
using Rsbc.Dmf.CaseManagement;

namespace MedicalPortal.API.Features.Endorsement.Services;
public class Endorsement : BaseClient, IEndorsement
{
    private readonly IUserManager userManager;
    public Endorsement(HttpClient client, ILogger<Endorsement> logger, IUserManager userManager) : base(client, logger)
    {
        this.userManager = userManager;
    }

    public async Task<IEnumerable<Model.Endorsement>> GetEndorsement(string hpDid)
    {
        var result = await this.GetAsync<IEnumerable<EndorsementData.Model>>($"/parties/{hpDid}/endorsement");

        if (!result.IsSuccess)
        {
            return null;
        }
        var endorsements = result.Value;
        if (!endorsements.Any())
        {
            this.Logger.LogNoEndorsementFound(hpDid);
            return null;
        }
        var endorsementRelations = endorsements
            .Select(endorsement => new
            {
                contact = userManager.GetPractitionerContact(endorsement.Hpdid),
                License = endorsement.Licences,
                HpDid = endorsement.Hpdid
            }).ToList();
       return endorsementRelations.Select(e => new Model.Endorsement
        {
            Email = e.contact.Result.Email,
            HpDid = e.HpDid!,
            FirstName = e.contact.Result.FirstName,
            LastName = e.contact.Result.LastName,
            Licences = (List<Model.Endorsement.LicenceInformation>)e.License.Select(license => new Model.Endorsement.LicenceInformation
            {
                IdentifierType = license.IdentifierType,
                StatusCode = license.StatusCode,
                StatusReasonCode= license.StatusReasonCode,
            })
        });
    }
}

public static partial class JustinParticipantClientLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Warning, "No Endorsement found in PiDP with Hpdid = {hpdid}.")]
    public static partial void LogNoEndorsementFound(this ILogger logger, string hpdid);

}