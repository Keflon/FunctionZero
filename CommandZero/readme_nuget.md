# FunctionZero.CommandZero 
Fully featured `ICommand` implementation

Documentation [here](https://functionzero.gitbook.io/)  
Source code and sample app [here](https://github.com/Keflon/FunctionZero)

## Basic Usage
`CommandZeroAsync` uses fluent API to build `ICommand` instances quickly and easily, like this:  
```csharp
ICommand CabbagesCommand = new CommandBuilder()
                .SetExecuteAsync(DoSomethingAsync)
                .SetCanExecute(CanDoSomething)
                .AddGuard(this)
                .SetName("Cabbages")
                .SetExceptionHandler(CabbagesExceptionHandler)
                // More builder methods can go here ...
                .Build(); 
```
Where
```csharp
private async Task DoSomethingAsync()
{
    // Do something awesome
}
private bool CanDoSomething()
{
    return CanDoSomethingAwesome;
}
private void CabbagesExceptionHandler(ICommandZero sourceCommand, Exception ex)
{
    Logger.Log("Not quite awesome yet");
}
```
