using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for ThemeWindow.xaml
    /// </summary>
    public partial class ThemeWindow : Window
    {
        FrameworkElement lastElement = null;
        Border lastHighlightedBorder = null;
        public ThemeWindow()
        {
            InitializeComponent();
            this.Closing += ThemeWindow_Closing;
            GameManagerWindow.Instance.MouseMove += Instance_MouseMove;
        }

        private void ThemeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClearLastSelection();
            GameManagerWindow.Instance.MouseMove -= Instance_MouseMove;
        }

        private void Instance_MouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement element = System.Windows.Input.Mouse.DirectlyOver as FrameworkElement;
            if (element != null && element != lastElement)
            {
                HighlightNearestElement(element);
            }
        }

        private void HighlightNearestElement(FrameworkElement element)
        {
            if (element.Name.Contains("Border"))
            {
                Border b = element as Border;
                if (b != null && b != lastHighlightedBorder)
                {
                    b.BorderBrush = Brushes.White;
                    ClearLastSelection();
                    lastHighlightedBorder = b;
                    lastElement = element;
                }
            }
            else
            {
                if (element.Parent != null)
                {
                    FrameworkElement parent = element.Parent as FrameworkElement;
                    if (parent != null)
                    {
                        HighlightNearestElement(parent);
                    }
                }
            }
        }

        private void ClearLastSelection()
        {
            if (lastHighlightedBorder != null)
            {
                lastHighlightedBorder.BorderBrush = Brushes.Transparent;
            }
        }

        public static string GetName(object obj)
        {
            // First see if it is a FrameworkElement
            var element = obj as FrameworkElement;
            if (element != null)
                return element.Name;
            // If not, try reflection to get the value of a Name property.
            try { return (string)obj.GetType().GetProperty("Name").GetValue(obj, null); }
            catch
            {
                // Last of all, try reflection to get the value of a Name field.
                try { return (string)obj.GetType().GetField("Name").GetValue(obj); }
                catch { return null; }
            }
        }
    }
}
