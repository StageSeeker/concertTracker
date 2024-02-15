using System.Net.Http;
using Newtonsoft.Json;
using StageSeeker.Models;

public class ConcertService {
    public List<string> GetPerformers(List<Performer> performers) {
        return performers.Select(p=> p.Artist).ToList();
    }
    public async Task<List<Concert>> GetConcertsByArtist(string artist) {
        try {
            var apiUrl = new Uri($"https://api.seatgeek.com/2/events?performers.slug={artist}&client_id=Mzk3MTcxMzZ8MTcwNjg5NDk0OS4xODkyMjA3");
            using(var client = new HttpClient()) {
                HttpResponseMessage  response = await client.GetAsync(apiUrl);
                if(response.IsSuccessStatusCode) {
                    var json = await response.Content.ReadAsStringAsync();
                    var concertData = JsonConvert.DeserializeObject<ConcertResponse>(json);
                   foreach (var concert in concertData.Concerts) {
                    concert.ArtistNames = GetPerformers(concert.Performers).ToArray();
                    concert.Performers = null!;
                   }
                    return concertData.Concerts;
                }
                else {
                    return new List<Concert>();
                }
            }
        } catch {
            Console.WriteLine("Invalid data return");
            return new List<Concert>();
        }
    }
}