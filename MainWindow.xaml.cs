using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Drawing;
using System.IO;


namespace Classfication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //宣告list&index
        private List<BitmapImage> images = new List<BitmapImage>(); // 儲存多張圖片
        private int currentImageIndex = 0; // 追蹤當前顯示的圖片的索引

        private InferenceSession? session;
        private BitmapImage Image = new BitmapImage();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Select_file_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ONNX model files (*.onnx)|*.onnx|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Model_Name_txt.Text = Path.GetFileName(openFileDialog.FileName);
                // 加載模型
                session = new InferenceSession(openFileDialog.FileName);
                foreach (var input in session.InputMetadata)
                {
                    var inputInfo = input.Value;
                    // 輸入張量的形狀
                    var dimensions = inputInfo.Dimensions;
                    Input_ImageNumber_txt.Text = "輸入張數 : " + dimensions[0].ToString();
                    Input_ImageChannel_txt.Text = "輸入影像通道數 : " + dimensions[1].ToString();
                    Input_ImageSize_txt.Text = "輸入影像尺寸 : " + dimensions[2].ToString() + "X" + dimensions[3].ToString();
                }
                foreach (var Output in session.OutputMetadata)
                {
                    var OutputInfo = Output.Value;
                    Output_ImageSize_txt.Text = OutputInfo.Dimensions[1].ToString() + "種結果";
                }

            }
        }



        private void Vertify_btn(object sender, RoutedEventArgs e)
        {
            // 預處理圖片
            var inputTensor = PreprocessImage(BitmapImageToBitmap(Image), 640, 640);
            // 
            var inputs = new[] { NamedOnnxValue.CreateFromTensor("images", inputTensor) };
            using var results = session?.Run(inputs);

            var output = results?.FirstOrDefault()?.AsTensor<float>();
            if (output != null)
            {
                string[] Result = new string[] { "焊接良好", "燒穿", "焊接中有汙染", "焊接斥有縫隙", "缺乏保護氣體", "銲槍移動速度過快" };
                float[] probabilities = output.ToArray();
                int predictedClassIndex = Array.IndexOf(probabilities, probabilities.Max());
                float maxProbability = probabilities[predictedClassIndex];
                Identify_Result_txt.Text = Result[predictedClassIndex];
            }
        }
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(memoryStream);
                return new Bitmap(memoryStream);
            }
        }
        private static Tensor<float> PreprocessImage(Bitmap bitmap, int width, int height)
        {
            var resizedBitmap = new Bitmap(bitmap, new System.Drawing.Size(width, height));

            float[] imageArray = new float[width * height * 3];

            int index = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = resizedBitmap.GetPixel(x, y);
                    imageArray[index++] = pixel.R / 255.0f; // Red
                    imageArray[index++] = pixel.G / 255.0f; // Green
                    imageArray[index++] = pixel.B / 255.0f; // Blue
                }
            }
            var tensor = new DenseTensor<float>(imageArray, new[] { 1, 3, height, width });
            return tensor;
        }

        //Select_Image_Click 更改
        private void Select_Image_Click(object sender, RoutedEventArgs e)
        {
            // 检查是否没有选择图片模式
            if (MutiplePic_Model.IsChecked == false && SinglePic_Model.IsChecked == false)
            {
                MessageBox.Show("請選取圖片模式", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // 直接返回，不执行后续代码
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*";

            if (MutiplePic_Model.IsChecked == true)
            {
                openFileDialog.Multiselect = true; // 允许选择多张图片
            }

            if (openFileDialog.ShowDialog() == true)
            {
                if (MutiplePic_Model.IsChecked == true)
                {
                    images.Clear(); // 清空之前选择的图片
                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fileName);
                        bitmap.EndInit();
                        images.Add(bitmap); // 添加到图片列表中
                    }
                    currentImageIndex = 0;
                    DisplayCurrentImage(); // 显示第一张图片
                }
                else if (SinglePic_Model.IsChecked == true)
                {
                    // 单张图片处理
                    images.Clear(); // 清空之前选择的图片
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();
                    Vertify_Image.Source = bitmap; // 将图片显示在 Image 控制项中
                    Image = bitmap;
                    SelectPic_txt.Text = Path.GetFileName(openFileDialog.FileName);
                    PicSize_txt.Text = bitmap.Width.ToString("0") + "X" + bitmap.Height.ToString("0");
                }
  
            }
            else
            {
                MessageBox.Show("請選取圖片", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //Model_Select更改
        private void Model_Select(object sender, RoutedEventArgs e)
        {
            // 检查 SinglePic_Model 和 MutiplePic_Model 是否为 null
            if (SinglePic_Model != null && MutiplePic_Model != null)
            {
                if (sender == SinglePic_Model && SinglePic_Model.IsChecked == true)
                {
                    MutiplePic_Model.IsChecked = false; // 切换到单张图片时取消多张图片选项
                }
                else if (sender == MutiplePic_Model && MutiplePic_Model.IsChecked == true)
                {
                    SinglePic_Model.IsChecked = false; // 切换到多张图片时取消单张图片选项
                }
            }
        }

        //新增 DisplayCurrentImage
        private void DisplayCurrentImage()
        {
            if (images.Count > 0 && currentImageIndex >= 0 && currentImageIndex < images.Count)
            {
                Vertify_Image.Source = images[currentImageIndex]; // 显示当前索引的图片
                SelectPic_txt.Text = Path.GetFileName(images[currentImageIndex].UriSource.LocalPath);
                PicSize_txt.Text = images[currentImageIndex].Width.ToString("0") + "X" + images[currentImageIndex].Height.ToString("0");
            }
        }
        //新增 Previous_Image_Click
        private void Previous_Image_Click(object sender, RoutedEventArgs e)
        {
            if (images.Count > 0)
            {
                currentImageIndex = (currentImageIndex - 1 + images.Count) % images.Count;
                DisplayCurrentImage();
            }
        }
        //新增 Next_Image_Click
        private void Next_Image_Click(object sender, RoutedEventArgs e)
        {
            if (images.Count > 0)
            {
                currentImageIndex = (currentImageIndex + 1) % images.Count;
                DisplayCurrentImage();
            }
        }



    }
}