using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace AudioPlayerUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            audioListBox.SelectionChanged += audioListBox_SelectionChanged;
        }

        private void audioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (audioListBox.SelectedIndex == -1) { }
            else
            {
                mediaPlayer.Stop();
                audioIndex = audioListBox.SelectedIndex;
                mediaPlayer.Open(mediaList[audioIndex]);
                if (MediaPlayerIsPlaying != false)
                    mediaPlayer.Play();
            }
        }

        private MediaPlayer mediaPlayer = new MediaPlayer();
        private bool MediaPlayerIsPlaying = false;
        private bool MediaPlayerIsStarted = false;
        public static List<Uri> mediaList = new List<Uri>();
        public static int audioIndex = 0;
        public void btnOpenAudioFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    mediaList.Add(new Uri(file));
                    audioListBox.Items.Add(Path.GetFileName(file));
                }
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                try
                {
                    if ((Music_Slider.Value == Music_Slider.Maximum - 1) && (audioIndex == mediaList.Count - 1))
                    {
                        MediaPlayerIsPlaying = false;
                        btnPlay.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/Resources/PlayAudio.png")) };
                    }
                    string audioName = Path.GetFileName(mediaList[audioIndex].ToString());
                    lblStatus.Content = audioName.Remove(audioName.IndexOf('.'), (audioName.IndexOf('3') + 1) - audioName.IndexOf('.'));
                    string[] timeArray = mediaPlayer.Position.ToString(@"mm\ ss").Split();
                    Music_Slider.Value = Convert.ToDouble(timeArray[0]) * 60 + Convert.ToDouble(timeArray[1]);
                    timeArray = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\ ss").Split();
                    Music_Slider.Maximum = Convert.ToDouble(timeArray[0]) * 60 + Convert.ToDouble(timeArray[1]);
                    Label_minimum.Content = mediaPlayer.Position.ToString(@"mm\:ss");
                    Label_maximum.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                    if (Music_Slider.Value == Music_Slider.Maximum && audioIndex != mediaList.Count - 1)
                    {
                        audioIndex++;
                        mediaPlayer.Open(mediaList[audioIndex]);
                        mediaPlayer.Play();
                    }
                }
                catch { }
            }
            else
            {
                lblStatus.Content = "No file selected...";
            }
        }
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayerIsPlaying == false && MediaPlayerIsStarted == true)
            {
                mediaPlayer.Play();
                MediaPlayerIsPlaying = true;
                btnPlay.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/Resources/StopAudio.png")) };
                if (Music_Slider.Value > 0) { }
                else
                {
                    try
                    {
                        string[] timeArray = mediaPlayer.Position.ToString(@"mm\ ss").Split();
                        Music_Slider.Minimum = Convert.ToDouble(timeArray[0]) * 60 + Convert.ToDouble(timeArray[1]);
                        timeArray = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\ ss").Split();
                        Music_Slider.Maximum = Convert.ToDouble(timeArray[0]) * 60 + Convert.ToDouble(timeArray[1]);
                    }
                    catch { }
                }
            }
            else
            {
                mediaPlayer.Pause();
                MediaPlayerIsPlaying = false;
                btnPlay.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/Resources/PlayAudio.png")) };
            }
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((Slider)sender).SelectionEnd = e.NewValue;
            string[] timeArray = mediaPlayer.Position.ToString(@"mm\ ss").Split();
            double res = Convert.ToDouble(timeArray[0]) * 60 + Convert.ToDouble(timeArray[1]);
            if (Math.Abs(e.NewValue - res) > 1)
            {
                string value = Math.Round(Music_Slider.Value, 0).ToString();

                int minutes = Convert.ToInt32(value) / 60;

                int seconds = Convert.ToInt32(value) - minutes * 60;

                TimeSpan ts = new TimeSpan(0, minutes, seconds);

                mediaPlayer.Position = ts;
            }
        }
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = (double)volumeSlider.Value;
            ((Slider)sender).SelectionEnd = e.NewValue;
        }
        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayerIsStarted == false)
            {
                try
                {
                    mediaPlayer.Open(mediaList[audioIndex]);
                    MediaPlayerIsStarted = true;
                    Start_Button.Content = "Stop";
                }
                catch
                {
                    MessageBox.Show("У вас в черзі відсутні пісні.", "Помилка.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                mediaPlayer.Stop();
                mediaList.Clear();
                Start_Button.Content = "Start";
                btnPlay.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/Resources/PlayAudio.png")) };
                MediaPlayerIsPlaying = false;
                MediaPlayerIsStarted = false;
                Music_Slider.Value = 0;
                Label_maximum.Content = "";
                Label_minimum.Content = "";
                mediaPlayer = new MediaPlayer();
                audioListBox.ItemsSource = null;
                audioListBox.Items.Clear();
            }
        }
        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            if (audioIndex == 0)
                audioIndex = mediaList.Count - 1;
            else
                audioIndex--;
            mediaPlayer.Open(mediaList[audioIndex]);
            if (MediaPlayerIsPlaying != false)
                mediaPlayer.Play();
        }
        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            if (audioIndex == mediaList.Count - 1)
                audioIndex = 0;
            else
                audioIndex++;
            mediaPlayer.Open(mediaList[audioIndex]);
            if (MediaPlayerIsPlaying != false)
                mediaPlayer.Play();
        }
        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SecondsPlus_Button_Click(object sender, RoutedEventArgs e)
        {
            Music_Slider.Value += 10;
        }
        private void SecondsMinuses_Button_Click(object sender, RoutedEventArgs e)
        {
            Music_Slider.Value -= 10;
        }
        private void Random_btn_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayerIsStarted == true)
            {
                Random random = new Random(Guid.NewGuid().ToByteArray().Sum(x => x));
                for (int i = 0; i < mediaList.Count; i++)
                {
                    var number = mediaList[0];
                    mediaList.RemoveAt(0);
                    mediaList.Insert(random.Next(mediaList.Count), number);
                }
                audioListBox.ItemsSource = null;
                audioListBox.Items.Clear();
                for (int i = 0; i < mediaList.Count; i++)
                {
                    string fileName = Path.GetFileName(mediaList[i].ToString());
                    audioListBox.Items.Add(fileName);
                }
                mediaPlayer.Stop();
                audioIndex = 0;
                string audioName = Path.GetFileName(mediaList[audioIndex].ToString());
                lblStatus.Content = audioName.Remove(audioName.IndexOf('.'), (audioName.IndexOf('3') + 1) - audioName.IndexOf('.'));
                string[] timeArray = mediaPlayer.Position.ToString(@"mm\ ss").Split();
                Music_Slider.Value = Convert.ToDouble(timeArray[0]) * 60 + Convert.ToDouble(timeArray[1]);
                timeArray = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\ ss").Split();
                Music_Slider.Maximum = Convert.ToDouble(timeArray[0]) * 60 + Convert.ToDouble(timeArray[1]);
                Label_minimum.Content = mediaPlayer.Position.ToString(@"mm\:ss");
                Label_maximum.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                mediaPlayer.Open(mediaList[audioIndex]);
                if (MediaPlayerIsPlaying == true)
                    mediaPlayer.Play();
            }
            else
            {
                MessageBox.Show("У вас в черзі відсутні пісні. Виберіть пісні та розпочніть роботу програвача.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
