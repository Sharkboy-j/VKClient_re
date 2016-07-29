using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VKMessenger.Framework
{
  public static class DependencyObjectExtension
  {
    public static IEnumerable<DependencyObject> Descendents(this DependencyObject root, int depth)
    {
      int count = VisualTreeHelper.GetChildrenCount(root);
      for (int i = 0; i < count; ++i)
      {
        DependencyObject child = VisualTreeHelper.GetChild(root, i);
        yield return child;
        if (depth > 0)
        {
          DependencyObject root1 = child;
          int num = depth - 1;
          depth = num;
          int depth1 = num;
          foreach (DependencyObject descendent in root1.Descendents(depth1))
            yield return descendent;
        }
        child = (DependencyObject) null;
      }
    }

    public static IEnumerable<DependencyObject> Descendents(this DependencyObject root)
    {
      return root.Descendents(int.MaxValue);
    }

    public static IEnumerable<DependencyObject> Ancestors(this DependencyObject root)
    {
      for (DependencyObject current = VisualTreeHelper.GetParent(root); current != null; current = VisualTreeHelper.GetParent(current))
        yield return current;
    }
  }
}
