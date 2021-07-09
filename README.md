## License
  
- Baaijte.Optimizely.ImageSharp.Web is licensed under the [Apache License, Version 2.0](https://opensource.org/licenses/Apache-2.0)  
- ***This package is supported in a best effort sense***

### Installation
  
Baaijte.OptimizelyImageSharp.Web is installed via the [Optimizely NuGet feed](https://nuget.episerver.com/package/?id=Baaijte.Optimizely.ImageSharp.Web) 

# [Package Manager](#tab/tabid-1)

```bash
PM > Install-Package Baaijte.Optimizely.ImageSharp.Web -Version VERSION_NUMBER
```

# [.NET CLI](#tab/tabid-2)

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

Also add `using Baaijte.Optimizely.ImageSharp.Web;` at the top of your `Startup.cs` =file if it was not automatically added