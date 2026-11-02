using Newtonsoft.Json;

namespace SaudiPlus.Helper.Filter
{
    [Serializable]
    public class EntityFilter
    {
        [JsonProperty("pageNumber")]
        public int pageNumber { get; set; }

        [JsonProperty("pageSize")]
        public int pageSize { get; set; }

        [JsonProperty("isActive")]
        public bool? isActive { get; set; }

        [JsonProperty("searchKeyword")]
        public string? searchKeyword { get; set; }

        public EntityFilter()
        {
            pageNumber = 1;
            pageSize = 10;
        }

        public EntityFilter(int pageNumber, int pageSize, bool? isActive = null, string? searchKeyword = null)
        {
            this.pageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.pageSize = pageSize <= 10 ? 10 : pageSize;
            this.isActive = isActive;
            this.searchKeyword = searchKeyword;
        }
    }

    public class DateEntityFilter
    {
        [JsonProperty("pageNumber")]
        public int pageNumber { get; set; }

        [JsonProperty("pageSize")]
        public int pageSize { get; set; }

        [JsonProperty("month")]
        public int? month { get; set; }

        [JsonProperty("year")]
        public int? year { get; set; }

        public DateEntityFilter()
        {
            pageNumber = 1;
            pageSize = 10;
        }

        public DateEntityFilter(int pageNumber, int pageSize, int? month, int? year)
        {
            this.pageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.pageSize = pageSize <= 10 ? 10 : pageSize;
            this.month = month;
            this.year = year;
        }
    }

    [Serializable]
    public class EntityBySearchFilter
    {
        [JsonProperty("searchKeyword")]
        public string? searchKeyword { get; set; }
    }
}
