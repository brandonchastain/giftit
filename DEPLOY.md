# Deploy RSS Reader web app frontend

1. Install prerequisites:
* dotnet
* azure swa cli

2. Run these commands to deploy the frontend:

```bash
cd c:\dev\gifted2\gifted\src\WasmApp
dotnet publish -c release -r win-x64 WasmApp.csproj --output bin/release/net9.0/win-x64/publish --self-contained true
swa deploy .\bin\release\net9.0\win-x64\publish\wwwroot\ --env production

```

3. When prompted, choose the WASM app to deploy to.

# Deploy RSS Reader to Azure Container Apps

This guide walks through deploying your RSS Reader API to Azure Container Apps with persistent storage using GitHub Container Registry.

## Prerequisites
1. Azure CLI installed: `az --version`
2. Docker installed (for local testing)
3. Azure subscription
4. GitHub account with access to create personal access tokens

## Step 1: Setup GitHub Container Registry Authentication

### Create GitHub Personal Access Token (PAT)

1. Navigate to https://github.com/settings/tokens/new?scopes=write:packages
2. Create a new personal access token (classic) with the following scopes:
   - `write:packages` - to download and upload container images
   - `read:packages` - to download container images (included with write)
   - `delete:packages` - (optional) to delete container images
3. Save your token securely - you'll need it for authentication

### Save Token as Environment Variable

```bash
# Save your GitHub personal access token
$env:GITHUB_PAT = "YOUR_GITHUB_TOKEN"
$env:GITHUB_USERNAME = "YOUR_GITHUB_USERNAME"

# Verify the variables are set (should not be empty)
Write-Host "GITHUB_USERNAME: $env:GITHUB_USERNAME"
Write-Host "GITHUB_PAT is set: $($null -ne $env:GITHUB_PAT -and $env:GITHUB_PAT -ne '')"

# Login to GitHub Container Registry
echo $env:GITHUB_PAT | docker login ghcr.io -u $env:GITHUB_USERNAME --password-stdin
```

**Important**: Ensure both environment variables are set before proceeding. The deployment will fail if `GITHUB_PAT` is empty.

## Step 2: Setup Azure Resources

```bash
# Login to Azure
az login

# Set your subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Create resource group
az group create --name gifted-rg --location westus2
```

## Step 3: Build and Push Docker Image to GitHub Container Registry

```bash
# Navigate to the src directory (parent of Server and Shared)
cd c:\dev\giftit2\giftit\src

# Build the Docker image with GitHub Container Registry tag
# Replace YOUR_GITHUB_USERNAME with your actual GitHub username or organization
docker build -t ghcr.io/$($env:GITHUB_USERNAME)/gifted-api:latest -f Server/Dockerfile .

# Push the image to GitHub Container Registry
docker push ghcr.io/$($env:GITHUB_USERNAME)/gifted-api:latest
```

**Note**: When you first publish a package, the default visibility is private. To make it accessible:
1. Go to your GitHub profile → Packages
2. Select the `gifted-api` package
3. Go to Package settings → Change visibility (if needed)
4. Link the package to your repository

### Linking Container Images to Repository

The [Dockerfile](src/Server/Dockerfile) includes a label that automatically links the container image to your GitHub repository:

```dockerfile
LABEL org.opencontainers.image.source=https://github.com/YOUR_GITHUB_USERNAME/YOUR_REPO
```

**Make sure to update this label** with your actual repository URL. This ensures:
- The container package is properly associated with your repository
- `GITHUB_TOKEN` in GitHub Actions has appropriate permissions
- Better organization and visibility in GitHub Packages

## Step 4: Deploy Infrastructure with GitHub Container Registry Credentials

```bash
# Navigate to infrastructure directory
cd ..\infrastructure

# NOTE: The container image used below must match the containerImage parameter in main.bicepparam
# Example: param containerImage = 'ghcr.io/YOUR_GITHUB_USERNAME/gifted-api:latest'

# Deploy the Bicep template with GitHub Container Registry credentials as secure parameters
az deployment group create `
  --resource-group gifted-rg `
  --template-file main.bicep `
  --parameters main.bicepparam `
  --parameters containerImage="ghcr.io/$($env:GITHUB_USERNAME)/gifted-api:latest" `
  --parameters ghcrUsername=$env:GITHUB_USERNAME `
  --parameters ghcrPassword=$env:GITHUB_PAT
```

**Important**: The GitHub Container Registry credentials are passed as secure parameters during deployment to protect your personal access token.

## Step 5: Get Your API URL

```bash
# Get the FQDN of your Container App
az containerapp show   --name gifted-api   --resource-group gifted-rg   --query properties.configuration.ingress.fqdn   -o tsv
```

This will output something like: `gifted-api.kindtree-12345678.westus2.azurecontainerapps.io`

Your API will be available at: `https://gifted-api.kindtree-12345678.westus2.azurecontainerapps.io/api/feed`

## Step 6: Update Your Frontend

Update your WasmApp configuration to point to the new Container App URL instead of the VM.

## Monitoring & Logs

```bash
# View container app logs
az containerapp logs show   --name gifted-api   --resource-group gifted-rg   --follow

# Check current replica count (should be 0 when idle)
az containerapp replica list   --name gifted-api   --resource-group gifted-rg
```

## Scale to Zero Behavior

- **When idle**: App scales to 0 replicas, you pay almost nothing
- **On request**: Cold start takes ~3-5 seconds for first request
- **After active**: Stays warm for ~15 minutes before scaling down

## Future Updates

When you update your code:

```bash
# Rebuild and push new image to GitHub Container Registry
cd c:\dev\giftit2\giftit\src

# Build the new version
docker build -t ghcr.io/YOUR_GITHUB_USERNAME/gifted-api:latest -f Server/Dockerfile .

# Push to GitHub Container Registry
docker push ghcr.io/YOUR_GITHUB_USERNAME/gifted-api:latest

# Update Container App with new image
az containerapp update `
  --name gifted-api `
  --resource-group gifted-rg `
  --image ghcr.io/YOUR_GITHUB_USERNAME/gifted-api:latest
```

## Troubleshooting

### Check container logs
```bash
az containerapp logs show --name gifted-api --resource-group gifted-rg --follow
```

### Verify storage mount
The SQLite database should persist at `/data/storage.db` inside the container, mounted from Azure Files.

### Check replica status
```bash
az containerapp show --name gifted-api --resource-group gifted-rg --query properties.runningStatus
```
