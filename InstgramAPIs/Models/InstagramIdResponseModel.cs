using System.Text.Json.Serialization;

namespace InstgramAPIs.Models
{
    abstract class BaseClass
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }

    class InstagramIdResponseModel : BaseClass
    {
        [JsonPropertyName("instagram_business_account")]
        public InstagramBuisnessAccountModel InstagramBuisnessAccount { get; set; } = null!;
    }

    class InstagramBuisnessAccountModel : BaseClass
    {
    }

    class MediaResponseModel : BaseClass
    {
    }

}

