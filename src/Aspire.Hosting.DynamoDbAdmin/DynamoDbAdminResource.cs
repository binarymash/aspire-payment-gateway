using Aspire.Hosting.ApplicationModel;
using System.Runtime.CompilerServices;

namespace Aspire.Hosting.DynamoDbAdmin
{
    public class DynamoDbAdminResource(string name) : ContainerResource(name)
    {
        //// Required property on IResourceWithConnectionString. Represents a connection
        //// string that applications can use to access the MailDev server. In this case
        //// the connection string is composed of the SmtpEndpoint endpoint reference.
        //public ReferenceExpression DynamoEndpointExpression =>
        //    ReferenceExpression.Create($"{DisplayEnvironmentVariables()}");

        //public string Whatever(EnvironmentCallbackContext ctx)
        //{
        //    return ctx.EnvironmentVariables[""].ToString();
        //}

        public string GetEndpointFromString()
        {
            if (!this.TryGetEnvironmentVariables(out var envs))
            {
                return string.Empty;
            }

            return "abc";
        }

        public Task GetEndpointFromEnvironmentCallbackContext(EnvironmentCallbackContext ctx)
        {
            var envs = ctx.EnvironmentVariables;

            return Task.FromResult("abc");
        }

        //private string DisplayEnvironmentVariables()
        //{
        //    var envs = Environment.GetEnvironmentVariables();
        //    var formattedEnvs = string.Join(';', envs.Keys.Cast<string>().Select(k => $"{k}={envs[k]}"));

        //    return formattedEnvs;
        //}
    }
}
