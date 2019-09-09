using System.Web;

namespace Impact.Core.Extension
{
    public static class SessionExtensions
    {
        public static T Get<T>(this HttpSessionStateBase session, string name)
        {
            return (T)session[name];
        }
    }
}