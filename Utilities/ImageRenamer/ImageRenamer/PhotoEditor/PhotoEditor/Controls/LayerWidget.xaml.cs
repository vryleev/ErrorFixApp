﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PhotoEditor.Controls
{
    public partial class LayerWidget : UserControl
    {
        public Layer ThisLayer;

        public LayerWidget(Layer layer, string name)
        {
            Height = 50;
            Width = 200;
            ThisLayer = layer;
            DataContext = ThisLayer;

            InitializeComponent();

            VisibilityON.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(VisibleChange_Click), true);
            VisibilityOFF.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(VisibleChange_Click), true);
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditBox.Text = WidgetText.Text;
            WidgetText.Visibility = Visibility.Hidden;
            EditBox.Visibility = Visibility.Visible;
            EditBox.Focus();
            EditBox.SelectAll();
        }

        private void EditBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                WidgetText.Text = EditBox.Text;
                EditBox.Visibility = Visibility.Hidden;
                WidgetText.Visibility = Visibility.Visible;
                ThisLayer.LayerName = WidgetText.Text;
            }
        }

        public void RefreshPreviewCanvas()
        {
            previewCanvas.Fill = ThisLayer.LayerImageBrush;
        }

        public void UserContol_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var widgetIndex = MainEditorWindow.LayersWidgets.IndexOf(this);
            GlobalState.CurrentLayerIndex = widgetIndex;
            ((MainEditorWindow)Application.Current.Windows[1]).sliderOpacity.Value = ThisLayer.Opacity * 100;
            ((MainEditorWindow)Application.Current.Windows[1]).widgetsCanvas.SelectedIndex = widgetIndex;
        }

        private void VisibleChange_Click(object sender, RoutedEventArgs e)
        {
            ThisLayer.VisibleChange();
        }
    }
}
