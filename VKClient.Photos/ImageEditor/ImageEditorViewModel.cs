using ExifLib;
using Microsoft.Phone;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VKCamera.Common;
using VKClient.Audio.Base;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Utils;

namespace VKClient.Photos.ImageEditor
{
    public class ImageEditorViewModel : ViewModelBase
    {
        //private static readonly int JPEG_QUALITY = 85;
        private static readonly string NormalFilterName = "Normal";
        private Guid _sessionId;
        private SessionEffects _sessionEffectsInfo;
        private string _albumId;
        private int _seqNo;
        private ImageEffectsInfo _currentEffects;
        private Size _viewportSize;
        private bool _applyingEffects;
        private WriteableBitmap _originalImage;
        private string _orImAlbumId;
        private int _orImSeqNo;
        private MediaLibrary _ml;
        private PictureAlbum _album;

        public Size ViewportSize
        {
            get
            {
                return this._viewportSize;
            }
        }

        public SolidColorBrush CropBrush
        {
            get
            {
                return this.BrushByBool(!this.CropApplied);
            }
        }

        public bool CropApplied
        {
            get
            {
                if (this._currentEffects.CropRect == null)
                    return this._currentEffects.RotateAngle != 0.0;
                return true;
            }
        }

        public SolidColorBrush TextBrush
        {
            get
            {
                return this.BrushByBool(!this.TextApplied);
            }
        }

        public bool TextApplied
        {
            get
            {
                return !string.IsNullOrEmpty(this._currentEffects.Text);
            }
        }

        public SolidColorBrush FilterBrush
        {
            get
            {
                return this.BrushByBool(!this.FilterApplied);
            }
        }

        public bool FilterApplied
        {
            get
            {
                return this._currentEffects.Filter != "Normal";
            }
        }

        public SolidColorBrush ContrastBrush
        {
            get
            {
                return this.BrushByBool(!this.ContrastApplied);
            }
        }

        public bool ContrastApplied
        {
            get
            {
                return this._currentEffects.Contrast;
            }
        }

        public bool ApplyingEffects
        {
            get
            {
                return this._applyingEffects;
            }
            set
            {
                this._applyingEffects = value;
                this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.ApplyingEffects));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.ApplyingEffectsVisibility));
            }
        }

        public Visibility ApplyingEffectsVisibility
        {
            get
            {
                return !this._applyingEffects ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public bool SuppressParseEXIF { get; set; }

        public ImageEditorViewModel()
        {
            this._sessionId = Guid.NewGuid();
            this.EnsureFolder();
            this._sessionEffectsInfo = new SessionEffects();
            this._viewportSize = ScaleFactor.GetScaleFactor() != 150 ? new Size((double)(480 * ScaleFactor.GetScaleFactor() / 100), (double)(800 * ScaleFactor.GetScaleFactor() / 100)) : new Size(720.0, 1280.0);
            this._currentEffects = new ImageEffectsInfo();
        }

        private SolidColorBrush BrushByBool(bool b)
        {
            if (!b)
                return Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            return new SolidColorBrush(Colors.White);
        }

        public void CleanupSession()
        {
            ThreadPool.QueueUserWorkItem((WaitCallback)(o =>
            {
                try
                {
                    this.ResetCachedMediaLibrary();
                    this.DeleteSessionDir();
                }
                catch
                {
                }
            }));
        }

        public void ResetCachedMediaLibrary()
        {
            try
            {
                if (this._album != (PictureAlbum)null)
                {
                    this._album.Dispose();
                    this._album = (PictureAlbum)null;
                }
                if (this._ml == null)
                    return;
                this._ml.Dispose();
                this._ml = (MediaLibrary)null;
            }
            catch
            {
            }
        }

        public ImageEffectsInfo GetImageEffectsInfo(string albumId, int seqNo)
        {
            return this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
        }

        public void SetCurrentPhoto(string albumId, int seqNo)
        {
            this._albumId = albumId;
            this._seqNo = seqNo;
            this._currentEffects = this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
            this.CallPropertyChanged();
        }

        private void CallPropertyChanged()
        {
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.ContrastBrush));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.FilterBrush));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.TextBrush));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.CropBrush));
        }

        public List<ImageEffectsInfo> GetAppliedEffects()
        {
            return this._sessionEffectsInfo.GetApplied();
        }

        public Stream GetImageStream(string albumId, int seqNo, bool preview = false)
        {
            ImageEffectsInfo imageEffectsInfo1 = this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
            if (!preview && imageEffectsInfo1.AppliedAny)
            {
                string pathForEffects = this.GetPathForEffects(albumId, seqNo);
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(pathForEffects, FileMode.Open, FileAccess.Read))
                    {
                        MemoryStream exifStream = new MemoryStream();
                        if (imageEffectsInfo1.Exif != null)
                            exifStream = new MemoryStream(this.ResetOrientation(imageEffectsInfo1.ParsedExif.OrientationOffset, imageEffectsInfo1.Exif, imageEffectsInfo1.ParsedExif.LittleEndian));
                        MemoryStream memoryStream = ImagePreprocessor.MergeExif((Stream)storageFileStream, exifStream);
                        exifStream.Close();
                        return (Stream)memoryStream;
                    }
                }
            }
            else
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Picture galleryImage = this.GetGalleryImage(albumId, seqNo);
                if (preview)
                {
                    Stream thumbnail = galleryImage.GetThumbnail();
                    galleryImage.Dispose();
                    stopwatch.Stop();
                    return thumbnail;
                }
                ImageEffectsInfo imageEffectsInfo2 = this.GetImageEffectsInfo(albumId, seqNo);
                Stream image = galleryImage.GetImage();
                if (imageEffectsInfo2.Exif == null && !this.SuppressParseEXIF)
                {
                    Stopwatch.StartNew();
                    MemoryStream exifStream;
                    ImagePreprocessor.PatchAwayExif(image, out exifStream);
                    image.Position = 0L;
                    imageEffectsInfo2.Exif = new byte[exifStream.Length];
                    exifStream.Read(imageEffectsInfo2.Exif, 0, (int)exifStream.Length);
                    JpegInfo info = new ExifReader(image).info;
                    imageEffectsInfo2.ParsedExif = info;
                    image.Position = 0L;
                }
                stopwatch.Stop();
                galleryImage.Dispose();
                return image;
            }
        }

        private byte[] ResetOrientation(long p, byte[] exifData, bool littleEndian)
        {
            return ImagePreprocessor.ResetOrientation(p, exifData, littleEndian);
        }

        public BitmapSource GetBitmapSource(string albumId, int seqNo, bool allowBackgroundCreation = true)
        {
            Stream imageStream = this.GetImageStream(albumId, seqNo, false);
            ImageEffectsInfo imageEffectsInfo = this._sessionEffectsInfo.GetImageEffectsInfo(albumId, seqNo);
            if (!imageEffectsInfo.AppliedAny && imageEffectsInfo.ParsedExif != null && (imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.TopRight || imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.BottomLeft || imageEffectsInfo.ParsedExif.Orientation == ExifOrientation.BottomRight))
                return (BitmapSource)this.ReadOriginalImage(albumId, seqNo);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.CreateOptions = !allowBackgroundCreation ? BitmapCreateOptions.None : BitmapCreateOptions.BackgroundCreation;
            bitmapImage.DecodePixelHeight = (int)this.ViewportSize.Height;
            bitmapImage.SetSource(imageStream);
            return (BitmapSource)bitmapImage;
        }

        public void SetCrop(double rotate, CropRegion rect, WriteableBitmap imSource, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                WriteableBitmap bmp1 = imSource;
                Picture galleryImage = this.GetGalleryImage(this._albumId, this._seqNo);
                bool rotated90 = false;
                double num = this.GetCorrectImageSize(galleryImage, this._albumId, this._seqNo, out rotated90).Width / (double)bmp1.PixelWidth;
                galleryImage.Dispose();
                WriteableBitmap bmp2 = bmp1.Crop((int)((double)rect.X / num), (int)((double)rect.Y / num), (int)((double)rect.Width / num), (int)((double)rect.Height / num));
                Rect fit = RectangleUtils.ResizeToFit(new Rect(new Point(), this.ViewportSize), new Size((double)bmp2.PixelWidth, (double)bmp2.PixelHeight));
                WriteableBitmap writeableBitmap = bmp2.Resize((int)fit.Width, (int)fit.Height, Interpolation.Bilinear);
                ImageEditorViewModel.SaveWB(writeableBitmap, this.GetPathForCrop());
                this._currentEffects.CropRect = rect;
                this._currentEffects.RotateAngle = rotate;
                this.ApplyEffects(writeableBitmap, (Action<WriteableBitmap>)(bitmap =>
                {
                    callback((BitmapSource)bitmap);
                    this.ApplyingEffects = false;
                }));
                this.CallPropertyChanged();
            }));
        }

        public void ResetCrop(Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            this._currentEffects.RotateAngle = 0.0;
            this._currentEffects.CropRect = (CropRegion)null;
            this.ApplyEffects((WriteableBitmap)null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        public void SetResetFilter(string filterName, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            this._currentEffects.Filter = filterName;
            this.ApplyEffects((WriteableBitmap)null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        public void SetResetContrast(bool set, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            this._currentEffects.Contrast = set;
            this.ApplyEffects((WriteableBitmap)null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        public void SetResetText(string text, Action<BitmapSource> callback)
        {
            if (this.ApplyingEffects)
                return;
            this.ApplyingEffects = true;
            text = (text ?? "").Trim();
            this._currentEffects.Text = text;
            this.ApplyEffects((WriteableBitmap)null, (Action<WriteableBitmap>)(bitmap =>
            {
                callback((BitmapSource)bitmap);
                this.ApplyingEffects = false;
            }));
            this.CallPropertyChanged();
        }

        private void ApplyEffects(WriteableBitmap croppedResizedWB, Action<WriteableBitmap> callback)
        {
            Stopwatch.StartNew();
            if (croppedResizedWB == null)
                croppedResizedWB = this._currentEffects.CropRect != null || this._currentEffects.RotateAngle != 0.0 ? this.ReadCroppedImage() : this.ReadOriginalImage("", -1);
            croppedResizedWB = this.ApplyContrastIfNeeded(croppedResizedWB);
            this.ApplyFilterIfNeeded(croppedResizedWB, (Action<WriteableBitmap>)(filteredWB => Execute.ExecuteOnUIThread((Action)(() =>
            {
                filteredWB = this.ApplyTextIfNeeded(filteredWB);
                ImageEditorViewModel.SaveWB(filteredWB, this.GetPathForEffects(this._albumId, this._seqNo));
                callback(filteredWB);
                croppedResizedWB = (WriteableBitmap)null;
                filteredWB = (WriteableBitmap)null;
                GC.Collect();
            }))));
        }

        private WriteableBitmap ApplyTextIfNeeded(WriteableBitmap wb)
        {
            if (string.IsNullOrWhiteSpace(this._currentEffects.Text))
                return wb;
            WriteableBitmap writeableBitmap = new WriteableBitmap((BitmapSource)wb);
            double fontSize;
            double yTranlsation;
            this.GetFontSizeAndYTranslation(wb.PixelWidth, wb.PixelHeight, out fontSize, out yTranlsation);
            TextBlock textBlock1 = ImageEditorViewModel.CreateTextBlock(this._currentEffects.Text, fontSize);
            textBlock1.Foreground = (Brush)new SolidColorBrush(Colors.Black);
            textBlock1.Opacity = 0.6;
            textBlock1.Width = (double)wb.PixelWidth - yTranlsation * 2.0;
            TextBlock textBlock2 = ImageEditorViewModel.CreateTextBlock(this._currentEffects.Text, fontSize);
            textBlock2.Width = (double)wb.PixelWidth - yTranlsation * 2.0;
            Size size = new Size((double)wb.PixelWidth, (double)wb.PixelHeight);
            Rectangle rectangle1 = new Rectangle();
            Rectangle rectangle2 = rectangle1;
            GradientStopCollection gradientStopCollection = new GradientStopCollection();
            GradientStop gradientStop1 = new GradientStop();
            Color color1 = new Color();
            color1.A = (byte)0;
            Color color2 = color1;
            gradientStop1.Color = color2;
            gradientStopCollection.Add(gradientStop1);
            GradientStop gradientStop2 = new GradientStop();
            color1 = new Color();
            color1.A = byte.MaxValue;
            Color color3 = color1;
            gradientStop2.Color = color3;
            double num1 = 1.0;
            gradientStop2.Offset = num1;
            gradientStopCollection.Add(gradientStop2);
            double angle = 90.0;
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientStopCollection, angle);
            rectangle2.Fill = (Brush)linearGradientBrush;
            rectangle1.Opacity = 0.4;
            rectangle1.Height = textBlock2.ActualHeight + yTranlsation + yTranlsation;
            rectangle1.Width = (double)wb.PixelWidth;
            Rectangle rectangle3 = rectangle1;
            writeableBitmap.Render((UIElement)rectangle3, (Transform)new TranslateTransform()
            {
                Y = (size.Height - rectangle1.Height)
            });
            TextBlock textBlock3 = textBlock1;
            writeableBitmap.Render((UIElement)textBlock3, (Transform)new TranslateTransform()
            {
                X = (yTranlsation + 1.0),
                Y = (size.Height - textBlock2.ActualHeight - yTranlsation)
            });
            TextBlock textBlock4 = textBlock2;
            TranslateTransform translateTransform = new TranslateTransform();
            translateTransform.X = yTranlsation;
            double num2 = size.Height - textBlock2.ActualHeight - yTranlsation - 1.0;
            translateTransform.Y = num2;
            writeableBitmap.Render((UIElement)textBlock4, (Transform)translateTransform);
            writeableBitmap.Invalidate();
            return writeableBitmap;
        }

        private void GetFontSizeAndYTranslation(int wbWidth, int wbHeight, out double fontSize, out double yTranlsation)
        {
            int num = Math.Min(wbHeight, wbWidth);
            fontSize = (double)num / 13.0;
            fontSize = Math.Max(14.0, fontSize);
            yTranlsation = fontSize / 2.0;
        }

        private void ApplyFilterIfNeeded(WriteableBitmap wb, Action<WriteableBitmap> callback)
        {
            if (this._currentEffects.Filter == ImageEditorViewModel.NormalFilterName || string.IsNullOrEmpty(this._currentEffects.Filter))
                callback(wb);
            else if (FilterStage.IsRendering)
            {
                callback(wb);
            }
            else
            {
                string key = FilterStage.CreateKey(this._currentEffects.GetUniqueKeyForFiltering(), wb.PixelWidth, wb.PixelHeight, (int[])null, false);
                Filter filter = (Filter)Enum.Parse(typeof(Filter), this._currentEffects.Filter);
                FilterStage.ApplyFilter(wb, key, filter, callback, (Action<FilterError>)(res => callback(wb)));
            }
        }

        private WriteableBitmap ApplyContrastIfNeeded(WriteableBitmap wb)
        {
            if (this._currentEffects.Contrast)
                return wb.Convolute(WriteableBitmapExtensions.KernelSharpen3x3);
            return wb;
        }

        public WriteableBitmap ReadOriginalImage(string albumId = "", int seqNo = -1)
        {
            string str = this._albumId;
            int num1 = this._seqNo;
            if (albumId != "" && seqNo != -1)
            {
                str = albumId;
                num1 = seqNo;
            }
            if (this._originalImage != null && this._orImAlbumId == str && this._orImSeqNo == num1)
                return this._originalImage.Clone();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Picture galleryImage = this.GetGalleryImage(str, num1);
            int maxPixelWidth = galleryImage.Width;
            int maxPixelHeight = galleryImage.Height;
            if (!MemoryInfo.IsLowMemDevice)
            {
                int num2 = galleryImage.Width * galleryImage.Height;
                if (num2 > VKConstants.ResizedImageSize)
                {
                    double num3 = Math.Sqrt((double)num2 / (double)VKConstants.ResizedImageSize);
                    maxPixelWidth = (int)Math.Round((double)galleryImage.Width / num3);
                    maxPixelHeight = (int)Math.Round((double)galleryImage.Height / num3);
                }
            }
            else
            {
                double x = 0.0;
                double y = 0.0;
                Size viewportSize = this.ViewportSize;
                double width = viewportSize.Width;
                viewportSize = this.ViewportSize;
                double height = viewportSize.Height;
                Rect fit = RectangleUtils.ResizeToFit(new Rect(x, y, width, height), new Size((double)galleryImage.Width, (double)galleryImage.Height));
                double num2 = 300.0;
                double num3 = Math.Min(fit.Width, fit.Height);
                double num4 = 1.0;
                if (num3 < num2)
                    num4 = num2 / num3;
                maxPixelWidth = (int)(fit.Width * num4);
                maxPixelHeight = (int)(fit.Height * num4);
            }
            WriteableBitmap wb = PictureDecoder.DecodeJpeg(galleryImage.GetImage(), maxPixelWidth, maxPixelHeight);
            WriteableBitmap bmp = this.RotateIfNeeded(str, num1, wb);
            if (albumId == "")
            {
                this._originalImage = bmp.Clone();
                this._orImAlbumId = str;
                this._orImSeqNo = num1;
            }
            stopwatch.Stop();
            galleryImage.Dispose();
            return bmp;
        }

        public WriteableBitmap RotateIfNeeded(string aId, int sNo, WriteableBitmap wb)
        {
            ImageEffectsInfo imageEffectsInfo = this._sessionEffectsInfo.GetImageEffectsInfo(aId, sNo);
            if (imageEffectsInfo.ParsedExif != null)
            {
                switch (imageEffectsInfo.ParsedExif.Orientation)
                {
                    case ExifOrientation.BottomRight:
                        wb = wb.Rotate(180);
                        break;
                    case ExifOrientation.TopRight:
                        wb = wb.Rotate(90);
                        break;
                    case ExifOrientation.BottomLeft:
                        wb = wb.Rotate(270);
                        break;
                }
            }
            return wb;
        }

        private WriteableBitmap ReadCroppedImage()
        {
            string pathForCrop = this.GetPathForCrop();
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream storageFileStream1 = storeForApplication.OpenFile(pathForCrop, FileMode.Open, FileAccess.Read))
                {
                    int num1 = 0;
                    int num2 = 0;
                    if (this._currentEffects.CropRect != null)
                    {
                        num1 = this._currentEffects.CropRect.Width;
                        num2 = this._currentEffects.CropRect.Height;
                    }
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.DecodePixelHeight = (int)RectangleUtils.ResizeToFit(new Rect(new Point(), this.ViewportSize), new Size((double)num1, (double)num2)).Height;
                    IsolatedStorageFileStream storageFileStream2 = storageFileStream1;
                    bitmapImage.SetSource((Stream)storageFileStream2);
                    return new WriteableBitmap((BitmapSource)bitmapImage);
                }
            }
        }

        public Picture GetGalleryImage(string albumId, int seqNo)
        {
            if (this._ml == null)
                this._ml = new MediaLibrary();
            if (this._album == (PictureAlbum)null || this._album.Name != albumId)
            {
                if (this._album != (PictureAlbum)null)
                    this._album.Dispose();
                this._album = this._ml.RootPictureAlbum.Albums.FirstOrDefault<PictureAlbum>((Func<PictureAlbum, bool>)(a => a.Name == albumId));
            }
            if (this._album != (PictureAlbum)null && this._album.Pictures.Count > seqNo)
            {
                Picture picture = this._album.Pictures[seqNo];
                if (picture != (Picture)null)
                    return picture;
            }
            return (Picture)null;
        }

        public Size GetCorrectImageSize(Picture p, string albumId, int seqNo, out bool rotated90)
        {
            ImageEffectsInfo imageEffectsInfo = this.GetImageEffectsInfo(albumId, seqNo);
            rotated90 = false;
            if (imageEffectsInfo.ParsedExif == null || imageEffectsInfo.ParsedExif.Orientation != ExifOrientation.TopRight && imageEffectsInfo.ParsedExif.Orientation != ExifOrientation.BottomLeft)
                return new Size((double)p.Width, (double)p.Height);
            rotated90 = true;
            return new Size((double)p.Height, (double)p.Width);
        }

        private string GetPathForCrop()
        {
            return this._sessionId.ToString() + "/" + this._albumId.Replace(" ", "") + (object)this._seqNo + "_crop";
        }

        private string GetPathForEffects(string albumId, int seqNo)
        {
            return this._sessionId.ToString() + "/" + albumId.Replace(" ", "") + (object)seqNo + "_effects";
        }

        private static void SaveWB(WriteableBitmap wb, string path)
        {
            try
            {
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream storageFileStream = storeForApplication.OpenFile(path, FileMode.Create, FileAccess.Write))
                        wb.SaveJpeg((Stream)storageFileStream, wb.PixelWidth, wb.PixelHeight, 0, VKConstants.JPEGQUALITY);
                }
            }
            catch
            {
            }
        }

        private static TextBlock CreateTextBlock(string textStr, double fontSize)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = textStr;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.FontSize = fontSize;
            FontFamily fontFamily = new FontFamily("Lobster.ttf#Lobster 1.4");
            textBlock.FontFamily = fontFamily;
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
            textBlock.Foreground = (Brush)solidColorBrush;
            int num1 = 1;
            textBlock.HorizontalAlignment = (HorizontalAlignment)num1;
            int num2 = 2;
            textBlock.VerticalAlignment = (VerticalAlignment)num2;
            return textBlock;
        }

        private void EnsureFolder()
        {
            using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                storeForApplication.CreateDirectory(this._sessionId.ToString());
        }

        private void DeleteSessionDir()
        {
            try
            {
                using (IsolatedStorageFile storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    foreach (string fileName in storeForApplication.GetFileNames(this._sessionId.ToString() + "\\*"))
                        storeForApplication.DeleteFile(this._sessionId.ToString() + "\\" + fileName);
                    storeForApplication.DeleteDirectory(this._sessionId.ToString());
                }
            }
            catch
            {
            }
        }
    }
}
