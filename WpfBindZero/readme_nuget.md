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

## Deferred functions and behaviors

`Function` compiles an expression for later evaluation. `TapTrigger` evaluates one when a button is clicked or another element is tapped. `EdgeTrigger` evaluates its `Rising` or `Falling` function when a bound condition changes.

Add the behaviors namespace alongside the BindZero namespace:

```xaml
<Window
	xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
	xmlns:z="clr-namespace:FunctionZero.Wpf.zBind.z;assembly=FunctionZero.Wpf.BindZero">
	<Grid>
		<Button Content="Increment">
			<i:Interaction.Behaviors>
				<z:TapTrigger TapAction="{z:Function Expression='Count = Count + 1'}" />
			</i:Interaction.Behaviors>
		</Button>

		<i:Interaction.Behaviors>
			<z:EdgeTrigger
				Condition="{Binding IsActive}"
				Rising="{z:Function Expression='Status = &quot;Active&quot;'}"
				Falling="{z:Function Expression='Status = &quot;Inactive&quot;'}" />
		</i:Interaction.Behaviors>
	</Grid>
</Window>
```

Without `Source`, a function evaluates against the associated element's current `DataContext`. Set `Source` to an object or an `x:Reference` when the expression should target a specific source instead.
