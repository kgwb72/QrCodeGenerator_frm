using Microsoft.Win32;
using QrCodeGenerator;
using QRCoder;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static iText.Layout.Properties.BackgroundPosition;
using System.Windows.Threading;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System;
using System.Linq;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QrCodeGenerator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FileItem> Files { get; } = new ObservableCollection<FileItem>();
        // Other properties and methods...
        public ICommand SelectFilesCommand { get; private set; }
        public ICommand AddQrCodeCommand { get; private set; }
        private float _sizeWidth;
        private float _sizeHeight;
        private float _positionX;
        private float _positionY;
        private int _currentProgress;
        private bool _isDottedSelected;
        private BitmapImage _qrCodePreview;

        public float SizeWidth
        {
            get => _sizeWidth;
            set
            {
                if (_sizeWidth != value)
                {
                    _sizeWidth = value;
                    OnPropertyChanged(nameof(SizeWidth));
                }
            }
        }
        public float SizeHeight
        {
            get => _sizeHeight;
            set
            {
                if (_sizeHeight != value)
                {
                    _sizeHeight = value;
                    OnPropertyChanged(nameof(SizeHeight));
                }
            }
        }
        public float PositionX
        {
            get => _positionX;
            set
            {
                if (_positionX != value)
                {
                    _positionX = value;
                    OnPropertyChanged(nameof(PositionX));
                }
            }
        }
        public float PositionY
        {
            get => _positionY;
            set
            {
                if (_positionY != value)
                {
                    _positionY = value;
                    OnPropertyChanged(nameof(PositionY));
                }
            }
        }
        public BitmapImage QrCodePreview
        {
            get => _qrCodePreview;
            set
            {
                _qrCodePreview = value;
                OnPropertyChanged(nameof(QrCodePreview));
            }
        }
        public bool IsDottedSelected
        {
            get => _isDottedSelected;
            set
            {
                _isDottedSelected = value;
                OnPropertyChanged(nameof(IsDottedSelected));
                // Additional logic if needed when this option is selected
            }
        }

        public int CurrentProgress
        {
            get => _currentProgress;
            set
            {
                if (_currentProgress != value)
                {
                    _currentProgress = value;
                    OnPropertyChanged(nameof(CurrentProgress));
                }
            }
        }


        public MainViewModel()
        {
            SelectFilesCommand = new RelayCommand(() => SelectFiles());
            AddQrCodeCommand = new RelayCommand(async () => await AddQrCodeAsync());
            GenerateInitialQrCodePreview();
        }

        private void SelectFiles()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    Files.Add(new FileItem
                    {
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath
                    });
                }
            }

        }
        private async Task AddQrCodeAsync()
        {
            int processedCount = 0;
            CurrentProgress = 0; // Reset progress at start
            float qrSize = SizeHeight;
            float positionX = PositionX;
            float positionY = PositionY;

            await Task.Run(() =>
            {
                foreach (var file in Files)
                {
                    var qr = new QRCodeGeneratorService();
                    Bitmap qrCode = qr.GenerateQRCode(file.FileName, IsDottedSelected);
                    PDFService pdf = new PDFService();
                    pdf.AddQrCodeToPdf(file.FilePath, qrCode, SizeWidth, PositionX, PositionY);
                    processedCount++;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentProgress = (processedCount * 100) / Files.Count;
                    });


                }

            });

            // Notify the user that the process is complete - consider using an event or callback for this
            CurrentProgress = 0;  // Reset progress bar
            OnProcessComplete?.Invoke();
        }

        public event Action OnProcessComplete;

        private void GenerateInitialQrCodePreview()
        {
            // Generate the initial QR code and set it to QrCodePreview
            var qr = new QRCodeGeneratorService();
            var qrBitmap = qr.GenerateQRCode("linetracker.zaunergroup.com", IsDottedSelected); // Replace with your initial value           
            QrCodePreview = ConvertBitmapToBitmapImage(qrBitmap);
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Important for use in WPF
                return bitmapImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class FloatValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                // Convert from float to string
                return value?.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                // Convert from string to float
                if (float.TryParse(value as string, out float result))
                {
                    return result;
                }
                return 0; // Or some default value, or handle the error
            }
        }


        private Bitmap GenerateCompositeImage(Bitmap qrCodeImage, string text)
        {
            int spacing = 5; // Space between QR Code and text
            Font font = new Font("Arial", 8); // Font for the text

            // Measure the text size
            SizeF textSize;
            using (Graphics graphics = Graphics.FromImage(qrCodeImage))
            {
                textSize = graphics.MeasureString(text, font);
            }

            // Create a new bitmap with extra space for text
            Bitmap compositeImage = new Bitmap(qrCodeImage.Width, qrCodeImage.Height + (int)textSize.Height + spacing);
            using (Graphics graphics = Graphics.FromImage(compositeImage))
            {
                // Draw the original QR code image
                graphics.DrawImage(qrCodeImage, 0, 0);

                // Draw the text below the QR code
                using (System.Drawing.Brush textBrush = new SolidBrush(System.Drawing.Color.Black)) // Text color
                {
                    PointF textPoint = new PointF(0, qrCodeImage.Height + spacing);
                    graphics.DrawString(text, font, textBrush, textPoint);
                }
            }
            return compositeImage;
        }
    }

}

