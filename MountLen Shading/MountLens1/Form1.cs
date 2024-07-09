using AVCapture2CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

using static AVCapture2CSharp.MWCapture;

namespace MountLens1
{
    public partial class MountLensWindow : Form
    {
        protected int m_nImageW = 1280;
        protected int m_nImageH = 720;
        protected double dRatioW, dRatioH;

        const int m_nDefaultChannelIndex = 0;
        DateTime dtStart;
        //DateTime dtEnd;
        FileName fnWindow;

        protected double[] pdSABSum = new double[9];
        protected double[] pdSATSum = new double[9];
        protected double[] pdSALSum = new double[9];

        protected double dBPSum;
        protected double dTPSum;
        protected double dLPSum;

        protected double dDefinitionThresh;
        protected double dFOVThresh;
        protected double dUniformityThresh;
        protected double dUniformityMax;
        protected double dUniformityMin;
        protected double dBrightnessThresh;

        protected double[] dParkDefinateParam = new double[9];
        protected double dDefinateParam = 0.0;


        double dLUAngleSum = 0.0, dRBAngleSum = 0.0, dRUAngleSum = 0.0, dLBAngleSum = 0.0;
        double dMaxUnformitySum = 0.0, dMinUnformitySum = 0.0, dMeanUnformitySum = 0.0;
        double dBrightnessRatioSum;


        UInt32 m_nFrameDuration = 400000;
        UInt32 m_dwFourcc = MWCap_FOURCC.MWCAP_FOURCC_YUY2;

        protected int m_nButtomEnable = 0;

        protected int m_nVideoChannelCount = 0;

        protected int m_nSaveVideoNum = 0;

        protected MWCapture m_MWCapture = null;
        public MWCapture M_MWCAPTURE
        {
            get
            {
                return m_MWCapture;
            }
            set
            {
                m_MWCapture = value;
            }
        }

        protected Thread threadShowData;
        protected bool bShowData = false;
        protected bool bEnhance = false;

        protected FileStream fsResult = null;

        public int m_nFigureDefinitionNo;
        public int m_nFigureFOVNo;
        public int m_nFigureBRNo;
        public int m_nFigureUnformityNo;
        public string strFileName;

        protected int m_nSerialPortNo;
        public int M_NSERIALPORTNO
        {
            get {
                return m_nSerialPortNo;
            }
            set {
                m_nSerialPortNo = value;
            }
        }


        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitializeSerialControl();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseSerialControl();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenSerialPort(int nInputSerialPort);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetGain(int nGV);


        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetExposure(int nExV);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetCommand(string strpcCSharpString);

        ~MountLensWindow() {
            if (fsResult != null) {
                fsResult.Close();
            }

            ReleaseSerialControl();
        }

        public MountLensWindow()
        {
            InitializeComponent();
            dRatioW = Convert.ToDouble(m_nImageW) / pictureBoxImg.Width;
            dRatioH = Convert.ToDouble(m_nImageH) / pictureBoxImg.Height;

            MWCapture.RefreshDevices();
            m_nVideoChannelCount = MWCapture.GetChannelCount();
            m_MWCapture = new MWCapture();

           //DialogResult dr = MessageBox.Show("是否创建文件？", "提示", MessageBoxButtons.YesNo);

           // if (dr == DialogResult.Yes) {
            DateTime now = DateTime.Now;
            strFileName = now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + now.Hour.ToString() + now.Minute.ToString() + ".txt";
            fsResult = new FileStream(strFileName, FileMode.OpenOrCreate, FileAccess.Write);
            fsResult.Close();
            //}

            //m_MWCapture.NPROCESSNO = 2;
            //m_MWCapture.DRATIO = 1.0;
            //m_MWCapture.M_FENHANCELEVEL =5.0f;
            //m_MWCapture.M_FGAMMA = 1.0f;
            //m_MWCapture.SetGammaValue();
            //m_MWCapture.M_FPLUS = 1.0f;
            //m_MWCapture.SetPlusValue();
            //m_MWCapture.M_FGFEPSILON = 10.0f;
            //m_MWCapture.SetGFEpsilon();
            //m_MWCapture.M_NEXPOSURE = 200;
            //m_MWCapture.M_NGAIN = 16;

            //m_MWCapture.M_BENHANCEENABLE = true;
            //m_MWCapture.SetEnhanceEnable();

            //m_MWCapture.M_BENHANCEENABLE = false;
            //m_MWCapture.UnsetEnhanceEnable();

            //m_MWCapture.M_BDENOICEENABLE = true;
            //m_MWCapture.SetDenoiceEnable();
            ////m_MWCapture.M_BGAMMAENHANCE = true;
            ////m_MWCapture.SetGammaEnhance();
            //m_MWCapture.M_BGAMMAENHANCE = false;
            //m_MWCapture.UnsetGammaEnhance();

            //m_MWCapture.M_BGAMMACONTRASTENHANCE = true;
            //m_MWCapture.SetGammaContrastEnhance();
            //m_MWCapture.M_BGUIDEDFILTERENABLE = true;
            //m_MWCapture.SetGuidedFilterEnable();
            //m_MWCapture.M_BDARKREFINE = false;
            //m_MWCapture.UnsetDarkRefineEnable();
            //m_MWCapture.M_BCOLORCORRECTMATRIX = true;
            //m_MWCapture.SetCCMCalibration();

            //m_MWCapture.M_NSATURATION1 = 100;
            //m_MWCapture.SetSaturation1MV(100);
            //m_MWCapture.SetYUVEnhance();
            ////m_MWCapture.M_BHSVALTER = true;
            ////m_MWCapture.SetHSVEnhance();
            ////m_MWCapture.M_BYUVALTER = true;
            ////m_MWCapture.SetYUVEnhance();
            //m_MWCapture.M_BHSVALTER = false;
            //m_MWCapture.UnSetHSVEnhance();
            //m_MWCapture.M_BYUVALTER = false;
            //m_MWCapture.UnSetYUVEnhance();

            ////m_MWCapture.M_NENHANCECHANGEENABLE = true;
            ////m_MWCapture.SetEnhanceChange();
            //m_MWCapture.M_NENHANCECHANGEENABLE = false;
            //m_MWCapture.UnSetEnhanceChange();

            //m_MWCapture.SetEnhanceChangeSigma(1.2f);

            //m_MWCapture.M_BLADJUSTENABLE = true;
            //m_MWCapture.SetLumaAdjustEnable();
            //m_MWCapture.SetLumaAdjustEnhance(10000, 100);
            //m_MWCapture.M_NCOLORMODE = 0;
            //m_MWCapture.SetIHBEnable(0);
            //m_MWCapture.M_BCHANGERGB = false;
            //m_MWCapture.UnSetCRGB();
            //m_MWCapture.M_BSEGMENT = true;
            //m_MWCapture.SetSeg();

            //m_MWCapture.M_BSAVEPROCESSVIDEO = false;
            //m_MWCapture.UnSetSavePV();
            //m_MWCapture.M_BCCMENABLE = false;
            //m_MWCapture.UnsetCCMEn();

            //m_MWCapture.M_FCOLORENHANCEVALUE = 1.6f;
            //m_MWCapture.SetCEV(1.6f);

            double[] pdTempX = new double[2];
            double[] pdTempY = new double[2];

            pdTempX[0] = 0.0;
            pdTempX[1] = 255.0;
            pdTempY[0] = 0.0;
            pdTempY[1] = 255.0;

            m_MWCapture.M_NDESHADINGMODE = 1;
            m_MWCapture.M_DBRTHRESHOLD = 60.0;


            m_nSerialPortNo = InitializeSerialControl();

            if (m_nSerialPortNo >= 0 && m_nSerialPortNo <= 256)
            {
                textBoxSerialPort.Text = m_nSerialPortNo.ToString();
            }
            else {
                textBoxSerialPort.Text = "3";
            }




            //M_MWCAPTURE.SetAnchorPoints(pdTempX, pdTempY, 2);

            //m_MWCapture.M_BCURVEIMAGEENHANCE = true;
            //m_MWCapture.SetCurveImageEnhance();

            //m_MWCapture.M_BCURVEIMAGEENHANCE = false;
            //m_MWCapture.UnSetCurveImageEnhance();
            //m_MWCapture.M_BLGENABLE = true;
            //m_MWCapture.SetLaplacianGaussEnable();

            //m_MWCapture.InitSerialPort();
            //m_MWCapture.SetExposureV(200);
            //m_MWCapture.SetGainV(16);


            if (m_nVideoChannelCount == 0)
            {
                MessageBox.Show(this, "Can't find capture devices!", "AVCapture2CSharp", MessageBoxButtons.OK);
                return;
            }

            for (int i = 0; i < m_nVideoChannelCount; i++)
            {
                LibMWCapture.MWCAP_CHANNEL_INFO channelInfo = new LibMWCapture.MWCAP_CHANNEL_INFO();
                MWCapture.GetChannelInfobyIndex(i, ref channelInfo);

                System.Windows.Forms.ToolStripMenuItem channelToolStripMenuItem = new ToolStripMenuItem();
                channelToolStripMenuItem.Name = "channelToolStripMenuItem" + "1";
                channelToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
                channelToolStripMenuItem.Text = channelInfo.byBoardIndex + ":" + channelInfo.byChannelIndex + " " + channelInfo.szProductName;
                channelToolStripMenuItem.Tag = i;
                channelToolStripMenuItem.Click += new System.EventHandler(this.OnChannelItem);
                toolStripMenuItemDevice.DropDownItems.Add(channelToolStripMenuItem);

                if (i == 0)
                {
                    m_MWCapture.Destory();
                    m_MWCapture.OpenVideoChannel(i, m_dwFourcc, m_nImageW, m_nImageH, m_nFrameDuration, pictureBoxImg.Handle, pictureBoxImg.ClientRectangle);
                }

            }



            string strDefinition = ConfigurationManager.AppSettings["DefinitionThresh"];
            dDefinitionThresh = Convert.ToDouble(strDefinition);

            string strFOVThresh = ConfigurationManager.AppSettings["FOVThresh"];
            dFOVThresh = Convert.ToDouble(strFOVThresh);

            string strUniformityThresh = ConfigurationManager.AppSettings["UniformityThresh"];
            dUniformityThresh = Convert.ToDouble(strUniformityThresh);

            string strUniformityMax = ConfigurationManager.AppSettings["UniformityMax"];
            dUniformityMax = Convert.ToDouble(strUniformityMax);

            string strUniformityMin = ConfigurationManager.AppSettings["UniformityMin"];
            dUniformityMin = Convert.ToDouble(strUniformityMin);

            string strBrightnessThresh = ConfigurationManager.AppSettings["BrightnessThresh"];
            dBrightnessThresh = Convert.ToDouble(strBrightnessThresh);



            //m_MWCapture.M_BOPENLIGHT = true;
            //m_MWCapture.OpenL();
            timer1.Enabled = true;
        }


        private void OnChannelItem(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            int nIndex = Convert.ToInt32(item.Tag);

            timer1.Enabled = false;
            m_MWCapture.Destory();
            m_MWCapture.OpenVideoChannel(nIndex, m_dwFourcc, m_nImageW, m_nImageH, m_nFrameDuration, pictureBoxImg.Handle, pictureBoxImg.ClientRectangle);

            for (int i = 0; i < toolStripMenuItemDevice.DropDownItems.Count; i++)
            {
                ToolStripMenuItem anIntem = (ToolStripMenuItem)toolStripMenuItemDevice.DropDownItems[i];
                anIntem.Checked = false;
            }

            item.Checked = true;
            timer1.Enabled = true;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (checkBox2.Checked)
                {
                    m_MWCapture.M_BDESHADINGENABLE = true;
                    m_MWCapture.M_NIMAGENO = 0;
                }
                else
                {
                    m_MWCapture.M_BDESHADINGENABLE = false;
                    m_MWCapture.M_NIMAGENO = 0;
                }
            }
        }



        private void buttonSaveImg_Click(object sender, EventArgs e)
        {
            m_MWCapture.M_BSAVE = true;
        }



        private void checkBoxDenoice_CheckedChanged(object sender, EventArgs e)
        {
            //if (m_MWCapture.M_BDENOICEENABLE)
            //{
            //    m_MWCapture.M_BDENOICEENABLE = false;
            //    m_MWCapture.UnsetDenoiceEnable();
            //    lock (MWCapture.objFrameNo)
            //    {
            //        m_MWCapture.M_NIMAGENO = 0;
            //    }
            //}
            //else {
            //    m_MWCapture.M_BDENOICEENABLE = true;
            //    m_MWCapture.SetDenoiceEnable();
            //    lock (MWCapture.objFrameNo)
            //    {
            //        m_MWCapture.M_NIMAGENO = 0;
            //    }
            //}
        }

        //private void checkBoxLEnhance_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BGAMMAENHANCE)
        //    {
        //        m_MWCapture.M_BGAMMAENHANCE = false;
        //        //m_MWCapture.UnsetGammaEnhance();
        //        lock (MWCapture.objFrameNo) {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }

        //    }
        //    else {
        //        m_MWCapture.M_BGAMMAENHANCE = true;
        //        //m_MWCapture.SetGammaEnhance();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        //private void checkBoxLocalEnhance_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BGAMMACONTRASTENHANCE)
        //    {
        //        m_MWCapture.M_BGAMMACONTRASTENHANCE = false;
        //        m_MWCapture.UnsetGammaContrastEnhance();
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BGAMMACONTRASTENHANCE = true;
        //        m_MWCapture.SetGammaContrastEnhance();
        //    }
        //}



        //private void checkBox1_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BGUIDEDFILTERENABLE)
        //    {
        //        m_MWCapture.M_BGUIDEDFILTERENABLE = false;
        //        m_MWCapture.UnsetGuidedFilterEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }

        //    }
        //    else
        //    {
        //        m_MWCapture.M_BGUIDEDFILTERENABLE = true;
        //        m_MWCapture.SetGuidedFilterEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }



        //}

        //private void checkBoxDarkRefine_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BDARKREFINE) {
        //        m_MWCapture.M_BDARKREFINE = false;
        //        m_MWCapture.UnsetDarkRefineEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else {
        //        m_MWCapture.M_BDARKREFINE = true;
        //        m_MWCapture.SetDarkRefineEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            //if (textBoxExposure.Text != "")
            //{
            //    int nExposure = Convert.ToInt32(textBoxExposure.Text);
            //    m_MWCapture.M_NEXPOSURE = nExposure;
            //    m_MWCapture.SetExposureV(nExposure);
            //}
        }

        private void textBox1_TextChanged_2(object sender, EventArgs e)
        {
            //if (textBoxGain.Text != "")
            //{
            //    int nGain = Convert.ToInt32(textBoxGain.Text);
            //    m_MWCapture.M_NGAIN = nGain;
            //    m_MWCapture.SetGainV(nGain);
            //}
        }

        private void checkBoxDF_CheckedChanged(object sender, EventArgs e)
        {

        }

        //private void checkBox2_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BOPENLIGHT)
        //    {
        //        m_MWCapture.M_BOPENLIGHT = false;
        //        m_MWCapture.CloseL();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        //m_MWCapture.M_BOPENLIGHT = true;
        //        //m_MWCapture.OpenL();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        //private void buttonCurve_Click(object sender, EventArgs e)
        //{
        //    if (curveWindows != null)
        //    {
        //        if (curveWindows.IsDisposed)
        //        {
        //            curveWindows = new Curve(this);
        //            curveWindows.Show();
        //        }
        //    }
        //    else {
        //        curveWindows = new Curve(this);
        //        curveWindows.Show();
        //    }


        //    //this.Enabled = false;
        //}

        //private void checkBoxCurve_CheckedChanged_1(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BCURVEIMAGEENHANCE)
        //    {
        //        m_MWCapture.M_BCURVEIMAGEENHANCE = false;
        //        m_MWCapture.UnSetCurveImageEnhance();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BCURVEIMAGEENHANCE = true;
        //        m_MWCapture.SetCurveImageEnhance();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        //private void checkBox3_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BCOLORCORRECTMATRIX)
        //    {
        //        m_MWCapture.M_BCOLORCORRECTMATRIX = false;
        //        m_MWCapture.UnSetCCMCalibration();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BCOLORCORRECTMATRIX = true;
        //        m_MWCapture.SetCCMCalibration();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}



        //private void checkBoxCHSV_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BHSVALTER)
        //    {
        //        m_MWCapture.M_BHSVALTER = false;
        //        m_MWCapture.UnSetHSVEnhance();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BHSVALTER = true;
        //        m_MWCapture.SetHSVEnhance();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        //private void checkBoxCYUV_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BYUVALTER)
        //    {
        //        m_MWCapture.M_BYUVALTER = false;
        //        m_MWCapture.UnSetYUVEnhance();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BYUVALTER = true;
        //        m_MWCapture.SetYUVEnhance();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        //private void checkBox4_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BLGENABLE)
        //    {
        //        m_MWCapture.M_BLGENABLE = false;
        //        m_MWCapture.UnSetLaplacianGaussEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BLGENABLE = true;
        //        m_MWCapture.SetLaplacianGaussEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}


        //private void checkBox5_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BCCMENABLE)
        //    {
        //        m_MWCapture.M_BCCMENABLE = false;
        //        m_MWCapture.UnsetCCMEn();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BCCMENABLE = true;
        //        m_MWCapture.SetCCMEn();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        //private void checkBoxLumaAdjust_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BLADJUSTENABLE)
        //    {
        //        m_MWCapture.M_BLADJUSTENABLE = false;
        //        m_MWCapture.UnSetLumaAdjustEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BLADJUSTENABLE = true;
        //        m_MWCapture.SetLumaAdjustEnable();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}




        private void textBox2_TextChanged_2(object sender, EventArgs e)
        {
            //float fEnhanceL = Convert.ToSingle(textBoxEnhanceL.Text);
            //if (fEnhanceL <= 100 && fEnhanceL >= 0)
            //{
            //    trackBarEnhanceL.Value = Convert.ToInt32(fEnhanceL*20.0f);
            //    if (m_MWCapture != null)
            //    {
            //        lock (MWCapture.objEnhance) {
            //            m_MWCapture.M_FENHANCELEVEL = fEnhanceL;
            //        }
            //    }
            //}
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            //float fEnhanceThresh = Convert.ToInt32(textBoxEnhanceThresh.Text);
            //if (fEnhanceThresh <= 255 && fEnhanceThresh >= 0)
            //{
            //    trackBarEnhanceThresh.Value = Convert.ToInt32(fEnhanceThresh * 20.0f);
            //    if (m_MWCapture != null)
            //    {
            //        lock (MWCapture.objEnhance) {
            //            m_MWCapture.M_FENHANCETHRESH = fEnhanceThresh;
            //        }
            //    }
            //}
        }



        //private void checkBoxAlterR_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BCHANGERGB)
        //    {
        //        m_MWCapture.M_BCHANGERGB = false;
        //        m_MWCapture.UnSetCRGB();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BCHANGERGB = true;
        //        m_MWCapture.SetCRGB();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        //private void checkBox6_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BSEGMENT)
        //    {
        //        m_MWCapture.M_BSEGMENT = false;
        //        m_MWCapture.UnSetSeg();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BSEGMENT = true;
        //        m_MWCapture.SetSeg();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            string strStartRecord = buttonStartRecord.Text;

            if (strStartRecord == "开始录像")
            {
                fnWindow = new FileName();
                fnWindow.ShowDialog();


                //if (fnWindow.strName == "")
                //{
                //    m_MWCapture.STRFILENAME = "Video" + m_nSaveVideoNum+".mp4";
                //    m_nSaveVideoNum++;
                //}
                //else {
                //    m_MWCapture.STRFILENAME = fnWindow.strName;
                //}
                m_MWCapture.M_NVIDEOCOUNTER = fnWindow.M_VIDEOCOUNTER;

                m_MWCapture.M_BVIDEOCAPTURE = true;
                m_MWCapture.M_NSAVEFRAMENO = 0;
                buttonStartRecord.Text = "结束录像";
                dtStart = System.DateTime.Now;
                timer2.Enabled = true;
            }
            else if (strStartRecord == "结束录像")
            {
                m_MWCapture.M_BVIDEOCAPTURE = false;
                m_MWCapture.M_NSAVEFRAMENO = 0;
                m_MWCapture.StopVC();
                buttonStartRecord.Text = "开始录像";
                timer2.Enabled = false;
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime dtEnd = System.DateTime.Now;
            TimeSpan tsProcess = dtEnd - dtStart;
            toolStripStatusLabel1.Text = tsProcess.ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (buttonDifinition.Text == "开启近场分辨率测试")
                {
                    if (SetExposure(800) < 0) {
                        MessageBox.Show("系统报告设置失败！", "错误");
                        Thread.Sleep(50);
                    }
                    if (SetGain(20) < 0) {
                        MessageBox.Show("系统报告设置失败！", "错误");
                        Thread.Sleep(50);
                    }

                    m_MWCapture.M_NFIGUREDEFINITION = 1;
                    buttonDifinition.Text = "关闭近场分辨率测试";
                    buttonFarField.Enabled = false;
                    if (m_MWCapture.M_NFIGUREFOV == 1 ||
                        m_MWCapture.M_NFIGUREDEFINITION == 1 ||
                        m_MWCapture.M_NFIGUREBR == 1 ||
                        m_MWCapture.M_NFIGUREUNFORMITY == 1)
                    {
                        timer1.Enabled = true;
                        checkBox1.Checked = true;
                        m_MWCapture.M_BDENOICEENABLE = true;

                    }
                    m_MWCapture.M_NIMAGENO = 0;
                    m_nFigureDefinitionNo = 0;

                    for (int n = 0; n < 9; n++)
                    {
                        pdSABSum[n] = 0.0;
                        pdSATSum[n] = 0.0;
                        pdSALSum[n] = 0.0;
                    }

                    dBPSum = 0.0;
                    dTPSum = 0.0;
                    dLPSum = 0.0;
                }
                else if (buttonDifinition.Text == "关闭近场分辨率测试")
                {
                    m_MWCapture.M_NFIGUREDEFINITION = 0;
                    buttonDifinition.Text = "开启近场分辨率测试";
                    buttonFarField.Enabled = true;
                    if (m_MWCapture.M_NFIGUREFOV == 0 &&
                        m_MWCapture.M_NFIGUREDEFINITION == 0 &&
                        m_MWCapture.M_NFIGUREBR == 0 &&
                        m_MWCapture.M_NFIGUREUNFORMITY == 0)
                    {
                        timer1.Enabled = false;
                        checkBox1.Checked = false;
                        m_MWCapture.M_BDENOICEENABLE = false;
                    }
                    m_MWCapture.M_NIMAGENO = 0;
                }
            }

        }


        double dFormerBP;
        double dFormerTP;
        double dFormerLP;
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            //if (m_MWCapture.M_NFIGUREDEFINITION == 1 ||
            //    m_MWCapture.M_NFIGUREFOV == 1 || 
            //    m_MWCapture.M_NFIGUREUNFORMITY == 1||
            //    m_MWCapture.M_NFIGUREBR == 1) {

            EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
            lock (MWCapture.objEnhance)
            {
                if ((m_MWCapture.M_NFIGUREDEFINITION == 1 && erStatus.nDefinitionNearAlready == 2) ||
                    (m_MWCapture.M_NFIGUREDEFINITION == 1 && erStatus.nDefinitionFarAlready == 2))
                {

                    if (m_nFigureDefinitionNo < 10)
                    {
                        double dBP = m_MWCapture.GetBP();
                        double dTP = m_MWCapture.GetTP();
                        double dLP = m_MWCapture.GetLP();

                        dFormerBP = dBP;
                        dFormerTP = dTP;
                        dFormerLP = dLP;

                        textBox1.Text = dBP.ToString("f2");
                        textBox2.Text = dTP.ToString("f2");
                        textBox3.Text = dLP.ToString("f2");
                        dDefinateParam = 0.2 * dBP + 0.4 * dTP + 0.4 * dLP;
                        textBoxWhole.Text = dDefinateParam.ToString("f2");

                        double[] pdSAB = new double[9];
                        double[] pdSAT = new double[9];
                        double[] pdSAL = new double[9];

                        m_MWCapture.GetSAB(ref pdSAB);
                        m_MWCapture.GetSAT(ref pdSAT);
                        m_MWCapture.GetSAL(ref pdSAL);

                        dBPSum = dBPSum+ dBP;
                        dTPSum = dTPSum + dTP;
                        dLPSum = dLPSum + dLP;

                        for (int n = 0; n < 9; n++) {
                            pdSABSum[n] = pdSABSum[n] + pdSAB[n];
                            pdSATSum[n] = pdSATSum[n] + pdSAT[n];
                            pdSALSum[n] = pdSALSum[n] + pdSAL[n];
                        }

                        textBoxBrenerLT.Text = pdSAB[0].ToString("f2");
                        textBoxSobelLT.Text = pdSAT[0].ToString("f2");
                        textBoxLaplacianLT.Text = pdSAL[0].ToString("f2");
                        dParkDefinateParam[0] = 0.2 * pdSAB[0]+ 0.4* pdSAT[0] + 0.4* pdSAL[0];
                        textBoxLT.Text = dParkDefinateParam[0].ToString("f2");

                        textBoxBrenerT.Text = pdSAB[1].ToString("f2");
                        textBoxSobelT.Text = pdSAT[1].ToString("f2");
                        textBoxLaplacianT.Text = pdSAL[1].ToString("f2");
                        dParkDefinateParam[1] = 0.2 * pdSAB[1] + 0.4 * pdSAT[1] + 0.4 * pdSAL[1];
                        textBoxT.Text = dParkDefinateParam[1].ToString("f2");

                        textBoxBrenerRT.Text = pdSAB[2].ToString("f2");
                        textBoxSobelRT.Text = pdSAT[2].ToString("f2");
                        textBoxLaplacianRT.Text = pdSAL[2].ToString("f2");
                        dParkDefinateParam[2] = 0.2 * pdSAB[2] + 0.4 * pdSAT[2] + 0.4 * pdSAL[2];
                        textBoxRT.Text = dParkDefinateParam[2].ToString("f2");

                        textBoxBrenerL.Text = pdSAB[3].ToString("f2");
                        textBoxSobelL.Text = pdSAT[3].ToString("f2");
                        textBoxLaplacianL.Text = pdSAL[3].ToString("f2");
                        dParkDefinateParam[3] = 0.2 * pdSAB[3] + 0.4 * pdSAT[3] + 0.4 * pdSAL[3];
                        textBoxL.Text = dParkDefinateParam[3].ToString("f2");

                        textBoxBrenerC.Text = pdSAB[4].ToString("f2");
                        textBoxSobelC.Text = pdSAT[4].ToString("f2");
                        textBoxLaplacianC.Text = pdSAL[4].ToString("f2");
                        dParkDefinateParam[4] = 0.2 * pdSAB[4] + 0.4 * pdSAT[4] + 0.4 * pdSAL[4];
                        textBoxCen.Text = dParkDefinateParam[4].ToString("f2");

                        textBoxBrenerR.Text = pdSAB[5].ToString("f2");
                        textBoxSobelR.Text = pdSAT[5].ToString("f2");
                        textBoxLaplacianR.Text = pdSAL[5].ToString("f2");
                        dParkDefinateParam[5] = 0.2 * pdSAB[5] + 0.4 * pdSAT[5] + 0.4 * pdSAL[5];
                        textBoxR.Text = dParkDefinateParam[5].ToString("f2");

                        textBoxBrenerLB.Text = pdSAB[6].ToString("f2");
                        textBoxSobelLB.Text = pdSAT[6].ToString("f2");
                        textBoxLaplacianLB.Text = pdSAL[6].ToString("f2");
                        dParkDefinateParam[6] = 0.2 * pdSAB[6] + 0.4 * pdSAT[6] + 0.4 * pdSAL[6];
                        textBoxLB.Text = dParkDefinateParam[6].ToString("f2");

                        textBoxBrenerB.Text = pdSAB[7].ToString("f2");
                        textBoxSobelB.Text = pdSAT[7].ToString("f2");
                        textBoxLaplacianB.Text = pdSAL[7].ToString("f2");
                        dParkDefinateParam[7] = 0.2 * pdSAB[7] + 0.4 * pdSAT[7] + 0.4 * pdSAL[7];
                        textBoxB.Text = dParkDefinateParam[7].ToString("f2");

                        textBoxBrenerRB.Text = pdSAB[8].ToString("f2");
                        textBoxSobelRB.Text = pdSAT[8].ToString("f2");
                        textBoxLaplacianRB.Text = pdSAL[8].ToString("f2");
                        dParkDefinateParam[8] = 0.2 * pdSAB[8] + 0.4 * pdSAT[8] + 0.4 * pdSAL[8];
                        textBoxRB.Text = dParkDefinateParam[8].ToString("f2");

                        toolStripStatusLabel4.Text = "清晰度计算中";
                    }
                    else {
                        if (m_nFigureDefinitionNo == 10) {
                            dBPSum /= 10.0;
                            dTPSum /= 10.0;
                            dLPSum /= 10.0;
                            dDefinateParam = 0.2 * dBPSum + 0.4 * dTPSum + 0.4 * dLPSum;

                            for (int n = 0; n < 9; n++)
                            {
                                pdSABSum[n] = pdSABSum[n] / 10.0;
                                pdSATSum[n] = pdSATSum[n] / 10.0;
                                pdSALSum[n] = pdSALSum[n] / 10.0;

                                dParkDefinateParam[n] = 0.2 * pdSABSum[n] + 0.4 * pdSATSum[n] + 0.4 * pdSALSum[n];
                            }

                            byte[] strPrompt = System.Text.Encoding.UTF8.GetBytes("清晰度计算结果:\n");
                            fsResult = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dBPSum.ToString() + " ");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dTPSum.ToString() + " ");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dLPSum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dDefinateParam.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);


                            strPrompt = System.Text.Encoding.UTF8.GetBytes("中心视场清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            byte[] strPrompt1;
                            byte[] strPrompt2;
                            byte[] strPrompt3;

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[4].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[4].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[4].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[4].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("左上角清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[0].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[0].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[0].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[0].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("正上清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[1].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[1].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[1].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[1].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("右上角清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[2].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[2].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[2].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[2].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("正左清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[3].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[3].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[3].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[3].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("正右清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[5].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[5].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[5].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[5].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("左下角清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[6].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[6].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[6].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[6].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("正下清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[7].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[7].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[7].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[7].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes("右下角清晰度结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt1 = System.Text.Encoding.UTF8.GetBytes(pdSABSum[8].ToString() + " ");
                            fsResult.Write(strPrompt1, 0, strPrompt1.Length);
                            strPrompt2 = System.Text.Encoding.UTF8.GetBytes(pdSATSum[8].ToString() + " ");
                            fsResult.Write(strPrompt2, 0, strPrompt2.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(pdSALSum[8].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            strPrompt3 = System.Text.Encoding.UTF8.GetBytes(dParkDefinateParam[8].ToString() + "\n");
                            fsResult.Write(strPrompt3, 0, strPrompt3.Length);
                            fsResult.Close();
                        }

                        textBox1.Text = dBPSum.ToString("f2");
                        textBox2.Text = dTPSum.ToString("f2");
                        textBox3.Text = dLPSum.ToString("f2");

                        if (dDefinateParam > dDefinitionThresh)
                        {
                            textBoxWhole.BackColor = Color.Green;
                        }
                        else {
                            textBoxWhole.BackColor = Color.Red;
                        }
                        textBoxWhole.ForeColor = Color.White;
                        textBoxWhole.Text = dDefinateParam.ToString("f2");

                        textBoxBrenerC.Text = pdSABSum[4].ToString("f2");
                        textBoxSobelC.Text = pdSATSum[4].ToString("f2");
                        textBoxLaplacianC.Text = pdSALSum[4].ToString("f2");
                        textBoxCen.ForeColor = Color.White;
                        textBoxCen.Text = dParkDefinateParam[4].ToString("f2");

                        if (dParkDefinateParam[4] > dDefinitionThresh)
                        {
                            textBoxCen.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxCen.BackColor = Color.Red;
                        }

                        textBoxBrenerLT.Text = pdSABSum[0].ToString("f2");
                        textBoxSobelLT.Text = pdSATSum[0].ToString("f2");
                        textBoxLaplacianLT.Text = pdSALSum[0].ToString("f2");
                        textBoxLT.Text = dParkDefinateParam[0].ToString("f2");

                        textBoxLT.ForeColor = Color.White;
                        if (dParkDefinateParam[0] > dDefinitionThresh)
                        {
                            textBoxLT.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxLT.BackColor = Color.Red;
                        }

                        textBoxBrenerT.Text = pdSABSum[1].ToString("f2");
                        textBoxSobelT.Text = pdSATSum[1].ToString("f2");
                        textBoxLaplacianT.Text = pdSALSum[1].ToString("f2");
                        textBoxT.Text = dParkDefinateParam[1].ToString("f2");

                        textBoxT.ForeColor = Color.White;
                        if (dParkDefinateParam[1] > dDefinitionThresh)
                        {
                            textBoxT.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxT.BackColor = Color.Red;
                        }

                        textBoxBrenerRT.Text = pdSABSum[2].ToString("f2");
                        textBoxSobelRT.Text = pdSATSum[2].ToString("f2");
                        textBoxLaplacianRT.Text = pdSALSum[2].ToString("f2");
                        textBoxRT.Text = dParkDefinateParam[2].ToString("f2");

                        textBoxRT.ForeColor = Color.White;
                        if (dParkDefinateParam[2] > dDefinitionThresh)
                        {
                            textBoxRT.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxRT.BackColor = Color.Red;
                        }

                        textBoxBrenerL.Text = pdSABSum[3].ToString("f2");
                        textBoxSobelL.Text = pdSATSum[3].ToString("f2");
                        textBoxLaplacianL.Text = pdSALSum[3].ToString("f2");
                        textBoxL.Text = dParkDefinateParam[3].ToString("f2");

                        textBoxL.ForeColor = Color.White;
                        if (dParkDefinateParam[3] > dDefinitionThresh)
                        {
                            textBoxL.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxL.BackColor = Color.Red;
                        }

                        textBoxBrenerR.Text = pdSABSum[5].ToString("f2");
                        textBoxSobelR.Text = pdSATSum[5].ToString("f2");
                        textBoxLaplacianR.Text = pdSALSum[5].ToString("f2");
                        textBoxR.Text = dParkDefinateParam[5].ToString("f2");

                        textBoxR.ForeColor = Color.White;
                        if (dParkDefinateParam[5] > dDefinitionThresh)
                        {
                            textBoxR.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxR.BackColor = Color.Red;
                        }


                        textBoxBrenerLB.Text = pdSABSum[6].ToString("f2");
                        textBoxSobelLB.Text = pdSATSum[6].ToString("f2");
                        textBoxLaplacianLB.Text = pdSALSum[6].ToString("f2");
                        textBoxLB.Text = dParkDefinateParam[6].ToString("f2");

                        textBoxLB.ForeColor = Color.White;
                        if (dParkDefinateParam[6] > dDefinitionThresh)
                        {
                            textBoxLB.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxLB.BackColor = Color.Red;
                        }

                        textBoxBrenerB.Text = pdSABSum[7].ToString("f2");
                        textBoxSobelB.Text = pdSATSum[7].ToString("f2");
                        textBoxLaplacianB.Text = pdSALSum[7].ToString("f2");
                        textBoxB.Text = dParkDefinateParam[7].ToString("f2");

                        textBoxB.ForeColor = Color.White;
                        if (dParkDefinateParam[7] > dDefinitionThresh)
                        {
                            textBoxB.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxB.BackColor = Color.Red;
                        }

                        textBoxBrenerRB.Text = pdSABSum[8].ToString("f2");
                        textBoxSobelRB.Text = pdSATSum[8].ToString("f2");
                        textBoxLaplacianRB.Text = pdSALSum[8].ToString("f2");
                        textBoxRB.Text = dParkDefinateParam[8].ToString("f2");

                        textBoxRB.ForeColor = Color.White;
                        if (dParkDefinateParam[8] > dDefinitionThresh)
                        {
                            textBoxRB.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxRB.BackColor = Color.Red;
                        }

                        toolStripStatusLabel4.Text = "清晰度计算完成";
                    }

                    m_nFigureDefinitionNo = m_nFigureDefinitionNo+1;
                    if (m_nFigureDefinitionNo == int.MaxValue) {
                        m_nFigureDefinitionNo = 10;
                    }
                }
                else
                {
                    textBox1.Text = " ";
                    textBox2.Text = " ";
                    textBox3.Text = " ";

                    textBoxWhole.Text = " ";
                    textBoxWhole.BackColor = Color.White;
                    textBoxWhole.ForeColor = Color.Black;

                    textBoxBrenerC.Text = " ";
                    textBoxSobelC.Text = " ";
                    textBoxLaplacianC.Text = " ";
                    textBoxCen.Text = " ";
                    textBoxCen.BackColor = Color.White;
                    textBoxCen.ForeColor = Color.Black;

                    textBoxBrenerLT.Text = " ";
                    textBoxSobelLT.Text = " ";
                    textBoxLaplacianLT.Text = " ";
                    textBoxLT.Text = " ";
                    textBoxLT.BackColor = Color.White;
                    textBoxLT.ForeColor = Color.Black;

                    textBoxBrenerT.Text = " ";
                    textBoxSobelT.Text = " ";
                    textBoxLaplacianT.Text = " ";
                    textBoxT.Text = " ";
                    textBoxT.BackColor = Color.White;
                    textBoxT.ForeColor = Color.Black;

                    textBoxBrenerRT.Text = " ";
                    textBoxSobelRT.Text = " ";
                    textBoxLaplacianRT.Text = " ";
                    textBoxRT.Text = " ";
                    textBoxRT.BackColor = Color.White;
                    textBoxRT.ForeColor = Color.Black;

                    textBoxBrenerL.Text = " ";
                    textBoxSobelL.Text = " ";
                    textBoxLaplacianL.Text = " ";
                    textBoxL.Text = " ";
                    textBoxL.BackColor = Color.White;
                    textBoxL.ForeColor = Color.Black;

                    textBoxBrenerR.Text = " ";
                    textBoxSobelR.Text = " ";
                    textBoxLaplacianR.Text = " ";
                    textBoxR.Text = " ";
                    textBoxR.BackColor = Color.White;
                    textBoxR.ForeColor = Color.Black;

                    textBoxBrenerLB.Text = " ";
                    textBoxSobelLB.Text = " ";
                    textBoxLaplacianLB.Text = " ";
                    textBoxLB.Text = " ";
                    textBoxLB.BackColor = Color.White;
                    textBoxLB.ForeColor = Color.Black;

                    textBoxBrenerB.Text = " ";
                    textBoxSobelB.Text = " ";
                    textBoxLaplacianB.Text = " ";
                    textBoxB.Text = " ";
                    textBoxB.BackColor = Color.White;
                    textBoxB.ForeColor = Color.Black;

                    textBoxBrenerRB.Text = " ";
                    textBoxSobelRB.Text = " ";
                    textBoxLaplacianRB.Text = " ";
                    textBoxRB.Text = " ";
                    textBoxRB.BackColor = Color.White;
                    textBoxRB.ForeColor = Color.Black;


                }

                if (m_MWCapture.M_NFIGUREFOV == 1 && erStatus.nFovAlready == 2)
                {
                    if (m_nFigureFOVNo < 10)
                    {
                        double dLUAngle = 0.0, dRBAngle = 0.0, dRUAngle = 0.0, dLBAngle = 0.0;
                        m_MWCapture.GetFOVAngles(ref dLUAngle, ref dRBAngle, ref dRUAngle, ref dLBAngle);

                        dLUAngleSum = dLUAngleSum + dLUAngle;
                        dRBAngleSum = dRBAngleSum + dRBAngle;
                        dRUAngleSum = dRUAngleSum + dRUAngle;
                        dLBAngleSum = dLBAngleSum + dLBAngle;

                        textBoxLUFOV.Text = dLUAngle.ToString("f2");
                        textBoxRBFOV.Text = dRBAngle.ToString("f2");
                        textBoxRTFOV.Text = dRUAngle.ToString("f2");
                        textBoxLBFOV.Text = dLBAngle.ToString("f2");
                        toolStripStatusLabel5.Text = "视场角计算中";
                    }
                    else {
                        if (m_nFigureFOVNo == 10)
                        {
                            dLUAngleSum = dLUAngleSum / 10.0;
                            dRBAngleSum = dRBAngleSum / 10.0;
                            dRUAngleSum = dRUAngleSum / 10.0;
                            dLBAngleSum = dLBAngleSum / 10.0;

                            byte[] strPrompt = System.Text.Encoding.UTF8.GetBytes("视场角测试结果:\n");
                            fsResult = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes("左上视场角:");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dLUAngleSum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes("右下视场角:");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dRBAngleSum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes("右上视场角:");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dRUAngleSum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes("左下视场角:");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dLBAngleSum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            fsResult.Close();
                        }

                        textBoxLUFOV.Text = dLUAngleSum.ToString("f2");
                        textBoxLUFOV.ForeColor = Color.White;
                        if (dLUAngleSum > dFOVThresh)
                        {
                            textBoxLUFOV.BackColor = Color.Green;
                        }
                        else if (Math.Abs(dLUAngleSum - dFOVThresh) < 1e-6)
                        {
                            textBoxLUFOV.BackColor = Color.Yellow;
                        }
                        else {
                            textBoxLUFOV.BackColor = Color.Red;
                        }

                        textBoxRBFOV.Text = dRBAngleSum.ToString("f2");
                        textBoxRBFOV.ForeColor = Color.White;
                        if (dRBAngleSum > dFOVThresh)
                        {
                            textBoxRBFOV.BackColor = Color.Green;
                        }
                        else if (Math.Abs(dRBAngleSum - dFOVThresh) < 1e-6)
                        {
                            textBoxRBFOV.BackColor = Color.Yellow;
                        }
                        else
                        {
                            textBoxRBFOV.BackColor = Color.Red;
                        }

                        textBoxRTFOV.Text = dRUAngleSum.ToString("f2");
                        textBoxRTFOV.ForeColor = Color.White;
                        if (dRUAngleSum > dFOVThresh)
                        {
                            textBoxRTFOV.BackColor = Color.Green;
                        }
                        else if (Math.Abs(dRUAngleSum - dFOVThresh) < 1e-6)
                        {
                            textBoxRTFOV.BackColor = Color.Yellow;
                        }
                        else
                        {
                            textBoxRTFOV.BackColor = Color.Red;
                        }

                        textBoxLBFOV.Text = dLBAngleSum.ToString("f2");
                        textBoxLBFOV.ForeColor = Color.White;
                        if (dLBAngleSum > dFOVThresh)
                        {
                            textBoxLBFOV.BackColor = Color.Green;
                        }
                        else if (Math.Abs(dLBAngleSum - dFOVThresh) < 1e-6)
                        {
                            textBoxLBFOV.BackColor = Color.Yellow;
                        }
                        else
                        {
                            textBoxLBFOV.BackColor = Color.Red;
                        }

                        toolStripStatusLabel5.Text = "视场角计算完成";
                    }

                    m_nFigureFOVNo = m_nFigureFOVNo + 1;
                    if (m_nFigureFOVNo == int.MaxValue)
                    {
                        m_nFigureFOVNo = 10;
                    }
                }
                else
                {
                    textBoxLUFOV.Text = " ";
                    textBoxLUFOV.BackColor = Color.White;
                    textBoxLUFOV.ForeColor = Color.Black;
                    textBoxRBFOV.Text = " ";
                    textBoxRBFOV.BackColor = Color.White;
                    textBoxRBFOV.ForeColor = Color.Black;
                    textBoxRTFOV.Text = " ";
                    textBoxRTFOV.BackColor = Color.White;
                    textBoxRTFOV.ForeColor = Color.Black;
                    textBoxLBFOV.Text = " ";
                    textBoxLBFOV.BackColor = Color.White;
                    textBoxLBFOV.ForeColor = Color.Black;
                }


                if (m_MWCapture.M_NFIGUREUNFORMITY == 1 && erStatus.nUnformityAlready == 2)
                {
                    if (m_nFigureUnformityNo < 10)
                    {
                        double dMaxUnformity = 0.0, dMinUnformity = 0.0, dMeanUnformity = 0.0;
                        m_MWCapture.GetUnformity(ref dMaxUnformity, ref dMinUnformity, ref dMeanUnformity);

                        dMaxUnformitySum += dMaxUnformity;
                        dMinUnformitySum += dMinUnformity;
                        dMeanUnformitySum += dMeanUnformity;

                        textBoxUniformC.Text = dMeanUnformity.ToString("f4");
                        textBoxUniformMax.Text = dMaxUnformity.ToString("f4");
                        textBoxUniformMin.Text = dMinUnformity.ToString("f4");
                        toolStripStatusLabel6.Text = "画面均匀性计算中";
                    }
                    else {
                        if (m_nFigureUnformityNo == 10) {
                            dMaxUnformitySum /= 10.0;
                            dMinUnformitySum /= 10.0;
                            dMeanUnformitySum /= 10.0;

                            byte[] strPrompt = System.Text.Encoding.UTF8.GetBytes("均匀度测试结果:\n");
                            fsResult = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes("最大均匀度:");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dMaxUnformitySum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes("最小均匀度:");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dMinUnformitySum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes("平均均匀度:");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dMeanUnformitySum.ToString() + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            fsResult.Close();
                        }

                        textBoxUniformC.ForeColor = Color.White;
                        if (dMeanUnformitySum > dUniformityThresh)
                        {
                            textBoxUniformC.BackColor = Color.Green;
                        }
                        else {
                            textBoxUniformC.BackColor = Color.Red;
                        }

                        textBoxUniformMax.ForeColor = Color.White;
                        if (dMaxUnformitySum > dUniformityThresh)
                        {
                            textBoxUniformMax.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxUniformMax.BackColor = Color.Red;
                        }

                        textBoxUniformMin.ForeColor = Color.White;
                        if (dMinUnformitySum > dUniformityThresh)
                        {
                            textBoxUniformMin.BackColor = Color.Green;
                        }
                        else
                        {
                            textBoxUniformMin.BackColor = Color.Red;
                        }

                        textBoxUniformC.Text = dMeanUnformitySum.ToString("f4");
                        textBoxUniformMax.Text = dMaxUnformitySum.ToString("f4");
                        textBoxUniformMin.Text = dMinUnformitySum.ToString("f4");
                        toolStripStatusLabel6.Text = "画面均匀性计算完成";
                    }

                    m_nFigureUnformityNo = m_nFigureUnformityNo + 1;
                    if (m_nFigureUnformityNo == int.MaxValue)
                    {
                        m_nFigureUnformityNo = 10;
                    }
                }
                else
                {
                    textBoxUniformC.Text = " ";
                    textBoxUniformMax.Text = " ";
                    textBoxUniformMin.Text = " ";
                }

                if (m_MWCapture.M_NFIGUREBR == 1 && erStatus.nBrAlready == 2)
                {
                    if (m_nFigureBRNo < 10)
                    {
                        double dBrightnessRatio = m_MWCapture.GetBr();
                        textBoxBrightnessRatio.Text = dBrightnessRatio.ToString("f4");

                        dBrightnessRatioSum += dBrightnessRatio;
                        toolStripStatusLabel7.Text = "画面暗区比例计算中";
                    }
                    else {
                        if (m_nFigureBRNo == 10) {
                            fsResult = new FileStream(strFileName, FileMode.Append, FileAccess.Write);
                            dBrightnessRatioSum /= 10.0;
                            byte[] strPrompt = System.Text.Encoding.UTF8.GetBytes("画面暗区测试结果:\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);

                            strPrompt = System.Text.Encoding.UTF8.GetBytes(dBrightnessRatioSum.ToString() + "%" + "\n");
                            fsResult.Write(strPrompt, 0, strPrompt.Length);
                            fsResult.Close();
                        }

                        textBoxBrightnessRatio.ForeColor = Color.White;
                        if (dBrightnessRatioSum > dBrightnessThresh) {
                            textBoxBrightnessRatio.BackColor = Color.Green;
                        }
                        else {
                            textBoxBrightnessRatio.BackColor = Color.Red;
                        }

                        textBoxBrightnessRatio.Text = dBrightnessRatioSum.ToString("f4");
                        toolStripStatusLabel7.Text = "画面暗区比例计算完成";

                    }

                    m_nFigureBRNo = m_nFigureBRNo + 1;
                    if (m_nFigureBRNo == int.MaxValue)
                    {
                        m_nFigureBRNo = 10;
                    }
                }
                else
                {
                    textBoxBrightnessRatio.Text = " ";
                }
            }


            toolStripStatusLabel1.Text = M_MWCAPTURE.TSELAPSED.ToString();

            //}
            lock (MWCapture.objShadingTag)
            {
                if (m_MWCapture.M_BSHADINGALREADY && m_nButtomEnable == 0)
                {
                    buttonShading.Enabled = true;
                    m_nButtomEnable++;
                    toolStripStatusLabel3.Text = "Shading测试完成！";
                }
            }

        }

        private void RadioButtonTF_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (m_MWCapture.M_BFLIPIMAGE)
                {
                    m_MWCapture.M_BFLIPIMAGE = false;
                    m_MWCapture.M_NIMAGENO = 0;
                }
                else
                {
                    m_MWCapture.M_BFLIPIMAGE = true;
                    m_MWCapture.M_NIMAGENO = 0;
                }


            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult drTemp = MessageBox.Show("请确保测试治具背光已经开启，且测试样品处于正常位置并且图像没有过亮！", "提示", MessageBoxButtons.YesNo);

            if (drTemp == DialogResult.Yes)
            {
                m_MWCapture.M_BSHADINGENABLE = true;
                buttonShading.Enabled = false;
                m_nButtomEnable = 0;
                toolStripStatusLabel3.Text = "开始Shading测试，请勿关闭程序！";
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (checkBox1.Checked)
                {
                    m_MWCapture.M_BDENOICEENABLE = true;
                    m_MWCapture.M_NIMAGENO = 0;
                }
                else
                {
                    m_MWCapture.M_BDENOICEENABLE = false;
                    m_MWCapture.M_NIMAGENO = 0;
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (checkBox2.Checked)
                {
                    m_MWCapture.M_BDESHADINGENABLE = true;
                    m_MWCapture.M_NIMAGENO = 0;
                }
                else
                {
                    m_MWCapture.M_BDESHADINGENABLE = false;
                    m_MWCapture.M_NIMAGENO = 0;
                }
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                m_MWCapture.M_BDRAWCROSS = true;
            }
            else {
                m_MWCapture.M_BDRAWCROSS = false;
            }

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                m_MWCapture.M_BDRAWCORNERS = true;
            }
            else
            {
                m_MWCapture.M_BDRAWCORNERS = false;
            }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            int nSerialPort = Convert.ToInt32(textBoxSerialPort.Text);
            if (OpenSerialPort(nSerialPort) < 0)
            {
                MessageBox.Show(this, "当前串口打开失败！");
            }
            else {
                MessageBox.Show(this, "当前串口打开成功！");
            }
        }

        private void MountLensWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReleaseSerialControl();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (buttonFarField.Text == "开启远场分辨率测试")
                {
                    if (SetExposure(780) < 0)
                    {
                        MessageBox.Show("系统报告设置失败！", "错误");
                        Thread.Sleep(50);
                    }
                    if (SetGain(10) < 0)
                    {
                        MessageBox.Show("系统报告设置失败！", "错误");
                        Thread.Sleep(50);
                    }

                    buttonDifinition.Enabled = false;
                    m_MWCapture.M_NFIGUREDEFINITION = 1;
                    buttonFarField.Text = "关闭远场分辨率测试";
                    if (m_MWCapture.M_NFIGUREFOV == 1 ||
                        m_MWCapture.M_NFIGUREDEFINITION == 1 ||
                        m_MWCapture.M_NFIGUREBR == 1 ||
                        m_MWCapture.M_NFIGUREUNFORMITY == 1)
                    {
                        timer1.Enabled = true;
                        checkBox1.Checked = true;
                        m_MWCapture.M_BDENOICEENABLE = true;

                    }
                    m_MWCapture.M_NIMAGENO = 0;
                    m_nFigureDefinitionNo = 0;

                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nDefinitionFarAlready = 1;
                    erStatus.nDefinitionNearAlready = 1;

                    for (int n = 0; n < 9; n++)
                    {
                        pdSABSum[n] = 0.0;
                        pdSATSum[n] = 0.0;
                        pdSALSum[n] = 0.0;
                    }

                    dBPSum = 0.0;
                    dTPSum = 0.0;
                    dLPSum = 0.0;
                }
                else if (buttonFarField.Text == "关闭远场分辨率测试")
                {
                    m_MWCapture.M_NFIGUREDEFINITION = 0;
                    buttonFarField.Text = "开启远场分辨率测试";
                    buttonDifinition.Enabled = true;
                    if (m_MWCapture.M_NFIGUREFOV == 0 &&
                        m_MWCapture.M_NFIGUREDEFINITION == 0 &&
                        m_MWCapture.M_NFIGUREBR == 0 &&
                        m_MWCapture.M_NFIGUREUNFORMITY == 0)
                    {
                        timer1.Enabled = false;
                        checkBox1.Checked = false;
                        m_MWCapture.M_BDENOICEENABLE = false;
                    }
                    m_MWCapture.M_NIMAGENO = 0;
                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nDefinitionFarAlready = 1;
                    erStatus.nDefinitionNearAlready = 1;
                }
            }
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            if (SetExposure(800) < 0)
            {
                MessageBox.Show("系统报告设置失败！", "错误");
                Thread.Sleep(50);
            }
            if (SetGain(20) < 0)
            {
                MessageBox.Show("系统报告设置失败！", "错误");
                Thread.Sleep(50);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (SetExposure(780) < 0)
            {
                MessageBox.Show("系统报告设置失败！", "错误");
                Thread.Sleep(50);
            }
            if (SetGain(10) < 0)
            {
                MessageBox.Show("系统报告设置失败！", "错误");
                Thread.Sleep(50);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (SetExposure(800) < 0)
            {
                MessageBox.Show("系统报告设置失败！", "错误");
                Thread.Sleep(50);
            }
            if (SetGain(20) < 0)
            {
                MessageBox.Show("系统报告设置失败！", "错误");
                Thread.Sleep(50);
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                string strCommand = textBoxCommand.Text;

                SetCommand(strCommand);

            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (checkBox3.Checked)
                {
                    m_MWCapture.M_NDESHADINGMODE = 2;
                    m_MWCapture.M_NIMAGENO = 0;
                }
                else
                {
                    m_MWCapture.M_NDESHADINGMODE = 1;
                    m_MWCapture.M_NIMAGENO = 0;
                }
            }
        }

        private void textBox19_TextChanged(object sender, EventArgs e)
        {
            if (textBoxBRThresh.Text != "")
            {
                m_MWCapture.M_DBRTHRESHOLD = Convert.ToDouble(textBoxBRThresh.Text);
            }

        }

        private void buttonFOV_Click(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (buttonFOV.Text == "开启视场角测试")
                {

                    if (SetExposure(800) < 0)
                    {
                        MessageBox.Show("系统报告设置失败！", "错误");
                        Thread.Sleep(50);
                    }
                    if (SetGain(20) < 0)
                    {
                        MessageBox.Show("系统报告设置失败！", "错误");
                        Thread.Sleep(50);
                    }

                    m_MWCapture.M_NFIGUREFOV = 1;
                    buttonFOV.Text = "关闭视场角测试";
                    if (m_MWCapture.M_NFIGUREFOV == 1 ||
                        m_MWCapture.M_NFIGUREDEFINITION == 1 ||
                        m_MWCapture.M_NFIGUREBR == 1 ||
                        m_MWCapture.M_NFIGUREUNFORMITY == 1)
                    {
                        timer1.Enabled = true;
                        checkBox1.Checked = true;
                        m_MWCapture.M_BDENOICEENABLE = true;
                    }
                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nFovAlready = 1;
                    m_MWCapture.M_NIMAGENO = 0;
                    m_nFigureFOVNo = 0;
                    dLUAngleSum = 0.0;
                    dRBAngleSum = 0.0;
                    dRUAngleSum = 0.0;
                    dLBAngleSum = 0.0;
                }
                else if (buttonFOV.Text == "关闭视场角测试")
                {
                    m_MWCapture.M_NFIGUREFOV = 0;
                    buttonFOV.Text = "开启视场角测试";
                    if (m_MWCapture.M_NFIGUREFOV == 0 &&
                        m_MWCapture.M_NFIGUREDEFINITION == 0 &&
                        m_MWCapture.M_NFIGUREBR == 0 &&
                        m_MWCapture.M_NFIGUREUNFORMITY == 0)
                    {
                        timer1.Enabled = false;
                        checkBox1.Checked = false;
                        m_MWCapture.M_BDENOICEENABLE = false;
                    }

                    m_MWCapture.M_NIMAGENO = 0;
                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nFovAlready = 1;
                }
            }
        }

        private void buttonUnforimity_Click(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (buttonUnforimity.Text == "开启均匀度测试")
                {
                    m_MWCapture.M_NFIGUREUNFORMITY = 1;
                    buttonUnforimity.Text = "关闭均匀度测试";
                    m_MWCapture.M_NFIGUREUNFORMITY = 1;
                    if (m_MWCapture.M_NFIGUREFOV == 1 ||
                    m_MWCapture.M_NFIGUREDEFINITION == 1 ||
                    m_MWCapture.M_NFIGUREBR == 1 ||
                    m_MWCapture.M_NFIGUREUNFORMITY == 1)
                    {
                        timer1.Enabled = true;
                        checkBox1.Checked = true;
                        m_MWCapture.M_BDENOICEENABLE = true;
                    }
                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nUnformityAlready = 1;
                    m_MWCapture.M_NIMAGENO = 0;
                    m_nFigureUnformityNo = 0;
                    dMaxUnformitySum = 0.0;
                    dMinUnformitySum = 0.0;
                    dMeanUnformitySum = 0.0;
                }
                else if (buttonUnforimity.Text == "关闭均匀度测试")
                {
                    m_MWCapture.M_NFIGUREUNFORMITY = 0;
                    buttonUnforimity.Text = "开启均匀度测试";
                    if (m_MWCapture.M_NFIGUREFOV == 0 &&
                        m_MWCapture.M_NFIGUREDEFINITION == 0 &&
                        m_MWCapture.M_NFIGUREBR == 0 &&
                        m_MWCapture.M_NFIGUREUNFORMITY == 0)
                    {
                        timer1.Enabled = false;
                        checkBox1.Checked = false;
                        m_MWCapture.M_BDENOICEENABLE = false;
                    }
                    m_MWCapture.M_NIMAGENO = 0;
                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nUnformityAlready = 1;
                }
            }
        }

        private void buttonBR_Click(object sender, EventArgs e)
        {
            lock (MWCapture.objFrameNo)
            {
                if (buttonBR.Text == "开启暗区面积测试")
                {
                    m_MWCapture.M_NFIGUREBR = 1;
                    buttonBR.Text = "关闭暗区面积测试";
                    if (m_MWCapture.M_NFIGUREFOV == 1 ||
                        m_MWCapture.M_NFIGUREDEFINITION == 1 ||
                        m_MWCapture.M_NFIGUREBR == 1 ||
                        m_MWCapture.M_NFIGUREUNFORMITY == 1)
                    {
                        timer1.Enabled = true;
                        checkBox1.Checked = true;
                        m_MWCapture.M_BDENOICEENABLE = true;
                    }
                    m_MWCapture.M_NIMAGENO = 0;
                    m_nFigureBRNo = 0;
                    dBrightnessRatioSum = 0.0;
                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nDarkAreaAlready = 1;
                }
                else if (buttonBR.Text == "关闭暗区面积测试")
                {
                    m_MWCapture.M_NFIGUREBR = 0;
                    buttonBR.Text = "开启暗区面积测试";
                    if (m_MWCapture.M_NFIGUREFOV == 0 &&
                        m_MWCapture.M_NFIGUREDEFINITION == 0 &&
                        m_MWCapture.M_NFIGUREBR == 0 &&
                        m_MWCapture.M_NFIGUREUNFORMITY == 0)
                    {
                        timer1.Enabled = false;
                        checkBox1.Checked = false;
                        m_MWCapture.M_BDENOICEENABLE = false;
                    }
                    EstimatedResult erStatus = (EstimatedResult)Marshal.PtrToStructure(MWCapture.intptrTemp, typeof(EstimatedResult));
                    erStatus.nDarkAreaAlready = 1;
                    m_MWCapture.M_NIMAGENO = 0;
                }
            }
        }

        //private void checkBox7_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (m_MWCapture.M_BSAVEPROCESSVIDEO)
        //    {
        //        m_MWCapture.M_BSAVEPROCESSVIDEO = false;
        //        m_MWCapture.UnSetSavePV();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //    else
        //    {
        //        m_MWCapture.M_BSAVEPROCESSVIDEO = true;
        //        m_MWCapture.SetSavePV();
        //        lock (MWCapture.objFrameNo)
        //        {
        //            m_MWCapture.M_NIMAGENO = 0;
        //        }
        //    }
        //}

        private void pictureBoxImg_MouseMove(object sender, MouseEventArgs e)
        {
            int nX = Convert.ToInt32(e.Location.X * dRatioW);
            int nY = Convert.ToInt32(e.Location.Y * dRatioH);
            String strPos = nX.ToString() + "," + nY.ToString();

            toolStripStatusLabel2.Text = strPos;
        }
    }
}
