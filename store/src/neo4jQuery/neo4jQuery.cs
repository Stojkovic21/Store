using Neo4j.Driver;

public class Neo4jQuery
{
    public string QueryByOneElement(string type, string name, string value,string behavioe)
    {
        return "MATCH (n:" + type + " {" + name + ":$" + value + "}) "+behavioe+" n";
    }

    public async Task<INode> ExecuteReadAsync(IAsyncSession session, string quary, Dictionary<string, object> parameters)
    {
        try
        {
            return await session.ExecuteReadAsync(async tx =>
            {
                var response = await tx.RunAsync(quary, parameters);
                if (await response.FetchAsync())
                {
                    return response.Current["n"].As<INode>();
                }
                return null;
            });
        }
        catch (System.Exception)
        {
            return null;
        }
    }
    public async Task<INode> ExecuteWriteAsync(IAsyncSession session, string quary, Dictionary<string, object> parameters)
    {
        return await session.ExecuteWriteAsync(async tx =>
        {
            var response = await tx.RunAsync(quary, parameters);
            if (await response.FetchAsync())
            {
                return response.Current["n"].As<INode>();
            }
            return null;
        });
    }
}