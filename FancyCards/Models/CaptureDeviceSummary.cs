using Newtonsoft.Json;

namespace FancyCards.Models
{
    public class CaptureDeviceSummary
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(new { Name, ID });
        }

    }
}
