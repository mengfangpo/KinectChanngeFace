using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        int count = 0;
        int count2 = 0;
        private KinectSensor KinectSensor = null;
        private ColorFrameReader colorFrameReader = null;
        Body[] bodies;
        MultiSourceFrameReader msfr;
        private WriteableBitmap colorBitmap = null;
        bool changeFace1 = false;
        bool changeFace2 = false;
        bool startFlag = false;
        bool startFlag2 = false;
        Random randDom;
        public MainWindow()
        {
            randDom = new Random();
            this.KinectSensor = KinectSensor.GetDefault();
            this.colorFrameReader = this.KinectSensor.ColorFrameSource.OpenReader();
            FrameDescription colorFrameDescription = this.KinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            //获得对色彩帧的描述，以Rgba格式
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            //创造用于显示的bitmap,图宽，图高，水平/垂直每英寸像素点数，像素点格式，调色板为空.
            bodies = new Body[6];
            msfr = KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Color);
            msfr.MultiSourceFrameArrived += msfr_MultiSourceFrameArrived;
            this.KinectSensor.Open();


        }

        private void msfr_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame msf = e.FrameReference.AcquireFrame();
            if (msf != null)
            {
                using (BodyFrame bodyFrame = msf.BodyFrameReference.AcquireFrame())
                {
                    using (ColorFrame colorFrame = msf.ColorFrameReference.AcquireFrame())
                    {
                        if (bodyFrame != null && colorFrame != null)
                        {
                            FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                            using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                            {
                                this.colorBitmap.Lock();
                                if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                                {
                                    colorFrame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4), ColorImageFormat.Bgra);
                                    this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                                }
                                this.colorBitmap.Unlock();
                                bodyFrame.GetAndRefreshBodyData(bodies);
                                //获取骨骼跟踪数据帧，保存到body数组中
                                bodyCanvas.Children.Clear();
                                count = 0;
                                count2 = 0;
                               
                                foreach (Body body in bodies)
                                {
                                    if (body.IsTracked)
                                    {
                                        Joint headJiont1 = body.Joints[JointType.Head];//头部骨骼点
                                        Joint handLeft = body.Joints[JointType.HandLeft];//左手骨骼点
                                        Joint handRight = body.Joints[JointType.HandRight];//右手骨骼点
                                        Joint neck = body.Joints[JointType.Neck];//脖子骨骼节点
                                        Joint shoulderLeft = body.Joints[JointType.ShoulderLeft];//左肩部骨骼点
                                        Joint shoulderRight = body.Joints[JointType.ShoulderRight];//右肩部骨骼点
                                        if (headJiont1.TrackingState == TrackingState.Tracked)
                                        {
                                            count++;
                                            // 获取骨骼节点的空间位置信息
                                            ColorSpacePoint joint_head = KinectSensor.CoordinateMapper.MapCameraPointToColorSpace(headJiont1.Position);
                                            ColorSpacePoint joint_handLeft = KinectSensor.CoordinateMapper.MapCameraPointToColorSpace(handLeft.Position);
                                            ColorSpacePoint joint_handRight = KinectSensor.CoordinateMapper.MapCameraPointToColorSpace(handRight.Position);
                                            ColorSpacePoint joint_neck = KinectSensor.CoordinateMapper.MapCameraPointToColorSpace(neck.Position);
                                            ColorSpacePoint joint_shoulderLeft = KinectSensor.CoordinateMapper.MapCameraPointToColorSpace(shoulderLeft.Position);
                                            ColorSpacePoint joint_shoulderRight = KinectSensor.CoordinateMapper.MapCameraPointToColorSpace(shoulderRight.Position);
                                            if (count == 1)
                                            {
                                                if(!startFlag)
                                                {
                                                    startFlag = this.startGame(joint_head, joint_handLeft, joint_handRight,startFlag );
                                                }
                                               // startFlag = this.startGame(joint_head, joint_handLeft, joint_handRight);
                                              // startnum.Content = string.Format("已有{0}{1}{2}人开始游戏", count2,startFlag,changeFace1);
                                                if(startFlag)
                                                {
                                                    count2++;
                                                    if (changeFaceMethod(joint_head, joint_handLeft, joint_handRight))
                                                    {

                                                        if (!changeFace1 && count2 == 1)
                                                        {
                                                            BitmapImage bitmapImage1 = null;
                                                            changeFace1 = true;//为保证每次挥手触发一次事件                                                       
                                                            beginToChangeFace(bitmapImage1, headimage);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        changeFace1 = false;

                                                    }
                                                } // 判断左右手关节点和头部关节点的距离来进行变脸；单位：毫米

                                               


                                                double angle2 = 0;
                                                double imageFaceSize1 = 0;

                                                this.faceSize(headJiont1, imageFaceSize1, imagelab);
                                                this.facePosion(joint_head, headJiont1, imagelab);
                                                this.faceAngle(joint_head, joint_neck, angle2, headimage);

                                                startFlag = this.endGame(joint_shoulderLeft, joint_shoulderRight, joint_handLeft, joint_handRight,startFlag );
                                                if (!startFlag)
                                                {
                                                    // bodyCanvas.Children.Clear();要彻底释放资源，防止下一个玩家进入时直接出现脸谱
                                                    headimage.Source = null;
                                                    changeFace1 = false;
                                                }
                                            }



                                            if (count == 2)
                                            {
                                                if (!startFlag2)
                                                {
                                                    startFlag2 = this.startGame(joint_head, joint_handLeft, joint_handRight,startFlag2 );
                                                }
                                                // startFlag = this.startGame(joint_head, joint_handLeft, joint_handRight);
                                                // startnum.Content = string.Format("已有{0}{1}{2}人开始游戏", count2,startFlag,changeFace1);
                                                if (startFlag2)
                                                {
                                                    count2 ++;
                                                    if (changeFaceMethod(joint_head, joint_handLeft, joint_handRight))
                                                    {
                                                        if (!changeFace2)
                                                        {
                                                            BitmapImage bitmapImage1 = null;
                                                            changeFace2 = true;//为保证每次挥手触发一次事件                                                       
                                                            beginToChangeFace(bitmapImage1, headimage1);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        changeFace2 = false;
                                                    }

                                                    double angle2 = 0;
                                                    double imageFaceSize2 = 0;

                                                    this.faceSize(headJiont1, imageFaceSize2, imagelab1);
                                                    this.facePosion(joint_head, headJiont1, imagelab1);
                                                   this.faceAngle(joint_head, joint_neck, angle2, headimage1);
                                                } // 判断左右手关节点和头部关节点的距离来进行变脸；单位：毫米
                                                startFlag2 = endGame(joint_shoulderLeft, joint_shoulderRight, joint_handLeft, joint_handRight,startFlag2);
                                                if (!startFlag2)
                                                {
                                                    // bodyCanvas.Children.Clear();
                                                    headimage1.Source = null;
                                                    changeFace2 = false;
                                                }
                                            }

                                        }
                                        if (count != 0)
                                        {
                                            sayhello.Content = string.Format("欢迎来带Kinect世界！目前已有{0}人进入.....", count);
                                            startnum.Content = string.Format("已有{0}人开始游戏",count2);
                                   
                                        }
                                        
                                    }
                                    
                                }
                                //if (count != 0)
                                //{
                                //    sayhello.Content = string.Format("欢迎来带Kinect世界！目前已有{0}{1}人进入.....", count, headJiont1);
                                //}


                            }

                        }

                    }
                }

            }
        }

        private bool changeFaceMethod(ColorSpacePoint joint_head, ColorSpacePoint joint_handLeft, ColorSpacePoint joint_handRight)
        {
            // 通过判断左手或右手关节点与头节点的距离是否小于5厘米来触发变脸； 单位：毫米
            bool isOverMatrix = (Math.Abs(joint_handLeft.X - joint_head.X) < 50 && Math.Abs(joint_handLeft.Y - joint_head.Y) < 50) ||
                (Math.Abs(joint_handRight.X - joint_head.X) < 50 && Math.Abs(joint_handRight.Y - joint_head.Y) < 50);
            return isOverMatrix;
        }
        private bool startGame(ColorSpacePoint joint_head,ColorSpacePoint joint_handleft,ColorSpacePoint joint_handright,bool startflag)
        {
            if((joint_handleft.Y<joint_head.Y)&&(joint_handright.Y<joint_head.Y))
            {
                return true;
            }
            return startflag ;
        }
        private bool endGame(ColorSpacePoint joint_shoulderLeft,ColorSpacePoint joint_shoulderRight, ColorSpacePoint joint_handleft, 
                             ColorSpacePoint joint_handright,bool startflag)
        {
            float y1, y2, x1, x2;
            y1 = Math.Abs(joint_handleft.Y - joint_shoulderLeft.Y);
            y2 = Math.Abs(joint_handright.Y - joint_shoulderRight.Y);
            x1 = Math.Abs(joint_handleft.X - joint_shoulderLeft.X);
            x2 = Math.Abs(joint_handright.X - joint_shoulderRight.X);
            if (y1 < 50 && y2 < 50 && x1 > 200 && x2 > 200)
            {
                return false;
            }
            return startflag;
      
        }
        private void beginToChangeFace(BitmapImage bitmapImages, Image headimages)
        {
            //变脸
            string path = string.Format(@"/image/{0}.png", randDom.Next(1, 11));//从十张图片中随机抽选
            bitmapImages = new BitmapImage();
            bitmapImages.BeginInit();//脸谱位图初始化
            bitmapImages.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmapImages.EndInit();
            headimages.Source = bitmapImages;//脸谱覆盖在脸上
        }
        private void faceSize(Joint head, double imageFaceSizes, Label imagelabs)
        {
            //脸谱大小
            imageFaceSizes = 230/head.Position.Z;//  设置脸谱大小
            imagelabs.Height = imageFaceSizes; // 脸谱图像的高
            imagelabs.Width = imageFaceSizes; // 脸谱图像的宽
        }
        private void facePosion(ColorSpacePoint joint_head, Joint head, Label imagelabs)
        {
            // (joint_head.X - imagelabs.Width / 2) * 1600 / 1980; // 在X轴方向矫正脸谱
            //(joint_head.Y - (imagelabs.Height / 2) * head.Position.Z) * 900 / 1080; // 在Y轴方向矫正脸谱
            Canvas.SetLeft(imagelabs, (joint_head.X - imagelab.Width / 2) ); // 脸谱距离画布左边的距离，向左为负，向右为正。单位：毫米
            Canvas.SetTop(imagelabs, (joint_head.Y - (imagelabs.Height / 2) * head.Position.Z) +50); 
            //脸谱距离画布顶端的距离，向上为负，向下为正。单位：毫米
            bodyCanvas.Children.Add(imagelabs);
        }
        private void faceAngle(ColorSpacePoint joint_head, ColorSpacePoint joint_neck, double angles, Image headimages)
        {
            double tanFace = ((joint_head.X - joint_neck.X) / (joint_head.Y - joint_neck.Y)); // 获取头节点和脖子节点所形成的正切值
            angles = -Math.Atan(tanFace) / Math.PI * 180; // 将正切值转化为角度
            headimages.RenderTransform = new RotateTransform(angles); // 控制变脸旋转的角度
        }
       

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }
            if (this.KinectSensor != null)
            {
                this.KinectSensor.Close();
                this.KinectSensor = null;
            }
        }

        private void image_Loaded(object sender, RoutedEventArgs e)
        {
            image.Source = colorBitmap;
        }
        //private void showJian(BitmapImage bitmapImages, Image jiantou)
        //{
        //    string path = string.Format(@"/images/001.png");
        //    bitmapImages = new BitmapImage();
        //    bitmapImages.BeginInit();
        //    bitmapImages.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
        //    bitmapImages.EndInit();
        //    jiantou.Source = bitmapImages;//脸谱覆盖在脸上
        //}
        //private void jiantouPosition(ColorSpacePoint joint_head,Joint headJoint,Image face)
        //{
        //    double x = joint_head.X - 130 / headJoint.Position.Z + angle1 * 1.4;
        //}

    }
}
