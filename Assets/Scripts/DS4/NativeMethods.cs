/********************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION This software is supplied under the 
terms of a license agreement or nondisclosure agreement with Intel Corporation 
and may not be copied or disclosed except in accordance with the terms of that 
agreement.
Copyright(c) 2011-2014 Intel Corporation. All Rights Reserved.

*********************************************************************************/
using System;
using System.Runtime.InteropServices;

class NativeMethods
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern void OutputDebugString(string message);

    ////////////////////////////////////////////////////////////////////////////////////////

    // Import from camera input module
    [DllImport("DFCameraDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool DFCAM_initCamera([In] int camType, [In] int resX, [In] int resY);

    [DllImport("DFCameraDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DFCAM_getDefaultCamIntrinsics([Out] float[] m_camParams);

    [DllImport("DFCameraDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int DFCAM_acquireRGB([In] string inputSrc, [Out] byte[] colorImage);

    [DllImport("DFCameraDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int DFCAM_acquireDImage([In] string inputSrc, [Out] ushort[] depthImage);

    [DllImport("DFCameraDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DFCAM_releaseCamera();

    ////////////////////////////////////////////////////////////////////////////////////////

    // Import from depth fusion module

    // Defined in the library:
    // Return status of functions.
    //typedef enum SP_STATUS 
    //{
    //    SP_SUCCESS = 0, SP_FAILURE = 1, SP_ERROR = 2, SP_ACCUMULATE = 3, SP_INVALIDARG = 4
    //} SP_STATUS;

    // Downscale/resize factor for image size. 
    // Speed is maximum at DOWNSCALE_BY_FOUR and minimum at DOWNSCALE_NONE.
    //enum DOWNSCALE 
    //{ 
    //    DOWNSCALE_NONE = 1, 
    //    DOWNSCALE_BY_TWO = 2, 
    //    DOWNSCALE_BY_FOUR = 4
    //};

    ////////////////////////////////////////////////////////////////////////////////////////

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SP_STATUS SP_setConfiguration([In] IntPtr pCamParam, [In] SP_Resolution resolution, [In] float dimen);

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SP_STATUS SP_getPoseMatrix([Out] float[] curPose);

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SP_STATUS SP_setPoseMatrix([In] float[] pose);

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SP_STATUS SP_doTracking([In] ushort[] depthSrc, [In] byte[] rgbSrc = null, [In] float[] pGravity = null, [In] bool doSceneReconstruction = true);

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SP_STATUS SP_init([In] float[] initPose, [In] ushort[] depthSrc,
        [In] byte[] rgbSrc = null);

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SP_STATUS SP_reset([In] float[] resetPose = null, [In] ushort[] depthSrc = null,
        [In] byte[] rgbSrc = null);

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern SP_STATUS SP_getMeshPieces([In] IntPtr pMeshes, [In] IntPtr pMeshVertices, [In] IntPtr pMeshFaces, [In] IntPtr accumDiffThresholds,
        [Out] IntPtr numMeshes, [Out] IntPtr numVertices, [Out] IntPtr numFaces);

    [DllImport("DepthFusionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SP_release();

    ////////////////////////////////////////////////////////////////////////////////////////

    [DllImport("DFInertialMotionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int MOTION_initIMU([In] SensorType type, [In] int windowSize);

    [DllImport("DFInertialMotionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool MOTION_getGravity([In] SensorType type, [In] IntPtr pGravity, [In] IntPtr pTimeStamp);

    [DllImport("DFInertialMotionDll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void MOTION_releaseIMU();
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SP_CameraIntrinsics
{
    public UInt32 imageWidth;
    public UInt32 imageHeight;
    public float focalLengthHorizontal;
    public float focalLengthVertical;
    public float principalPointCoordU;
    public float principalPointCoordV;
}

public enum SP_Resolution
{
    SP_LOW_RESOLUTION = 128, SP_HIGH_RESOLUTION = 256
}

public enum SP_STATUS
{
    SP_SUCCESS = 0, 
    SP_FAILURE, 
    SP_ERROR, 
    SP_ACCUMULATE, 
    SP_INVALIDARG, 
    SP_SUCCESS_LOW_ACCURACY, 
    SP_INIT, 
    DF_SUCCESS_HIGH_ACCURACY, 
    DF_NOT_CONFIGURED, 
    DF_NOT_INITIALIZED
}

enum DOWNSCALE
{
    DOWNSCALE_NONE = 1,
    DOWNSCALE_BY_TWO = 2,
    DOWNSCALE_BY_FOUR = 4
};

enum SensorType
{
    HILLCREST = 0,
    PLATFORM = 1
};