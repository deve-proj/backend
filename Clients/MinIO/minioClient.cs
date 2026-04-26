using Minio;
using Minio.DataModel.Args;
using Minio.DataModel;

public interface IDeveMinioClient
{
    public Task PutObject(Stream file, string key, string contentType, long size);
    public Task DeleteObject(string key);
}

public class DeveMinioClient : IDeveMinioClient
{
    private IMinioClient _client = new MinioClient()
        .WithEndpoint("localhost:9000")
        .WithCredentials("minioadmin", "minioadmin")
        .WithSSL(false)
        .Build();

    public DeveMinioClient(){}

    public async Task PutObject(Stream file, string key, string contentType, long size)
    {
        try
        {
            var args = new PutObjectArgs()
                .WithBucket("users")
                .WithObject(key)
                .WithStreamData(file)
                .WithContentType(contentType)
                .WithObjectSize(size);

            await _client.PutObjectAsync(args);
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task DeleteObject(string key)
    {
        try
        {

            var args = new RemoveObjectArgs()
                .WithBucket("users")
                .WithObject(key);

            await _client.RemoveObjectAsync(args);
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}