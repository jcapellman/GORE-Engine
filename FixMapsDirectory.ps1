# Fix Maps directory structure
Write-Host "Fixing Maps directory structure..."

# Remove the Maps file if it exists
if (Test-Path "test\Assets\Maps" -PathType Leaf) {
    Remove-Item "test\Assets\Maps" -Force
    Write-Host "Removed Maps file"
}

# Create Maps directory if it doesn't exist
if (-not (Test-Path "test\Assets\Maps" -PathType Container)) {
    New-Item -ItemType Directory -Path "test\Assets\Maps" -Force | Out-Null
    Write-Host "Created Maps directory"
}

# Copy JSON files from src to test
Copy-Item "src\Assets\Maps\*.json" "test\Assets\Maps\" -Force -ErrorAction SilentlyContinue
Write-Host "Copied map files to test project"

Write-Host "Done! Close Visual Studio and run this script, then reopen VS."
