using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HomePi.Class
{
    class Utilities
    {
        public static async Task<int> GetNextBus()
        {

            int nextbusIn = 0;
            try
            {
                var response = await GetjsonStream("http://www.wienerlinien.at/ogd_routing/XML_TRIP_REQUEST2?locationServerActive=1&outputFormat=JSON&type_origin=stopid&name_origin=60200884&type_destination=stopid&name_destination=60200641");
                RootObject obj = JsonConvert.DeserializeObject<RootObject>(response);

                foreach (Trip trip in obj.trips)
                {
                    BUSDateTime nxt = trip.trip.legs[0].points[0].dateTime;
                    DateTime nextBus = DateTime.ParseExact(nxt.time, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                    if (DateTime.Compare(DateTime.Now, nextBus) == -1) //Return the first next bus
                    {
                        TimeSpan diff = nextBus - DateTime.Now;
                        int minutes = Convert.ToInt16(Math.Round(diff.TotalMinutes, 0));
                        if (minutes > 2)
                        {
                            nextbusIn = minutes;
                            return nextbusIn;
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Handle error
            }
            return nextbusIn;
        }

        public async static Task<List<SoundCloudTrack>> GetLikes()
        {
            List<SoundCloudTrack> likes = new List<SoundCloudTrack>();
            try
            {
                string responseText = await GetjsonStream("http://api.soundcloud.com/" + "users/" + "shoban-kumar" + ".json?client_id=YOUR_CLIENT_ID_HERE");
                SoundCloudUser user = JsonConvert.DeserializeObject<SoundCloudUser>(responseText);
                int userId = user.id;
                responseText = await GetjsonStream("http://api.soundcloud.com/" + "users/" + userId + "/favorites.json?client_id=YOUR_CLIENT_ID_HERE");
                likes = JsonConvert.DeserializeObject<List<SoundCloudTrack>>(responseText);
            }
            catch (Exception ex)
            {

            }

            return likes;
        }

        public static async Task<string> GetjsonStream(string url) //Function to read from given url
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            HttpResponseMessage v = new HttpResponseMessage();
            return await response.Content.ReadAsStringAsync();
        }

    }

    #region Classes

    //For Sound cloud
    public class SoundCloudUser
    {
        public int id { get; set; }
        public string permalink { get; set; }
        public string username { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public string avatar_url { get; set; }
    }
    public class SoundCloudTrack
    {
        public int id { get; set; }
        public string created_at { get; set; }
        public int user_id { get; set; }
        public int duration { get; set; }
        public bool commentable { get; set; }
        public string state { get; set; }
        public string sharing { get; set; }
        public string tag_list { get; set; }
        public string permalink { get; set; }
        public object description { get; set; }
        public bool streamable { get; set; }
        public bool downloadable { get; set; }
        public object genre { get; set; }
        public object release { get; set; }
        public object purchase_url { get; set; }
        public object label_id { get; set; }
        public object label_name { get; set; }
        public object isrc { get; set; }
        public object video_url { get; set; }
        public string track_type { get; set; }
        public object key_signature { get; set; }
        public object bpm { get; set; }
        public string title { get; set; }
        public object release_year { get; set; }
        public object release_month { get; set; }
        public object release_day { get; set; }
        public string original_format { get; set; }
        public int original_content_size { get; set; }
        public string license { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
        public object artwork_url { get; set; }
        public string waveform_url { get; set; }
        public SoundCloudUser user { get; set; }
        public string stream_url { get; set; }
        public string download_url { get; set; }
        public int playback_count { get; set; }
        public int download_count { get; set; }
        public int favoritings_count { get; set; }
        public int comment_count { get; set; }
        public SoundCloudCreatedWith created_with { get; set; }
        public string attachments_uri { get; set; }
    }
    public class SoundCloudCreatedWith
    {
        public int id { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
        public string permalink_url { get; set; }
    }

    //For bus
    public class Parameter
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class BUSDateTime
    {
        public string date { get; set; }
        public string time { get; set; }
    }

    public class Stamp
    {
        public string date { get; set; }
        public string time { get; set; }
    }

    public class Link
    {
        public string name { get; set; }
        public string type { get; set; }
        public string href { get; set; }
    }

    public class Ref
    {
        public string id { get; set; }
        public string area { get; set; }
        public string platform { get; set; }
        public string NaPTANID { get; set; }
        public List<object> attrs { get; set; }
        public string coords { get; set; }
    }

    public class Point
    {
        public string name { get; set; }
        public string place { get; set; }
        public string nameWithPlace { get; set; }
        public string usage { get; set; }
        public string omc { get; set; }
        public string placeID { get; set; }
        public string desc { get; set; }
        public BUSDateTime dateTime { get; set; }
        public Stamp stamp { get; set; }
        public List<Link> links { get; set; }
        public Ref @ref { get; set; }
    }

    public class Diva
    {
        public string branch { get; set; }
        public string line { get; set; }
        public string supplement { get; set; }
        public string dir { get; set; }
        public string project { get; set; }
        public string network { get; set; }
        public string stateless { get; set; }
        public string @operator { get; set; }
        public string opCode { get; set; }
    }

    public class Mode
    {
        public string name { get; set; }
        public string number { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string destination { get; set; }
        public string destID { get; set; }
        public string desc { get; set; }
        public Diva diva { get; set; }
    }

    public class Ref2
    {
        public string id { get; set; }
        public string area { get; set; }
        public string platform { get; set; }
        public string NaPTANID { get; set; }
        public List<object> attrs { get; set; }
        public string coords { get; set; }
        public string depDateTime { get; set; }
        public string arrDateTime { get; set; }
    }

    public class StopSeq
    {
        public string name { get; set; }
        public string nameWO { get; set; }
        public string place { get; set; }
        public string nameWithPlace { get; set; }
        public string omc { get; set; }
        public string placeID { get; set; }
        public string platformName { get; set; }
        public Ref2 @ref { get; set; }
    }

    public class Frequency
    {
        public string hasFrequency { get; set; }
        public string tripIndex { get; set; }
        public string minTimeGap { get; set; }
        public string maxTimeGap { get; set; }
        public string avTimeGap { get; set; }
        public string minDuration { get; set; }
        public string maxDuration { get; set; }
        public string avDuration { get; set; }
        public List<object> modes { get; set; }
    }

    public class TurnInst
    {
        public string dir { get; set; }
        public string manoeuvre { get; set; }
        public string name { get; set; }
        public string dirHint { get; set; }
        public string place { get; set; }
        public string tTime { get; set; }
        public string ctTime { get; set; }
        public string dis { get; set; }
        public string cDis { get; set; }
        public string coords { get; set; }
    }

    public class Leg
    {
        public List<Point> points { get; set; }
        public Mode mode { get; set; }
        public List<StopSeq> stopSeq { get; set; }
        public Frequency frequency { get; set; }
        public string path { get; set; }
        public List<TurnInst> turnInst { get; set; }
    }

    public class Trip2
    {
        public string duration { get; set; }
        public string interchange { get; set; }
        public string desc { get; set; }
        public List<Leg> legs { get; set; }
        public List<object> attrs { get; set; }
    }

    public class Trip
    {
        public Trip2 trip { get; set; }
    }

    public class RootObject
    {
        public List<Parameter> parameters { get; set; }
        public List<Trip> trips { get; set; }
    }
    #endregion

}
