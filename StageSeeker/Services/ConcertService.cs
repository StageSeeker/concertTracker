using System.Net.Http;
using Newtonsoft.Json;
using StageSeeker.Models;

public class ConcertService {
    private readonly IConfiguration _configuration;
    public ConcertService(IConfiguration configuration) {
        _configuration = configuration;
    }
    
    public async Task<List<Concert>> GetConcertsByArtist(string artist) {
        try {
            if(string.IsNullOrEmpty(artist)){
                throw new Exception("Invalid artist entry, Please enter an artist");
            }
            if(artist.EndsWith(' ')) {
                artist = artist.TrimEnd();
            }
            if(artist.Contains(' ')) {
                artist = artist.Replace(' ', '-');
            }
            Console.WriteLine($"Searching Concerts for artist:{artist}");
            var client_id = _configuration["SeatGeekData:ClientId"];
            if (string.IsNullOrEmpty(client_id)) {
                throw new Exception("Invalid Client Id:Client ID is empty");
            }
            var apiUrl = new Uri($"https://api.seatgeek.com/2/events?performers.slug={artist}&client_id={client_id}");
            using(var client = new HttpClient()) {
                HttpResponseMessage  response = await client.GetAsync(apiUrl);
                if(response.IsSuccessStatusCode) {
                    var json = await response.Content.ReadAsStringAsync();
                    var concertData = JsonConvert.DeserializeObject<ConcertResponse>(json);
                    if(concertData is null) {
                        throw new Exception("Failed to deserialize concert data");
                    }
                    return concertData.Concerts;
                }
                else {
                    throw new Exception($"No concerts available for the {artist}");
                }
            }
        } catch(Exception ex) {
            Console.WriteLine(ex.Message);
            return [];
        }
    }
}