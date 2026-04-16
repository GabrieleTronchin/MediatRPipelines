<#
.SYNOPSIS
    Connects to MediatR Playground stream endpoints and prints elements as they arrive.

.DESCRIPTION
    Uses HttpClient with ReadAsStreamAsync to consume IAsyncEnumerable endpoints
    in real time, printing each JSON element as it is received from the server.

    This is useful because standard HTTP clients (browsers, .http files) wait for
    the full response before displaying anything. This script shows the streaming
    behavior as the server produces each element.

.PARAMETER BaseUrl
    The base URL of the running API. Defaults to http://localhost:5005.

.PARAMETER Endpoint
    Which stream endpoint to call:
      - stream          : /StreamRequests/SampleStreamEntity
      - streamfilter    : /StreamRequests/SampleStreamEntityWithPipeFilter
    Defaults to "stream".

.EXAMPLE
    # Stream with default logging pipeline
    .\scripts\stream-client.ps1

.EXAMPLE
    # Stream with authorization filter pipeline
    .\scripts\stream-client.ps1 -Endpoint streamfilter

.EXAMPLE
    # Custom base URL
    .\scripts\stream-client.ps1 -BaseUrl http://localhost:5050 -Endpoint stream
#>

param(
    [string]$BaseUrl = "http://localhost:5005",
    [ValidateSet("stream", "streamfilter")]
    [string]$Endpoint = "stream"
)

$ErrorActionPreference = "Stop"

# Map friendly names to paths
$paths = @{
    "stream"       = "/StreamRequests/SampleStreamEntity"
    "streamfilter" = "/StreamRequests/SampleStreamEntityWithPipeFilter"
}

$url = "$BaseUrl$($paths[$Endpoint])"

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " MediatR Playground — Stream Client" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " Endpoint : $Endpoint" -ForegroundColor Gray
Write-Host " URL      : $url" -ForegroundColor Gray
Write-Host " Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

try {
    $handler = [System.Net.Http.HttpClientHandler]::new()
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [System.TimeSpan]::FromMinutes(5)

    $request = [System.Net.Http.HttpRequestMessage]::new(
        [System.Net.Http.HttpMethod]::Get, $url
    )
    $request.Headers.Accept.Add(
        [System.Net.Http.Headers.MediaTypeWithQualityHeaderValue]::new("application/json")
    )

    # Use ResponseHeadersRead so we start reading as soon as headers arrive
    $response = $client.SendAsync(
        $request,
        [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead
    ).GetAwaiter().GetResult()

    $response.EnsureSuccessStatusCode()

    Write-Host "Connected! Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Waiting for stream elements..." -ForegroundColor Gray
    Write-Host ""

    $stream = $response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
    $reader = [System.IO.StreamReader]::new($stream)

    $elementCount = 0
    $buffer = ""

    while (-not $reader.EndOfStream) {
        $char = [char]$reader.Read()
        $buffer += $char

        # Try to detect complete JSON objects by tracking braces
        $openBraces = ($buffer.ToCharArray() | Where-Object { $_ -eq '{' }).Count
        $closeBraces = ($buffer.ToCharArray() | Where-Object { $_ -eq '}' }).Count

        if ($openBraces -gt 0 -and $openBraces -eq $closeBraces) {
            $elementCount++
            $trimmed = $buffer.Trim().TrimStart(',').TrimStart('[').TrimEnd(']').Trim()

            if ($trimmed.Length -gt 0) {
                $timestamp = Get-Date -Format "HH:mm:ss.fff"
                Write-Host "[$timestamp] Element #$elementCount" -ForegroundColor Green -NoNewline
                Write-Host " $trimmed" -ForegroundColor White
            }

            $buffer = ""
        }
    }

    Write-Host ""
    Write-Host "Stream completed. Total elements received: $elementCount" -ForegroundColor Cyan
}
catch [System.Net.Http.HttpRequestException] {
    Write-Host "ERROR: Could not connect to $url" -ForegroundColor Red
    Write-Host "Make sure the API is running: dotnet run --project src/MediatR.Playground.API" -ForegroundColor Yellow
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    if ($reader) { $reader.Dispose() }
    if ($stream) { $stream.Dispose() }
    if ($response) { $response.Dispose() }
    if ($client) { $client.Dispose() }
}
