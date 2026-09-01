using FunctionZero.ExpressionParserZero.Binding;
using System.ComponentModel;

namespace ExpressionParserZero.Tests
{
    [TestClass]
    public sealed class ExpressionParserTests
    {
        [TestMethod]
        public void ConstructorRejectsNullHost()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new PathBind(null!, nameof(BindingModel.TargetValue)));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void ConstructorRejectsMissingPath(string? path)
        {
            Assert.ThrowsExactly<ArgumentException>(() => new PathBind(new BindingModel(), path!));
        }

        [TestMethod]
        public void UnresolvedPathDoesNotThrowWhenBound()
        {
            var host = new BindingModel { SourceValue = 42 };

            using var binding = new PathBind(host, "MissingProperty");

            binding.BindTo(nameof(BindingModel.SourceValue), PathBindMode.OneWay);
        }

        [TestMethod]
        public void OneWayToSourcePropagatesOnlyFromPathToHostProperty()
        {
            var host = new BindingModel { SourceValue = 1, TargetValue = 2 };
            using var binding = new PathBind(host, nameof(BindingModel.TargetValue));

            binding.BindTo(nameof(BindingModel.SourceValue), PathBindMode.OneWayToSource);
            Assert.AreEqual(2, host.SourceValue);

            host.TargetValue = 3;
            Assert.AreEqual(3, host.SourceValue);

            host.SourceValue = 4;
            Assert.AreEqual(3, host.TargetValue);
        }

        [TestMethod]
        public void TwoWayPropagatesInBothDirections()
        {
            var host = new BindingModel { SourceValue = 1, TargetValue = 2 };
            using var binding = new PathBind(host, nameof(BindingModel.TargetValue));

            binding.BindTo(nameof(BindingModel.SourceValue), PathBindMode.TwoWay);
            Assert.AreEqual(1, host.TargetValue);

            host.TargetValue = 3;
            Assert.AreEqual(3, host.SourceValue);

            host.SourceValue = 4;
            Assert.AreEqual(4, host.TargetValue);
        }

        [TestMethod]
        public void DisposeRecursivelyUnsubscribesOnce()
        {
            var child = new ChildModel();
            var host = new BindingModel { Child = child };
            var binding = new PathBind(host, $"{nameof(BindingModel.Child)}.{nameof(ChildModel.Value)}");

            Assert.AreEqual(1, host.SubscriptionCount);
            Assert.AreEqual(1, child.SubscriptionCount);

            binding.Dispose();
            binding.Dispose();

            Assert.AreEqual(0, host.SubscriptionCount);
            Assert.AreEqual(0, child.SubscriptionCount);
        }

        [TestMethod]
        public void ExpressionBindDisposesOwnedPathBindings()
        {
            var host = new BindingModel();
            var binding = new ExpressionBind(host, nameof(BindingModel.TargetValue));

            Assert.AreEqual(1, host.SubscriptionCount);

            binding.Dispose();
            binding.Dispose();

            Assert.AreEqual(0, host.SubscriptionCount);
        }

        private sealed class BindingModel : INotifyPropertyChanged
        {
            private int _sourceValue;
            private int _targetValue;
            private ChildModel? _child;
            private PropertyChangedEventHandler? _propertyChanged;

            public event PropertyChangedEventHandler? PropertyChanged
            {
                add
                {
                    _propertyChanged += value;
                    SubscriptionCount++;
                }
                remove
                {
                    _propertyChanged -= value;
                    SubscriptionCount--;
                }
            }

            public int SubscriptionCount { get; private set; }

            public int SourceValue
            {
                get => _sourceValue;
                set
                {
                    _sourceValue = value;
                    _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceValue)));
                }
            }

            public int TargetValue
            {
                get => _targetValue;
                set
                {
                    _targetValue = value;
                    _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetValue)));
                }
            }

            public ChildModel? Child
            {
                get => _child;
                set
                {
                    _child = value;
                    _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Child)));
                }
            }
        }

        private sealed class ChildModel : INotifyPropertyChanged
        {
            private int _value;
            private PropertyChangedEventHandler? _propertyChanged;

            public event PropertyChangedEventHandler? PropertyChanged
            {
                add
                {
                    _propertyChanged += value;
                    SubscriptionCount++;
                }
                remove
                {
                    _propertyChanged -= value;
                    SubscriptionCount--;
                }
            }

            public int SubscriptionCount { get; private set; }

            public int Value
            {
                get => _value;
                set
                {
                    _value = value;
                    _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }
    }
}
