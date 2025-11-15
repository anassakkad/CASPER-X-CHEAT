using BHaimMemN;
using Guna.UI2.WinForms;
using Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using Guna.UI2.WinForms.Suite;
using System.Runtime.CompilerServices;

namespace casper_panel
{
    public partial class Form2 : Form
    {

        private static BHaimMem BHaimMemN = new BHaimMem();

        string AimbotScan = ("FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 A5 43");
        string headoffset = ("0xAA");
        string chestoffset = ("0xA6");

        private Dictionary<long, int> OrginalValues1 = new Dictionary<long, int>();
        private Dictionary<long, int> OrginalValues2 = new Dictionary<long, int>();
        private Dictionary<long, int> OrginalValues3 = new Dictionary<long, int>();
        private Dictionary<long, int> OrginalValues4 = new Dictionary<long, int>();


        private const int ParticleCount = 100;
        private const int DrawCount = 100;
        private readonly Random _random = new Random();
        private readonly PointF[] _particlePositions = new PointF[ParticleCount];
        private readonly PointF[] _particleTargetPositions = new PointF[ParticleCount];
        private readonly float[] _particleSpeeds = new float[ParticleCount];
        private readonly float[] _particleSizes = new float[ParticleCount];
        private readonly float[] _particleRadii = new float[ParticleCount];
        private readonly float[] _particleRotations = new float[ParticleCount];
        private readonly PointF[] _vertices = new PointF[3];

        private Color _particleColor = Color.Red;

        private Color _glowColor = Color.Red;
        private bool _particlesEnabled = true;

        Mem memory = new Mem();
        private Keys hotkey = Keys.None;
        private bool waitingForKey = false;

        public Form2()
        {
            InitializeComponent();
            ShowMenuPanel();

            DoubleBuffered = true;

            Timer timer = new Timer
            {
                Interval = 3 // Roughly 60 FPS
            };
            timer.Tick += (sender, args) =>
            {
                UpdateParticles();
                Invalidate();
            };
            timer.Start();
        }




        // Initialize Particles
        private void InitializeParticles()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            for (int i = 0; i < ParticleCount; i++)
            {
                _particlePositions[i] = new PointF(0, 0);
                _particleTargetPositions[i] = new PointF(_random.Next(screenSize.Width), screenSize.Height * 2);
                _particleSpeeds[i] = 1 + _random.Next(25);
                _particleSizes[i] = _random.Next(8);
                _particleRadii[i] = _random.Next(4);
                _particleRotations[i] = 0;
            }
        }

        // Update Particles
        private void UpdateParticles()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            for (int i = 0; i < ParticleCount; i++)
            {
                if (_particlePositions[i].X == 0 || _particlePositions[i].Y == 0)
                {
                    _particlePositions[i] = new PointF(_random.Next(screenSize.Width + 1), 15f);
                    _particleSpeeds[i] = 1 + _random.Next(25);
                    _particleRadii[i] = _random.Next(4);
                    _particleSizes[i] = _random.Next(8);
                    _particleTargetPositions[i] = new PointF(_random.Next(screenSize.Width), screenSize.Height * 2);
                }

                float deltaTime = 2.5f / 60;
                _particlePositions[i] = Lerp(_particlePositions[i], _particleTargetPositions[i], deltaTime * (_particleSpeeds[i] / 60));
                _particleRotations[i] += deltaTime;

                if (_particlePositions[i].Y > screenSize.Height)
                {
                    _particlePositions[i] = new PointF(0, 0);
                    _particleRotations[i] = 0;
                }
            }
        }

        // Linear interpolation (Lerp)
        private PointF Lerp(PointF start, PointF end, float t)
        {
            return new PointF(start.X + (end.X - start.X) * t, start.Y + (end.Y - start.Y) * t);
        }

        // Handle drawing the particles and glow effect
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Add this check to skip drawing
            if (!_particlesEnabled)
                return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            for (int i = 0; i < DrawCount; i++)
            {
                DrawTriangleWithGlow(e.Graphics, _particlePositions[i], _particleSizes[i], _particleRotations[i]);
            }
        }


        // Drawing the triangle with glow effect
        private void DrawTriangleWithGlow(Graphics graphics, PointF position, float size, float rotation)
        {
            float angle = (float)(Math.PI * 2 / 3);
            PointF[] vertices = new PointF[3];

            for (int i = 0; i < 3; i++)
            {
                vertices[i] = new PointF(
                    position.X + size * (float)Math.Cos(rotation + i * angle),
                    position.Y + size * (float)Math.Sin(rotation + i * angle)
                );
            }

            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw glow effect
            int maxGlowLayers = 10;
            for (int j = 0; j < maxGlowLayers; j++)
            {
                int alpha = 25 - 2 * j;
                using (Brush glowBrush = new SolidBrush(Color.FromArgb(alpha, _glowColor))) // Using glow color
                {
                    float glowSize = size + j * 4;
                    graphics.FillEllipse(glowBrush, position.X - glowSize / 2, position.Y - glowSize / 2, glowSize, glowSize);
                }
            }

            // Draw triangle
            using (Brush brush = new SolidBrush(_particleColor)) // Using particle color
            {
                graphics.FillPolygon(brush, vertices);
            }
        }


        // Optional: show panelMenu on start


        private void Form2_Load(object sender, EventArgs e)
        {

        }


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        const uint PROCESS_CREATE_THREAD = 0x2;
        const uint PROCESS_QUERY_INFORMATION = 0x400;
        const uint PROCESS_VM_OPERATION = 0x8;
        const uint PROCESS_VM_WRITE = 0x20;
        const uint PROCESS_VM_READ = 0x10;

        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_READWRITE = 4;
        private static void ExtractEmbeddedResource(string resourceName, string outputPath)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            // Get the embedded resource stream
            using (Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new ArgumentException($"Resource '{resourceName}' not found.");
                }

                // Read the embedded resource and save it to the specified path
                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    byte[] buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void guna2ToggleSwitch2_CheckedChanged_1(object sender, EventArgs e)
        {
            string processName = "HD-Player"; // Specify your target process name
            string dllResourceName = "casper_panel.Jinixo.dll"; // Correct resource name

            // Extract the embedded msdrmi.dll to a temporary file
            string tempDllPath = Path.Combine(Path.GetTempPath(), "nazmul.dll");
            ExtractEmbeddedResource(dllResourceName, tempDllPath);

            Console.WriteLine($"DLL extracted successfully to: {tempDllPath}");


            Process[] targetProcesses = Process.GetProcessesByName(processName);
            if (targetProcesses.Length == 0)
            {
                Console.WriteLine($"Waiting for {processName}.exe...");
            }
            else
            {
                Process targetProcess = targetProcesses[0];
                IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)tempDllPath.Length, MEM_COMMIT, PAGE_READWRITE);

                IntPtr bytesWritten;
                WriteProcessMemory(hProcess, allocMemAddress, System.Text.Encoding.ASCII.GetBytes(tempDllPath), (uint)tempDllPath.Length, out bytesWritten);

                CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

                Console.Beep(240, 300);
                //Type Here Chams Is Already Injected or code invaible

            }
        }







        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void bindbtn_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientButton12_Click_1(object sender, EventArgs e)
        {
            PlaySound();
        }


        private void label44_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PanelVisual.Visible = true;
            PanelVisual.BringToFront();

            PanelMenu.Visible = false;
            SniperPanel.Visible = false;
        }



        private void guna2Panel21_Paint(object sender, PaintEventArgs e)
        {

        }
        private void btnMenu_Click(object sender, EventArgs e)
        {
            ShowMenuPanel();

        }

        private void btnVisual_Click(object sender, EventArgs e)
        {
            ShowVisualPanel();

        }
        private void PlaySound()
        {
            SoundPlayer player = new SoundPlayer("Activada.wav"); // المسار ديال الصوت
            player.Play();
        }
        private void ShowMenuPanel()
        {
            PanelMenu.Visible = true;
            PanelMenu.BringToFront();

            PanelVisual.Visible = false;
        }

        private void ShowVisualPanel()
        {
            PanelVisual.Visible = true;
            PanelVisual.BringToFront();

            PanelMenu.Visible = true;
        }


        private void chamsv1_CheckedChanged(object sender, EventArgs e)
        {
            string processName = "HD-Player"; // Specify your target process name
            string dllResourceName = "casper_panel.Jinixo.dll"; // Correct resource name

            // Extract the embedded msdrmi.dll to a temporary file
            string tempDllPath = Path.Combine(Path.GetTempPath(), "nazmul.dll");
            ExtractEmbeddedResource(dllResourceName, tempDllPath);

            Console.WriteLine($"DLL extracted successfully to: {tempDllPath}");


            Process[] targetProcesses = Process.GetProcessesByName(processName);
            if (targetProcesses.Length == 0)
            {
                Console.WriteLine($"Waiting for {processName}.exe...");
            }
            else
            {
                Process targetProcess = targetProcesses[0];
                IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)tempDllPath.Length, MEM_COMMIT, PAGE_READWRITE);

                IntPtr bytesWritten;
                WriteProcessMemory(hProcess, allocMemAddress, System.Text.Encoding.ASCII.GetBytes(tempDllPath), (uint)tempDllPath.Length, out bytesWritten);

                CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

                Console.Beep(240, 300);
                //Type Here Chams Is Already Injected or code invaible

            }
        }

        private async void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {

            //AimBot X
            OrginalValues1.Clear();
            OrginalValues2.Clear();
            OrginalValues3.Clear();
            OrginalValues4.Clear();
            Sta.Text = ("AimBot X Injecting....");
            Int64 readOffset = Convert.ToInt64(headoffset, 16);
            Int64 writeOffset = Convert.ToInt64(chestoffset, 16);
            Int32 proc = Process.GetProcessesByName("HD-Player")[0].Id;
            BHaimMemN.OpenProcess(proc);
            var result = await BHaimMemN.AoBScan2(AimbotScan, true, true);
            if (result.Count() != 0)
            {

                foreach (var CurrentAddress in result)
                {
                    Int64 addressToSave = CurrentAddress + writeOffset;
                    var CurrentBytes = BHaimMemN.readMemory(addressToSave.ToString("X"), sizeof(int));
                    int CurrentValue = BitConverter.ToInt32(CurrentBytes, 0); OrginalValues1[addressToSave] = CurrentValue;
                    Int64 addressToSave9 = CurrentAddress + readOffset;
                    var CurrentBytes9 = BHaimMemN.readMemory(addressToSave9.ToString("X"), sizeof(int));
                    int CurrentValue9 = BitConverter.ToInt32(CurrentBytes9, 0); OrginalValues2[addressToSave9] = CurrentValue9;
                    Int64 headbytes = CurrentAddress + readOffset;
                    Int64 chestbytes = CurrentAddress + writeOffset;
                    var bytes = BHaimMemN.readMemory(headbytes.ToString("X"), sizeof(int));
                    int read = BitConverter.ToInt32(bytes, 0);
                    var bytes2 = BHaimMemN.readMemory(chestbytes.ToString("X"), sizeof(int));
                    int read2 = BitConverter.ToInt32(bytes2, 0);
                    BHaimMemN.WriteMemory(chestbytes.ToString("X"), "int", read.ToString());
                    BHaimMemN.WriteMemory(headbytes.ToString("X"), "int", read2.ToString());
                    Int64 addressToSave1 = CurrentAddress + writeOffset;
                    var CurrentBytes1 = BHaimMemN.readMemory(addressToSave1.ToString("X"), sizeof(int));
                    int CurrentValue1 = BitConverter.ToInt32(CurrentBytes1, 0); OrginalValues3[addressToSave1] = CurrentValue1;
                    Int64 addressToSave19 = CurrentAddress + readOffset;
                    var CurrentBytes19 = BHaimMemN.readMemory(addressToSave19.ToString("X"), sizeof(int));
                    int CurrentValue19 = BitConverter.ToInt32(CurrentBytes19, 0); OrginalValues2[addressToSave19] = CurrentValue19;

                }
                Sta.Text = "AimBot X Injected";
                Console.Beep(1000, 400);
            }
            else
            {
                Sta.Text = "AimBot X Faild";
                Console.Beep(2000, 400);
            }
            PlaySound();
        }

        private void sta_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void PanelMenu_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void guna2ToggleSwitch15_CheckedChanged(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("HD-Player").Length == 0)
            {
                //Type Here Emulator not found Status
                Console.Beep(240, 300);
            }
            else
            {
                //Type Here Waiting Status

                string search = "09 0E 00 00 80 3F 00 00 80 3F";
                string replace = "09 0E 00 00 A0 4F 00 00 80 3F";

                bool k = false;
                memory.OpenProcess("HD-Player");

                int i2 = 22000000;
                IEnumerable<long> wl = await memory.AoBScan(search, writable: true, true);
                string u = "0x" + wl.FirstOrDefault().ToString("X");
                if (wl.Count() != 0)
                {
                    for (int i = 0; i < wl.Count(); i++)
                    {
                        i2++;
                        memory.WriteMemory(wl.ElementAt(i).ToString("X"), "bytes", replace);
                    }
                    k = true;
                }


                if (k == true)
                {
                    Console.Beep(400, 300);
                    //Type Here Code Inject Success Status
                    Sta.Text = "Wallhack Injected";
                    PlaySound();
                }
                else
                {
                    //Type Here Code Inject Faild Status
                    Console.Beep(240, 300);
                    Sta.Text = "Failed";

                }
            }
        }



        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnChooseKey_Click(object sender, EventArgs e)
        {
            waitingForKey = true;
            btnChooseKey.Text = "...";
        }



        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (waitingForKey)
            {
                hotkey = e.KeyCode;
                waitingForKey = false;
                btnChooseKey.Text = "" + hotkey.ToString();
                return;
            }
            if (e.KeyCode == hotkey && hotkey != Keys.None)
            {
                this.Hide();
            }
            if (e.KeyCode == hotkey && hotkey != Keys.None)
            {
                this.Show();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void sniper_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            ShowSniperPanel();

        }
        private void ShowSniperPanel()
        {
           
            SniperPanel.Visible = true;
            SniperPanel.BringToFront();


            PanelMenu.Visible = false;
            PanelVisual.Visible = false;
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
    }



      
    
    
    

