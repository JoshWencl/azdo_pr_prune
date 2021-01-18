using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AbandonOldPullRequests
{
    class Program
    {
        /// <summary>
        /// The base url
        /// Ex https://something.visualstudio.com
        /// </summary>
        private const string baseUrl = "";

        /// <summary>
        /// project name
        /// </summary>
        private const string project = "";

        /// <summary>
        /// repository name
        /// </summary>
        private const string repository = "";
        #region password
        /// <summary>
        /// personal access token
        /// </summary>
        private const string personalaccesstoken = "";
        #endregion

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            try
            {
                using (HttpClient client = GetClient())
                {
                    using (HttpResponseMessage response = client.GetAsync(
                                $"{baseUrl}/{project}/_apis/git/repositories/{repository}/pullrequests?api-version=6.0").Result)
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        PullRequestArray pullRequests = JsonSerializer.Deserialize<PullRequestArray>(responseBody);

                        // Where active pull requests are older than 8 months
                        var oldPullRequests = pullRequests.value.Where(j => j.creationDate < DateTime.Now.AddMonths(-8) && j.status == "active");
                        foreach (PullRequest pullRequest in oldPullRequests)
                        {
                            using (HttpClient anotherClient = GetClient())
                            {
                                StringContent status = new StringContent("{status: 2}", Encoding.UTF8, "application/json");

                                using (HttpResponseMessage request = anotherClient.PatchAsync(
                                            $"{baseUrl}/{project}/_apis/git/repositories/{repository}/pullrequests/{pullRequest.pullRequestId}?api-version=6.0", status).Result)
                                {
                                    Console.WriteLine($"Abandoned PR {pullRequest.pullRequestId}.  Goodbye to {pullRequest.createdBy}");
                                    request.EnsureSuccessStatusCode();
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", personalaccesstoken))));

            return client;
        }
    }
}
