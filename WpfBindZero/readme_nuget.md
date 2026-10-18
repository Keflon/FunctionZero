`FunctionZero.Wpf.BindZero` provides XAML markup extensions for **.NET 10 WPF** that allow bindings to evaluate expressions.

## Setup

Install `FunctionZero.Wpf.BindZero` in the WPF project and add its XAML namespace:

```xaml
<Window
	xmlns:z="clr-namespace:FunctionZero.Wpf.zBind.z;assembly=FunctionZero.Wpf.BindZero">
	<StackPanel>
		<TextBlock Text="{z:Bind 'Items.Count + SelectedIndex'}" />
		<TextBox Text="{z:Bind Items[SelectedIndex], Mode=TwoWay}" />
	</StackPanel>
</Window>
```

`Bind` creates a WPF `MultiBinding` containing one child binding for each distinct expression variable. Changes to any variable reevaluate the expression. Direct and reversible unary expressions support WPF-native two-way updates; writable arrays, lists, and dictionaries support indexed write-through.

An in-place collection mutation is read on the next evaluation but does not itself guarantee reevaluation. Notify the owning collection property, change a bound index/key, or otherwise trigger reevaluation when required.

Install this package only in WPF projects. MAUI projects should continue to use the separate `FunctionZero.Maui.BindZero` package because WPF and MAUI use different XAML binding frameworks.

Source code: https://github.com/Keflon/FunctionZero
