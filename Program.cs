using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTestingApp
{
    class Program
    {
        private static readonly ManualResetEvent _quitEvent = new ManualResetEvent(false);

        private static string _apiURI = "https://cloud.signature-io.com/api/";
        private static string _apiIngestExt = "ingest/push";

        private static string _apiKey = "93116a9c-5cac-49ea-bd95-a4c315d4e547";
        private static string _deviceId = "70e5f486-df37-4b1f-ab80-d17d330739d2";

        

        private static HttpClient _httpClient;

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            Console.WriteLine("Testing the SIO.Cloud API...");
            Console.WriteLine();

            Console.WriteLine("Sending random data to:");
            Console.WriteLine($"\tAPI Key: {_apiKey}");
            Console.WriteLine($"\tDevice ID: {_deviceId}");
            Console.WriteLine();

            int pointsQuantity = 0;

            double frequency = 50;
            double sampleRate = 8000;
            double _amplitude = 200;

        _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header

            _httpClient.DefaultRequestHeaders.Add("apiKey", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("deviceId", _deviceId);

            while (!_quitEvent.WaitOne(50))
            {
                //compose message
                var epochNowMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var newValue = Math.Abs((short)(_amplitude * Math.Sin((2 * Math.PI * pointsQuantity * frequency) / sampleRate)));

                var jsonMessage = $@"
                    {{ 
                        ""dateTime"": {epochNowMs},
                        ""mySensor"": {newValue}
                    }}
                ";

                var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(string.Concat(_apiURI, _apiIngestExt), content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"\tepochMs: {epochNowMs}, \tmySensor: {newValue}");
                }
                else
                {
                    var contents = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("There was a problem pushing metrics:");
                    Console.WriteLine(contents);
                    Console.WriteLine();
                }

                pointsQuantity++;
            }
        }
    }
}
