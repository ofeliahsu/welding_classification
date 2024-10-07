# welding_classification

##★ MainWindow.xaml 新增:
AFTER
> <TextBlock Grid.Row="0" Style="{StaticResource TextStyle}" Text="熔池影像" />
> <Border Grid.Row="0" VerticalAlignment="Top" Margin="0 29 0 0">
>	<Image x:Name="Vertify_Image" VerticalAlignment="Top" Margin="0 0 0 10"/>
> </Border>


           
            <!-- 按鈕區域 -->

            <!-- "上一張" 按鈕，放置在左下角 -->
            <Button x:Name="Previous_Image_Button" HorizontalAlignment="Left" VerticalAlignment="Bottom" 
                Width="40" Height="40" Margin="10 0 0 10" Click="Previous_Image_Click">
                <Image Source="C:\Users\Hsu\Desktop\ver2\Classfication\resources\previous_image_button.png"/>
                <!--更換圖片路徑-->
            </Button>

            <!-- "下一張" 按鈕，放置在右下角 -->
            <Button x:Name="Next_Image_Button" HorizontalAlignment="Right" VerticalAlignment="Bottom" 
                Width="40" Height="40" Margin="0 0 10 10" Click="Next_Image_Click">
                <Image Source="C:\Users\Hsu\Desktop\ver2\Classfication\resources\next_image_buttom.png"/>
                <!--更換圖片路徑-->
            </Button>


—————————————————————————————————————————————————————

##★ MainWindow.xaml.cs 調整:


1. [新增] 第17行 宣告list & index

//  public partial class MainWindow : Window {...

	//宣告list & index
        private List<BitmapImage> images = new List<BitmapImage>(); // 儲存多張圖片
        private int currentImageIndex = 0; // 追蹤當前顯示的圖片的索引


2. [更改] 第106行 Select_Image_Click

勾選"單張圖片"模式只能選一張，勾選"多張模式"能選多張。

若兩個模式都沒選，就按了"確定"，則跳出警告[請選取圖片模式]。

若選擇模式後沒選圖片按了"取消"，則跳出警告[請選取圖片]。

每次切換模式則清空紀錄list。

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


3. [更改] 第161行 Model_Select 使user無法同時勾選兩個模式，且模式的勾選可以一鍵切換。

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

4. [新增] DisplayCurrentImage 以顯示當前圖片名稱及大小

        private void DisplayCurrentImage()
        {
            if (images.Count > 0 && currentImageIndex >= 0 && currentImageIndex < images.Count)
            {
                Vertify_Image.Source = images[currentImageIndex]; // 显示当前索引的图片
                SelectPic_txt.Text = Path.GetFileName(images[currentImageIndex].UriSource.LocalPath);
                PicSize_txt.Text = images[currentImageIndex].Width.ToString("0") + "X" + images[currentImageIndex].Height.ToString("0");
            }
        }


5. [新增] Previous_Image_Click 讓user在選擇"多張圖片"模式時，可選擇"上一張"以顯示上個順序的圖片

        private void Previous_Image_Click(object sender, RoutedEventArgs e)
        {
            if (images.Count > 0)
            {
                currentImageIndex = (currentImageIndex - 1 + images.Count) % images.Count;
                DisplayCurrentImage();
            }
        }

6. [新增] Next_Image_Click 讓user在選擇"多張圖片"模式時，可選擇"下一張"以顯示下個順序的圖片

        private void Next_Image_Click(object sender, RoutedEventArgs e)
        {
            if (images.Count > 0)
            {
                currentImageIndex = (currentImageIndex + 1) % images.Count;
                DisplayCurrentImage();
            }
        }
