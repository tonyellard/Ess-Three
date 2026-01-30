# Getting Started with ess-three

This guide will help you get started with ess-three, a local S3 emulator for development.

## Prerequisites

- Docker and Docker Compose (recommended)
- OR Go 1.22+ for local development

## Quick Start (Docker - Recommended)

### 1. Start the Emulator

```bash
docker-compose up -d
```

This will:
- Build the Docker image
- Start the container on port 9000
- Mount `./data` for persistent storage

### 2. Verify It's Running

```bash
curl http://localhost:9000/health
```

You should see: `OK`

### 3. Test with AWS CLI

First, configure AWS CLI with dummy credentials:

```bash
aws configure set aws_access_key_id test
aws configure set aws_secret_access_key test
aws configure set region us-east-1
```

Create a test file and upload it:

```bash
echo "Hello from ess-three!" > test.txt
aws s3 cp test.txt s3://mybucket/test.txt --endpoint-url=http://localhost:9000
```

List objects:

```bash
aws s3 ls s3://mybucket/ --endpoint-url=http://localhost:9000
```

Download the file:

```bash
aws s3 cp s3://mybucket/test.txt downloaded.txt --endpoint-url=http://localhost:9000
cat downloaded.txt
```

## Alternative: Local Development

### 1. Build and Run

```bash
make build
./ess-three --port=9000 --data-dir=./data
```

Or run directly:

```bash
make run
```

### 2. Run Tests

```bash
make test
```

## Using with Your Application

### Python (boto3)

```python
import boto3

s3 = boto3.client(
    's3',
    endpoint_url='http://localhost:9000',
    aws_access_key_id='test',
    aws_secret_access_key='test',
    region_name='us-east-1'
)

# Upload
with open('myfile.txt', 'rb') as f:
    s3.put_object(Bucket='mybucket', Key='myfile.txt', Body=f)

# Download
response = s3.get_object(Bucket='mybucket', Key='myfile.txt')
content = response['Body'].read()

# List
response = s3.list_objects_v2(Bucket='mybucket')
for obj in response.get('Contents', []):
    print(f"{obj['Key']} - {obj['Size']} bytes")
```

### Node.js (AWS SDK v3)

```javascript
const { S3Client, PutObjectCommand, GetObjectCommand, ListObjectsV2Command } = require("@aws-sdk/client-s3");
const fs = require('fs');

const client = new S3Client({
    endpoint: "http://localhost:9000",
    region: "us-east-1",
    credentials: {
        accessKeyId: "test",
        secretAccessKey: "test"
    },
    forcePathStyle: true
});

// Upload
const fileContent = fs.readFileSync('myfile.txt');
await client.send(new PutObjectCommand({
    Bucket: "mybucket",
    Key: "myfile.txt",
    Body: fileContent
}));

// List
const list = await client.send(new ListObjectsV2Command({
    Bucket: "mybucket"
}));
console.log(list.Contents);
```

### Go (AWS SDK v2)

```go
package main

import (
    "context"
    "github.com/aws/aws-sdk-go-v2/aws"
    "github.com/aws/aws-sdk-go-v2/config"
    "github.com/aws/aws-sdk-go-v2/service/s3"
    "os"
)

func main() {
    cfg, _ := config.LoadDefaultConfig(context.TODO(),
        config.WithRegion("us-east-1"),
    )
    
    client := s3.NewFromConfig(cfg, func(o *s3.Options) {
        o.BaseEndpoint = aws.String("http://localhost:9000")
        o.UsePathStyle = true
    })
    
    // Upload
    file, _ := os.Open("myfile.txt")
    defer file.Close()
    
    client.PutObject(context.TODO(), &s3.PutObjectInput{
        Bucket: aws.String("mybucket"),
        Key:    aws.String("myfile.txt"),
        Body:   file,
    })
}
```

## Integration Tests

Run the Python integration tests:

```bash
# Install boto3
pip install boto3

# Make sure the emulator is running
docker-compose up -d

# Run tests
python test/integration_test.py
```

## Troubleshooting

### Port Already in Use

If port 9000 is already in use, edit `docker-compose.yml`:

```yaml
ports:
  - "9001:9000"  # Change 9001 to any available port
```

Then use `http://localhost:9001` as your endpoint.

### Permission Denied on Data Directory

Make sure the `./data` directory is writable:

```bash
chmod 755 ./data
```

### Objects Disappear After Restart

Make sure you're using a volume mount in `docker-compose.yml`:

```yaml
volumes:
  - ./data:/data
```

## Next Steps

- See [README.md](README.md) for full documentation
- Check [test/README.md](test/README.md) for testing strategies
- Review supported S3 operations and limitations

## Stopping the Emulator

```bash
docker-compose down
```

To also remove the stored data:

```bash
docker-compose down -v
rm -rf ./data
```
