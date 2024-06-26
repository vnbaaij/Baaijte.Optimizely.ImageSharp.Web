## What's New

### V3.0.1
Package now uses:
SixLabors.ImageSharp 3.1.4
SixLabors.ImageSharp.Web 3.1.2
SixLabors.ImageSharp.Web.Providers.Azure 3.1.2

### V3.0.0
**Targeting .NET 8** 

Package now uses:
SixLabors.ImageSharp Version="3.1.3"
SixLabors.ImageSharp.Web Version="3.1.1"
SixLabors.ImageSharp.Web.Providers.Azure Version="3.1.1"

### V2.2.1
Package now uses SixLabors.ImageSharp 3.0.2 and SixLabors.ImageSharp.Web 3.0.1.
Package now also references SixLabors.ImageSharp.Web.Providers.Azure

### V2.2.0
Package now uses SixLabors.ImageSharp 3.0.1 and SixLabors.ImageSharp.Web 3.0.1

### License
  
- Baaijte.Optimizely.ImageSharp.Web is licensed under the [Apache License, Version 2.0](https://opensource.org/licenses/Apache-2.0)  
- ***This package is supported in a best effort sense***

 ***Note*** 
 As of version 2.0.0 this package targets .NET6.0 and Optimizely.CMS.Core version 12.5.0 or higher

### Installation
  
Baaijte.OptimizelyImageSharp.Web is installed via the [Optimizely NuGet feed](https://nuget.optimizely.com/package/?id=Baaijte.Optimizely.ImageSharp.Web) 

#### [Package Manager](#tab/tabid-1)

```bash
PM > Install-Package Baaijte.Optimizely.ImageSharp.Web -Version VERSION_NUMBER
```

#### [.NET CLI](#tab/tabid-2)

```
dotnet add package Baaijte.Optimizely.ImageSharp.Web --version VERSION_NUMBER
```

### Setup and Configuration
Once installed you will need to add the following code  to `ConfigureServices` and `Configure` in your `Startup.cs` file.

This installs the the default service and options.

``` c#
public void ConfigureServices(IServiceCollection services) {
    // Add the default service and options.
    services.AddBaaijteOptimizelyImageSharp();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

    // Add the image processing middleware.
    app.UseBaaijteOptimizelyImageSharp();
}
```
***DO NOT*** add other `SixLabors.ImageSharp.Web` settings!!

Also add `using Baaijte.Optimizely.ImageSharp.Web;` at the top of your `Startup.cs` file if it was not automatically added.

### Configuration for use with DXP
``` c#
public void ConfigureServices(IServiceCollection services) {
    services.AddImageSharp()
        .Configure<AzureBlobStorageCacheOptions>(options =>
        {
            options.ConnectionString = _configuration.GetConnectionString("EPiServerAzureBlobs");
            options.ContainerName = "mysitemedia";
        })
        .ClearProviders()
        .AddProvider<BlobImageProvider>()
        .SetCache<AzureBlobStorageCache>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

    // Add the image processing middleware.
    app.UseBaaijteOptimizelyImageSharp();
}
```
