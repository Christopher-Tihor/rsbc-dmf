// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Rsbc.Dmf.Interfaces.IcbcAdapter.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class CLNTRESPONSE
    {
        /// <summary>
        /// Initializes a new instance of the CLNTRESPONSE class.
        /// </summary>
        public CLNTRESPONSE()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the CLNTRESPONSE class.
        /// </summary>
        public CLNTRESPONSE(CLNT cLNT = default(CLNT))
        {
            CLNT = cLNT;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "CLNT")]
        public CLNT CLNT { get; set; }

    }
}
