﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using System.IO;
namespace GeoVar {
    public partial class frmInterpolationKridging : Form
    {
        private bool bDataPath = false;// '判断是否通过打开对话框选择点特征类
        private bool bDataLinePath = false;// '判断是否通过打开对话框选择线障碍特征类
        static FormMain frm;
        public frmInterpolationKridging(FormMain frmm)
        {
            InitializeComponent();
            frm = frmm;
        }

        private void frmInterpolationKridging_Load(object sender, EventArgs e)
        {
            FillComboWithMapLayers(comboBoxInPoint, true, false);
            RadioOrdinary.Checked = true;
            comboBoxSearchRadius.Items.Add("固定");
            comboBoxSearchRadius.Items.Add("变化");
            comboBoxSearchRadius.Text = "变化";
            //txtOutputRasterPath.Text = FormGIS.SAoption.AnalysisPath;
            //txtCellSize.Text = FormGIS.SAoption.RasterCellSize.ToString();
            txtPointNumbers.Text = "12";

            groupBoxVariable.Visible = true;
            groupBoxFixed.Visible = false;

            comboBoxZValueField.Text = "无";
        }
        private void FillComboWithMapLayers(ComboBox combLayers, bool bLayer, bool bType)
        {
            combLayers.Items.Clear();
            ILayer aLayer;

            for (int i = 0; i < frm.mainMapControl.Map.LayerCount; i++)
            {
                aLayer = frm.mainMapControl.Map.get_Layer(i);
                if (aLayer != null)
                {
                    if (bLayer == true)
                    {
                        if (aLayer is IFeatureLayer)
                        {
                            IFeatureLayer flyr;
                            IFeatureClass pFeatureClass;
                            flyr = (IFeatureLayer)aLayer;
                            pFeatureClass = flyr.FeatureClass;

                            if (bType == false)
                            {
                                if (flyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                                {
                                    combLayers.Items.Add(aLayer.Name);
                                }
                            }
                            else
                            {
                                if (flyr.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                                {
                                    combLayers.Items.Add(aLayer.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void comboBoxInPoint_SelectedIndexChanged(object sender, EventArgs e)
        {
            String sLayerName = comboBoxInPoint.Text;
            IFeatureLayer pFeatLyr;

            comboBoxZValueField.Items.Clear();
            comboBoxZValueField.Items.Add("无");
            try
            {


                for (int i = 0; i < frm.mainMapControl.Map.LayerCount; i++)
                {
                    ILayer pLyr = frm.mainMapControl.Map.get_Layer(i);
                    if (pLyr.Name == sLayerName)
                    {
                        if (pLyr is IFeatureLayer)
                        {

                            pFeatLyr = (IFeatureLayer)pLyr;
                            IFeatureClass m_pFeatCls = pFeatLyr.FeatureClass;

                            for (int j = 0; i < m_pFeatCls.Fields.FieldCount - 1; j++)
                            {
                                if (m_pFeatCls.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeDouble || m_pFeatCls.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeSingle || m_pFeatCls.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeSmallInteger || m_pFeatCls.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeInteger)
                                {
                                    comboBoxZValueField.Items.Add(m_pFeatCls.Fields.get_Field(j).Name);
                                }
                            }
                        }
                    }
                }
                bDataPath = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOpenSourceData_Click(object sender, EventArgs e)
        {
            OpenFileDlg.FileName = "";
            OpenFileDlg.Filter = "GIS（*.shp）|*.shp";
            OpenFileDlg.FilterIndex = 1;
            OpenFileDlg.ShowDialog();
            if (OpenFileDlg.FileName != "")
            {
                comboBoxInPoint.Text = OpenFileDlg.FileName;
                IWorkspaceFactory pWorkspaceFactory;
                pWorkspaceFactory = new ShapefileWorkspaceFactory();
                String wsp;
                int r;
                int l;
                r = comboBoxInPoint.Text.LastIndexOf("\\");
                l = comboBoxInPoint.Text.LastIndexOf(".");
                wsp = comboBoxInPoint.Text.Substring(0, r);
                IFeatureWorkspace pFWS;
                pFWS = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(wsp, 0);
                try
                {
                    String sp;
                    IFeatureClass pFeatureClass;
                    sp = comboBoxInPoint.Text.Substring(r + 1, l - r - 1);
                    pFeatureClass = pFWS.OpenFeatureClass(sp);

                    comboBoxZValueField.Items.Clear();
                    comboBoxZValueField.Items.Add("无");
                    try
                    {
                        for (int j = 0; j < pFeatureClass.Fields.FieldCount; j++)
                        {
                            if (pFeatureClass.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeDouble || pFeatureClass.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeSingle || pFeatureClass.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeSmallInteger || pFeatureClass.Fields.get_Field(j).Type == esriFieldType.esriFieldTypeInteger)
                            {
                                comboBoxZValueField.Items.Add(pFeatureClass.Fields.get_Field(j).Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                bDataPath = true;
            }
        }

        private void RadioOrdinary_CheckedChanged(object sender, EventArgs e)
        {
            ComboBoxMethod.Items.Clear();
            ComboBoxMethod.Items.Add("Spherical");
            ComboBoxMethod.Items.Add("Cicular");
            ComboBoxMethod.Items.Add("Exponential");
            ComboBoxMethod.Items.Add("Gaussian");
            ComboBoxMethod.SelectedIndex = 0;
        }

        private void RadioButtonUniveral_CheckedChanged(object sender, EventArgs e)
        {
            ComboBoxMethod.Items.Clear();
            ComboBoxMethod.Items.Add("Linear with linear drift");
            ComboBoxMethod.Items.Add("Linear with Quadric drift");
            ComboBoxMethod.SelectedIndex = 0;
        }

        private void comboBoxSearchRadius_SelectedIndexChanged(object sender, EventArgs e)
        {
            String sRadiusType = comboBoxSearchRadius.Text;
            if (sRadiusType == "变化")
            {
                groupBoxVariable.Visible = true;
                groupBoxFixed.Visible = false;
            }
            else
            {
                groupBoxFixed.Visible = true;
                groupBoxVariable.Visible = false;
            }
        }

        private void btnGO_Click(object sender, EventArgs e)
        {
            IFeatureClass pInPointFClass;//   '获得输入点特征数据类

            String fileName;
            String rasterPath;
            String shpFile;
            int startX, endX;

            if (bDataPath == true)
            {
                fileName = comboBoxInPoint.Text;
                String shpDir = fileName.Substring(0, fileName.LastIndexOf("\\"));
                startX = fileName.LastIndexOf("\\");
                endX = fileName.Length;
                shpFile = fileName.Substring(startX + 1, endX - startX - 1);
                IWorkspaceFactory pWorkspaceFactory;
                pWorkspaceFactory = new ShapefileWorkspaceFactory();
                IFeatureWorkspace pFWS;
                pFWS = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(shpDir, 0);

                pInPointFClass = pFWS.OpenFeatureClass(shpFile);
            }
            else
            {
                pInPointFClass = GetFeatureFromMapLyr(comboBoxInPoint.Text);
            }

            IFeatureLayer pFeatLayer = new FeatureLayerClass();
            pFeatLayer.FeatureClass = pInPointFClass;
            rasterPath = txtOutputRasterPath.Text;

            //'FeatureClassDescriptor
            //这个对象主要用于提供访问控制FeatureClass descriptor的成员；例如，定义FeatureClass的那个属性作为分析操作。
            IFeatureClassDescriptor pFeatClsDes = new FeatureClassDescriptorClass();

            if (comboBoxZValueField.Text != "无")
            {
                pFeatClsDes.Create(pInPointFClass, null, comboBoxZValueField.Text);
            }
            else
            {
                pFeatClsDes.Create(pInPointFClass, null, "");
            }

            try
            {
                IInterpolationOp pInterpolationOp = new RasterInterpolationOpClass();
                String sCellSize = txtCellSize.Text;

                if (Convert.ToDouble(sCellSize) <= 0)
                {
                    MessageBox.Show("还没输入栅格单元！请输入后在进行插值。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                double dCellSize = Convert.ToDouble(sCellSize);

                IRasterRadius pRsRadius = new RasterRadiusClass();
                if (comboBoxSearchRadius.Text == "变化")
                {
                    String sPointNums = txtPointNumbers.Text;
                    if (sPointNums != "")
                    {
                        int iPointNums = Convert.ToInt32(sPointNums);
                        if (txtDisMaxValue.Text != "")
                        {
                            double maxDis = Convert.ToDouble(txtDisMaxValue.Text);
                            object objMaxDis = maxDis;
                            pRsRadius.SetVariable(iPointNums, ref objMaxDis);
                        }
                        else
                        {
                            object objMaxDis = null;
                            pRsRadius.SetVariable(iPointNums, ref objMaxDis);
                        }
                    }
                    else
                    {
                        MessageBox.Show("还没输入点的数目！请输入后在进行插值。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;

                    }
                }
                else
                {
                    String sDistance = txtDisValue.Text;
                    if (sDistance != "")
                    {
                        double dDistance = Convert.ToDouble(sDistance);
                        if (txtMaxPointNums.Text != "")
                        {
                            double maxPointNums = Convert.ToDouble(txtMaxPointNums.Text);
                            object objMinPointNums = maxPointNums;
                            pRsRadius.SetFixed(dDistance, ref objMinPointNums);
                        }
                        else
                        {
                            object objMinPointNums = null;
                            pRsRadius.SetFixed(dDistance, ref objMinPointNums);
                        }
                    }
                    else
                    {
                        MessageBox.Show("还没输入距离的值！请输入后在进行插值。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                pInterpolationOp = SetRasterInterpolationAnalysisEnv(rasterPath, dCellSize, pFeatLayer);

                esriGeoAnalysisSemiVariogramEnum semi = esriGeoAnalysisSemiVariogramEnum.esriGeoAnalysisNoneVariogram;
                if (RadioOrdinary.Checked)
                {
                    switch (ComboBoxMethod.SelectedIndex)
                    {
                        case 0:
                            semi = esriGeoAnalysisSemiVariogramEnum.esriGeoAnalysisSphericalSemiVariogram;
                            break;
                        case 1:
                            semi = esriGeoAnalysisSemiVariogramEnum.esriGeoAnalysisCircularSemiVariogram;
                            break;
                        case 2:
                            semi = esriGeoAnalysisSemiVariogramEnum.esriGeoAnalysisExponentialSemiVariogram;
                            break;
                        case 3:
                            semi = esriGeoAnalysisSemiVariogramEnum.esriGeoAnalysisGaussianSemiVariogram;
                            break;
                    }
                }
                else
                {
                    switch (ComboBoxMethod.SelectedIndex)
                    {
                        case 0:
                            semi = esriGeoAnalysisSemiVariogramEnum.esriGeoAnalysisUniversal1SemiVariogram;
                            break;
                        case 1:
                            semi = esriGeoAnalysisSemiVariogramEnum.esriGeoAnalysisUniversal2SemiVariogram;
                            break;
                    }

                }
                IRaster pOutRaster;
                object barrier = null;

                pOutRaster = (IRaster)pInterpolationOp.Krige((IGeoDataset)pFeatClsDes, semi, pRsRadius, true, ref barrier);
                //着色                
                IRasterLayer pRasterLayer = new RasterLayerClass();
                pRasterLayer.Name = "反距离栅格";
                ConvertRasterToRsDataset(rasterPath, ref pOutRaster, "反距离栅格");
                pRasterLayer = SetRsLayerClassifiedColor(pOutRaster);
                frm.mainMapControl.AddLayer(pRasterLayer, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private IFeatureClass GetFeatureFromMapLyr(String sLyrName)
        {
            IFeatureClass pFeatCls = null;
            for (int i = 0; i < frm.mainMapControl.Map.LayerCount; i++)
            {
                ILayer pLyr = frm.mainMapControl.Map.get_Layer(i);
                if (pLyr != null)
                {
                    if (pLyr.Name == sLyrName)
                    {
                        if (pLyr is IFeatureLayer)
                        {
                            IFeatureLayer pFLyr = (IFeatureLayer)pLyr;
                            pFeatCls = pFLyr.FeatureClass;
                        }
                    }
                }
            }
            return pFeatCls;
        }


        private IInterpolationOp SetRasterInterpolationAnalysisEnv(String rasterpath, double cellsize, IFeatureLayer pFeatLayer)
        {
            object Missing = Type.Missing;
            IWorkspaceFactory pWSF;
            pWSF = new RasterWorkspaceFactoryClass();
            IWorkspace pWorkspace = pWSF.OpenFromFile(rasterpath, 0);
            IInterpolationOp pInterpolationOp = new RasterInterpolationOpClass();
            IRasterAnalysisEnvironment pRsEnv = (IRasterAnalysisEnvironment)pInterpolationOp;
            pRsEnv.OutWorkspace = pWorkspace;
            //装箱操作
            object objCellSize = cellsize;

            pRsEnv.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref objCellSize);

            IEnvelope pEnv = new EnvelopeClass();
            pEnv.XMin = pFeatLayer.AreaOfInterest.XMin;
            pEnv.XMax = pFeatLayer.AreaOfInterest.XMax;
            pEnv.YMin = pFeatLayer.AreaOfInterest.YMin;
            pEnv.YMax = pFeatLayer.AreaOfInterest.YMax;
            object objExtent = pEnv;
            pRsEnv.SetExtent(esriRasterEnvSettingEnum.esriRasterEnvValue, ref objExtent, ref Missing);
            return pInterpolationOp;
        }

        private void ConvertRasterToRsDataset(String sPath, ref IRaster pRaster, String sOutName)
        {
            try
            {
                IWorkspaceFactory pWSF = new RasterWorkspaceFactoryClass();
                if (pWSF.IsWorkspace(sPath) == false)
                {
                    return;
                }

                IWorkspace pRWS = pWSF.OpenFromFile(sPath, 0);

                if (System.IO.File.Exists(sPath + "\\" + sOutName + ".img") == true)
                {
                    System.IO.File.Delete(sPath + "\\" + sOutName + ".img");
                }


                IRasterBandCollection pRasBandCol = (IRasterBandCollection)pRaster;

                IDataset pDS = pRasBandCol.SaveAs(sOutName + ".img", pRWS, "IMAGINE Image");
                ITemporaryDataset pRsGeo = (ITemporaryDataset)pDS;
                if (pRsGeo.IsTemporary())
                {
                    pRsGeo.MakePermanent();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private IRasterLayer SetRsLayerClassifiedColor(IRaster pRaster)
        {
            IRasterClassifyColorRampRenderer pClassRen = new RasterClassifyColorRampRendererClass();
            IRasterRenderer pRasRen = (IRasterRenderer)pClassRen;
            //Set raster for the render and update
            pRasRen.Raster = pRaster;
            pClassRen.ClassCount = 9;
            pRasRen.Update();
            //Create a color ramp to use
            //定义起点和终点颜色
            IColor pFromColor = new RgbColorClass();
            IRgbColor pRgbColor = (IRgbColor)pFromColor;
            pRgbColor.Red = 255;
            pRgbColor.Green = 200;
            pRgbColor.Blue = 0;
            IColor pToColor = new RgbColorClass();
            pRgbColor = (IRgbColor)pToColor;
            pRgbColor.Red = 0;
            pRgbColor.Green = 0;
            pRgbColor.Blue = 255;
            //创建颜色分级
            IAlgorithmicColorRamp pRamp = new AlgorithmicColorRampClass();
            pRamp.Size = 9;
            pRamp.FromColor = pFromColor;
            pRamp.ToColor = pToColor;
            bool ok = true;
            pRamp.CreateRamp(out ok);

            //获得栅格统计数值
            IRasterBandCollection pRsBandCol = (IRasterBandCollection)pRaster;
            IRasterBand pRsBand = pRsBandCol.Item(0);
            pRsBand.ComputeStatsAndHist();
            IRasterStatistics pRasterStatistic = pRsBand.Statistics;
            double dMaxValue = pRasterStatistic.Maximum;
            double dMinValue = pRasterStatistic.Minimum;

            //Create symbol for the classes
            IFillSymbol pFSymbol = new SimpleFillSymbolClass();
            double LabelValue = Convert.ToDouble((dMaxValue - dMinValue) / 9);

            for (int i = 0; i < pClassRen.ClassCount; i++)
            {
                pFSymbol.Color = pRamp.get_Color(i);
                pClassRen.set_Symbol(i, (ISymbol)pFSymbol);
                double h1 = (LabelValue * i) + dMinValue;
                double h2 = (LabelValue * (i + 1)) + dMinValue;
                pClassRen.set_Label(i, h1.ToString() + "-" + h2.ToString());
            }

            //Update the renderer and plug into layer
            pRasRen.Update();
            IRasterLayer pRLayer = new RasterLayerClass();
            pRLayer.CreateFromRaster(pRaster);
            pRLayer.Renderer = (IRasterRenderer)pClassRen;
            return pRLayer;
        }

        private void btnSaveRasterPath_Click(object sender, EventArgs e)
        {
            String pOutDSName;
            int iOutIndex;
            String sOutRasPath;
            String sOutRasName;

            saveFileDlg.Title = "保存栅格运算结果";
            saveFileDlg.Filter = "(*.img)|*.img";
            saveFileDlg.OverwritePrompt = false;
            saveFileDlg.InitialDirectory = Application.StartupPath;
            if (saveFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pOutDSName = saveFileDlg.FileName;
                FileInfo fFile = new FileInfo(pOutDSName);
                //判断文件名是否已经存在，如果存在，则弹出提示
                if (fFile.Exists == true)
                {
                    MessageBox.Show("文件名已存在，请重新输入", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    txtOutputRasterPath.Text = "";
                }
                else
                {
                    iOutIndex = pOutDSName.LastIndexOf("\\");
                    sOutRasPath = pOutDSName.Substring(0, iOutIndex + 1);
                    sOutRasName = pOutDSName.Substring(iOutIndex + 1, pOutDSName.Length - iOutIndex - 1);
                    txtOutputRasterPath.Text = pOutDSName;
                }

            }
        }




    }
}
