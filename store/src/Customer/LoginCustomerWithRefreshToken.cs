using Neo4j.Driver;

internal sealed class LoginCustomerWithRefreshToken(IDriver driver, ResponseTokenModel tokenModel)
{
    public sealed record Request(string RefreshToken);
    public sealed record Response(string AccessToken, string RefreshToken);

    public async void Handle(Request request)
    {
        try
        {
            await driver.VerifyConnectivityAsync();
            await using var session = driver.AsyncSession();

            var query = @"
            MATCH (n:Customer {refreshToken: $refreshToken})
            RETURN n";
            var parameters = new Dictionary<string, object>
            {
                {"refreshToken",request.RefreshToken}
            };
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(query, parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });

            if (result is null || result.Properties["refreshToken"].ToString() is null)
            {

            }
        }
        catch
        {
            Console.WriteLine("Puklo u LoginCustomerWithRefreshToken");
        }
    }
}