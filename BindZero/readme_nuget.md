`FunctionZero.Maui.BindZero` is a `xaml` markup extension for **MAUI** that allows you to bind directly to an `expression` 

See [here](https://www.nuget.org/packages/FunctionZero.Maui.zBind) for `.NET 8` and below 

If you want to do things like this: (note the expression is enclosed inside quotes)
```xaml
<StackLayout IsVisible="{z:Bind '(Item.Count != 0) AND (Status == \'Administrator\')'}" > ...
```

1. Install `FunctionZero.Maui.BindZero` to your shared project
2. add  `xmlns:z="clr-namespace:FunctionZero.Maui.zBind.z;assembly=FunctionZero.Maui.BindZero"
`  
To your `xaml` page (or let Visual Studio do it for you)

Documentation [here](https://functionzero.gitbook.io/)  
Source code and sample app [here](https://github.com/Keflon/FunctionZero)
