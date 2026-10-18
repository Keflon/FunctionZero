`FunctionZero.Maui.BindZero` provides XAML markup extensions for **.NET 10 MAUI** that allow bindings to evaluate expressions.

For legacy .NET 8 and earlier releases, see [`FunctionZero.Maui.zBind`](https://www.nuget.org/packages/FunctionZero.Maui.zBind).

If you want to do things like this: (note the expression is enclosed inside quotes)
```xaml
<StackLayout IsVisible="{z:Bind '(Item.Count != 0) AND (Status == \'Administrator\')'}" > ...
```

1. Install `FunctionZero.Maui.BindZero` in the MAUI project.
2. Add `xmlns:z="clr-namespace:FunctionZero.Maui.zBind.z;assembly=FunctionZero.Maui.BindZero"
`  
To your `xaml` page (or let Visual Studio do it for you)

Documentation [here](https://functionzero.gitbook.io/)  
Source code and sample app [here](https://github.com/Keflon/FunctionZero)
