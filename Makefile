.PHONY: build run test clean docker-build docker-run docker-stop help

# Build the application
build:
	@echo "Building ess-three..."
	@go build -o ess-three ./cmd/ess-three

# Run locally
run:
	@echo "Running ess-three..."
	@go run ./cmd/ess-three --port=9000 --data-dir=./data

# Run tests
test:
	@echo "Running tests..."
	@go test -v ./...

# Run tests with coverage
test-coverage:
	@echo "Running tests with coverage..."
	@go test -cover ./...

# Build Docker image
docker-build:
	@echo "Building Docker image..."
	@docker build -t ess-three:latest .

# Run with Docker Compose
docker-run:
	@echo "Starting ess-three with Docker Compose..."
	@docker-compose up -d
	@echo "ess-three is running at http://localhost:9000"

# Stop Docker Compose
docker-stop:
	@echo "Stopping ess-three..."
	@docker-compose down

# View Docker logs
docker-logs:
	@docker-compose logs -f

# Run integration tests
integration-test:
	@echo "Running integration tests..."
	@python3 test/integration_test.py

# Clean build artifacts
clean:
	@echo "Cleaning..."
	@rm -f ess-three
	@rm -rf data/
	@go clean

# Install dependencies
deps:
	@echo "Installing dependencies..."
	@go mod download
	@go mod tidy

# Help
help:
	@echo "Available targets:"
	@echo "  build            - Build the application binary"
	@echo "  run              - Run the application locally"
	@echo "  test             - Run unit tests"
	@echo "  test-coverage    - Run tests with coverage"
	@echo "  docker-build     - Build Docker image"
	@echo "  docker-run       - Run with Docker Compose"
	@echo "  docker-stop      - Stop Docker Compose"
	@echo "  docker-logs      - View Docker logs"
	@echo "  integration-test - Run integration tests"
	@echo "  clean            - Clean build artifacts"
	@echo "  deps             - Install dependencies"
	@echo "  help             - Show this help message"
