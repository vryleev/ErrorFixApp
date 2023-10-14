using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ErrorDataLayer;
using ErrorFixApp.Properties;
using PhotoEditor;


namespace ErrorFixApp
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            MainPanelControlVm = new MainPanelControlModel();
        }
        private RenderTargetBitmap _rtb;
        private bool _isEditorOpened = false;

        public string ApplicationName
        {
            get
            {
                if (ConfigurationManager.AppSettings["WorkingType"] == "Local")
                {
                    return
                        $"Logos Error Fix App/ {Resources.User}: {ConfigurationManager.AppSettings["User"]}/ {Resources.WorkingType}: {ConfigurationManager.AppSettings["WorkingType"]}";
                }
                else
                {
                    return
                        $"Logos Error Fix App/ {Resources.User}: {ConfigurationManager.AppSettings["User"]}/ {Resources.WorkingType}: {ConfigurationManager.AppSettings["WorkingType"]}/ Host: {ConfigurationManager.AppSettings["RemoteUrl"]}";
                }
            }
        }
        
        private MainPanelControlModel _mainPanelControlVm;
        public MainPanelControlModel MainPanelControlVm
        {
            get => _mainPanelControlVm;
            set
            {
                _mainPanelControlVm = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private ICommand _closingCommand;

        public ICommand ClosingCommand
        {
            get
            {
                return _closingCommand ?? (_closingCommand = new RelayCommand(
                    param => this.ClosingDb(),
                    param => true
                ));
            }
        }

        private ICommand _editImageCommand;

        public ICommand EditImageCommand
        {
            get
            {
                return _editImageCommand ?? (_editImageCommand = new RelayCommand(
                    param => this.EditImage(Convert.ToString(param)),
                    param => true
                ));
            }
        }
        public void ClosingDb()
        {
            SqLiteManager.IsCheckQueue = false;
        }

        private void EditImage(string pictureType)
        {
            if (!_isEditorOpened)
            {
                _isEditorOpened = true;
                var editorWindow = new MainEditorWindow();
                editorWindow.Closing += EditorWindowOnClosing;
                double layerWidth =  MainPanelControlVm.Error.ImageV.Width;
                double layerHeight =  MainPanelControlVm.Error.ImageV.Height;

                GlobalState.NewLayerHeight = layerHeight;
                GlobalState.NewLayerWidth = layerWidth;
                GlobalState.CurrentLayerIndex = 0;
                GlobalState.LayersCount = 1;
                GlobalState.CurrentTool = GlobalState.Instruments.Brush;

                MainEditorWindow.WindowTrigger = 4;
                MainEditorWindow.PictureType = pictureType;
                MainEditorWindow.Picture = pictureType == "Visual"
                    ?  MainPanelControlVm.Error.ImageV.ToStream(ImageFormat.Bmp)
                    :  MainPanelControlVm.Error.ImageM.ToStream(ImageFormat.Bmp);

                MainEditorWindow.EnableBlur(editorWindow);
                MainEditorWindow.ShowMainWindow();
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
                    MainPanelControlVm.Error.ImageV = editedImage;
                    MainPanelControlVm.Error.BImageV = ImageUtils.BitmapToImageSource(editedImage);
                }
                else
                {
                    MainPanelControlVm.Error.ImageM = editedImage;
                    MainPanelControlVm.Error.BImageM = ImageUtils.BitmapToImageSource(editedImage);
                }
            }
        }
    }
}