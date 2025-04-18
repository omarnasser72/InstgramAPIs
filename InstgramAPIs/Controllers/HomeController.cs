using InstgramAPIs.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Web;

namespace InstgramAPIs.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public HomeController(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private async Task<InstagramIdResponseModel?> FacebookPageIdApiAsync(string accessToken)
        {
            var queryParams = new Dictionary<string, string>()
            {
                {"fields", Constants.InstagramBusinessAccountField },
                { "access_token", accessToken }
            };

            var query = HttpUtility.ParseQueryString("");

            foreach (var queryParam in queryParams)
                query[queryParam.Key] = queryParam.Value;

            var url = $"{Constants.GraphBaseURL}/{_configuration["FacebookPageId"]}?{query}";
            var res = await _httpClient.GetAsync(url);

            if (res.IsSuccessStatusCode)
            {
                var model = await res.Content.ReadFromJsonAsync<InstagramIdResponseModel>();
                return model;
            }
            return null;
        }

        private async Task<MediaResponseModel?> InstagramMediaApiAsync(InstagramIdResponseModel model, string image_url, string caption, string accessToken)
        {
            var mediaPayload = new Dictionary<string, string>
            {
                { "image_url", image_url},
                { "caption", caption },
                { "access_token", accessToken }
            };

            var content = new FormUrlEncodedContent(mediaPayload);

            var url = $"{Constants.GraphBaseURL}/{model.InstagramBuisnessAccount.Id}/media";

            var res = await _httpClient.PostAsync(url, content);

            if (res.IsSuccessStatusCode)
                return await res.Content.ReadFromJsonAsync<MediaResponseModel>();
            return null;
        }

        private async Task<HttpResponseMessage> InstagramMediaPublishApiAsync(MediaResponseModel mediaResponseModel, InstagramIdResponseModel instagramIdResponseModel, string accessToken)
        {
            var publishPayload = new Dictionary<string, string>
            {
                {"creation_id", mediaResponseModel.Id },
                {"access_token", accessToken }
            };

            var content = new FormUrlEncodedContent(publishPayload);

            var url = $"{Constants.GraphBaseURL}/{instagramIdResponseModel.InstagramBuisnessAccount.Id}/media_publish";

            return await _httpClient.PostAsync(url, content);
        }


        [HttpGet]
        public async Task<IActionResult> GetInstagramId([FromBody] InstagramRequestModel instagramRequestModel)
        {

            string accessToken = _configuration["AccessToken"]!;

            var instagramIdResponseModel = await FacebookPageIdApiAsync(accessToken);

            if (instagramIdResponseModel is null)
                return BadRequest(new { success = false, message = "Failed to get instagram Id." });


            var mediaResponseModel = await InstagramMediaApiAsync(instagramIdResponseModel, instagramRequestModel.ImageUrl, instagramRequestModel.Caption!, accessToken);

            if (mediaResponseModel is null)
                return BadRequest(new { success = false, message = "Failed to get instagram media post Id." });

            var res = await InstagramMediaPublishApiAsync(mediaResponseModel, instagramIdResponseModel, accessToken);


            return res.IsSuccessStatusCode
                   ? Created("", new { success = true, message = "Post published to account successfully." })
                   : BadRequest(new { success = false, message = "Failed to publish your post." });
        }
    }
}
