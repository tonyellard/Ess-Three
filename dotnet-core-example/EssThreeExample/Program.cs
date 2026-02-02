using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        // Configuration
        const string endpointUrl = "http://localhost:9000";
        const string bucketName = "test-bucket";
        const string testKey = "test-file.txt";
        const string testContent = "Hello from .NET Core example!";

        // Create S3 client
        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpointUrl,
            ForcePathStyle = true,
            UseHttp = true,
            // Disable SSL verification for local testing
            SignatureVersion = "4"
        };

        var s3Client = new AmazonS3Client(
            accessKey: "test",
            secretKey: "test",
            config: s3Config
        );

        try
        {
            Console.WriteLine("=== ess-three .NET Core Example ===");
            Console.WriteLine($"Endpoint: {endpointUrl}");
            Console.WriteLine($"Bucket: {bucketName}");
            Console.WriteLine("====================================\n");

            // Test PutObject
            await TestPutObject(s3Client, bucketName, testKey, testContent);

            // Test HeadObject
            await TestHeadObject(s3Client, bucketName, testKey);

            // Test GetObject
            await TestGetObject(s3Client, bucketName, testKey, testContent);

            // Test ListObjects
            await TestListObjects(s3Client, bucketName);

            // Test PutObject with Metadata
            await TestPutObjectWithMetadata(s3Client, bucketName, "metadata-test.txt");

            // Test DeleteObject
            await TestDeleteObject(s3Client, bucketName, testKey);

            // Test DeleteObjects (batch delete)
            await TestDeleteObjects(s3Client, bucketName);

            Console.WriteLine("\n=== All tests passed! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Details: {ex}");
            Environment.Exit(1);
        }
    }

    static async Task TestPutObject(IAmazonS3 client, string bucket, string key, string content)
    {
        Console.WriteLine("Testing PutObject...");
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                ContentBody = content,
                ContentType = "text/plain"
            };

            var response = await client.PutObjectAsync(request);
            Console.WriteLine($"✓ PutObject successful");
            Console.WriteLine($"  ETag: {response.ETag}");
            Console.WriteLine($"  HTTP Status: {response.HttpStatusCode}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ PutObject failed: {ex.Message}\n");
            throw;
        }
    }

    static async Task TestHeadObject(IAmazonS3 client, string bucket, string key)
    {
        Console.WriteLine("Testing HeadObject...");
        try
        {
            var request = new HeadObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            var response = await client.HeadObjectAsync(request);
            Console.WriteLine($"✓ HeadObject successful");
            Console.WriteLine($"  Content-Type: {response.Headers.ContentType}");
            Console.WriteLine($"  Content-Length: {response.ContentLength}");
            Console.WriteLine($"  Last Modified: {response.LastModified}");
            Console.WriteLine($"  ETag: {response.ETag}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ HeadObject failed: {ex.Message}\n");
            throw;
        }
    }

    static async Task TestGetObject(IAmazonS3 client, string bucket, string key, string expectedContent)
    {
        Console.WriteLine("Testing GetObject...");
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            var response = await client.GetObjectAsync(request);
            using (var reader = new StreamReader(response.ResponseStream))
            {
                var content = await reader.ReadToEndAsync();
                if (content == expectedContent)
                {
                    Console.WriteLine($"✓ GetObject successful");
                    Console.WriteLine($"  Content: {content}");
                    Console.WriteLine($"  Content-Type: {response.Headers.ContentType}\n");
                }
                else
                {
                    throw new Exception($"Content mismatch. Expected: {expectedContent}, Got: {content}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ GetObject failed: {ex.Message}\n");
            throw;
        }
    }

    static async Task TestListObjects(IAmazonS3 client, string bucket)
    {
        Console.WriteLine("Testing ListObjects...");
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucket,
                MaxKeys = 10
            };

            var response = await client.ListObjectsV2Async(request);
            Console.WriteLine($"✓ ListObjects successful");
            Console.WriteLine($"  Objects found: {response.S3Objects.Count}");
            foreach (var obj in response.S3Objects)
            {
                Console.WriteLine($"    - {obj.Key} ({obj.Size} bytes)");
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ ListObjects failed: {ex.Message}\n");
            throw;
        }
    }

    static async Task TestPutObjectWithMetadata(IAmazonS3 client, string bucket, string key)
    {
        Console.WriteLine("Testing PutObject with Metadata...");
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                ContentBody = "File with metadata",
                ContentType = "text/plain"
            };

            // Add custom metadata
            request.Metadata.Add("author", "dotnet-example");
            request.Metadata.Add("version", "1.0");

            var response = await client.PutObjectAsync(request);
            Console.WriteLine($"✓ PutObject with metadata successful");
            Console.WriteLine($"  ETag: {response.ETag}\n");

            // Verify metadata with HeadObject
            var headRequest = new HeadObjectRequest
            {
                BucketName = bucket,
                Key = key
            };
            var headResponse = await client.HeadObjectAsync(headRequest);
            Console.WriteLine($"  Metadata retrieved:");
            foreach (var kvp in headResponse.Metadata)
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value}");
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ PutObject with metadata failed: {ex.Message}\n");
            throw;
        }
    }

    static async Task TestDeleteObject(IAmazonS3 client, string bucket, string key)
    {
        Console.WriteLine("Testing DeleteObject...");
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            var response = await client.DeleteObjectAsync(request);
            Console.WriteLine($"✓ DeleteObject successful");
            Console.WriteLine($"  HTTP Status: {response.HttpStatusCode}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ DeleteObject failed: {ex.Message}\n");
            throw;
        }
    }

    static async Task TestDeleteObjects(IAmazonS3 client, string bucket)
    {
        Console.WriteLine("Testing DeleteObjects (batch delete)...");
        try
        {
            // First, create some test files
            var keys = new[] { "batch-delete-1.txt", "batch-delete-2.txt", "batch-delete-3.txt" };
            foreach (var key in keys)
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucket,
                    Key = key,
                    ContentBody = "test content"
                };
                await client.PutObjectAsync(putRequest);
            }

            // Delete them in batch
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucket,
                Objects = keys.Select(k => new KeyVersion { Key = k }).ToList()
            };

            var response = await client.DeleteObjectsAsync(deleteRequest);
            Console.WriteLine($"✓ DeleteObjects successful");
            Console.WriteLine($"  Deleted: {response.DeletedObjects.Count} objects");
            foreach (var obj in response.DeletedObjects)
            {
                Console.WriteLine($"    - {obj.Key}");
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ DeleteObjects failed: {ex.Message}\n");
            throw;
        }
    }
}
