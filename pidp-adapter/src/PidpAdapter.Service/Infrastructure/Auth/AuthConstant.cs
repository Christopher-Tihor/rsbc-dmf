﻿namespace PidpAdapter.Infrastructure.Auth;

public static class Claims
{
    public const string Address = "address";
    public const string AssuranceLevel = "identity_assurance_level";
    public const string Birthdate = "birthdate";
    public const string Email = "email";
    public const string PidpEmail = "pidp_email";
    public const string FamilyName = "family_name";
    public const string GivenName = "given_name";
    public const string GivenNames = "given_names";
    public const string Gender = "gender";
    public const string IdentityProvider = "identity_provider";
    public const string PreferredUsername = "preferred_username";
    public const string ResourceAccess = "resource_access";
    public const string RealmAccess = "realm_access";
    public const string Subject = "sub";
}
public static class Policies
{
    public const string BcpsAuthentication = "bcps-authentication-policy";
    public const string MedicalPractitioner = "medical-practitioner";
    public const string DmftEnroledUser = "dfmt-enroled-user";
}
public static class IdentityProviders
{
    public const string BCServicesCard = "bcsc";
    public const string BCProvider = "bcprovider_aad";
    public const string Idir = "idir";
    public const string Phsa = "phsa";
}
public static class Roles
{
    // PIdP Role Placeholders
    public const string Practitoner = "PRACTITIONER";
    public const string Moa = "MOA";
    public const string DfmtEnroledRole = "DMFT_ENROLLED"; //DMFT_ENROLLED  //remove this later!
    public const string ViewEndorsements = "view_endorsement_data";
}
public static class Clients
{
    public const string PidpAdapterApi = "DMFT-SEVICE";
    public const string PidpExternalApi = "DMFT-WEBAPP";
    public const string LicenceStatus = "LICENCE-STATUS";
}