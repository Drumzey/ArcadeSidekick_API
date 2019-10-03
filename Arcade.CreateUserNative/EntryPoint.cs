using Amazon.Lambda.APIGatewayEvents;

namespace Arcade.CreateUserNative
{
    public static class EntryPoint
    {
        public static void Main()
        {
            LambdaNative.LambdaNative.Run<Handler, APIGatewayProxyRequest, APIGatewayProxyResponse>();
        }
    }
}
