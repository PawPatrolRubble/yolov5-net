using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using OpenCvSharp;

using Yolov5Net.Scorer;

using Pen = System.Drawing.Pen;

namespace Yolo.Sharp.App
{
    public class YoloSharpModel
    {

        private YoloScorer<NopModel> yoloScorer;
        public void LoadOnnxModel(string modelPath, string labelClassesTextPath)
        {
            var model = new NopModel();
            //model.LoadLabelFromTxtFile(Path.Combine(@"F:\source codes\yolov5-net\labels", "classes.txt"));
            model.LoadLabelFromTxtFile(labelClassesTextPath);

            //var modelPath = @"F:\source codes\yolov5-net\models\cross1.onnx";
            //NativeApiStatus.VerifySuccess(NativeMethods.OrtCreateEnv(LogLevel.Warning, @"CSharpOnnxRuntime", out handle));
            //var sessionOptions = new SessionOptions();

            //for cpu
            //var _inferenceSession = new InferenceSession(modelPath);


            //sessionOptions.AppendExecutionProvider_CUDA(0);

            //gpu version, workable with cuda 11.7, cudnn 8.9
            //实测速度和cpu推理差不多
            //yoloScorer = new YoloScorer<NopModel>(model, modelPath, SessionOptions.MakeSessionOptionWithCudaProvider(0));

            yoloScorer = new YoloScorer<NopModel>(model,modelPath);
        }

        public ImageSource Predict(Mat mat, string imageName, bool enableImageCropping = false)
        {
            var img = Image.FromStream(mat.ToMemoryStream());
            return PredictLocal(img, imageName, enableImageCropping);
        }



        public ImageSource Predict(string imagePath, bool enableImageCropping = false)
        {
            using var image = Image.FromFile(imagePath);
            return PredictLocal(image, Path.GetFileNameWithoutExtension(imagePath), enableImageCropping);
        }

        private ImageSource PredictLocal(Image image, string imageName, bool enableImageCropping)
        {
            try
            {
                var predictions = yoloScorer.Predict(image);
                using var graphics = Graphics.FromImage(image);

                foreach (var prediction in predictions) // iterate predictions to draw results
                {
                    double score = Math.Round(prediction.Score, 2);

                    graphics.DrawRectangles(new Pen(prediction.Label.Color, 1),
                        new[] { prediction.Rectangle });

                    var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);

                    graphics.DrawString($"{prediction.Label.Name} ({score})",
                        new Font("Consolas", 16, GraphicsUnit.Pixel), new SolidBrush(prediction.Label.Color),
                        new PointF(x, y));
                }

                CropImagesAndSaveToDirectory(predictions, image, imageName, enableImageCropping);

                return ImageToImageSource(image);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void SaveImage(Bitmap image, string fileName, string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CroppedImages");
            }

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                image.Save(Path.Combine(directory, fileName));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }


        private void CropImagesAndSaveToDirectory(List<YoloPrediction> predictions, Image sourceImage,
            string parentImageId, bool enableImageCropping)
        {
            if (enableImageCropping)
            {
                foreach (var prediction in predictions)
                {
                    using (Bitmap target = new Bitmap((int)prediction.Rectangle.Width, (int)prediction.Rectangle.Height))
                    {
                        using (Graphics g = Graphics.FromImage(target))
                        {
                            g.DrawImage(sourceImage, new Rectangle(0, 0, target.Width, target.Height),
                                prediction.Rectangle,
                                GraphicsUnit.Pixel);
                        }

                        try
                        {
                            var imageFileName =
                                $"{parentImageId}_{prediction.Rectangle.Location.X:f0}_{prediction.Rectangle.Y:f0}.bmp";
                            //target.Save(imageFileName);
                            SaveImage(target, imageFileName, "");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }
        }




        private ImageSource ImageToImageSource(Image img)
        {
            var bmImg = new BitmapImage();
            using MemoryStream memStream2 = new MemoryStream();

            img.Save(memStream2, System.Drawing.Imaging.ImageFormat.Png);
            memStream2.Position = 0;

            bmImg.BeginInit();
            bmImg.CacheOption = BitmapCacheOption.OnLoad;
            bmImg.UriSource = null;
            bmImg.StreamSource = memStream2;
            bmImg.EndInit();

            return bmImg;
        }


        public void PredictFromVideo(string videoPath)
        {

        }
    }
}
