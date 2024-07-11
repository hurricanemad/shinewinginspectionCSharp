# 项目名称

图像检测治具软件

##版本号
T0.0.5.4

## 简介

本程序为包含丽翼图像检测治具中软件执行界面的程序。

## 功能特点

- 功能1：驱动采集卡获取检测图像。
- 功能2：程序包含去阴影功能，可以消除由镜头带来的图像亮度不均匀的情况。
- 功能2：提供交互界面，供操作人员执行近远场分辨率、视场角、图像均匀性和暗区面积测试。
- 功能3：调用动态库中函数执行近远场分辨率、视场角、图像均匀性和暗区面积测试。
- 功能4：显示检测结果至界面，并生成报告保存在程序目录中。

##改进点
1.修正实时检测，视场角、图像均匀性和暗区均匀性，检测状态变量没有更新的问题。
2.添加Lens shading操作中，不能开启图像去阴影功能按钮的功能，以防止程序出现问题。

## 快速开始

1.该程序需要配合Megewell采集卡采集检测图像。
2.检测开始前宜先执行去阴影操作以方便进行目视检测。
3.程序对近远场分辨率、视场角、图像均匀性和暗区面积测试均执行客观测试，
客观测试结果显示至UI界面上，通过界面显示为绿色，不通过显示为红色。

### 先决条件

1.程序使用visual studio 2022运行。
2.程序执行需要以下动态库文件：（动态库均包含与程序中）
api-ms-win-crt-heap-l1-1-0.dll
api-ms-win-crt-runtime-l1-1-0.dll
api-ms-win-crt-stdio-l1-1-0.dll
avcodec-57.dll
avdevice-57.dll
avfilter-6.dll
avformat-57.dll
avutil-55.dll
CUDAMANRDLL.dll（T0.0.5.3）
D3DCompiler_43.dll
d3dcompiler_47.dll
d3dx11_43.dll
glew32.dll
jawt.dll
kernel32.dll
libblas.dll
libgcc_s_seh_64-1.dll
libgcc_s_seh-1.dll
libgfortran_64-3.dll
liblapack.dll
liblapacke.dll
LibMWCapture.dll
LibMWMedia.dll
libquadmath-0.dll
libwinpthread-1.dll
opencv_core453.dll
opencv_highgui453.dll
opencv_imgcodecs453.dll
opencv_imgproc453.dll
opencv_video453.dll
opencv_videoio453.dll
swresample-2.dll
postproc-54.dll
Processing.NDI.Lib.UWP.x64.dll
Processing.NDI.Lib.x64.dll
swscale-4.dll