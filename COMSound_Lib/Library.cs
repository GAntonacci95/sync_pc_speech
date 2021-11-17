using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Linq;
using System.Windows.Forms;//.net assembly import to add drawing reference
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Data.SqlClient;

namespace COMSound_Lib
{
    namespace Audio
    {
        public class Info
        {
            public const int Durata = 1000;
            public const int FrequenzaSplitter = 15000;
            public const long samp_rate = 44100;
            public const ushort channel = 2;
            public const double fraz = 333.333333 / 1000;
            public const int FreqInc = 200;
            
            //private static string _key = "F3N<L2r4", _iv = "lLiJ4O£~";

            public static Dictionary<char, int> CBuilder()
            {
                Dictionary<char, int> ret = new Dictionary<char, int>();
                char[] chars = {' ','a','b','c','d','e','f','g','h','i',
                               'j','k','l','m','n','o','p','q','r','s',
                               't','u','v','w','x','y','z',
                               '0','1','2','3','4','5','6','7','8','9',
                                '!','"','?','$','%','&','/','(',')','=',
                                'è','[',']','ò','à','ù',',','.','-','@',
                                '#',';',':','<','>','\\','{','}','_'};

                for (int i = 1500; i < FrequenzaSplitter - 300; i += 200)
                {
                    ret.Add(chars[(i - 1500) / 200].ToString().ToLower()[0], i);
                }

                ret.Add('\n'.ToString().ToLower()[0], FrequenzaSplitter);
                return ret;
            }

            public static string WHAT(string message)
            {
                string ret = "";
                byte[] input = Encoding.ASCII.GetBytes(message);


                
                return ret;
            }

            public static string ReverseWHAT(string hash)
            {
                string ret = "";
                byte[] input = Encoding.ASCII.GetBytes(hash);

                //uscita
                return ret;
            }
        }

        public class Speaker
        {
            private static Dictionary<char, int> Corrispondence = new Dictionary<char, int>();

            public Speaker()
            {
                Corrispondence = Info.CBuilder();
            }

            public void Transmit(string message)
            {
                int[] info = ToIntArray(message.ToLower());
                Console.Beep(Info.FrequenzaSplitter, Info.Durata);

                foreach (int char_freq in info)
                {
                    Console.Beep(char_freq, Info.Durata);
                }

                Console.Beep(Info.FrequenzaSplitter, Info.Durata);
            }

            public void Transmit()
            {
                string message = "";

                foreach (char c in Corrispondence.Keys)
                    message += c;

                int[] info = ToIntArray(message.ToLower());
                foreach (int char_freq in info)
                {
                    Console.Beep(char_freq, Info.Durata);
                }
            }

            private static int[] ToIntArray(string mex)
            {
                List<int> ret = new List<int>();
                foreach (char c in mex)
                {
                    ret.Add(Corrispondence[c]);
                }
                return ret.ToArray();
            }
        }

        public class Microphone
        {
            private WaveInRecorder _recorder;
            private byte[] _recorderBuffer;
            private WaveFormat _waveFormat;
            private AudioFrame _audioFrame;

            private PictureBox[] referred = new PictureBox[2];
            private RichTextBox referredtxt = new RichTextBox();

            public Microphone(ref PictureBox l, ref PictureBox r, ref RichTextBox txt)
            {
                referred[0] = l;
                referred[1] = r;
                referredtxt = txt;
            }

            public void Receive()
            {
                if (WaveNative.waveInGetNumDevs() != 0)
                {
                    _audioFrame = new AudioFrame();
                    _audioFrame.IsDetectingEvents = Properties.Settings.Default.SettingIsDetectingEvents;
                    _audioFrame.AmplitudeThreshold = Properties.Settings.Default.SettingAmplitudeThreshold;
                    Start();
                }
            }

            private void Start()
            {
                try
                {
                    _waveFormat = new WaveFormat(Properties.Settings.Default.SettingSamplesPerSecond, Properties.Settings.Default.SettingBitsPerSample, Properties.Settings.Default.SettingChannels);
                    _recorder = new WaveInRecorder(Properties.Settings.Default.SettingAudioInputDevice, _waveFormat, Properties.Settings.Default.SettingBytesPerFrame * Properties.Settings.Default.SettingChannels, 3, new BufferDoneEventHandler(DataArrived));
                }
                catch (Exception ex)
                { }
            }

            private void DataArrived(IntPtr data, int size)
            {
                if (_recorderBuffer == null || _recorderBuffer.Length != size)
                    _recorderBuffer = new byte[size];
                if (_recorderBuffer != null)
                {
                    System.Runtime.InteropServices.Marshal.Copy(data, _recorderBuffer, 0, size);
                    _audioFrame.Process(ref _recorderBuffer);
                    _audioFrame.RenderFrequencyDomainLeft(ref referred[0], Properties.Settings.Default.SettingSamplesPerSecond);
                    _audioFrame.RenderFrequencyDomainRight(ref referred[1], ref referredtxt, Properties.Settings.Default.SettingSamplesPerSecond);
                }
            }
        }

        #region NeededObjects

        class AudioFrame
        {
            private double[] _waveLeft;
            private double[] _fftLeft;
            private ArrayList _fftLeftSpect = new ArrayList();
            private int _maxHeightLeftSpect = 0;
            private double[] _waveRight;
            private double[] _fftRight;
            private ArrayList _fftRightSpect = new ArrayList();
            private int _maxHeightRightSpect = 0;

            public bool IsDetectingEvents = false;
            public bool IsEventActive = false;
            public int AmplitudeThreshold = 16384;

            private List<List<Point>> lgp_r = new List<List<Point>>();
            private List<List<Point>> lgp_l = new List<List<Point>>();
            private System.Timers.Timer t = new System.Timers.Timer();
            private System.Timers.Timer t2 = new System.Timers.Timer();

            private int index = 0;
            private bool started = false;
            private bool called = false;
            private Dictionary<char, int> built = new Dictionary<char, int>();

            public AudioFrame()
            {
                built = Info.CBuilder();

                lgp_r.Add(new List<Point>());
                lgp_l.Add(new List<Point>());

                t.Interval = Info.Durata;
                t2.Interval = Info.Durata + 50;

                t.Elapsed += new System.Timers.ElapsedEventHandler(IncAndRestart);
                t2.Elapsed += new System.Timers.ElapsedEventHandler(EndMessageHandler);
            }

            private void EndMessageHandler(object sender, System.Timers.ElapsedEventArgs e)
            {
                referredtxt.Invoke(new MethodInvoker(Writer));
                Clear();
            }

            private void Clear()
            {
                t2.Stop();
                lgp_r = new List<List<Point>>();
                lgp_l = new List<List<Point>>();
                lgp_r.Add(new List<Point>());
                lgp_l.Add(new List<Point>());
                index = 0;
                started = false;
                called = false;

                towrite = "";
                t2.Interval = Info.Durata + 50;
            }

            private void IncAndRestart(object sender, System.Timers.ElapsedEventArgs e)
            {
                index++;
                t.Interval = Info.Durata;
                lgp_r.Add(new List<Point>());
                lgp_l.Add(new List<Point>());
                t.Start();
            }

            public void Process(ref byte[] wave)
            {
                IsEventActive = false;
                _waveLeft = new double[wave.Length / 4];
                _waveRight = new double[wave.Length / 4];

                // Split out channels from sample
                int h = 0;
                for (int i = 0; i < wave.Length; i += 4)
                {
                    _waveLeft[h] = (double)BitConverter.ToInt16(wave, i);
                    if (IsDetectingEvents == true)
                        if (_waveLeft[h] > AmplitudeThreshold || _waveLeft[h] < -AmplitudeThreshold)
                            IsEventActive = true;
                    _waveRight[h] = (double)BitConverter.ToInt16(wave, i + 2);
                    if (IsDetectingEvents == true)
                        if (_waveLeft[h] > AmplitudeThreshold || _waveLeft[h] < -AmplitudeThreshold)
                            IsEventActive = true;
                    h++;
                }

                // Generate frequency domain data in decibels
                _fftLeft = FourierTransform.FFT(ref _waveLeft);
                _fftLeftSpect.Add(_fftLeft);
                if (_fftLeftSpect.Count > _maxHeightLeftSpect)
                    _fftLeftSpect.RemoveAt(0);
                _fftRight = FourierTransform.FFT(ref _waveRight);
                _fftRightSpect.Add(_fftRight);
                if (_fftRightSpect.Count > _maxHeightRightSpect)
                    _fftRightSpect.RemoveAt(0);
            }

            public void RenderFrequencyDomainLeft(ref PictureBox pictureBox, int samples)
            {
                // Set up for drawing
                Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
                Graphics offScreenDC = Graphics.FromImage(canvas);
                SolidBrush brush = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 255, 255));
                Pen pen = new System.Drawing.Pen(Color.WhiteSmoke);
                Font font = new Font("Arial", 10);

                // Determine channnel boundries
                int width = canvas.Width;
                int height = canvas.Height;

                double min = double.MaxValue;
                double minHz = 0;
                double max = double.MinValue;
                double maxHz = 0;
                double range = 0;
                double scale = 0;
                double scaleHz = (double)(samples / 2) / (double)_fftLeft.Length;

                // get left min/max
                for (int x = 0; x < _fftLeft.Length; x++)
                {
                    double amplitude = _fftLeft[x];
                    if (min > amplitude)
                    {
                        min = amplitude;
                        minHz = (double)x * scaleHz;
                    }
                    if (max < amplitude)
                    {
                        max = amplitude;
                        maxHz = (double)x * scaleHz;
                    }
                }

                // get left range
                if (min < 0 || max < 0)
                    if (min < 0 && max < 0)
                        range = max - min;
                    else
                        range = Math.Abs(min) + max;
                else
                    range = max - min;
                scale = range / height;

                // draw left channel
                for (int xAxis = 0; xAxis < width; xAxis++)
                {
                    double amplitude = (double)_fftLeft[(int)(((double)(_fftLeft.Length) / (double)(width)) * xAxis)];
                    if (amplitude == double.NegativeInfinity || amplitude == double.PositiveInfinity || amplitude == double.MinValue || amplitude == double.MaxValue)
                        amplitude = 0;
                    int yAxis;
                    if (amplitude < 0)
                        yAxis = (int)(height - ((amplitude - min) / scale));
                    else
                        yAxis = (int)(0 + ((max - amplitude) / scale));
                    if (yAxis < 0)
                        yAxis = 0;
                    if (yAxis > height)
                        yAxis = height;
                    pen.Color = pen.Color = Color.FromArgb(0, GetColor(min, max, range, amplitude), 0);
                    offScreenDC.DrawLine(pen, xAxis, height, xAxis, yAxis);

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    int rappwidth = width / 50;
                    double rappheight = height * Info.fraz;

                    if (xAxis > rappwidth && yAxis < rappheight)
                    {
                        if (xAxis < 220)
                        {
                            if (!started)
                            { t.Start(); started = true; }
                            lgp_l[index].Add(new Point(xAxis, yAxis));
                        }
                    }
                }
                offScreenDC.DrawString("Min: " + minHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + min.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 1);
                offScreenDC.DrawString("Max: " + maxHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + max.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 18);

                // Clean up
                pictureBox.Image = canvas;
                offScreenDC.Dispose();
            }

            private RichTextBox referredtxt;
            public void RenderFrequencyDomainRight(ref PictureBox pictureBox, ref RichTextBox txt, int samples)
            {
                referredtxt = txt;
                // Set up for drawing
                Bitmap canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
                Graphics offScreenDC = Graphics.FromImage(canvas);
                SolidBrush brush = new System.Drawing.SolidBrush(Color.FromArgb(128, 255, 255, 255));
                Pen pen = new System.Drawing.Pen(Color.WhiteSmoke);
                Font font = new Font("Arial", 10);

                // Determine channnel boundries
                int width = canvas.Width;
                int height = canvas.Height;

                double min = double.MaxValue;
                double minHz = 0;
                double max = double.MinValue;
                double maxHz = 0;
                double range = 0;
                double scale = 0;
                double scaleHz = (double)(samples / 2) / (double)_fftRight.Length;

                // get left min/max
                for (int x = 0; x < _fftRight.Length; x++)
                {
                    double amplitude = _fftRight[x];
                    if (min > amplitude && amplitude != double.NegativeInfinity)
                    {
                        min = amplitude;
                        minHz = (double)x * scaleHz;
                    }
                    if (max < amplitude && amplitude != double.PositiveInfinity)
                    {
                        max = amplitude;
                        maxHz = (double)x * scaleHz;
                    }
                }

                // get right range
                if (min < 0 || max < 0)
                    if (min < 0 && max < 0)
                        range = max - min;
                    else
                        range = Math.Abs(min) + max;
                else
                    range = max - min;
                scale = range / height;

                // draw right channel
                for (int xAxis = 0; xAxis < width; xAxis++)
                {
                    double amplitude = (double)_fftRight[(int)(((double)(_fftRight.Length) / (double)(width)) * xAxis)];
                    if (amplitude == double.NegativeInfinity || amplitude == double.PositiveInfinity || amplitude == double.MinValue || amplitude == double.MaxValue)
                        amplitude = 0;
                    int yAxis;
                    if (amplitude < 0)
                        yAxis = (int)(height - ((amplitude - min) / scale));
                    else
                        yAxis = (int)(0 + ((max - amplitude) / scale));
                    if (yAxis < 0)
                        yAxis = 0;
                    if (yAxis > height)
                        yAxis = height;
                    pen.Color = pen.Color = Color.FromArgb(0, GetColor(min, max, range, amplitude), 0);
                    offScreenDC.DrawLine(pen, xAxis, height, xAxis, yAxis);

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    int rappwidth = width / 50;
                    double rappheight = height * Info.fraz;

                    if (xAxis > rappwidth && yAxis < rappheight)
                    {
                        if (xAxis < 220)//ricevo frequenze da considerare
                        {
                            if (!started)
                            { t.Start(); started = true; }
                            lgp_r[index].Add(new Point(xAxis, yAxis));
                        }
                        else//frequenza di fine messaggio
                        {
                            t.Stop(); started = false;

                            if (!called)
                            {
                                towrite = GetMessageFromFrequencies(GetFrequencies(new List<List<Point>>[2] { lgp_r, lgp_l }));
                                t2.Start();
                                called = true;
                            }
                        }
                    }
                }
                offScreenDC.DrawString("Min: " + minHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + min.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 1);
                offScreenDC.DrawString("Max: " + maxHz.ToString(".#") + " Hz (±" + scaleHz.ToString(".#") + ") = " + max.ToString(".###") + " dB", font, brush, 0 + 1, 0 + 18);

                // Clean up
                pictureBox.Image = canvas;
                offScreenDC.Dispose();
            }

            private string towrite = "";
            private void Writer()
            {
                Comandi.Command cmd = new Comandi.Command(towrite);
                cmd.Execute();

                referredtxt.Text += towrite + "\n";

                towrite = "";
            }

            private int GetSignificative(List<Point> l)
            {
                return l.GroupBy(x => x.X).OrderByDescending(x => x.Count()).Take(1).Select(x => x.Key).ToArray()[0];
            }

            private List<int> GetFrequencies(List<List<Point>>[] ll)
            {
                List<int> signpx = new List<int>();
                List<int> freqs = new List<int>();

                ll[0] = ll[0].Where(x => x.Count > 1).ToList();
                ll[1] = ll[1].Where(x => x.Count > 1).ToList();

                //per ogni lista di punti nella lista estraggo il valore più volte ripetuto
                for (int i = 0; i < ll[0].Count; i++)
                {
                    int x_p = 0;
                    try
                    {
                        if (ll[0][i].Count > ll[1][i].Count)
                            x_p = GetSignificative(ll[0][i]);
                        else
                            x_p = GetSignificative(ll[1][i]);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        x_p = GetSignificative(ll[0][i]);
                    }

                    signpx.Add(x_p);
                }
                //

                List<int> px_discreti = new List<int>();
                //analizzo i pixel discreti sapendo che per 200Hz di frequenza di incremento ho 3px in più
                List<int> f_discreti = new List<int>();
                //creo un vettore di corrispondenza pixel-frequenze
                int n_px = 22;
                int n_f = 1500;
                while (n_px <= 222)
                {
                    px_discreti.Add(n_px);
                    f_discreti.Add(n_f);
                    n_px += 3;
                    n_f += 200;
                }

                //modifivo i valori imprecisi rispetto a quelli discreti
                //approssimo parecchio...
                //Altresì: SPERO CHE LA FORTUNA MI ASSISTA
                for (int i = 0; i < signpx.Count; i++)
                {
                    if (px_discreti.Contains(signpx[i] + 1))
                        signpx[i] += 1;
                    else if (px_discreti.Contains(signpx[i] - 1))
                        signpx[i] -= 1;
                }

                //analizzo il dominio delle frequenze
                foreach (int elem in signpx)
                {
                    freqs.Add(f_discreti[px_discreti.IndexOf(elem)]);
                }

                return freqs;
            }

            private string GetMessageFromFrequencies(List<int> freqs)
            {
                //ottengo la corrispondenza inversa dal dizionario
                string ret = "";
                foreach (int f in freqs)
                    ret += built.Keys.Where(v => built[v] == f).ToArray()[0];
                return ret;
            }

            private static int GetColor(double min, double max, double range, double amplitude)
            {
                double color;
                if (min != double.NegativeInfinity && min != double.MaxValue & max != double.PositiveInfinity && max != double.MinValue && range != 0)
                {
                    if (min < 0 || max < 0)
                        if (min < 0 && max < 0)
                            color = (255 / range) * (Math.Abs(min) - Math.Abs(amplitude));
                        else
                            if (amplitude < 0)
                                color = (255 / range) * (Math.Abs(min) - Math.Abs(amplitude));
                            else
                                color = (255 / range) * (amplitude + Math.Abs(min));
                    else
                        color = (255 / range) * (amplitude - min);
                }
                else
                    color = 0;
                return (int)color;
            }
        }

        #region sub
        public class FourierTransform
        {
            static private int n, nu;

            static private int BitReverse(int j)
            {
                int j2;
                int j1 = j;
                int k = 0;
                for (int i = 1; i <= nu; i++)
                {
                    j2 = j1 / 2;
                    k = 2 * k + j1 - 2 * j2;
                    j1 = j2;
                }
                return k;
            }

            static public double[] FFT(ref double[] x)
            {
                // Assume n is a power of 2
                n = x.Length;
                nu = (int)(Math.Log(n) / Math.Log(2));
                int n2 = n / 2;
                int nu1 = nu - 1;
                double[] xre = new double[n];
                double[] xim = new double[n];
                double[] magnitude = new double[n2];
                double[] decibel = new double[n2];
                double tr, ti, p, arg, c, s;
                for (int i = 0; i < n; i++)
                {
                    xre[i] = x[i];
                    xim[i] = 0.0f;
                }
                int k = 0;
                for (int l = 1; l <= nu; l++)
                {
                    while (k < n)
                    {
                        for (int i = 1; i <= n2; i++)
                        {
                            p = BitReverse(k >> nu1);
                            arg = 2 * (double)Math.PI * p / n;
                            c = (double)Math.Cos(arg);
                            s = (double)Math.Sin(arg);
                            tr = xre[k + n2] * c + xim[k + n2] * s;
                            ti = xim[k + n2] * c - xre[k + n2] * s;
                            xre[k + n2] = xre[k] - tr;
                            xim[k + n2] = xim[k] - ti;
                            xre[k] += tr;
                            xim[k] += ti;
                            k++;
                        }
                        k += n2;
                    }
                    k = 0;
                    nu1--;
                    n2 = n2 / 2;
                }
                k = 0;
                int r;
                while (k < n)
                {
                    r = BitReverse(k);
                    if (r > k)
                    {
                        tr = xre[k];
                        ti = xim[k];
                        xre[k] = xre[r];
                        xim[k] = xim[r];
                        xre[r] = tr;
                        xim[r] = ti;
                    }
                    k++;
                }
                for (int i = 0; i < n / 2; i++)
                    //magnitude[i] = (float)(Math.Sqrt((xre[i] * xre[i]) + (xim[i] * xim[i])));
                    decibel[i] = 10.0 * Math.Log10((float)(Math.Sqrt((xre[i] * xre[i]) + (xim[i] * xim[i]))));
                //return magnitude;
                return decibel;
            }
        }

        class SignalGenerator
        {
            private string _waveForm = "Sine";
            private double _amplitude = 128.0;
            private double _samplingRate = 44100;
            private double _frequency = 5000.0;
            private double _dcLevel = 0.0;
            private double _noise = 0.0;
            private int _samples = 16384;
            private bool _addDCLevel = false;
            private bool _addNoise = false;

            public SignalGenerator()
            {
            }

            public void SetWaveform(string waveForm)
            {
                _waveForm = waveForm;
            }

            public String GetWaveform()
            {
                return _waveForm;
            }

            public void SetAmplitude(double amplitude)
            {
                _amplitude = amplitude;
            }

            public double GetAmplitude()
            {
                return _amplitude;
            }

            public void SetFrequency(double frequency)
            {
                _frequency = frequency;
            }

            public double GetFrequency()
            {
                return _frequency;
            }

            public void SetSamplingRate(double rate)
            {
                _samplingRate = rate;
            }

            public double GetSamplingRate()
            {
                return _samplingRate;
            }

            public void SetSamples(int samples)
            {
                _samples = samples;
            }

            public int GetSamples()
            {
                return _samples;
            }

            public void SetDCLevel(double dc)
            {
                _dcLevel = dc;
            }

            public double GetDCLevel()
            {
                return _dcLevel;
            }

            public void SetNoise(double noise)
            {
                _noise = noise;
            }

            public double GetNoise()
            {
                return _noise;
            }

            public void SetDCLevelState(bool dcstate)
            {
                _addDCLevel = dcstate;
            }

            public bool IsDCLevel()
            {
                return _addDCLevel;
            }

            public void SetNoiseState(bool noisestate)
            {
                _addNoise = noisestate;
            }

            public bool IsNoise()
            {
                return _addNoise;
            }

            public double[] GenerateSignal()
            {
                double[] values = new double[_samples];
                if (_waveForm.Equals("Sine"))
                {
                    double theta = 2.0 * Math.PI * _frequency / _samplingRate;
                    for (int i = 0; i < _samples; i++)
                    {
                        values[i] = _amplitude * Math.Sin(i * theta);
                    }
                }
                if (_waveForm.Equals("Cosine"))
                {
                    double theta = 2.0f * (double)Math.PI * _frequency / _samplingRate;
                    for (int i = 0; i < _samples; i++)
                        values[i] = _amplitude * Math.Cos(i * theta);
                }
                if (_waveForm.Equals("Square"))
                {
                    double p = 2.0 * _frequency / _samplingRate;
                    for (int i = 0; i < _samples; i++)
                        values[i] = Math.Round(i * p) % 2 == 0 ? _amplitude : -_amplitude;
                }
                if (_waveForm.Equals("Triangular"))
                {
                    double p = 2.0 * _frequency / _samplingRate;
                    for (int i = 0; i < _samples; i++)
                    {
                        int ip = (int)Math.Round(i * p);
                        values[i] = 2.0 * _amplitude * (1 - 2 * (ip % 2)) * (i * p - ip);
                    }
                }
                if (_waveForm.Equals("Sawtooth"))
                {
                    for (int i = 0; i < _samples; i++)
                    {
                        double q = i * _frequency / _samplingRate;
                        values[i] = 2.0 * _amplitude * (q - Math.Round(q));
                    }
                }
                if (_addDCLevel)
                {
                    for (int i = 0; i < _samples; i++)
                        values[i] += _dcLevel;
                }
                if (_addNoise)
                {
                    Random r = new Random();
                    for (int i = 0; i < _samples; i++)
                        values[i] += _noise * r.Next();
                }
                return values;
            }
        }

        internal class WaveInHelper
        {
            public static void Try(int err)
            {
                if (err != WaveNative.MMSYSERR_NOERROR)
                    throw new Exception(err.ToString());
            }
        }

        public delegate void BufferDoneEventHandler(IntPtr data, int size);

        internal class WaveInBuffer : IDisposable
        {
            public WaveInBuffer NextBuffer;

            private AutoResetEvent m_RecordEvent = new AutoResetEvent(false);
            private IntPtr m_WaveIn;

            private WaveNative.WaveHdr m_Header;
            private byte[] m_HeaderData;
            private GCHandle m_HeaderHandle;
            private GCHandle m_HeaderDataHandle;

            private bool m_Recording;

            internal static void WaveInProc(IntPtr hdrvr, int uMsg, int dwUser, ref WaveNative.WaveHdr wavhdr, int dwParam2)
            {
                if (uMsg == WaveNative.MM_WIM_DATA)
                {
                    try
                    {
                        GCHandle h = (GCHandle)wavhdr.dwUser;
                        WaveInBuffer buf = (WaveInBuffer)h.Target;
                        buf.OnCompleted();
                    }
                    catch
                    {
                    }
                }
            }

            public WaveInBuffer(IntPtr waveInHandle, int size)
            {
                m_WaveIn = waveInHandle;

                m_HeaderHandle = GCHandle.Alloc(m_Header, GCHandleType.Pinned);
                m_Header.dwUser = (IntPtr)GCHandle.Alloc(this);
                m_HeaderData = new byte[size];
                m_HeaderDataHandle = GCHandle.Alloc(m_HeaderData, GCHandleType.Pinned);
                m_Header.lpData = m_HeaderDataHandle.AddrOfPinnedObject();
                m_Header.dwBufferLength = size;
                WaveInHelper.Try(WaveNative.waveInPrepareHeader(m_WaveIn, ref m_Header, Marshal.SizeOf(m_Header)));
            }
            ~WaveInBuffer()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (m_Header.lpData != IntPtr.Zero)
                {
                    WaveNative.waveInUnprepareHeader(m_WaveIn, ref m_Header, Marshal.SizeOf(m_Header));
                    m_HeaderHandle.Free();
                    m_Header.lpData = IntPtr.Zero;
                }
                m_RecordEvent.Close();
                if (m_HeaderDataHandle.IsAllocated)
                    m_HeaderDataHandle.Free();
                GC.SuppressFinalize(this);
            }

            public int Size
            {
                get { return m_Header.dwBufferLength; }
            }

            public IntPtr Data
            {
                get { return m_Header.lpData; }
            }

            public bool Record()
            {
                lock (this)
                {
                    m_RecordEvent.Reset();
                    m_Recording = WaveNative.waveInAddBuffer(m_WaveIn, ref m_Header, Marshal.SizeOf(m_Header)) == WaveNative.MMSYSERR_NOERROR;
                    return m_Recording;
                }
            }

            public void WaitFor()
            {
                if (m_Recording)
                    m_Recording = m_RecordEvent.WaitOne();
                else
                    Thread.Sleep(0);
            }

            private void OnCompleted()
            {
                m_RecordEvent.Set();
                m_Recording = false;
            }
        }

        public class WaveInRecorder : IDisposable
        {
            private IntPtr m_WaveIn;
            private WaveInBuffer m_Buffers; // linked list
            private WaveInBuffer m_CurrentBuffer;
            private Thread m_Thread;
            private BufferDoneEventHandler m_DoneProc;
            private bool m_Finished;

            private WaveNative.WaveDelegate m_BufferProc = new WaveNative.WaveDelegate(WaveInBuffer.WaveInProc);

            public static int DeviceCount
            {
                get { return WaveNative.waveInGetNumDevs(); }
            }

            public WaveInRecorder(int device, WaveFormat format, int bufferSize, int bufferCount, BufferDoneEventHandler doneProc)
            {
                m_DoneProc = doneProc;
                WaveInHelper.Try(WaveNative.waveInOpen(out m_WaveIn, device, format, m_BufferProc, IntPtr.Zero, WaveNative.CALLBACK_FUNCTION));
                AllocateBuffers(bufferSize, bufferCount);
                for (int i = 0; i < bufferCount; i++)
                {
                    SelectNextBuffer();
                    m_CurrentBuffer.Record();
                }
                WaveInHelper.Try(WaveNative.waveInStart(m_WaveIn));
                m_Thread = new Thread(new ThreadStart(ThreadProc));
                m_Thread.Start();
            }
            ~WaveInRecorder()
            {
                Dispose();
            }
            public void Dispose()
            {
                if (m_Thread != null)
                    try
                    {
                        m_Finished = true;
                        if (m_WaveIn != IntPtr.Zero)
                            WaveNative.waveInReset(m_WaveIn);
                        WaitForAllBuffers();
                        m_Thread.Join();
                        m_DoneProc = null;
                        FreeBuffers();
                        if (m_WaveIn != IntPtr.Zero)
                            WaveNative.waveInClose(m_WaveIn);
                    }
                    finally
                    {
                        m_Thread = null;
                        m_WaveIn = IntPtr.Zero;
                    }
                GC.SuppressFinalize(this);
            }
            private void ThreadProc()
            {
                while (!m_Finished)
                {
                    Advance();
                    if (m_DoneProc != null && !m_Finished)
                        m_DoneProc(m_CurrentBuffer.Data, m_CurrentBuffer.Size);
                    m_CurrentBuffer.Record();
                }
            }
            private void AllocateBuffers(int bufferSize, int bufferCount)
            {
                FreeBuffers();
                if (bufferCount > 0)
                {
                    m_Buffers = new WaveInBuffer(m_WaveIn, bufferSize);
                    WaveInBuffer Prev = m_Buffers;
                    try
                    {
                        for (int i = 1; i < bufferCount; i++)
                        {
                            WaveInBuffer Buf = new WaveInBuffer(m_WaveIn, bufferSize);
                            Prev.NextBuffer = Buf;
                            Prev = Buf;
                        }
                    }
                    finally
                    {
                        Prev.NextBuffer = m_Buffers;
                    }
                }
            }
            private void FreeBuffers()
            {
                m_CurrentBuffer = null;
                if (m_Buffers != null)
                {
                    WaveInBuffer First = m_Buffers;
                    m_Buffers = null;

                    WaveInBuffer Current = First;
                    do
                    {
                        WaveInBuffer Next = Current.NextBuffer;
                        Current.Dispose();
                        Current = Next;
                    } while (Current != First);
                }
            }
            private void Advance()
            {
                SelectNextBuffer();
                m_CurrentBuffer.WaitFor();
            }
            private void SelectNextBuffer()
            {
                m_CurrentBuffer = m_CurrentBuffer == null ? m_Buffers : m_CurrentBuffer.NextBuffer;
            }
            private void WaitForAllBuffers()
            {
                WaveInBuffer Buf = m_Buffers;
                while (Buf.NextBuffer != m_Buffers)
                {
                    Buf.WaitFor();
                    Buf = Buf.NextBuffer;
                }
            }
        }

        public enum WaveFormats
        {
            Pcm = 1,
            Float = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public class WaveFormat
        {
            public short wFormatTag;
            public short nChannels;
            public int nSamplesPerSec;
            public int nAvgBytesPerSec;
            public short nBlockAlign;
            public short wBitsPerSample;
            public short cbSize;

            public WaveFormat(int rate, int bits, int channels)
            {
                wFormatTag = (short)WaveFormats.Pcm;
                nChannels = (short)channels;
                nSamplesPerSec = rate;
                wBitsPerSample = (short)bits;
                cbSize = 0;

                nBlockAlign = (short)(channels * (bits / 8));
                nAvgBytesPerSec = nSamplesPerSec * nBlockAlign;
            }
        }

        internal class WaveNative
        {
            // consts
            public const int MMSYSERR_NOERROR = 0; // no error

            public const int MM_WOM_OPEN = 0x3BB;
            public const int MM_WOM_CLOSE = 0x3BC;
            public const int MM_WOM_DONE = 0x3BD;

            public const int MM_WIM_OPEN = 0x3BE;
            public const int MM_WIM_CLOSE = 0x3BF;
            public const int MM_WIM_DATA = 0x3C0;

            public const int CALLBACK_FUNCTION = 0x00030000;    // dwCallback is a FARPROC 

            public const int TIME_MS = 0x0001;  // time in milliseconds 
            public const int TIME_SAMPLES = 0x0002;  // number of wave samples 
            public const int TIME_BYTES = 0x0004;  // current byte offset 

            // callbacks
            public delegate void WaveDelegate(IntPtr hdrvr, int uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2);

            // structs 

            [StructLayout(LayoutKind.Sequential)]
            public struct WaveHdr
            {
                public IntPtr lpData; // pointer to locked data buffer
                public int dwBufferLength; // length of data buffer
                public int dwBytesRecorded; // used for input only
                public IntPtr dwUser; // for client's use
                public int dwFlags; // assorted flags (see defines)
                public int dwLoops; // loop control counter
                public IntPtr lpNext; // PWaveHdr, reserved for driver
                public int reserved; // reserved for driver
            }

            private const string mmdll = "winmm.dll";

            // WaveOut calls
            [DllImport(mmdll)]
            public static extern int waveOutGetNumDevs();
            [DllImport(mmdll)]
            public static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);
            [DllImport(mmdll)]
            public static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);
            [DllImport(mmdll)]
            public static extern int waveOutWrite(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);
            [DllImport(mmdll)]
            public static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, WaveFormat lpFormat, WaveDelegate dwCallback, IntPtr dwInstance, int dwFlags);
            [DllImport(mmdll)]
            public static extern int waveOutReset(IntPtr hWaveOut);
            [DllImport(mmdll)]
            public static extern int waveOutClose(IntPtr hWaveOut);
            [DllImport(mmdll)]
            public static extern int waveOutPause(IntPtr hWaveOut);
            [DllImport(mmdll)]
            public static extern int waveOutRestart(IntPtr hWaveOut);
            [DllImport(mmdll)]
            public static extern int waveOutGetPosition(IntPtr hWaveOut, out int lpInfo, int uSize);
            [DllImport(mmdll)]
            public static extern int waveOutSetVolume(IntPtr hWaveOut, int dwVolume);
            [DllImport(mmdll)]
            public static extern int waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);

            // WaveIn calls
            [DllImport(mmdll)]
            public static extern int waveInGetNumDevs();
            [DllImport(mmdll)]
            public static extern int waveInAddBuffer(IntPtr hwi, ref WaveHdr pwh, int cbwh);
            [DllImport(mmdll)]
            public static extern int waveInClose(IntPtr hwi);
            [DllImport(mmdll)]
            public static extern int waveInOpen(out IntPtr phwi, int uDeviceID, WaveFormat lpFormat, WaveDelegate dwCallback, IntPtr dwInstance, int dwFlags);
            [DllImport(mmdll)]
            public static extern int waveInPrepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
            [DllImport(mmdll)]
            public static extern int waveInUnprepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
            [DllImport(mmdll)]
            public static extern int waveInReset(IntPtr hwi);
            [DllImport(mmdll)]
            public static extern int waveInStart(IntPtr hwi);
            [DllImport(mmdll)]
            public static extern int waveInStop(IntPtr hwi);
        }

        public class WaveStream : Stream, IDisposable
        {
            private Stream m_Stream;
            private long m_DataPos;
            private int m_Length;

            private WaveFormat m_Format;

            public WaveFormat Format
            {
                get { return m_Format; }
            }

            private string ReadChunk(BinaryReader reader)
            {
                byte[] ch = new byte[4];
                reader.Read(ch, 0, ch.Length);
                return System.Text.Encoding.ASCII.GetString(ch);
            }

            private void ReadHeader()
            {
                BinaryReader Reader = new BinaryReader(m_Stream);
                if (ReadChunk(Reader) != "RIFF")
                    throw new Exception("Invalid file format");

                Reader.ReadInt32(); // File length minus first 8 bytes of RIFF description, we don't use it

                if (ReadChunk(Reader) != "WAVE")
                    throw new Exception("Invalid file format");

                if (ReadChunk(Reader) != "fmt ")
                    throw new Exception("Invalid file format");

                int len = Reader.ReadInt32();
                if (len < 16) // bad format chunk length
                    throw new Exception("Invalid file format");

                m_Format = new WaveFormat(22050, 16, 2); // initialize to any format
                m_Format.wFormatTag = Reader.ReadInt16();
                m_Format.nChannels = Reader.ReadInt16();
                m_Format.nSamplesPerSec = Reader.ReadInt32();
                m_Format.nAvgBytesPerSec = Reader.ReadInt32();
                m_Format.nBlockAlign = Reader.ReadInt16();
                m_Format.wBitsPerSample = Reader.ReadInt16();

                // advance in the stream to skip the wave format block 
                len -= 16; // minimum format size
                while (len > 0)
                {
                    Reader.ReadByte();
                    len--;
                }

                // assume the data chunk is aligned
                while (m_Stream.Position < m_Stream.Length && ReadChunk(Reader) != "data")
                    ;

                if (m_Stream.Position >= m_Stream.Length)
                    throw new Exception("Invalid file format");

                m_Length = Reader.ReadInt32();
                m_DataPos = m_Stream.Position;

                Position = 0;
            }

            /// <summary>ReadChunk(reader) - Changed to CopyChunk(reader, writer)</summary>
            /// <param name="reader">source stream</param>
            /// <returns>four characters</returns>
            private string CopyChunk(BinaryReader reader, BinaryWriter writer)
            {
                byte[] ch = new byte[4];
                reader.Read(ch, 0, ch.Length);

                //copy the chunk
                writer.Write(ch);

                return System.Text.Encoding.ASCII.GetString(ch);
            }

            /// <summary>ReadHeader() - Changed to CopyHeader(destination)</summary>
            private void CopyHeader(Stream destinationStream)
            {
                BinaryReader reader = new BinaryReader(m_Stream);
                BinaryWriter writer = new BinaryWriter(destinationStream);

                if (CopyChunk(reader, writer) != "RIFF")
                    throw new Exception("Invalid file format");

                writer.Write(reader.ReadInt32()); // File length minus first 8 bytes of RIFF description

                if (CopyChunk(reader, writer) != "WAVE")
                    throw new Exception("Invalid file format");

                if (CopyChunk(reader, writer) != "fmt ")
                    throw new Exception("Invalid file format");

                int len = reader.ReadInt32();
                if (len < 16)
                { // bad format chunk length
                    throw new Exception("Invalid file format");
                }
                else
                {
                    writer.Write(len);
                }

                m_Format = new WaveFormat(22050, 16, 2); // initialize to any format
                m_Format.wFormatTag = reader.ReadInt16();
                m_Format.nChannels = reader.ReadInt16();
                m_Format.nSamplesPerSec = reader.ReadInt32();
                m_Format.nAvgBytesPerSec = reader.ReadInt32();
                m_Format.nBlockAlign = reader.ReadInt16();
                m_Format.wBitsPerSample = reader.ReadInt16();

                //copy format information
                writer.Write(m_Format.wFormatTag);
                writer.Write(m_Format.nChannels);
                writer.Write(m_Format.nSamplesPerSec);
                writer.Write(m_Format.nAvgBytesPerSec);
                writer.Write(m_Format.nBlockAlign);
                writer.Write(m_Format.wBitsPerSample);


                // advance in the stream to skip the wave format block 
                len -= 16; // minimum format size
                writer.Write(reader.ReadBytes(len));
                len = 0;
                /*while (len > 0)
                {
                    reader.ReadByte();
                    len--;
                }*/

                // assume the data chunk is aligned
                while (m_Stream.Position < m_Stream.Length && CopyChunk(reader, writer) != "data")
                    ;

                if (m_Stream.Position >= m_Stream.Length)
                    throw new Exception("Invalid file format");

                m_Length = reader.ReadInt32();
                writer.Write(m_Length);

                m_DataPos = m_Stream.Position;
                Position = 0;
            }

            /// <summary>Write a new header</summary>
            public static Stream CreateStream(Stream waveData, WaveFormat format)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF".ToCharArray()));

                writer.Write((Int32)(waveData.Length + 36)); //File length minus first 8 bytes of RIFF description

                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ".ToCharArray()));

                writer.Write((Int32)16); //length of following chunk: 16

                writer.Write((Int16)format.wFormatTag);
                writer.Write((Int16)format.nChannels);
                writer.Write((Int32)format.nSamplesPerSec);
                writer.Write((Int32)format.nAvgBytesPerSec);
                writer.Write((Int16)format.nBlockAlign);
                writer.Write((Int16)format.wBitsPerSample);

                writer.Write(System.Text.Encoding.ASCII.GetBytes("data".ToCharArray()));

                writer.Write((Int32)waveData.Length);

                waveData.Seek(0, SeekOrigin.Begin);
                byte[] b = new byte[waveData.Length];
                waveData.Read(b, 0, (int)waveData.Length);
                writer.Write(b);

                writer.Seek(0, SeekOrigin.Begin);
                return stream;
            }


            public WaveStream(Stream sourceStream, Stream destinationStream)
            {
                m_Stream = sourceStream;
                CopyHeader(destinationStream);
            }

            public WaveStream(Stream sourceStream)
            {
                m_Stream = sourceStream;
                ReadHeader();
            }

            ~WaveStream()
            {
                Dispose();
            }

            //public void Dispose()
            //{
            //	if (m_Stream != null)
            //		m_Stream.Close();
            //	GC.SuppressFinalize(this);
            //}

            public override bool CanRead
            {
                get { return true; }
            }
            public override bool CanSeek
            {
                get { return true; }
            }
            public override bool CanWrite
            {
                get { return false; }
            }
            public override long Length
            {
                get { return m_Length; }
            }

            /// <summary>Length of the data (in samples)</summary>
            public long CountSamples
            {
                get { return (long)((m_Length - m_DataPos) / (m_Format.wBitsPerSample / 8)); }
            }

            public override long Position
            {
                get { return m_Stream.Position - m_DataPos; }
                set { Seek(value, SeekOrigin.Begin); }
            }
            public override void Close()
            {
                Dispose();
            }
            public override void Flush()
            {
            }
            public override void SetLength(long len)
            {
                throw new InvalidOperationException();
            }
            public override long Seek(long pos, SeekOrigin o)
            {
                switch (o)
                {
                    case SeekOrigin.Begin:
                        m_Stream.Position = pos + m_DataPos;
                        break;
                    case SeekOrigin.Current:
                        m_Stream.Seek(pos, SeekOrigin.Current);
                        break;
                    case SeekOrigin.End:
                        m_Stream.Position = m_DataPos + m_Length - pos;
                        break;
                }
                return this.Position;
            }

            public override int Read(byte[] buf, int ofs, int count)
            {
                int toread = (int)Math.Min(count, m_Length - Position);
                return m_Stream.Read(buf, ofs, toread);
            }

            /// <summary>Read - Changed to Copy</summary>
            /// <param name="buf">Buffer to receive the data</param>
            /// <param name="ofs">Offset</param>
            /// <param name="count">Count of bytes to read</param>
            /// <param name="destination">Where to copy the buffer</param>
            /// <returns>Count of bytes actually read</returns>
            public int Copy(byte[] buf, int ofs, int count, Stream destination)
            {
                int toread = (int)Math.Min(count, m_Length - Position);
                int read = m_Stream.Read(buf, ofs, toread);
                destination.Write(buf, ofs, read);

                if (m_Stream.Position != destination.Position)
                {
                    Console.WriteLine();
                }

                return read;
            }

            public override void Write(byte[] buf, int ofs, int count)
            {
                throw new InvalidOperationException();
            }
        }
        #endregion

        #endregion
    }

    namespace Comandi
    {
        class Command
        {
            enum Rads
            {
                MSG = 0, EXE = 1
            }

            private string _des;
            private Rads _rad;

            private List<string> _rads = new List<string>() { "msg: ", "exe: " };

            public Command(string command)
            {
                try
                {
                    CommandController(command);
                }
                catch (Exception ex) { throw ex; }
            }

            private void CommandController(string command)
            {
                int count = 0;
                foreach (string r in _rads)
                {
                    if (command.Contains(r))
                    {
                        _rad = (Rads)Enum.ToObject(typeof(Rads), _rads.IndexOf(r));
                        _des = command.Replace(r, "");
                        if (_des != null)
                            break;
                    }
                    else count++;
                }
                if (count == _rads.Count)
                    throw new Exception("Errore Radice COMANDO");
                return;
            }

            public void Execute()
            {
                switch (_rad)
                {
                    case Rads.MSG:
                        MessageBox.Show(_des, "Message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    case Rads.EXE:
                        System.Reflection.MethodInfo m = typeof(Exe).GetMethod(_des);
                        m.Invoke(null, null);
                        break;
                    default: break;
                }
            }
        }

        public class Exe
        {
            public static void shut()
            {
                System.Diagnostics.Process.Start("shutdown", "-s -t 0");
            }

            public static void paint()
            {
                string[] cowsay = { "\t_________________", "  < Hi! I'm Cowsay =D >", "\t-----------------",
                                  "\t\t\t\\   ^__^", "\t\t\t \\  (oo)\\_______", "\t\t\t\t(__)\\       )\\/\\",
                                  "\t\t\t\t\t||----w |", "\t\t\t\t\t||     ||"};
                foreach (string s in cowsay)
                {
                    Console.WriteLine(s);
                }
            }

            #region CD-ROM
            #region cdrom dllimport
            [DllImport("winmm.dll")]
            static extern Int32 mciSendString(String command, StringBuilder buffer, Int32 bufferSize, IntPtr hwndCallback);
            #endregion

            public static void cd()
            {
                //apri-chiudi lettore cd
                Thread th = new Thread(new ThreadStart(cdhandler));
                th.Start();
            }

            private static void cdhandler()
            {
                mciSendString("set CDAudio door open", null, 0, IntPtr.Zero);
                Thread.Sleep(2000);
                mciSendString("set CDAudio door closed", null, 0, IntPtr.Zero);
            }
            #endregion

            #region NETWORK-ADAPTER
            public static void net()
            {
                //attiva-disattiva rete
                Thread th = new Thread(new ThreadStart(nethandler));
                th.Start();
            }

            private static void nethandler()
            {
                System.Diagnostics.Process.Start("netsh", "interface set interface name=\"Local Area Connection\" disabled");
                System.Threading.Thread.Sleep(5000);
                System.Diagnostics.Process.Start("netsh", "interface set interface name=\"Local Area Connection\" enabled");
            }
            #endregion

            public static void land()
            {
                //gira monitor

            }

            public static void res()
            {
                //cambia risoluzione

            }

            #region PING
            public static void ping()
            {
                //ping bigG
                Thread th = new Thread(new ThreadStart(pinghandler));
                th.Start();
            }

            private static void pinghandler()
            {
                Ping p = new Ping();
                PingOptions po = new PingOptions(); po.DontFragment = true;
                byte[] buffer = new byte[32];
                PingReply pr = p.Send("www.google.it", 120, buffer, po);

                if (pr.Status == IPStatus.Success)
                {
                    string msg = string.Format("Indirizzo: www.google.it (IP: {0})\nLatenza: {1}ms\nBuffer Size: {2}byte.",
                        pr.Address.ToString(), pr.Options.Ttl, pr.Buffer.Length);
                    MessageBox.Show(msg, "Ping", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show("Connessione non stabilita", "Ping", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            #endregion

            public static void note()
            {
                //notepad con scritte

            }

            #region START-BTN
            #region keyboard dllimport
            [DllImport("user32.dll")]
            private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
               UIntPtr dwExtraInfo);
            #endregion

            public static void start()
            {
                keybd_event(0x11, 0, 0, UIntPtr.Zero);//CTRL Press
                keybd_event(0x1B, 0, 0, UIntPtr.Zero);//+ESC Press
                keybd_event(0x11, 0, 0x02, UIntPtr.Zero);//CTRL Release
                keybd_event(0x1B, 0, 0x02, UIntPtr.Zero);//+ESC Release
            }
            #endregion
        }
    }
}