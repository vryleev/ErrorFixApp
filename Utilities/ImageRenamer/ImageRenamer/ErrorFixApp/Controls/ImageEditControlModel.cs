using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PhotoEditor;

namespace ErrorFixApp.Controls
{
    public class ImageEditControlModel : INotifyPropertyChanged
    { 
        private RenderTargetBitmap _rtb;
        private bool _isEditorOpened = false;
        public Image ImageV { get; set; }

        public Image ImageM { get; set; }

        private BitmapImage _bitmapImageV;
        
        public BitmapImage BImageV
        {
            get => _bitmapImageV;
            set
            {
                _bitmapImageV?.StreamSource.Dispose();
                _bitmapImageV = value;
                OnPropertyChanged();
            }
        }
        private BitmapImage _bitmapImageM;
        
        public BitmapImage BImageM
        {
            get => _bitmapImageM;
            set
            {   _bitmapImageM?.StreamSource.Dispose();
                _bitmapImageM = value;
                OnPropertyChanged();
            }
        }
        
        
        private Visibility _isVisible;
        
        public Visibility IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        
        private ICommand _editImageCommand;

        public ICommand EditImageCommand
        {
            get
            {
                return _editImageCommand ?? (_editImageCommand = new RelayCommand<object>(
                    param => this.EditImage(Convert.ToString(param)),
                    param => true
                ));
            }
        }
        
        private void EditImage(string pictureType)
        {
            try
            {
                if (!_isEditorOpened)
                {
                    _isEditorOpened = true;
                    var editorWindow = new MainEditorWindow();
                    editorWindow.Closing += EditorWindowOnClosing;
                    double layerWidth = ImageV.Width;
                    double layerHeight = ImageV.Height;

                    GlobalState.NewLayerHeight = layerHeight;
                    GlobalState.NewLayerWidth = layerWidth;
                    GlobalState.CurrentLayerIndex = 0;
                    GlobalState.LayersCount = 1;
                    GlobalState.CurrentTool = GlobalState.Instruments.Brush;

                    MainEditorWindow.WindowTrigger = 4;
                    MainEditorWindow.PictureType = pictureType;
                    MainEditorWindow.Picture = pictureType == "Visual"
                        ? ImageV.ToStream(ImageFormat.Bmp)
                        : ImageM.ToStream(ImageFormat.Bmp);

                    MainEditorWindow.EnableBlur(editorWindow);
                    //editorWindow.ShowDialog();
                    MainEditorWindow.ShowMainWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }
        
        private void EditorWindowOnClosing(object sender, CancelEventArgs e)
        {
            _isEditorOpened = false;
            _rtb = MainEditorWindow.RTB;

            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(_rtb));

            using (MemoryStream stm = new MemoryStream())
            {
                enc.Save(stm);
                Bitmap editedImage = new Bitmap(stm);
                if (MainEditorWindow.PictureType == "Visual")
                {
                    ImageV = editedImage;
                    BImageV = ImageUtils.BitmapToImageSource(editedImage);
                }
                else
                {
                   ImageM = editedImage;
                   BImageM = ImageUtils.BitmapToImageSource(editedImage);
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}