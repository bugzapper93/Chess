using Chess.Objects;
using Chess.Tools;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chess;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    #region Initialization
 
    #endregion
    #region UIManagement
    private static async Task AnimateAndRemoveItems(ListBox listBox)
    {
        if (listBox.Items.Count == 0) return;

        var itemsToRemove = listBox.Items.Cast<object>().ToList();
        var tcs = new TaskCompletionSource<bool>();
        int animationsPending = itemsToRemove.Count;

        if (animationsPending == 0)
        {
            tcs.SetResult(true);
        }
        else
        {
            foreach (var item in itemsToRemove)
            {
                var listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                if (listBoxItem != null)
                {
                    var fadeOut = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(0.1)
                    };
                    fadeOut.Completed += (s, e) =>
                    {
                        listBox.Items.Remove(item);
                        if (System.Threading.Interlocked.Decrement(ref animationsPending) == 0)
                        {
                            tcs.SetResult(true);
                        }
                    };
                    listBoxItem.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
                else
                {
                    listBox.Items.Remove(item);
                    if (System.Threading.Interlocked.Decrement(ref animationsPending) == 0)
                    {
                        tcs.SetResult(true);
                    }
                }
            }
        }

        await tcs.Task;
    }
    #endregion
}