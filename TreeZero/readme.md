# FunctionZero.TreeZero

This is a simple library for managing an observable tree hierarchy of nodes.  
If you use DataBinding, you can bind a TreeView directly to a Node and it will track changes.

## Basic usage

First, specialize a Node like this:  
```csharp
using FunctionZero.TreeZero;  

public class MyLovelyNode : Node<MyLovelyNode>
{  
    // Make it yours ...  
}
```
Or if you want to inject children into the constructor:
```csharp
using FunctionZero.TreeZero;  

public class MyLovelyNode : Node<MyLovelyNode>
{  
    public MyLovelyNode(ObservableCollection<MyLovelyNode> children, ...) : base(children)
    {
    }
    // Make it yours ...  
}
```

Then it's simply a case of sticking nodes together. You can do that by:  
* `someNode.Children.Add(childNode);`  
Or
* `childNode.Parent = parentNode;`
* Or by injecting a collection of nodes into a parent node constructor.
```csharp
  ObservableCollection<MyLovelyNode> children = GetSomeChildren();
  var someNode = new MyLovelyNode(children)
```

If a node is already in the tree and you set its Parent property to a different node, the node will be moved to the new parent.  
If a node is already in the tree and you add it to the Children of a different node, the node will be moved to the new parent.

### If you try any of the following you'll get a suitable exception:
* Set a node parent to itself
* Add a node to its own children
* Move a node into one of its descendants
* Add a node to the children of its current parent
* Inject Children into a node constructor when one or more child nodes are already part of the tree
### Note:
Children are ObservableCollections, so you can DataBind a TreeView to a node and it will track changes.

# Examples
## For the following node:
```csharp
using FunctionZero.TreeZero;  

public class TestNode : Node<TestNode>
{  
    public TestNode(string name, ObservableCollection<TestNode> children = null) : base(children)
    {
        Name = name;
    }
    public Name{ get; }
}
```
## Building a tree by adding to the Children of a node
```csharp
var root = new TestNode("Root");

var child0 = new TestNode("0");
var child1 = new TestNode("1");
var child2 = new TestNode("2");

root.Children.Add(child0);
root.Children.Add(child1);
root.Children.Add(child2);
```
## Building a tree by setting the Parent property of a node

```csharp
var root = new TestNode("Root");

var child0 = new TestNode("0");
var child1 = new TestNode("1");
var child2 = new TestNode("2");

child0.Parent = root;
child1.Parent = root;
child2.Parent = root;
```
## Building a tree by injecting a collection of child nodes

```csharp
ObservableCollection<TestNode> childNodes = new ObservableCollection<TestNode>();
childNodes.Add(new TestNode("0"));
childNodes.Add(new TestNode("1"));
childNodes.Add(new TestNode("2"));

var root = new TestNode("Root", childNodes);
```

For more examples, take a look at the unit tests on GitHub.