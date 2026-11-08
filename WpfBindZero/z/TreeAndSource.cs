using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Evaluator;
using System.Windows;

namespace FunctionZero.Wpf.zBind.z
{
    public sealed class TreeAndSource
    {
        internal TreeAndSource(ExpressionTree tree, Func<FrameworkElement, IBackingStore?> sourceGetter)
        {
            Tree = tree;
            SourceGetter = sourceGetter;
        }

        internal ExpressionTree Tree { get; }

        internal Func<FrameworkElement, IBackingStore?> SourceGetter { get; }

        internal void Evaluate(FrameworkElement host)
        {
            IBackingStore? backingStore = SourceGetter(host);
            if (backingStore != null)
                Tree.Evaluate(backingStore);
        }
    }
}
