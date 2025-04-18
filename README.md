# Instagram Publishing Post API Reference Document

## Overview

This ASP.NET Core project automates Instagram Business Account post publishing via Facebook’s Graph API. It walks you through configuring your Facebook/Instagram assets, securely storing credentials, writing the three‑step API calls (`GET` the IG Business Account ID → `POST` a media container → `POST` publish), testing with Postman, and handling common errors—while adhering to Meta’s security policies and Microsoft’s best practices citeturn0search10turn0search0.

---

## Prerequisites

1. **Facebook Developer Account**

   - Sign up at developers.facebook.com and create an app to access Graph API endpoints citeturn0search10.

2. **Facebook Business App**

   - In your app’s dashboard, under **Products**, add **Instagram Graph API** citeturn0search10.

3. **Instagram Business Account**

   - Convert your IG profile to a Business Account in Meta Business Suite and link it to a Facebook Page citeturn0search8.

4. **Page Access Token**

   - Generate via Graph API Explorer or the Facebook Login flow. Requires scopes:
     - `instagram_basic` citeturn0search1
     - `pages_show_list`
     - `pages_read_engagement`
     - `publish_to_page` citeturn0search9
   - Validate using the Access Token Debugger citeturn0search2.

5. **Facebook Page ID**
   - Retrieve from Meta Business Suite or via:
     ```http
     GET /me/accounts?access_token={PAGE_ACCESS_TOKEN}
     ```
   - Copy the `id` of the linked page citeturn0search19.

---

## Project Setup

### 1. Configuration

Store secrets in **appsettings.json** (or better, [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) for dev) citeturn0search6:

```json
{
  "Instagram": {
    "AccessToken": "YOUR_PAGE_ACCESS_TOKEN",
    "FacebookPageId": "YOUR_FACEBOOK_PAGE_ID"
  }
}
```

For production, consider Azure Key Vault integration citeturn0search7.

### 2. Dependencies

- **ASP.NET Core** (hosts your API)
- **HttpClient** via `IHttpClientFactory` for robust HTTP calls citeturn0search3
- **Newtonsoft.Json** for JSON (de‑)serialization citeturn0search5

### 3. Service Registration

In `Program.cs`:

```csharp
builder.Services.AddHttpClient();
builder.Services.Configure<InstagramSettings>(
    builder.Configuration.GetSection("Instagram"));
```

This uses DI to manage `HttpClient` lifetimes and avoids socket exhaustion citeturn0search13.

---

## Code Implementation

### Workflow

1. **Fetch** Instagram Business Account ID
2. **Create** a media container
3. **Publish** the container as a post

### Step 1: Fetch IG Business Account ID

```csharp
private async Task<InstagramIdResponseModel?> GetInstagramBusinessAccountIdAsync(string accessToken)
{
    var url = $"{Constants.GraphBaseURL}/{_settings.FacebookPageId}"
            + "?fields=instagram_business_account"
            + $"&access_token={accessToken}";
    var response = await _httpClient.GetAsync(url);
    return await response.Content.ReadFromJsonAsync<InstagramIdResponseModel>();
}
```

> **Endpoint**:  
> `GET https://graph.facebook.com/v22.0/{PageId}?fields=instagram_business_account&access_token={token}` citeturn0search10

### Step 2: Create Media Container

```csharp
private async Task<MediaResponseModel?> CreateMediaContainerAsync(
    string igBusinessAccountId, string imageUrl, string caption, string accessToken)
{
    var payload = new Dictionary<string, string>
    {
        { "image_url", imageUrl },
        { "caption",  caption  },
        { "access_token", accessToken }
    };
    using var content = new FormUrlEncodedContent(payload);
    var url = $"{Constants.GraphBaseURL}/{igBusinessAccountId}/media";
    var response = await _httpClient.PostAsync(url, content);
    return await response.Content.ReadFromJsonAsync<MediaResponseModel>();
}
```

> **Notes**:
>
> - Use `FormUrlEncodedContent` for `application/x-www-form-urlencoded` bodies citeturn0search4
> - Image URL must be publicly accessible to Facebook’s servers citeturn0search2

### Step 3: Publish the Post

```csharp
private async Task<HttpResponseMessage> PublishMediaAsync(
    string igBusinessAccountId, string creationId, string accessToken)
{
    var payload = new Dictionary<string, string>
    {
        { "creation_id",   creationId   },
        { "access_token", accessToken }
    };
    using var content = new FormUrlEncodedContent(payload);
    var url = $"{Constants.GraphBaseURL}/{igBusinessAccountId}/media_publish";
    return await _httpClient.PostAsync(url, content);
}
```

> **Endpoint**:  
> `POST https://graph.facebook.com/v22.0/{igBusinessAccountId}/media_publish` citeturn0search0

---

## Testing with Postman

1. **Fetch IG Account ID**
   ```http
   GET https://graph.facebook.com/v22.0/{PageId}
     ?fields=instagram_business_account
     &access_token=YOUR_TOKEN
   ```
2. **Create Media Container**

   ```http
   POST https://graph.facebook.com/v22.0/{IG_ID}/media
   Content-Type: application/x-www-form-urlencoded

   image_url=https://example.com/image.jpg
   &caption=Hello World!
   &access_token=YOUR_TOKEN
   ```

3. **Publish Post**

   ```http
   POST https://graph.facebook.com/v22.0/{IG_ID}/media_publish
   Content-Type: application/x-www-form-urlencoded

   creation_id={MEDIA_ID}
   &access_token=YOUR_TOKEN
   ```

---

## Troubleshooting

- **Invalid OAuth token**  
  Use the [Debug Token](https://developers.facebook.com/docs/graph-api/reference/debug_token/) endpoint to inspect token validity citeturn0search12.
- **Missing `publish_to_page` permission**  
  Regenerate your token ensuring that `publish_to_page` is granted citeturn0search1.
- **Unsupported post type**  
  Verify that your `image_url` is reachable by Facebook’s crawlers citeturn0search2.
- **Parameter name errors**  
  Always use `access_token` (lowercase, underscore) in query/body citeturn0search9.
