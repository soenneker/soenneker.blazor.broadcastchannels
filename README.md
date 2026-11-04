[![](https://img.shields.io/nuget/v/soenneker.blazor.broadcastchannels.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.broadcastchannels/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.broadcastchannels/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.broadcastchannels/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.broadcastchannels.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.broadcastchannels/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.broadcastchannels)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.broadcastchannels/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.broadcastchannels/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Blazor.BroadcastChannels
### Blazor interop for the browser BroadcastChannel API.

## Installation

```bash
dotnet add package Soenneker.Blazor.BroadcastChannels
```

## Setup

Register services in `Program.cs`:

```csharp
builder.Services.AddBroadcastChannelsAsScoped();
```

Inject the higher-level utility where you need it:

```csharp
@inject IBroadcastChannels BroadcastChannels
```

## Usage

Initialize the package once before first use:

```csharp
await BroadcastChannels.Initialize();
```
