# OpenAI Cmdlet
A simple PowerShell cmdlet for invoking OpenAI API. 

For API details, please check [OpenAI API Documentation](https://platform.openai.com/docs/introduction/overview)

For more usage examples, please check [OpenAI API Examples](https://platform.openai.com/examples)

## Requirements
- [ OpenAI API Key ]( https://openai.com/blog/openai-api )
- [ PowerShell >= 7.3.3](https://github.com/PowerShell/powershell/releases)

## Getting started
### Import the module
```powershell
git clone git@github.com:Neroro64/OpenAICmdlet.git
Import-Module OpenAICmdlet/PSCmdlet/InvokeOpenAI.psd1

# To set the API key
Set-OpenAIAPIKey
> Enter your API key: : ********
"Success! The API key is encrypted and stored in $ENV:OPENAI_API_KEY (default: $PSScriptRoot/OpenAI_API.key)"
```

### Chat example
NOTE: The last returned response is automatically stored in `$Global:LastOpenAIResponse` and the last chat conversation is stored in `$Global:LastChatGPTConversation`.
```powershell
Invoke-OpenAI -Task:"Chat" -Prompt:"Help me learn dotnet Dependency injection" -Temperature:1 -MaxTokens:500 -Samples:1 -StopSequences:"\n" | % Response
```
---
>Sure, I can help you with that!
>
>In .NET, Dependency Injection (DI) is a design pattern that helps developers achieve loosely coupled code, which is more maintainable, testable, and extensible. DI allows developers to inject dependencies (such as interface contracts) into their code rather than creating the dependencies themselves.
>
>Here is a basic example of how to use DI in C#:
>
>1. Define the interface contract for the dependency:
>
>```c#
>public interface IMyService
>{
>    void DoSomething();
>}
>```
>
>2. Implement the dependency:
>
>```c#
>public class MyService : IMyService
>{
>    public void DoSomething()
>    {
>        Console.WriteLine("Did something!");
>    }
>}
>```
>
>3. In the class where you want to use the dependency, create a constructor that takes the dependency as a parameter:
>
>```c#
>public class MyClass
>{
>    private readonly IMyService _myService;
>
>    public MyClass(IMyService myService)
>    {
>        _myService = myService;
>    }
>
>    public void MyMethod()
>    {
>        _myService.DoSomething();
>    }
>}
>```
>
>4. Register the dependency with the dependency injection container:
>
>```c#
>services.AddScoped<IMyService, MyService>();
>```
>
>5. In the code that uses the class, inject the dependency using the dependency injection container:
>
>```c#
>public static void Main(string[] args)
>{
>    var serviceProvider = new ServiceCollection()
>        .AddScoped<IMyService, MyService>()
>        .BuildServiceProvider();
>
>    var myClass = new MyClass(serviceProvider.GetService<IMyService>());
>    myClass.MyMethod();
>}
>```
>
>This is just a basic example, but there's a lot more to learn about DI and how to use it effectively. I hope this helps as a starting point!

---
To continue chatting

```powershell
Invoke-OpenAI -Task:"Chat" -Prompt:"Show me more complex examples" -Temperature:1 -MaxTokens:500 -Samples:1 -StopSequences:"\n" | % Reponse
```
> Sure, here's a more complex example that involves multiple dependencies and configuration:
> 
> 1. Define the interface contracts for the dependencies:
> 
> ```c#
> public interface IMyService1
> {
>     void DoSomething();
> }
> 
> public interface IMyService2
> {
>     void DoSomethingElse();
> }
> ```
> 
> 2. Implement the dependencies:
> 
> ```c#
> public class MyService1 : IMyService1
> {
>     public void DoSomething()
>     {
>         Console.WriteLine("Did something!");
>     }
> }
> 
> public class MyService2 : IMyService2
> {
>     private readonly string _configValue;
> 
>     public MyService2(IConfiguration configuration)
>     {
>         _configValue = configuration["MyConfigValue"];
>     }
> 
>     public void DoSomethingElse()
>     {
>         Console.WriteLine($"Did something else with config value: {_configValue}");
>     }
> }
> ```
> 
> 3. In the class where you want to use the dependencies, create a constructor that takes the dependencies as parameters:
> 
> ```c#
> public class MyClass
> {
>     private readonly IMyService1 _myService1;
>     private readonly IMyService2 _myService2;
> 
>     public MyClass(IMyService1 myService1, IMyService2 myService2)
>     {
>         _myService1 = myService1;
>         _myService2 = myService2;
>     }
> 
>     public void MyMethod()
>     {
>         _myService1.DoSomething();
>         _myService2.DoSomethingElse();
>     }
> }
> ```
> 
> 4. Register the dependencies and the configuration with the dependency injection container:
> 
> ```c#
> services.AddScoped<IMyService1, MyService1>();
> services.AddScoped<IMyService2, MyService2>();
> services.AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
>     .AddJsonFile("appsettings.json")
>     .Build());
> ```
> 
> 5. In the code that uses the class, inject the dependencies using the dependency injection container:
> 
> ```c#
> public static void Main(string[] args)
> {
>     var serviceProvider = new ServiceCollection()
>         .AddScoped<IMyService1, MyService1>()
>         .AddScoped<IMyService2, MyService2>()
>         .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
>             .AddJsonFile("appsettings.json")
>             .Build())
>         .BuildServiceProvider();
> 
>     var myClass = new MyClass(
>         serviceProvider.GetService<IMyService1>(),
>         serviceProvider.GetService<IMyService2>());
>     myClass.MyMethod();
> }
> ```
> 
> This example shows how DI can be used to inject multiple dependencies, including dependencies that require configuration values. Again, this is just scratching the surface of DI, but I hope it helps you understand how it can be used in a more complex scenario.


### Image generation example
```powershell
Invoke-OpenAI -Task:ImageGeneration -Prompt:"A happy man eating hot dog" -ImageSize:"256x256" | Select Response -First 1 | Start-process
# The response contains an url to the generated image. By pipling this url to Start-Process we can open the link in the browser
```
![the_generated_image]( resources/a_happy_man_eating_hot_dog.png )

## To run the tests
```powershell
Invoke-Pester PSCmdlet/Tests/InvokeOpenAI.Tests.ps1 -Output Detailed
```