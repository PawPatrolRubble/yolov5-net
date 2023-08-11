using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.WindowsAPICodePack.Dialogs;

using OpenCvSharp;

using Prism.Commands;
using Prism.Mvvm;

namespace Yolo.Sharp.App;

public class MainWindowViewModel : BindableBase
{
    #region constructors

    private string _modelPath;
    private string _labelPath;

    public MainWindowViewModel()
    {
        Predict = new DelegateCommand(PredictImp);
        LoadModel = new DelegateCommand(LoadModelImpl);
        PredictImageFromVideoCommand = new DelegateCommand(PredictImageFromVideo);
        _model = new YoloSharpModel();
        ImagePath = "D:\\BaiduNetdiskDownload\\frames\\2.jpg";

        VideoPath = "D:\\BaiduNetdiskDownload\\07.mp4";

        ChooseFolderCommand = new DelegateCommand(ChooseFolderCommandExecute);

        IterateImagesCommand = new DelegateCommand(
            IterateImagesWithinDir
        );
    }

    #endregion

    #region fields

    private readonly YoloSharpModel _model;


    private ImageSource _imageSource;

    private ImageSource _result = new BitmapImage();

    private int _lastPredictionTimeUsed;

    private string _imageDirectory;

    private string _selectedImagePath;


    private string _splitImageDirectory;

    #endregion

    #region properties

    public DelegateCommand ChooseFolderCommand { get; }
    public ICommand IterateImagesCommand { get; set; }
    public ICommand LoadModel { get; set; }

    public ICommand Predict { get; set; }
    public ICommand PredictImageFromVideoCommand { get; set; }

    public ImageSource ImageSource
    {
        get => _imageSource;
        set =>
            //_imageSource = value;
            SetProperty(ref _imageSource, value);
    }

    public int LastPredictionTimeUsed
    {
        get => _lastPredictionTimeUsed;
        set => SetProperty(ref _lastPredictionTimeUsed, value);
    }

    public ObservableCollection<string> ImagePaths { get; set; } = new();

    public string ImageDirectory
    {
        get => _imageDirectory;
        set => SetProperty(ref _imageDirectory, value, UpdateImageFiles);
    }

    public string ImagePath { get; set; }

    public string SelectedImagePath
    {
        get => _selectedImagePath;
        set => SetProperty(ref _selectedImagePath, value);
    }

    public string SplitImageDirectory
    {
        get => _splitImageDirectory;
        set => SetProperty(ref _splitImageDirectory, value);
    }

    public string VideoPath { get; set; }

    #endregion

    #region all other memebers

    private void UpdateImageFiles()
    {
        try
        {
            if (string.IsNullOrEmpty(ImageDirectory)) return;

            var files = Directory.GetFiles(ImageDirectory).Where(x => x.EndsWith(".jpg") || x.EndsWith(".bmp"));
            ImagePaths.Clear();
            ImagePaths.AddRange(files);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    #endregion

    #region all other members

    private async void PredictImageFromVideo()
    {
        var capture = new VideoCapture(VideoPath);
        var image = new Mat();
        //capture.FrameCount;
        capture.Read(image);
        var frameCount = 0;
        while (capture.IsOpened())
        {
            capture.Read(image);
            if (image.Empty())
                break;
            try
            {
                _model.Predict(image, $"{Path.GetFileNameWithoutExtension(VideoPath)}_{frameCount}");
                frameCount++;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(200);
        }
    }

    private async void IterateImagesWithinDir()
    {
        if (string.IsNullOrEmpty(ImagePath)) return;

        var dir = Path.GetDirectoryName(ImagePath);
        var files = Directory.GetFiles(dir).Where(x => x.EndsWith(".jpg"));
        foreach (var file in files)
            try
            {
                ImageSource = _model.Predict(file);
                await Task.Delay(200);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
    }

    private void LoadModelImpl()
    {
        _model.LoadOnnxModel(_modelPath, _labelPath);
    }

    private void ChooseFolderCommandExecute()
    {
        var dialog = new CommonOpenFileDialog();
        dialog.IsFolderPicker = true;
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            if (dialog.FileName != null)
                ImageDirectory = dialog.FileName;
    }

    private void PredictImp()
    {
        if (string.IsNullOrEmpty(SelectedImagePath))
        {
            LastPredictionTimeUsed = 0;
            return;
        }

        var stop = Stopwatch.StartNew();

        ImageSource = _model.Predict(SelectedImagePath, EnableCropping);
        stop.Stop();
        LastPredictionTimeUsed = (int)stop.ElapsedMilliseconds;
    }

    private bool _enableCropping;

    public bool EnableCropping
    {
        get => _enableCropping;
        set => SetProperty(ref _enableCropping, value);
    }

    private DelegateCommand _predictAndCropCommand;

    public DelegateCommand PredictAndCropCommand
    {
        get => _predictAndCropCommand ??= new DelegateCommand(PredictAndCrop);

    }

    private void PredictAndCrop()
    {
        EnableCropping = true;
        foreach (var imagePath in ImagePaths)
        {
            _model.Predict(imagePath, EnableCropping);
        }
    }

    #endregion
}