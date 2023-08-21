namespace Pssg.Interfaces.Icbc.ViewModels
{
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DriverMasterStatus
    {
        /// <summary>
        /// Initializes a new instance of the DR1MST class.
        /// </summary>
        public DriverMasterStatus()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the DR1MST class.
        /// </summary>
        public DriverMasterStatus(string mSCD = default(string),
            List<int> restrictionCodes = default(List<int>),
            System.DateTime? rRDT = default(System.DateTime?), string lNUM = default(string), int? lCLS = default(int?), List<DriverMedical> dR1MEDN = default(List<DriverMedical>))
        {
            MasterStatusCode = mSCD; 
            RestrictionCodes = restrictionCodes;
            LicenceExpiryDate = rRDT;
            LicenceNumber = lNUM;            
            LicenceClass = lCLS;
            DriverMedicals = dR1MEDN;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>        
        public string MasterStatusCode { get; set; }

        /// <summary>
        /// </summary>        
        public List<int> RestrictionCodes { get; set; }

        /// <summary>
        /// </summary>
        [JsonConverter(typeof(DateJsonConverter))]        
        public System.DateTime? LicenceExpiryDate { get; set; }

        /// <summary>
        /// </summary>        
        public string LicenceNumber { get; set; }

        /// <summary>
        /// </summary>        
        public List<DriverStatus> DriverStatus { get; set; }

        /// <summary>
        /// </summary>        
        public int? LicenceClass { get; set; }

        /// <summary>
        /// </summary>        
        public List<DriverMedical> DriverMedicals { get; set; }

    }
}
