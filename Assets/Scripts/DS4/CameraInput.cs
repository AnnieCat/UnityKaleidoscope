/********************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION This software is supplied under the 
terms of a license agreement or nondisclosure agreement with Intel Corporation 
and may not be copied or disclosed except in accordance with the terms of that 
agreement.
Copyright(c) 2011-2014 Intel Corporation. All Rights Reserved.

*********************************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class CameraInput : MonoBehaviour
{
    private	byte[] 		m_colorImg; //!< holds rgb data of input camera
    public	byte[] 		GetColorImage ()
    {
        return m_colorImg;
    }
    
    private	ushort[]	m_depthImg; //!< holds depth data of input camera
    public	ushort[]	GetDepthImage ()
    {
        return m_depthImg;
    }

    private ushort[]    m_depthImgFlipped;
    
    private	int			m_imgW; //!< image width in pixels
    public 	int			GetImageWidth ()
    {
        return m_imgW;
    }
    
    private int			m_imgH; //!< image height in pixels
    public 	int 		GetImageHeight ()
    {
        return m_imgH;
    }
        
    private bool		m_doSceneReconstruction; //!< whether to accumulate current depth data
    public 	int 		GetDFrameCount ()
    {
        return m_reconstructionCounter;
    }
    
    private int 		m_reconstructionCounter; //!< how many depth image so far
    private int 		m_reconstructionRate; //!< accumulation frequency

    private float[]		m_poseMatrix; //!< camera pose 6 DOF in matrix	
    public 	float[] 	GetCameraPoseMatrix ()
    {
        return m_poseMatrix;
    }
    
    private float[] 	m_camParams; //!< camera intrinsic parameters
    private SP_CameraIntrinsics camParams;
    private Matrix4x4 	m_camParamsMatrix; //!< camera intrinsic in matrix
    
    private Matrix4x4 	m_orthMatrix; //!< orthographic matrix
    
    //! internal configuration params include: 
    //! volume size: volume resolution in pixels in each dimension (x, y , z axes).
    //! volume size in meters, set for all dimensions.
    //! downscale factor: scale division factor of depth input image, applied to each dimension.
    //! width and height: sizes of depth input image.
    //! camID : ID for camera type, is 3 for TYZX camera.
    private int[] 		m_camConfig;
    public int[]        GetCameraConfig()
    {
        return m_camConfig; // variables: downscale factor of camera image, width, height, camID (3=TYZX, 0=Kinect)
    }

    private Vector3 	m_avgCamPosition; //!< running average of camera position
    private Quaternion 	m_avgCamRotation; //!< running average of camera rotation

    private int 		m_volSize = 256; //!< accumulation 3D volume's dimension size
    public 	int 		getVolSize ()
    {
        return m_volSize;
    }
    
    // Parameters for OFFLINE_CAM
    string m_sourceFile;
    string m_datasetFolderName;
    string m_sequenceFolderName;
    string m_sequenceName;
    string m_configurationFilename;
    List<String> m_configurationFileData = new List<string>();
    private TextReader m_rgbDepthFileReader;
    private int m_frameCount;    // TODO: Make private, made public for debugging

    private const int VOLUME_SIZE = 4;
    float[] DEFAULT_INITIAL_TRANSLATION = new float[] { VOLUME_SIZE / 2.0f, VOLUME_SIZE / 2.0f, 0.0f };

    private bool m_newRGBImageAvailable;

    private void Awake()
    {
        //set camera input type
        //for TYZX camera, downscale factor is either 1 or 2.
        int[] PRIMESENSE_CAM = new int[5] {4, 2, 320, 240, 2}; //volume size, downscale factor, width, height, camID
        int[] OFFLINE_CAM = new int[5] {4, 2, 640, 480, 4};
        int[] TYZX_DS4_CAM = new int[5] {4, 2, 640, 480, 6};

        m_camConfig = TYZX_DS4_CAM;

        try
        {
            if (m_camConfig[2]/m_camConfig[1] <= 80)
            {
                throw new Exception("Downscale factor is too large.");
            }
        }
        catch (Exception)
        {
            throw;
        }

        //depth fusion configuration	
        m_imgW = m_camConfig[2];
        m_imgH = m_camConfig[3];
        m_doSceneReconstruction = true;
        m_reconstructionCounter = 0;
        m_reconstructionRate = 1;
        m_camParams = new float[4];
        m_poseMatrix = new float[12];
        m_colorImg = new byte[m_imgW*m_imgH*3];
        m_depthImg = new ushort[m_imgW*m_imgH];
        m_depthImgFlipped = new ushort[m_imgW*m_imgH];
        camParams = new SP_CameraIntrinsics();
        m_frameCount = 0;

        //camera configuration			
        m_avgCamPosition = Vector3.zero;
        m_avgCamRotation = Quaternion.identity;
        m_newRGBImageAvailable = false;

    }

    void Start()
    {
        // initialize camera input
        if (!NativeMethods.DFCAM_initCamera(m_camConfig[4], m_imgW, m_imgH))
        {
            NativeMethods.OutputDebugString("error - can not initialize camera.");
        }

        // get default camera intrinsic parameters from camera input's module
        NativeMethods.DFCAM_getDefaultCamIntrinsics(m_camParams);

        // Setup the parameters for OFFLINE_CAM
        if (m_camConfig[4] == 4)
        {
            m_datasetFolderName =
                @".\Dataset";

            m_sequenceFolderName = m_datasetFolderName + Path.DirectorySeparatorChar;
            m_configurationFilename = m_sequenceFolderName + "DatasetParameters.txt";
            using (TextReader configFileReader = File.OpenText(m_configurationFilename))
            {
                m_configurationFileData.AddRange(configFileReader.ReadLine().Split(' '));
                configFileReader.Dispose();
            }

            using (TextReader camParamsReader = File.OpenText(m_sequenceFolderName + m_configurationFileData[3]))
            {
                string[] tmp = camParamsReader.ReadLine().Split(' ');
                camParams.imageWidth = UInt32.Parse(tmp[0]);
                camParams.imageHeight = UInt32.Parse(tmp[1]);
                m_camParams[0] = camParams.focalLengthHorizontal = float.Parse(tmp[2]);
                m_camParams[1] = camParams.focalLengthVertical = float.Parse(tmp[3]);
                m_camParams[2] = camParams.principalPointCoordU = float.Parse(tmp[4]);
                m_camParams[3] = camParams.principalPointCoordV = float.Parse(tmp[5]);
                camParamsReader.Dispose();
            }

            m_rgbDepthFileReader = File.OpenText(m_sequenceFolderName + m_configurationFileData[0]);
        }
        else
        {
            camParams.imageWidth = (UInt32) m_imgW;
            camParams.imageHeight = (UInt32) m_imgH;
            camParams.focalLengthHorizontal = m_camParams[0];
            camParams.focalLengthVertical = m_camParams[1];
            camParams.principalPointCoordU = m_camParams[2];
            camParams.principalPointCoordV = m_camParams[3];
        }

        // Initialize the IMU
        NativeMethods.MOTION_initIMU(SensorType.HILLCREST, 1);
        
        GCHandle camParamsHandle = GCHandle.Alloc(camParams, GCHandleType.Pinned);
        SP_STATUS status = NativeMethods.SP_setConfiguration(
            camParamsHandle.AddrOfPinnedObject(),
            SP_Resolution.SP_HIGH_RESOLUTION, // Should be read in not hard coded!!
            VOLUME_SIZE //volume dimension
            );
        camParamsHandle.Free();

        if (status != 0) {
            NativeMethods.OutputDebugString ("error - invalid configuration setting: " + status);
        }	
        
        m_camParamsMatrix = new Matrix4x4 ();
        
        //update unity's camera intrinsic parameters. 
        SetUnityCamParamsMatrix (m_camParams);

        float[] camDefaultPoseMatrix = new float[]{ 
            1.0f,0.0f,0.0f, (float)m_camConfig[0]/2,
            0.0f,1.0f,0.0f, (float)m_camConfig[0]/2,
            0.0f,0.0f,1.0f,0.0f
        }; 
    
        // override default initial pose 
        NativeMethods.SP_setPoseMatrix(camDefaultPoseMatrix);
        
        // initialize camera's pose
        NativeMethods.SP_getPoseMatrix(m_poseMatrix);
        
        // set projection matrix of camera
        ConfigureAugmentedCamera();
    }
    
    //! redefine main camera's projection matrix, overriding unity's default.
    //! projection matrix is calculated based on orthographic matrix and 
    //! camera's intrinsic parameter matrix. 
    void ConfigureAugmentedCamera ()
    {
        //derive orthographic matrix m_orthMatrix
        CalOrthographicMat ();
        
        Matrix4x4 camProjMatrix = new Matrix4x4 ();	
        
        camProjMatrix = m_orthMatrix * m_camParamsMatrix; 
                    
        Camera.main.projectionMatrix = camProjMatrix;		
    }
    
    void OnApplicationQuit ()
    {
        NativeMethods.DFCAM_releaseCamera();
        NativeMethods.SP_release();
        NativeMethods.MOTION_releaseIMU();
    }
    
    void Update ()
    {
        ProcessKeyInput();
        m_newRGBImageAvailable = false;
        string rgbSourcePath = null;
        string depthSourcePath = null;

        if (m_camConfig[4] == 4)    // For offline sequence
        {
            string strFileRgbDepth = "";
            if ((strFileRgbDepth = m_rgbDepthFileReader.ReadLine()) != null)
            {
                List<String> tokens = new List<string>(strFileRgbDepth.Split(' '));
                if (tokens.Count != 4)
                    return;

                rgbSourcePath = m_sequenceFolderName + tokens[1];
                depthSourcePath = m_sequenceFolderName + tokens[3];
            }
            else
            {
                return;
            }
        }

        // get camera's RGB image
        if (NativeMethods.DFCAM_acquireRGB(rgbSourcePath, m_colorImg) != 0)
        {
            NativeMethods.OutputDebugString("error in getting RGB data");
        }
        m_newRGBImageAvailable = true;

        // get camera's depth image
        if (NativeMethods.DFCAM_acquireDImage(depthSourcePath, m_depthImg) != 0)
        {
            NativeMethods.OutputDebugString("error in getting depth data");
        }

       
        //feed depth fusion module with (flipped) depth image
        FlipDepthImage(ref m_depthImg,ref m_depthImgFlipped, m_imgH, m_imgW);

        float[] gravity = new float[3];
        double timestamp = 0.0;
        float[] initPoseMatrix = new float[12];

        if (!GetNormalizedGravityIfAvailable(ref gravity, ref timestamp))
        {
            gravity = null;
        }

        if (m_frameCount == 0)  // Only on the first frame
        {
            if ((gravity != null) && CreatePoseMatrixFromRow(ref initPoseMatrix, ref gravity, DEFAULT_INITIAL_TRANSLATION, 2))
            {
                NativeMethods.SP_init(initPoseMatrix, m_depthImgFlipped, m_colorImg);
            }
            else
            {
                NativeMethods.SP_init(null, m_depthImgFlipped, m_colorImg);
            }
        }

        //only accumulate new depth data into volume at some frequencies. 
        m_doSceneReconstruction = !((m_reconstructionCounter++) % m_reconstructionRate != 0);

        // track depth fusion camera's position and optionally accumulate depth image
        NativeMethods.SP_doTracking(m_depthImgFlipped, m_colorImg, gravity, m_doSceneReconstruction);
        
        //get depth fusion camera's pose
        NativeMethods.SP_getPoseMatrix(m_poseMatrix);
        m_frameCount++;
        //NativeMethods.OutputDebugString("FrameCount: " + m_frameCount);
    }
    
    void ProcessKeyInput ()
    {
        //reset depth fusion
        if (Input.GetKeyDown (KeyCode.R)) {

            float[] gravity = new float[] { 0, 1, 0 };
            double timestamp = 0.0;
            float[] initPoseMatrix = new float[12];

            if (GetNormalizedGravityIfAvailable(ref gravity, ref timestamp) &&
                CreatePoseMatrixFromRow(ref initPoseMatrix, ref gravity, DEFAULT_INITIAL_TRANSLATION, 2))
            {
                NativeMethods.SP_reset(initPoseMatrix);
            }
            else
            {
                NativeMethods.SP_reset();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.Quit();
        }	
    }
    
    //! update unity's main camera pose based on depth fusion's camera's pose matrix
    //! updating is based on running average of main camera's positions and rotations (coming)
    public void UpdateMainCamPose ()
    {		
        //use running average for locations to reduce jittering. 
        if (m_avgCamPosition == Vector3.zero) {
            m_avgCamPosition = new Vector3 (m_poseMatrix [3], m_poseMatrix [7], m_poseMatrix [11]);
        }
        else
        {
            m_avgCamPosition = (m_avgCamPosition + new Vector3(m_poseMatrix[3], m_poseMatrix[7], m_poseMatrix[11])) / 2;
        }
        
        Camera.main.transform.localPosition = m_avgCamPosition;

        Matrix4x4 rotMat = new Matrix4x4 ();
        rotMat.SetRow(0, new Vector4(m_poseMatrix[0], m_poseMatrix[1], m_poseMatrix[2], 0));
        rotMat.SetRow(1, new Vector4(m_poseMatrix[4], m_poseMatrix[5], m_poseMatrix[6], 0));
        rotMat.SetRow(2, new Vector4(m_poseMatrix[8], m_poseMatrix[9], m_poseMatrix[10], 0));
        rotMat.m33 = 1;	

        Quaternion curCamRot = RotationMatrixToQuaterion (rotMat);
        //use running "average" for rotations to reduce jittering
        if (m_avgCamRotation == Quaternion.identity)
        {
            m_avgCamRotation = curCamRot;
        }
        else
        {
            m_avgCamRotation = Quaternion.Lerp(m_avgCamRotation, curCamRot, 0.75f);
        }

        Camera.main.transform.localRotation = m_avgCamRotation;
    }
    
    //! derive camera's intrinsic parameter matrix based on 
    //! focal lengths (fx, fy) and principle points (u, v).
    private void SetUnityCamParamsMatrix (float[] cameraParams)
    {
        m_camParamsMatrix.m00 = cameraParams [0];//negative for mirroring
        m_camParamsMatrix.m02 = -cameraParams [2];
        m_camParamsMatrix.m11 = cameraParams [1];//negative for flipping
        m_camParamsMatrix.m12 = -cameraParams [3];
        m_camParamsMatrix.m22 = (Camera.main.nearClipPlane + Camera.main.farClipPlane);
        m_camParamsMatrix.m23 = (Camera.main.nearClipPlane * Camera.main.farClipPlane);
        m_camParamsMatrix.m32 = -1f;		
    }	
    
    //! calculate orthographic matrix to be used with camera's intrinsic matrix
    //! to derive camera projection matrix. 
    private void CalOrthographicMat ()
    {
        int L = 0;
        int R = m_imgW;
        int B = 0;
        int T = m_imgH;		
        m_orthMatrix = new Matrix4x4 ();		
        m_orthMatrix.m00 = 2.0f / (R - L); 
        m_orthMatrix.m03 = -(R + L) / (R - L);
        m_orthMatrix.m11 = 2.0f / (T - B); 
        m_orthMatrix.m13 = -(T + B) / (T - B); 
        m_orthMatrix.m22 = -2.0f / (Camera.main.farClipPlane - Camera.main.nearClipPlane); 
        m_orthMatrix.m23 = -(Camera.main.farClipPlane + Camera.main.nearClipPlane) /
                        (Camera.main.farClipPlane - Camera.main.nearClipPlane);
        m_orthMatrix.m33 = 1.0f;
    }	
    
    //! utility function to convert a rotation matrix to quaterion. 
    //! use code from http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
    static public Quaternion RotationMatrixToQuaterion (Matrix4x4 rotMat)
    {
        Quaternion q = new Quaternion ();
        q.w = Mathf.Sqrt (Mathf.Max (0, 1 + rotMat [0, 0] + rotMat [1, 1] + rotMat [2, 2])) / 2; 
        q.x = Mathf.Sqrt (Mathf.Max (0, 1 + rotMat [0, 0] - rotMat [1, 1] - rotMat [2, 2])) / 2; 
        q.y = Mathf.Sqrt (Mathf.Max (0, 1 - rotMat [0, 0] + rotMat [1, 1] - rotMat [2, 2])) / 2; 
        q.z = Mathf.Sqrt (Mathf.Max (0, 1 - rotMat [0, 0] - rotMat [1, 1] + rotMat [2, 2])) / 2; 
        q.x *= Mathf.Sign (q.x * (rotMat [2, 1] - rotMat [1, 2]));
        q.y *= Mathf.Sign (q.y * (rotMat [0, 2] - rotMat [2, 0]));
        q.z *= Mathf.Sign (q.z * (rotMat [1, 0] - rotMat [0, 1]));
        return q;		
    }

    private void FlipDepthImage(ref ushort[] srcArray, ref ushort[] destArray, int rows, int cols)
    {
        int byteOffset = sizeof (ushort);
        for (int i = 0; i < rows; ++i)
        {
            int srcOffset = i * cols * byteOffset;
            int destOffset = (rows - i - 1) * cols * byteOffset;
            Buffer.BlockCopy(srcArray, srcOffset, destArray, destOffset, cols * byteOffset);
        }
    }

    bool GetNormalizedGravityIfAvailable(ref float[] gravityPtr, ref double timeStamp)
    {
        float[] curGravity = new float[3];
        double[] curTimeStamp = new double[1];  // Try boxing to object
        int maxAttempts = 15;
        int numAttempts = 1;
        bool isSuccessful = false;
        double magnitude = 0;
        const float MAGNITUDE_THRESHOLD = 0.7f; //value should match quality of IMU

        if (gravityPtr != null)
        {
            while (!isSuccessful)
            {
                if (numAttempts > maxAttempts)
                {
                    break;
                }
                GCHandle gravityHandle = GCHandle.Alloc(curGravity, GCHandleType.Pinned);
                IntPtr pGravity = gravityHandle.AddrOfPinnedObject();

                GCHandle curTimeStampHandle = GCHandle.Alloc(curTimeStamp, GCHandleType.Pinned);
                IntPtr pCurTimeStampHandle = curTimeStampHandle.AddrOfPinnedObject();

                if (NativeMethods.MOTION_getGravity(SensorType.HILLCREST, pGravity, pCurTimeStampHandle))
                {
                    magnitude = Math.Sqrt(curGravity[0] * curGravity[0] + curGravity[1] * curGravity[1] +
                        curGravity[2] * curGravity[2]);
                    if (Math.Abs(magnitude - 9.8) < MAGNITUDE_THRESHOLD)
                    {
                        //change the orientation due to mounting position of the IMU
                        gravityPtr[0] = -(float)(curGravity[0] / magnitude);
                        gravityPtr[1] = -(float)(curGravity[1] / magnitude);
                        gravityPtr[2] = -(float)(curGravity[2] / magnitude);
                        //if (timeStampPtr != nullptr)
                        //{
                        //    *timeStampPtr = curTimeStamp;
                        //}
                        timeStamp = curTimeStamp[0];

                        isSuccessful = true;
                    }
                }
                numAttempts++;
                gravityHandle.Free();
            }
        }
        return isSuccessful;
    }

    private void CrossProduct(ArraySegment<float> result, ArraySegment<float> lhs, ArraySegment<float> rhs)
    {
        result.Array[result.Offset + 0] = lhs.Array[lhs.Offset + 1] * rhs.Array[rhs.Offset + 2] - lhs.Array[lhs.Offset + 2] * rhs.Array[rhs.Offset + 1];
        result.Array[result.Offset + 1] = lhs.Array[lhs.Offset + 2] * rhs.Array[rhs.Offset + 0] - lhs.Array[lhs.Offset + 0] * rhs.Array[rhs.Offset + 2];
        result.Array[result.Offset + 2] = lhs.Array[lhs.Offset + 0] * rhs.Array[rhs.Offset + 1] - lhs.Array[lhs.Offset + 1] * rhs.Array[rhs.Offset + 0];
    }

    private bool CreatePoseMatrixFromRow(ref float[] poseMatrix, ref float[] rowValues, float[] translation, uint rowPosition)
    {
        const float FLT_EPSILON = 1.192092896e-07F;
        if (rowPosition == 0 || rowPosition > 3 || poseMatrix == null || rowValues == null || translation == null)
        {
            return false;
        }

        //mapping the coordinate systems of IMU and camera  
        poseMatrix[(rowPosition - 1) * 4] = rowValues[0] * -1;   // Make IMU X rotation negative to counter initial mirrored pose
        poseMatrix[(rowPosition - 1) * 4 + 1] = rowValues[1];
        poseMatrix[(rowPosition - 1) * 4 + 2] = rowValues[2] * -1; // Make IMU Z rotation negative to counter initial mirrored pose


        float dotProdXY = poseMatrix[(rowPosition - 1) * 4] * poseMatrix[(rowPosition - 1) * 4] +
                    poseMatrix[(rowPosition - 1) * 4 + 1] * poseMatrix[(rowPosition - 1) * 4 + 1];
        float dotProdXYZ = dotProdXY + poseMatrix[(rowPosition - 1) * 4 + 2] * poseMatrix[(rowPosition - 1) * 4 + 2];
        if (Math.Abs(dotProdXYZ - 1.0f) > FLT_EPSILON)
        {
            return false;
        }

        double meanXY = Math.Sqrt(dotProdXY);   // Should convert to float here and then avoid conversions below

        //fill in first row
        if (rowPosition == 1)
        {
            //second row
            if (meanXY > FLT_EPSILON)
            {
                poseMatrix[4] = (float)(poseMatrix[1] / meanXY);
                poseMatrix[5] = -(float)(poseMatrix[0] / meanXY);
                poseMatrix[6] = 0.0f;
            }
            else //passed in row is: (0,0,+/-1)
            {
                poseMatrix[4] = 0.0f;
                poseMatrix[5] = 1.0f;
                poseMatrix[6] = 0.0f;
            }
            //third row is a cross product
            CrossProduct(new ArraySegment<float>(poseMatrix, 8, 3), new ArraySegment<float>(poseMatrix, 0, 3), new ArraySegment<float>(poseMatrix, 4, 3));
        }

        //fill in the second row
        if (rowPosition == 2)
        {
            //first row
            if (meanXY > FLT_EPSILON)
            {
                //first row
                poseMatrix[0] = (float)(poseMatrix[5] / meanXY)  ;
                poseMatrix[1] = -(float)(poseMatrix[4] / meanXY);
                poseMatrix[2] = 0.0f;
            }
            else //passed in row is: (0,0,+/-1)
            {
                //first row
                poseMatrix[0] = 1.0f;
                poseMatrix[1] = 0.0f;
                poseMatrix[2] = 0.0f;
            }
            //third row is a cross product
            CrossProduct(new ArraySegment<float>(poseMatrix, 8, 3), new ArraySegment<float>(poseMatrix, 0, 3), new ArraySegment<float>(poseMatrix, 4, 3));
        }

        //fill in the third row
        if (rowPosition == 3)
        {
            //third row contains passed in values
            poseMatrix[8] = rowValues[0];
            poseMatrix[9] = rowValues[1];
            poseMatrix[10] = rowValues[2];

            //first row
            if (meanXY > FLT_EPSILON)
            {
                poseMatrix[0] = (float)(poseMatrix[9] / meanXY);
                poseMatrix[1] = -(float)(poseMatrix[8] / meanXY);
                poseMatrix[2] = 0.0f;
            }
            else //passed in row is: (0,0,+/-1)
            {
                poseMatrix[0] = 1.0f;
                poseMatrix[1] = 0.0f;
                poseMatrix[2] = 0.0f;
            }
            //second row is a cross product: third row x first row
            CrossProduct(new ArraySegment<float>(poseMatrix, 4, 3), new ArraySegment<float>(poseMatrix, 8, 3), new ArraySegment<float>(poseMatrix, 0, 3));
        }

        poseMatrix[3] = translation[0];
        poseMatrix[7] = translation[1];
        poseMatrix[11] = translation[2];

        return true;
    }

    public bool RGBImageUpdateAvailable()
    {
        return m_newRGBImageAvailable;
    }

    public void RGBImageUpdated()
    {
        m_newRGBImageAvailable = false;
        return;
    }
}
 