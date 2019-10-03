using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Impact.Website.Helper
{
    public static class JavaScriptConvert
    {
        public static IHtmlString SerializeObject(object value)
        {
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                var serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                };

                jsonWriter.QuoteName = false;
                serializer.Serialize(jsonWriter, value);
                jsonWriter.Flush();

                var htmlString = stringWriter.ToString();
                htmlString = htmlString.Replace("zero", "0");

                return new HtmlString(htmlString);
            }
        }
    }
}