using System.Net.Http.Headers;
using Newtonsoft.Json;
using Ticketing.Model.Domain;
namespace RestServices;

class MainClass {
    static HttpClient client = new HttpClient();

    public static async Task Main(string[] args) {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        try {
            // Test GET all spectacles
            string path = "http://localhost:8080/mpp/spectacles";
            Console.WriteLine($"Calling: {path}...");

            var result = await GetSpectaclesAsync(path);
            
            if (result != null) {
                foreach (var s in result) {
                    Console.WriteLine($"Found: {s.Name} at {s.Location}");
                }
            }
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }
    
    static async Task<Spectacle[]> GetSpectaclesAsync(string path) {
        Spectacle[] spectacles = null;
        HttpResponseMessage response = await client.GetAsync(path);

        if (response.IsSuccessStatusCode) { // 
            var responseString = await response.Content.ReadAsStringAsync();
            spectacles = JsonConvert.DeserializeObject<Spectacle[]>(responseString);
        }
        return spectacles; // [cite: 307]
    }
}