# Build stage
FROM golang:1.23-alpine AS builder

WORKDIR /build

# Copy go mod files
COPY go.mod go.sum ./
RUN go mod download

# Copy source code
COPY . .

# Build the application
RUN CGO_ENABLED=0 GOOS=linux go build -a -installsuffix cgo -o ess-three ./cmd/ess-three

# Final stage
FROM alpine:latest

RUN apk --no-cache add ca-certificates

WORKDIR /root/

# Copy the binary from builder
COPY --from=builder /build/ess-three .

# Create data directory
RUN mkdir -p /data

# Expose port
EXPOSE 9000

# Run the application
CMD ["./ess-three", "--port=9000", "--data-dir=/data"]
