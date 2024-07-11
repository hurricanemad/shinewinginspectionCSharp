using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Data;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Collections;
//using NationalInstruments.UI;

namespace AVCapture2CSharp
{
    public class MWCapture
    {
        //video device 
        protected IntPtr m_hVideoChannel = IntPtr.Zero;
        protected IntPtr m_hD3DRenderer = IntPtr.Zero;
        protected IntPtr m_hAudioRender = IntPtr.Zero;

        //video params
        protected IntPtr m_hWnd = IntPtr.Zero;
        //protected CAPTURE_PARAMS m_capParams;

        // video device index
        protected int m_nCurrentIndex = -1;
        protected Boolean m_bIsCapture = false;

        protected int m_nBoard = 0;
        protected int m_nChannelIndex = 0;

        // capture thread
        // Thread m_capThread = null;
        IntPtr m_hExitEvent = IntPtr.Zero;

        protected IntPtr m_hVideo;
        protected IntPtr m_hAudio;
        LibMWCapture.VIDEO_CAPTURE_STDCALLBACK video_callback;
        LibMWCapture.AUDIO_CAPTURE_STDCALLBACK audio_callback;

        protected static int llCount;
        protected static long m_llCurrentTime;
        protected static long m_llRefTime;
        protected static double m_dfps;

        protected static int m_nImageW;
        protected static int m_nImageH;

        protected unsafe static byte* m_pbBackupImagePtr;

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct EstimatedResult {
            public int nDefinitionFarAlready;
            public int nDefinitionNearAlready;
            public int nFovAlready;
            public int nBrAlready;
            public int nUnformityAlready;
            public int nDarkAreaAlready;
        }

        //protected EstimatedResult erStatus = new EstimatedResult();

        protected EstimatedResult erStatus;
        public static IntPtr intptrTemp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(EstimatedResult)));

        //[DllImport("MANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void ProcessImg(byte* pucImagePtr, int nImageW, int nImageH, int nFrameNo, int nProcessNo, double dRatio);
        //[DllImport("MANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void InitilizeImgPtr(int nImageW, int nImageH, double dRatio);
        //[DllImport("MANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void ReleaseImagePtr();
        //[DllImport("MANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void SaveImage(byte* pucImagePtr, int nImageW, int nImageH, int nNo);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void ProcessImg(byte* pucImagePtr, int nImageW, int nImageH, int nFrameNo, int nProcessNo, float fEnhanceL, float fEnhanceThresh, double dRatio);
        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void InitilizeImgPtr(int nImageW, int nImageH, double dRatio);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern  void InitializeCurvePtsPtr();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void InitializeCCMatrix();

        ////[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        ////public static extern unsafe void InitializeImageNo();
        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void InitilizeSerialPort();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void ReleaseSerialPort();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void ReleaseImagePtr();
        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SaveImage(byte* pucImagePtr, int nImageW, int nImageH, int nNo);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetGamma(float fG);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetCEP(float fPlus);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetGammaCEnhance();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetGammaCEnhance();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetGammaIamge();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetGammaIamge();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetEnhanceIamge();
        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]f
        //public static extern void UnSetEnhanceIamge();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetEnhanceImageChange();
        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetEnhanceImageChange();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetEnhanceSigma(float fEP);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetDenoiceIamge();
        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetDenoiceIamge();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetEpsilon(float fEpsilon);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetGuidedFilter();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetGuidedFilter();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetDarkRefine();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetDarkRefine();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void OpenLight();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void CloseLight();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetExposure(int nExposeV);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetGain(int nGainV);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetHue(int nH);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetSaturation(int nS);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetValue(int nV);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetCurveImage();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetCurveImage();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetCCMConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetCCMConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern unsafe void SetCurvePts(double* pdCXs, double* pdCYs, int nCPtSz);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetHSVConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetHSVConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetYUVConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetYUVConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetLaplaceGauss();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetLaplaceGauss();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetMatrixConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetMatrixConvert();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetSaturation1M(int nS);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetLumaAdjust();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetLumaAdjust();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetLumaAdjustValue(Int64 lBThresh, Int64 lDThresh);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int GetExposure();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int GetGain();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetIHBMode(int nIHb);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetChangeRGB();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetChangeRGB();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetSegment();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetSegment();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartVideoCapture(int nVideoCounter, int nFrameRate, int nWidth, int nHeight);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopVideoCapture();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void SaveVideo(byte* pucImagePtr, int nImageW, int nImageH);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void FigureParameters(byte* pucImagePtr, int nImageW, int nImageH, int nFrameNo, 
                                                                                         double dRatio, int nEnFilter, double dThresh, int nEnDefinition, 
                                                                                         int nEnFov, int nEnBR, int nEnUnformit, IntPtr intptrTemp);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetBrenner();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetTenegrad();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetLaplacian();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetBrighnessRatio();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitializeImage(int nImageW, int nImageH, double dRatio);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseImage();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void FigureCross(byte* pucImagePtr, int nImageW, int nImageH);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void FigureCorners(byte* pucImagePtr, int nImageW, int nImageH);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe double* GetSubAreaBrenner();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe double* GetSubAreaTenegrad();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe double* GetSubAreaLaplacian();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void FlipImage(byte* pucImagePtr, int nImageW, int nImageH);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPosLeftFOV();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPosRightFOV();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNegLeftFOV();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNegRightFOV();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe double* GetUnformityVs();

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void LensShading(byte* pbImagePtr, int nImageWidth, int nImageHeight);

        [DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void DeShading(byte* pbImagePtr, int nImageWidth, int nImageHeight, int nMode);

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetSaveProcessVideo();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetSaveProcessVideo();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetCCMEnable();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void UnSetCCMEnable();

        //[DllImport("CUDAMANRDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetColorEnhanceV(float fCEV);

        static protected double m_dBRThreshold;
        public double M_DBRTHRESHOLD
        {
            get {
                return m_dBRThreshold;
            }
            set {
                m_dBRThreshold = value;
            }
        } 

        static protected bool m_bShadingEnable;
        public bool M_BSHADINGENABLE
        {
            set {
                m_bShadingEnable = value;
            }

            get {
                return m_bShadingEnable;
            }
        }

        static protected bool m_bShadingAlready;
        public bool M_BSHADINGALREADY
        {
            set {
                m_bShadingAlready = value;
            }
            get {
                return m_bShadingAlready;
            }
        }


        static protected int m_nVideoCounter;
        public int M_NVIDEOCOUNTER
        {
            set
            {
                m_nVideoCounter = value;
            }
            get
            {
                return m_nVideoCounter;
            }
        }

        protected static float m_fGamma;
        public float M_FGAMMA
        {
            get
            {
                return m_fGamma;
            }
            set
            {
                m_fGamma = value;
            }
        }

        protected static float m_fPlus;
        public float M_FPLUS
        {
            get
            {
                return m_fPlus;
            }
            set
            {
                m_fPlus = value;
            }
        }

        protected static float m_fGFEpsilon;
        public float M_FGFEPSILON
        {
            get
            {
                return m_fGFEpsilon;
            }
            set
            {
                m_fGFEpsilon = value;
            }
        }

        protected static float m_fColorEnhanceV;
        public float M_FCOLORENHANCEVALUE
        {
            get
            {
                return m_fColorEnhanceV;
            }
            set
            {
                m_fColorEnhanceV = value;
            }
        }

        protected static int m_nColorMode;
        public int M_NCOLORMODE
        {
            get
            {
                return m_nColorMode;
            }
            set
            {
                m_nColorMode = value;
            }
        }

        protected static bool m_bDarkRefine;
        public bool M_BDARKREFINE
        {
            get
            {
                return m_bDarkRefine;
            }
            set
            {
                m_bDarkRefine = value;
            }
        }

        protected static bool m_bCurveImageEnhance;
        public bool M_BCURVEIMAGEENHANCE
        {
            get
            {
                return m_bCurveImageEnhance;
            }
            set
            {
                m_bCurveImageEnhance = value;
            }
        }

        protected static bool m_bGammaEnhance;
        public bool M_BGAMMAENHANCE
        {
            get
            {
                return m_bGammaEnhance;
            }
            set
            {
                m_bGammaEnhance = value;
            }
        }

        protected static bool m_bGammaContrastEnhance;
        public bool M_BGAMMACONTRASTENHANCE
        {
            get
            {
                return m_bGammaContrastEnhance;
            }
            set
            {
                m_bGammaContrastEnhance = value;
            }
        }

        protected static bool m_bColorCorrectMatrix;
        public bool M_BCOLORCORRECTMATRIX
        {
            get
            {
                return m_bColorCorrectMatrix;
            }
            set
            {
                m_bColorCorrectMatrix = value;
            }
        }

        protected static bool m_bSave;
        public bool M_BSAVE
        {
            get
            {
                return m_bSave;
            }
            set
            {
                m_bSave = value;
            }
        }

        protected static bool m_bVideoCapture;
        public bool M_BVIDEOCAPTURE
        {
            get
            {
                return m_bVideoCapture;
            }
            set
            {
                m_bVideoCapture = value;
            }
        }

        protected static bool m_bDrawCross;
        public bool M_BDRAWCROSS
        {
            get
            {
                return m_bDrawCross;
            }
            set
            {
                m_bDrawCross = value;
            }
        }

        protected static bool m_bDrawCorners;
        public bool M_BDRAWCORNERS
        {
            get
            {
                return m_bDrawCorners;
            }
            set
            {
                m_bDrawCorners = value;
            }
        }

        protected static bool m_bGuidedFilterEnable;
        public bool M_BGUIDEDFILTERENABLE
        {
            get
            {
                return m_bGuidedFilterEnable;
            }
            set
            {
                m_bGuidedFilterEnable = value;
            }
        }

        protected static bool m_bEnhanceEnable;
        public bool M_BENHANCEENABLE
        {
            get
            {
                return m_bEnhanceEnable;
            }
            set
            {
                m_bEnhanceEnable = value;
            }
        }

        protected static bool m_bEnhanceChangeEnable;
        public bool M_NENHANCECHANGEENABLE
        {
            get
            {
                return m_bEnhanceChangeEnable;
            }
            set
            {
                m_bEnhanceChangeEnable = value;
            }
        }

        protected static bool m_bDenoiceEnable;
        public bool M_BDENOICEENABLE
        {
            get
            {
                return m_bDenoiceEnable;
            }
            set
            {
                m_bDenoiceEnable = value;
            }
        }

        protected static int m_nDeShadingMode;
        public int M_NDESHADINGMODE
        {
            get {
                return m_nDeShadingMode;
            }

            set {
                m_nDeShadingMode = value;
            }
        }


        protected static bool m_bDeShadingEnable;
        public bool M_BDESHADINGENABLE
        {
            get
            {
                return m_bDeShadingEnable;
            }
            set
            {
                m_bDeShadingEnable = value;
            }
        }

        protected static bool m_bSaveProcessVideo;
        public bool M_BSAVEPROCESSVIDEO
        {
            get
            {
                return m_bSaveProcessVideo;
            }
            set
            {
                m_bSaveProcessVideo = value;
            }
        }

        protected static bool m_bProcessData;
        public bool M_BPROCESSDATA
        {
            get
            {
                return m_bProcessData;
            }
            set
            {
                m_bProcessData = value;
            }
        }

        protected static bool m_bFlipImage;
        
        public bool M_BFLIPIMAGE
        {
            get {
                return m_bFlipImage;
            }

            set {
                m_bFlipImage = value;
            }
        }

        protected static int m_nFigureDefinition;
        public int M_NFIGUREDEFINITION
        {
            get
            {
                return m_nFigureDefinition;
            }
            set
            {
                m_nFigureDefinition = value;
            }
        }

        protected static int m_nFigureFOV;
        public int M_NFIGUREFOV
        {
            get
            {
                return m_nFigureFOV;
            }
            set
            {
                m_nFigureFOV = value;
            }
        }

        protected static int m_nFigureUnformity;
        public int M_NFIGUREUNFORMITY
        {
            get
            {
                return m_nFigureUnformity;
            }
            set
            {
                m_nFigureUnformity = value;
            }
        }

        protected static int m_nFigureBR;
        public int M_NFIGUREBR
        {
            get
            {
                return m_nFigureBR;
            }
            set
            {
                m_nFigureBR = value;
            }
        }

        protected static bool m_bOpenLight;
        public bool M_BOPENLIGHT
        {
            get
            {
                return m_bOpenLight;
            }
            set
            {
                m_bOpenLight = value;
            }
        }

        protected static bool m_bChangeRGB;
        public bool M_BCHANGERGB
        {
            get
            {
                return m_bChangeRGB;
            }
            set
            {
                m_bChangeRGB = value;
            }
        }

        protected static bool m_bSegment;
        public bool M_BSEGMENT
        {
            get
            {
                return m_bSegment;
            }
            set
            {
                m_bSegment = value;
            }
        }

        protected static bool m_bHSVAlter;
        public bool M_BHSVALTER
        {
            get
            {
                return m_bHSVAlter;
            }
            set
            {
                m_bHSVAlter = value;
            }
        }

        protected static bool m_bYUVAlter;
        public bool M_BYUVALTER
        {
            get
            {
                return m_bYUVAlter;
            }
            set
            {
                m_bYUVAlter = value;
            }
        }

        protected static bool m_bLGEnable;
        public bool M_BLGENABLE
        {
            get
            {
                return m_bLGEnable;
            }
            set
            {
                m_bLGEnable = value;
            }
        }

        protected static bool m_bLAdjustEnable;
        public bool M_BLADJUSTENABLE
        {
            get
            {
                return m_bLAdjustEnable;
            }
            set
            {
                m_bLAdjustEnable = value;
            }
        }

        protected static bool m_bCCMEnable;
        public bool M_BCCMENABLE
        {
            get
            {
                return m_bCCMEnable;
            }
            set
            {
                m_bCCMEnable = value;
            }
        }

        protected static int m_nExposure;
        public int M_NEXPOSURE
        {
            get
            {
                return m_nExposure;
            }
            set
            {
                m_nExposure = value;
            }
        }

        protected static int m_nGain;
        public int M_NGAIN
        {
            get
            {
                return m_nGain;
            }
            set
            {
                m_nGain = value;
            }
        }

        protected static TimeSpan tsElapsed;
        public TimeSpan TSELAPSED
        {
            set {
                tsElapsed = value;
            }
            get {
                return tsElapsed;
            }
        }



        protected static volatile float m_fEnhanceLevel;
        public float M_FENHANCELEVEL
        {
            get
            {
                return m_fEnhanceLevel;
            }
            set
            {
                m_fEnhanceLevel = value;
            }
        }

        protected static volatile float m_fEnhanceThresh;
        public float M_FENHANCETHRESH
        {
            get
            {
                return m_fEnhanceThresh;
            }
            set
            {
                m_fEnhanceThresh = value;
            }
        }

        protected static int m_nSaveImageNo;
        protected static int m_nSaveVideoCaptureNo;

        protected static volatile int m_nImageNo;
        public int M_NIMAGENO
        {
            get
            {
                return m_nImageNo;
            }
            set
            {
                m_nImageNo = value;
            }
        }

        protected static int m_nSaveFrameNo;

        public int M_NSAVEFRAMENO
        {
            get
            {
                return m_nSaveFrameNo;
            }
            set
            {
                m_nSaveFrameNo = value;
            }
        }

        protected static volatile int m_nHue;
        public int M_NHUE
        {
            get
            {
                return m_nHue;
            }
            set
            {
                m_nHue = value;
            }
        }

        protected static volatile int m_nSaturation;
        public int M_NSATURATION
        {
            get
            {
                return m_nSaturation;
            }
            set
            {
                m_nSaturation = value;
            }
        }


        protected static volatile int m_nValue;
        public int M_NVALUE
        {
            get
            {
                return m_nValue;
            }
            set
            {
                m_nValue = value;
            }
        }

        protected static volatile int m_nSaturation1;
        public int M_NSATURATION1
        {
            get
            {
                return m_nSaturation1;
            }
            set
            {
                m_nSaturation1 = value;
            }
        }

        protected Int64 m_lBUBrightThresh = 100;
        public Int64 M_LBUBRIGHTTHRESH
        {
            get
            {
                return m_lBUBrightThresh;
            }
            set
            {
                m_lBUBrightThresh = value;
            }
        }

        protected Int64 m_lBUDarkThresh = 100;
        public Int64 M_LBUDARKTHRESH
        {
            get
            {
                return m_lBUDarkThresh;
            }
            set
            {
                m_lBUDarkThresh = value;
            }
        }

        //protected static object objTemp = new object();
        //public static object objShow = new object();
        public static object objFrameNo = new object();
        public static object objEnhance = new object();
        public static object objAnchorPoint = new object();
        public static object objShadingTag = new object();

        protected static Thread threadProcessImage = null;

        protected static bool bProcessImg = true;



        protected static bool bStartProcess = false;

        public bool BSTARTPROCESS
        {
            get
            {
                return bStartProcess;
            }
            set
            {
                bStartProcess = value;
            }
        }


        protected static int nProcessNo;
        public int NPROCESSNO
        {
            get
            {
                return nProcessNo;
            }
            set
            {
                nProcessNo = value;
            }
        }

        protected static double dRatio;
        public double DRATIO
        {
            get
            {
                return dRatio;
            }
            set
            {
                dRatio = value;
            }
        }

        public double GetBP()
        {
            return GetBrenner();
        }

        public double GetTP()
        {
            return GetTenegrad();
        }

        public double GetLP()
        {
            return GetLaplacian();
        }

        //protected double[,] pdAnchorPoints = new double[16,2];
        protected double[] pdAnchorPointsX = new double[16];
        protected double[] pdAnchorPointsY = new double[16];
        protected int m_nValidNum = 0;

        protected static Thread threadShading;

        //public void SetAnchorPoints(double[] dX, double[] dY, int nVN) {

        //    double[] pdTempX = new double[nVN];
        //    double[] pdTempY = new double[nVN];
        //    lock (objAnchorPoint) {
        //        for (int n = 0; n < nVN; n++)
        //        {
        //            pdAnchorPointsX[n] = dX[n];
        //            pdAnchorPointsY[n] = dY[n];
        //            pdTempX[n] = dX[n];
        //            pdTempY[n] = dY[n];
        //        }
        //        m_nValidNum = nVN;

        //        unsafe {
        //            fixed (double* pdCXs = pdTempX, pdCYs = pdTempY) {
        //                SetCurvePts(pdCXs, pdCYs, m_nValidNum);
        //            }
        //        }

        //    }
        //}

        public static void ShadingFunc() {
            unsafe {
                LensShading(m_pbBackupImagePtr, 1280, 720);
            }


            m_bShadingAlready = true;
        }

        //public void GetAnchorPoints(ref XYPointAnnotation[] xypaAnchorPoints, ref int nVN)
        //{
        //    lock (objAnchorPoint)
        //    {
        //        for (int n = 0; n < 16; n++)
        //        {
        //            xypaAnchorPoints[n].XPosition = pdAnchorPointsX[n];
        //            xypaAnchorPoints[n].YPosition = pdAnchorPointsY[n];
        //        }
        //        nVN = m_nValidNum;
        //    }
        //}

        public static void video_callback_sub(IntPtr pbFrame, int cbFrame, ulong u64TimeStamp, IntPtr pParam)
        {
            byte[] m_pbImagePtr = new byte[m_nImageW * m_nImageH * 4];
            Marshal.Copy(pbFrame, m_pbImagePtr, 0, m_nImageW * m_nImageH * 2);
            //if (m_bProcessData)
            //{
            //    lock (objFrameNo)
            //    {
            //        lock (objEnhance) {
            //            unsafe
            //            {
            //                fixed (byte* pbTempImagePtr = m_pbImagePtr)
            //                {
            //                    ProcessImg(pbTempImagePtr, m_nImageW, m_nImageH, m_nImageNo, nProcessNo, m_fEnhanceThresh, m_fEnhanceLevel, dRatio);
            //                    m_nImageNo++;
            //                }
            //            }
            //        }
            //    }
            //    Marshal.Copy(m_pbImagePtr, 0, pbFrame, m_nImageW * m_nImageH * 2);
            //}
            DateTime dtStart = DateTime.Now;

            if (m_bShadingEnable) {
                lock (objShadingTag) {
                    m_bShadingAlready = false;
                }

                unsafe {
                    fixed (byte* pbTempImagePtr = m_pbImagePtr) {
                        m_pbBackupImagePtr = pbTempImagePtr;
                    }
                
                }

                ThreadStart tsShading = new ThreadStart(ShadingFunc);

                threadShading = new Thread(tsShading);

                threadShading.Start();
                
                m_bShadingEnable = false;
            }

            if (m_bDeShadingEnable) {
                unsafe
                {
                    fixed (byte* pbTempImagePtr = m_pbImagePtr)
                    {
                        DeShading(pbTempImagePtr, m_nImageW, m_nImageH, m_nDeShadingMode);
                    }
                }
            }

            if (m_bFlipImage) {

                unsafe
                {
                    fixed (byte* pbTempImagePtr = m_pbImagePtr)
                    {
                        FlipImage(pbTempImagePtr, m_nImageW, m_nImageH);
                    }
                }
            }

            if (m_nFigureDefinition == 1 || m_nFigureFOV == 1 || m_nFigureUnformity == 1 || m_nFigureBR == 1)
            {
                lock (objFrameNo)
                {
                    lock (objEnhance)
                    {
                        unsafe
                        {   
                            fixed (byte* pbTempImagePtr = m_pbImagePtr)
                            {
                                if (m_bDenoiceEnable)
                                {
                                    FigureParameters(pbTempImagePtr, m_nImageW, m_nImageH, m_nImageNo, 1.0, 1, m_dBRThreshold, 
                                                                  m_nFigureDefinition, m_nFigureFOV, m_nFigureBR, m_nFigureUnformity, intptrTemp);
                                }
                                else
                                {
                                    FigureParameters(pbTempImagePtr, m_nImageW, m_nImageH, m_nImageNo, 1.0, 0, m_dBRThreshold, 
                                                                 m_nFigureDefinition, m_nFigureFOV, m_nFigureBR, m_nFigureUnformity, intptrTemp);
                                }

                                m_nImageNo++;
                            }
                        }
                    }
                }
                Marshal.Copy(m_pbImagePtr, 0, pbFrame, m_nImageW * m_nImageH * 2);
            }


            if (m_bSave)
            {
                unsafe
                {
                    fixed (byte* pbTempImagePtr1 = m_pbImagePtr)
                    {

                        SaveImage(pbTempImagePtr1, m_nImageW, m_nImageH, m_nSaveImageNo);


                        m_nSaveImageNo++;
                    }
                }
                m_bSave = false;
            }

            if (m_bVideoCapture)
            {
                if (m_nSaveFrameNo == 0)
                {
                    StartVideoCapture(m_nVideoCounter, 30, m_nImageW, m_nImageH);
                    m_nSaveVideoCaptureNo++;
                }
                unsafe
                {
                    fixed (byte* pbTempImagePtr1 = m_pbImagePtr)
                    {
                        SaveVideo(pbTempImagePtr1, m_nImageW, m_nImageH);
                        m_nSaveFrameNo++;
                    }
                }
            }

            if (m_bDrawCross) {
                unsafe
                {
                    fixed (byte* pbTempImagePtr1 = m_pbImagePtr)
                    {
                        FigureCross(pbTempImagePtr1, m_nImageW, m_nImageH);
                    }
                }
            }

            if (m_bDrawCorners) {
                unsafe
                {
                    fixed (byte* pbTempImagePtr1 = m_pbImagePtr)
                    {
                        FigureCorners(pbTempImagePtr1, m_nImageW, m_nImageH);
                    }
                }
            }

            DateTime dtEnd = DateTime.Now;
            tsElapsed = dtEnd - dtStart;
            

            Marshal.Copy(m_pbImagePtr, 0, pbFrame, m_nImageW * m_nImageH * 2);
            LibMWMedia.MWD3DRendererPushFrame(pParam, pbFrame, m_nImageH * 4);

            llCount += 1;
            if (llCount >= 10)
            {
                m_llCurrentTime = Libkernel32.GetTickCount();
                m_dfps = (llCount * 1000) / (m_llCurrentTime - m_llRefTime);
                m_llRefTime = m_llCurrentTime;
                llCount = 0;
            }
        }


        public static void audio_callback_sub(IntPtr pbFrame, int cbFrame, ulong u64TimeStamp, IntPtr pParam)
        {
            LibMWMedia.MWDSoundRendererPushFrame(pParam, pbFrame, cbFrame);
        }

        public MWCapture()
        {
            m_nImageNo = 0;
            m_nSaveImageNo = 0;
            m_nSaveVideoCaptureNo = 0;
            m_bProcessData = false;
            m_bSave = false;
            m_bVideoCapture = false;
            nProcessNo = 1;
            dRatio = 1.0;
            m_bEnhanceEnable = true;
            m_fEnhanceLevel = 0.0f;
            m_bGammaEnhance = true;
            m_bGammaContrastEnhance = true;
            m_bGammaContrastEnhance = true;
            m_bDarkRefine = true;

            erStatus = (EstimatedResult)Marshal.PtrToStructure(intptrTemp, typeof(EstimatedResult));
            erStatus.nBrAlready = 0;
            erStatus.nDefinitionFarAlready = 0;
            erStatus.nDefinitionNearAlready = 0;
            erStatus.nFovAlready = 0;
            erStatus.nUnformityAlready = 0;
        }

        static public void Init()
        {
            LibMWCapture.MWCaptureInitInstance();

        }

        static public void Exit()
        {
            LibMWCapture.MWCaptureExitInstance();
        }

        static public Boolean RefreshDevices()
        {
            LibMWCapture.MW_RESULT mr;
            mr = LibMWCapture.MWRefreshDevice();
            if (mr != LibMWCapture.MW_RESULT.MW_SUCCEEDED)
                return false;

            return true;
        }

        static public int GetChannelCount()
        {
            return LibMWCapture.MWGetChannelCount();
        }

        static public void GetChannelInfobyIndex(int nChannelIndex, ref LibMWCapture.MWCAP_CHANNEL_INFO channelInfo)
        {
            int iSize = Marshal.SizeOf(typeof(LibMWCapture.MWCAP_CHANNEL_INFO));
            IntPtr pChannelInfo = Marshal.AllocCoTaskMem(iSize);
            LibMWCapture.MWGetChannelInfoByIndex(nChannelIndex, pChannelInfo);
            channelInfo = (LibMWCapture.MWCAP_CHANNEL_INFO)Marshal.PtrToStructure(pChannelInfo, typeof(LibMWCapture.MWCAP_CHANNEL_INFO));
            Marshal.FreeCoTaskMem(pChannelInfo);

            return;
        }

        public void GetSAB(ref double[] pdBrenners) {
            unsafe {
                double* pdSABPtr = GetSubAreaBrenner();

                if (pdSABPtr != null) {
                    for (int n = 0; n < 9; n++)
                    {
                        pdBrenners[n] = pdSABPtr[n];
                    }
                }
            }
        }

        public void GetSAT(ref double[] pdTenegrads)
        {
            unsafe
            {
                double* pdSATPtr = GetSubAreaTenegrad();

                if (pdSATPtr != null)
                {
                    for (int n = 0; n < 9; n++)
                    {
                        pdTenegrads[n] = pdSATPtr[n];
                    }
                }
            }
        }

        public void GetUnformity(ref double dMaxUnformity, ref double dMinUnformity, ref double dMeanUnformity) {

            unsafe {
                dMaxUnformity = 0.0;
                dMinUnformity = 1.0;
                dMeanUnformity = 0.0;
                double* pdUnformityVs = GetUnformityVs();

                if (pdUnformityVs != null) {
                    for (int n = 0; n < 8; n++)
                    {
                        if (dMaxUnformity < pdUnformityVs[n])
                        {
                            dMaxUnformity = pdUnformityVs[n];
                        }
                        if (dMinUnformity > pdUnformityVs[n])
                        {
                            dMinUnformity = pdUnformityVs[n];
                        }
                        dMeanUnformity += pdUnformityVs[n];
                    }

                    dMeanUnformity /= 8.0;
                }
            }
            dMaxUnformity *= 100;
            dMinUnformity *= 100;
            dMeanUnformity *= 100;
        }

        public void GetSAL(ref double[] pdLaplacians)
        {
            unsafe
            {
                double* pdSALPtr = GetSubAreaLaplacian();

                if (pdSALPtr != null) {
                    for (int n = 0; n < 9; n++)
                    {
                        pdLaplacians[n] = pdSALPtr[n];
                    }
                }
            }
        }

        public void GetFOVAngles(ref double dLeftTopAngle, ref double dRightBottomAngle, ref double dRightUpAngle, ref double dLeftBottomAngle) {
            int nPLFOV = GetPosLeftFOV();
            int nPRFOV = GetPosRightFOV();
            int nNLFOV = GetNegLeftFOV();
            int nNRFOV = GetNegRightFOV();

            double dPLFOV = 0.0;
            double dPRFOV = 0.0;
            double dNLFOV = 0.0;
            double dNRFOV = 0.0;

            if (nPLFOV == 7)
            {
                dPLFOV = 7.5;
            }
            else if (nPLFOV == 8) {
                dPLFOV = 8.2;
            }
            else
            {
                dPLFOV = nPLFOV;
            }

            if (nPRFOV == 7)
            {
                dPRFOV = 7.5;
            }
            else if (nPRFOV == 8)
            {
                dPRFOV = 8.2;
            }
            else
            {
                dPRFOV = nPRFOV;
            }

            if (nNLFOV == 7)
            {
                dNLFOV = 7.5;
            }
            else if (nNLFOV == 8)
            {
                dNLFOV = 8.2;
            }
            else
            {
                dNLFOV = nNLFOV;
            }

            if (nNRFOV == 7)
            {
                dNRFOV = 7.5;
            }
            else if (nNRFOV == 8)
            {
                dNRFOV = 8.2;
            }
            else
            {
                dNRFOV = nNRFOV;
            }

            dLeftTopAngle = 20 + dPLFOV * 10;
            dRightBottomAngle = 20 + dNRFOV * 10;
            dRightUpAngle = 20 + dPRFOV * 10;
            dLeftBottomAngle = 20 + dNLFOV * 10;

        }

        public double GetBr() {
            return GetBrighnessRatio();
        }

        //public void SetGammaValue()
        //{
        //    SetGamma(m_fGamma);
        //}

        //public void SetPlusValue()
        //{
        //    SetCEP(m_fPlus);
        //}

        //public void SetGFEpsilon()
        //{
        //    SetEpsilon(m_fGFEpsilon);
        //}

        //public void SetGammaEnhance()
        //{
        //    SetGammaIamge();
        //}

        //public void UnsetGammaEnhance()
        //{
        //    UnSetGammaIamge();
        //}


        //public void SetGuidedFilterEnable()
        //{
        //    SetGuidedFilter();
        //}

        //public void UnsetGuidedFilterEnable()
        //{
        //    UnSetGuidedFilter();
        //}

        //public void SetGammaContrastEnhance()
        //{
        //    SetGammaCEnhance();
        //}

        //public void UnsetGammaContrastEnhance()
        //{
        //    UnSetGammaCEnhance();
        //}

        //public void SetEnhanceEnable()
        //{
        //    SetEnhanceIamge();
        //}

        //public void UnsetEnhanceEnable()
        //{
        //    UnSetEnhanceIamge();
        //}

        //public void SetDenoiceEnable()
        //{
        //    SetDenoiceIamge();
        //}

        //public void UnsetDenoiceEnable()
        //{
        //    UnSetDenoiceIamge();
        //}

        //public void SetDarkRefineEnable()
        //{
        //    SetDarkRefine();
        //}

        //public void UnsetDarkRefineEnable()
        //{
        //    UnSetDarkRefine();
        //}


        //public void SetCurveImageEnhance()
        //{
        //    SetCurveImage();
        //}

        //public void UnSetCurveImageEnhance()
        //{
        //    UnSetCurveImage();
        //}


        //public void SetCCMCalibration()
        //{
        //    SetCCMConvert();
        //}

        //public void UnSetCCMCalibration()
        //{
        //    UnSetCCMConvert();
        //}

        //public void SetCCMEn()
        //{
        //    SetCCMEnable();
        //}

        //public void UnsetCCMEn()
        //{
        //    UnSetCCMEnable();
        //}

        //public void SetHSVEnhance()
        //{
        //    SetHSVConvert();
        //}

        //public void UnSetHSVEnhance()
        //{
        //    UnSetHSVConvert();
        //}

        //public void SetYUVEnhance()
        //{
        //    SetYUVConvert();
        //}

        //public void UnSetYUVEnhance()
        //{
        //    UnSetYUVConvert();
        //}

        //public void SetMatrixConvertEnhance()
        //{
        //    SetMatrixConvert();
        //}

        //public void UnSetMatrixConvertEnhance()
        //{
        //    UnSetMatrixConvert();
        //}

        //public void SetLumaAdjustEnable()
        //{
        //    SetLumaAdjust();
        //}

        //public void UnSetLumaAdjustEnable()
        //{
        //    UnSetLumaAdjust();
        //}

        //public void SetEnhanceChange()
        //{
        //    SetEnhanceImageChange();
        //}

        //public void UnSetEnhanceChange()
        //{
        //    UnSetEnhanceImageChange();
        //}

        //public void SetSeg()
        //{
        //    SetSegment();
        //}

        //public void UnSetSeg()
        //{
        //    UnSetSegment();
        //}

        //public void SetEnhanceChangeSigma(float fSigma)
        //{
        //    SetEnhanceSigma(fSigma);
        //}

        //public void OpenL() {
        //    OpenLight();
        //}

        //public void SetExposureV(int nE) {
        //    SetExposure(nE);
        //}

        //public void SetGainV(int nG)
        //{
        //    SetGain(nG);
        //}

        //public void SetHueV(int nHV)
        //{
        //    SetHue(nHV);
        //}

        //public void SetSaturationV(int nSV)
        //{
        //    SetSaturation(nSV);
        //}

        //public void SetSaturation1MV(int nSV)
        //{
        //    SetSaturation1M(nSV);
        //}

        //public void SetValueV(int nVV)
        //{
        //    SetValue(nVV);
        //}

        //public void SetCEV(float fCEV)
        //{
        //    SetColorEnhanceV(fCEV);
        //}

        public void StopVC()
        {
            StopVideoCapture();
        }

        //public void SetSavePV() {
        //    SetSaveProcessVideo();
        //}

        //public void UnSetSavePV()
        //{
        //    UnSetSaveProcessVideo();
        //}

        //public int GetExposureValue()
        //{
        //    return GetExposure();
        //}

        //public int GetGainValue()
        //{
        //    return GetGain();
        //}

        //public void SetLumaAdjustEnhance(Int64 lBThresh, Int64 lDThresh)
        //{
        //    SetLumaAdjustValue(lBThresh, lDThresh);
        //}

        //public void SetLaplacianGaussEnable()
        //{
        //    SetLaplaceGauss();
        //}

        //public void UnSetLaplacianGaussEnable()
        //{
        //    UnSetLaplaceGauss();
        //}

        //public void SetIHBEnable(int nIHb)
        //{
        //    SetIHBMode(nIHb);
        //}


        //public void SetCRGB()
        //{
        //    SetChangeRGB();
        //}

        //public void UnSetCRGB()
        //{
        //    UnSetChangeRGB();
        //}

        //public void CloseL() {
        //    CloseLight();
        //}

        //public void InitSerialPort() {
        //    InitilizeSerialPort();
        //}

        public Boolean OpenVideoChannel(int nChannelIndex, UInt32 dwFourcc, int cx, int cy, UInt32 nFrameDuration, IntPtr hWnd, Rectangle rcPanel)
        {
            // open video device
            LibMWCapture.MW_RESULT mr;

            m_nImageW = cx;
            m_nImageH = cy;

            //InitilizeImgPtr(m_nImageW, m_nImageH, dRatio);
            //InitializeImageNo();
            //InitilizeSerialPort();


            int iSize = Marshal.SizeOf(typeof(LibMWCapture.MWCAP_CHANNEL_INFO));
            IntPtr pChannelInfo = Marshal.AllocCoTaskMem(iSize);
            mr = LibMWCapture.MWGetChannelInfoByIndex(nChannelIndex, pChannelInfo);
            LibMWCapture.MWCAP_CHANNEL_INFO channelInfo = (LibMWCapture.MWCAP_CHANNEL_INFO)Marshal.PtrToStructure(pChannelInfo, typeof(LibMWCapture.MWCAP_CHANNEL_INFO));
            Marshal.FreeCoTaskMem(pChannelInfo);

            ushort[] wpath = new ushort[512];
            IntPtr pwpath = GCHandle.Alloc(wpath, GCHandleType.Pinned).AddrOfPinnedObject();
            LibMWCapture.MWGetDevicePath(nChannelIndex, pwpath);

            m_nBoard = channelInfo.byBoardIndex;
            m_nChannelIndex = channelInfo.byChannelIndex;

            m_hVideoChannel = LibMWCapture.MWOpenChannelByPath(pwpath);
            if (m_hVideoChannel == IntPtr.Zero)
                return false;

            video_callback = new LibMWCapture.VIDEO_CAPTURE_STDCALLBACK(video_callback_sub);
            // create video renderer

            bool t_b_reverse = false;
            if (dwFourcc == MWCap_FOURCC.MWCAP_FOURCC_BGR24 || dwFourcc == MWCap_FOURCC.MWCAP_FOURCC_BGRA)
            {
                t_b_reverse = true;
            }
            m_hD3DRenderer = LibMWMedia.MWCreateD3DRenderer(cx, cy, dwFourcc, t_b_reverse, hWnd);

            if (m_hD3DRenderer == IntPtr.Zero)
            {
                return false;
            }

            llCount = 0;
            m_llCurrentTime = m_llRefTime = Libkernel32.GetTickCount();
            uint fourcc = (uint)dwFourcc;
            int frameduration = (int)nFrameDuration;
            m_hVideo = LibMWCapture.MWCreateVideoCaptureWithStdCallBack(m_hVideoChannel, cx, cy, fourcc, frameduration, video_callback, m_hD3DRenderer);

            if (m_hVideo == IntPtr.Zero)
            {
                return false;
            }

            audio_callback = new LibMWCapture.AUDIO_CAPTURE_STDCALLBACK(audio_callback_sub);
            m_hAudioRender = LibMWMedia.MWCreateDSoundRenderer(48000, 2, 480, 10);
            if (m_hAudioRender == IntPtr.Zero)
            {
                return false;
            }
            m_hAudio = LibMWCapture.MWCreateAudioCaptureWithStdCallBack(m_hVideoChannel, LibMWCapture.MWCAP_AUDIO_CAPTURE_NODE.MWCAP_AUDIO_CAPTURE_NODE_DEFAULT, 48000, 16, 2, audio_callback, m_hAudioRender);
            if (m_hAudio == IntPtr.Zero)
            {
                return false;
            }

            int n;
            pdAnchorPointsX[0] = 0.0;
            pdAnchorPointsY[0] = 0.0;
            pdAnchorPointsX[1] = 255.0;
            pdAnchorPointsY[1] = 255.0;
            for (n = 2; n < 16; n++)
            {
                pdAnchorPointsX[n] = -10.0;
                pdAnchorPointsY[n] = -10.0;
            }
            m_nValidNum = 2;
            //InitializeCurvePtsPtr();
            //InitializeCCMatrix();
            InitializeImage(1280, 720, 1.0);
            //unsafe
            //{
            //    fixed (double* pdTempX = pdAnchorPointsX, dTempY = pdAnchorPointsY)
            //    {
            //        SetAnchorPoints(pdAnchorPointsX, pdAnchorPointsY, m_nValidNum);
            //    }
            //}


            return true;
        }

        public void Destory()
        {
            if (m_hVideo != IntPtr.Zero)
            {
                LibMWCapture.MWDestoryVideoCapture(m_hVideo);
                m_hVideo = IntPtr.Zero;
            }

            if (m_hAudio != IntPtr.Zero)
            {
                LibMWCapture.MWDestoryAudioCapture(m_hAudio);
                m_hAudio = IntPtr.Zero;
            }

            if (m_hVideoChannel != IntPtr.Zero)
            {
                LibMWCapture.MWCloseChannel(m_hVideoChannel);
                m_hVideoChannel = IntPtr.Zero;
            }

            if (m_hD3DRenderer != IntPtr.Zero)
            {
                LibMWMedia.MWDestroyD3DRenderer(m_hD3DRenderer);
                m_hD3DRenderer = IntPtr.Zero;
            }

            if (m_hAudioRender != IntPtr.Zero)
            {
                LibMWMedia.MWDestroyDSoundRenderer(m_hAudioRender);
                m_hAudioRender = IntPtr.Zero;
            }

            if (threadProcessImage != null)
            {
                bProcessImg = false;
                threadProcessImage.Join();
                threadProcessImage = null;
            }

            ReleaseImage();
            //ReleaseImagePtr();
            //ReleaseSerialPort();
        }

        public double GetFps()
        {
            return m_dfps;
        }


    }
}
