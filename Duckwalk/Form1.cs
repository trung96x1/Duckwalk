using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Duckwalk {
    public partial class Form1 : Form {
        private int distance = 10;
        private bool isAutoRun = false;
        private int sleepTime = 200;
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        Bitmap gifleft = new Bitmap(Application.StartupPath + @"\resources\duckwalk_rotate.gif");
        Bitmap gifright = new Bitmap(Application.StartupPath + @"\resources\duckwalk.gif");


        SoundPlayer sound;
        Thread tCatchMouse;
        Thread tAutoRun;
        Thread tPVLock;
        public Form1() {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            tCatchMouse = new Thread(getCursorPos);
            tCatchMouse.IsBackground = true;
            tCatchMouse.Start();
            this.catchMouseToolStripMenuItem.CheckState = CheckState.Checked;

            tAutoRun = new Thread(autoRun);
            tAutoRun.IsBackground = true;

            tPVLock = new Thread(preventLock);
            tPVLock.IsBackground = true;
            tPVLock.Start();
            this.preventLockToolStripMenuItem.CheckState = CheckState.Checked;

            sound = new SoundPlayer(Application.StartupPath + @"\resources\duck.wav");
            sound.Play();
            sound.PlayLooping();
            
        }
        void getCursorPos() {
            while(true) {
                Point p = GetCursorPosition();
                if(this.Left + distance <= p.X)
                    this.Left += distance;
                if(this.Top + distance <= p.Y)
                    this.Top += distance;
                if(this.Left - distance >= p.X)
                    this.Left -= distance;
                if(this.Top - distance >= p.Y)
                    this.Top -= distance;
                try { //crash when left~X & top~Y
                    if(this.Left <= p.X)
                        pictureBox1.Image = new Bitmap(Application.StartupPath + @"\resources\duckwalk.gif");
                    else
                        pictureBox1.Image = new Bitmap(Application.StartupPath + @"\resources\duckwalk_rotate.gif");
                } catch(Exception e) {

                }
                
                Thread.Sleep(sleepTime);
            }
        }
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos( out Point point );
        public static Point GetCursorPosition() {
            Point point;
            GetCursorPos(out point);
            return point;
        }

        void autoRun() {
            while(true) {
                Random random = new Random();
                int derect = random.Next(1, 8);
                try {
                    if(derect == 1 || derect == 5 || derect == 6)
                        pictureBox1.Image = gifleft;
                    else
                        pictureBox1.Image = gifright;
                } catch(Exception e) {
                    
                }
                for(int i = 0; i < 10; i++) {
                    move(derect);
                    Thread.Sleep(sleepTime);
                }
            }
        }


        void move( int derect ) {
            switch(derect) {
                case 1:     //L
                    this.Left -= distance;
                    if(this.Left <= 0)
                        this.Left = screenWidth;
                    break;
                case 2:     //R
                    this.Left += distance;
                    if(this.Left >= screenWidth)
                        this.Left = 0;
                    break;
                case 3:     //U
                    this.Top += distance;
                    if(this.Top >= screenHeight)
                        this.Top = 0;
                    break;
                case 4:     //D
                    this.Top -= distance;
                    if(this.Top <= 0)
                        this.Top = screenHeight;
                    break;
                case 5:     //LD
                    this.Left -= distance;
                    this.Top -= distance;
                    if(this.Top <= 0)
                        this.Top = screenHeight;
                    if(this.Left <= 0)
                        this.Left = screenWidth;
                    break;
                case 6:     //LU
                    this.Left -= distance;
                    this.Top += distance;
                    if(this.Top >= screenHeight)
                        this.Top = 0;
                    if(this.Left <= 0)
                        this.Left = screenWidth;
                    break;
                case 7:     //RD
                    this.Left += distance;
                    this.Top -= distance;
                    if(this.Top <= 0)
                        this.Top = screenHeight;
                    if(this.Left >= screenWidth)
                        this.Left = 0;
                    break;
                case 8:     //RU
                    this.Left += distance;
                    this.Top += distance;
                    if(this.Left >= screenWidth)
                        this.Left = 0;
                    if(this.Top >= screenHeight)
                        this.Top = 0;
                    break;
            }
        }

        //PREVENT LOCK
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState( EXECUTION_STATE esFlags );
        public enum EXECUTION_STATE : uint {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }
        private void preventLock() {
            while(true) {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
                Thread.Sleep(1000);
            }
        }
        private void exitToolStripMenuItem_Click( object sender, EventArgs e ) {
            Application.Exit();
        }

        private void muteToolStripMenuItem_Click( object sender, EventArgs e ) {
            if(this.muteToolStripMenuItem.Checked)
                sound.Stop();
            else
                sound.PlayLooping();
        }

        private void catchMouseToolStripMenuItem_Click( object sender, EventArgs e ) {
            if(catchMouseToolStripMenuItem.Checked) {
                tCatchMouse.Resume();
                tAutoRun.Suspend();
            } else {
                tCatchMouse.Suspend();
                if(!isAutoRun) {
                    tAutoRun.Start();
                    isAutoRun = true;
                } else
                    tAutoRun.Resume();
            }
        }

        private void preventLockToolStripMenuItem_Click( object sender, EventArgs e ) {
            if(!this.preventLockToolStripMenuItem.Checked)
                tPVLock.Suspend();
            else
                tPVLock.Resume();
        }

        //DRAG FORM FROM PICTURE
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage( IntPtr hWnd, int Msg, int wParam, int lParam );
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void pictureBox1_MouseDown( object sender, MouseEventArgs e ) {
            if(e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
