using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using devDept.Eyeshot.Entities;
using System.Globalization;
using devDept.Geometry;
using devDept.Eyeshot;
using devDept.Eyeshot.Labels;
using System.Text.RegularExpressions;

namespace tsolesetting
{
    public partial class FormSettings : DevExpress.XtraEditors.XtraForm
    {
        public FormSettings()
        {
            InitializeComponent();
            model1.Unlock("US20-M125J-J8EJX-6608-RERC");
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            model1.Layers.Add("Right", Color.FromArgb(100, Color.GreenYellow), false);
            model1.Layers.Add("Left", Color.FromArgb(100, Color.GreenYellow), true);
            model1.Layers.Add("RightHigh", Color.FromArgb(100, Color.GreenYellow), false);
            model1.Layers.Add("LeftHigh", Color.FromArgb(100, Color.GreenYellow), false);

            model1.Layers.Add("RightStructure", Color.Yellow, false);
            model1.Layers.Add("LeftStructure", Color.Yellow, false);
            model1.Layers.Add("RightHighStructure", Color.Yellow, false);
            model1.Layers.Add("LeftHighStructure", Color.Yellow, false);

            model1.Layers.Add("RightTetraPoints", Color.Red, false);
            model1.Layers.Add("LeftTetraPoints", Color.Red, true);
            model1.Layers.Add("RightHighTetraPoints", Color.Red, false);
            model1.Layers.Add("LeftHighTetraPoints", Color.Red, false);

            model1.Layers.Add("RightArrowsPoints", Color.Red, false);
            model1.Layers.Add("LeftArrowsPoints", Color.Red, true);
            model1.Layers.Add("RightHighArrowsPoints", Color.Red, false);
            model1.Layers.Add("LeftHighArrowsPoints", Color.Red, false);

            LoadBones(false, "Right", ref LeftInputBonesNormal, ref RightInputBonesNormal);
            LoadBones(false, "Left", ref LeftInputBonesNormal, ref RightInputBonesNormal);
            LoadBones(true, "Right", ref LeftInputBonesHigh, ref RightInputBonesHigh);
            LoadBones(true, "Left", ref LeftInputBonesHigh, ref RightInputBonesHigh);
        }

        Dictionary<string, Mesh> LeftInputBonesNormal = new Dictionary<string, Mesh>();
        Dictionary<string, Mesh> RightInputBonesNormal = new Dictionary<string, Mesh>();
        Dictionary<string, Mesh> LeftInputBonesHigh = new Dictionary<string, Mesh>();
        Dictionary<string, Mesh> RightInputBonesHigh = new Dictionary<string, Mesh>();

        Dictionary<string, Dictionary<int, List<int>>> BoneStructurePoints = new Dictionary<string, Dictionary<int, List<int>>>();

        ConfigParams_FormSetting configs = new ConfigParams_FormSetting();
        StructureTypes structType;
        bool ChangeBone = false;

        devDept.Eyeshot.Labels.LabelList labels = new devDept.Eyeshot.Labels.LabelList();

        void LoadBones(bool High, string Foot, ref Dictionary<string, Mesh> LeftInputBones, ref Dictionary<string, Mesh> RightInputBones)
        {
            var foot = Foot;
            if (High)
                foot = Foot + "High";

            if (Foot == "Left")
                LeftInputBones.Clear();
            else
                RightInputBones.Clear();

            for (int i = 0; i < 28; i++)
            {
                var filename = Application.StartupPath + "\\RegistrationAlgorithm\\Bones\\" + foot + "\\Bones" + (i + 1).ToString() + ".stl";
                devDept.Eyeshot.Translators.ReadSTL stlreader = new devDept.Eyeshot.Translators.ReadSTL(filename);
                stlreader.DoWork();

                if (Foot == "Left")
                    LeftInputBones["bones" + (i + 1)] = (Mesh)stlreader.Entities[0];
                else
                    RightInputBones["bones" + (i + 1)] = (Mesh)stlreader.Entities[0];

                stlreader.Entities[0].EntityData = "bones" + (i + 1);
                model1.Entities.Add(stlreader.Entities[0], foot);
            }

            model1.Entities.Regen();
            model1.Invalidate();
        }

        int entityIndex = -1;
        HitVertex selectedPoint;
        bool isBoneSelected = false;
        Mesh selectedBone;

        private void model1_MouseDown(object sender, MouseEventArgs e)
        {
            // gets the entity index
            entityIndex = model1.GetEntityUnderMouseCursor(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                foreach (var item in model1.Entities)
                {
                    item.Visible = true;
                }

                var foot = FindFoot();

                if (structType == StructureTypes.Arrows)
                    DrawArrows(foot);
                else if (structType == StructureTypes.Tetra)
                    DrawTetra(foot);

                isBoneSelected = false;
                model1.Entities.Regen();
                model1.Invalidate();
                return;
            }
            else if (isBoneSelected)
            {
                if (entityIndex < 0 || model1.Entities[entityIndex] is Mesh)
                {
                    entityIndex = -1;
                    return;
                }
            }
            else if (!isBoneSelected)
            {
                if (entityIndex < 0 || model1.Entities[entityIndex] is PointCloud || model1.Entities[entityIndex].LayerName.EndsWith("Structure") || e.Button == MouseButtons.Middle)
                {
                    entityIndex = -1;
                    return;
                }
            }

            isBoneSelected = true;
            model1.ActionMode = actionType.None;
            var ent = model1.Entities[entityIndex];

            if (ent is Mesh)
                foreach (var item in model1.Entities)
                {
                    if (item.EntityData == null)
                        item.Visible = false;
                    else if (item.EntityData.ToString() != ent.EntityData.ToString() || !(item.LayerName == ent.LayerName || item.LayerName == ent.LayerName + structType + "Points"))
                        item.Visible = false;
                    else
                    {
                        if (item is Mesh)
                            selectedBone = (Mesh)item;
                    }
                }

            selectedPoint = model1.FindClosestVertex(model1.Entities[entityIndex], e.Location, 30);
        }

        private void model1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var movedItem = model1.GetEntityUnderMouseCursor(e.Location);

                if (isBoneSelected)
                {
                    if (movedItem != -1 && model1.Entities[movedItem] is PointCloud)
                        model1.Cursor = Cursors.Hand;
                    else
                        model1.Cursor = Cursors.Default;
                }
                else
                {
                    if (movedItem != -1 && model1.Entities[movedItem] is Mesh && !model1.Entities[movedItem].LayerName.EndsWith("Structure"))
                        model1.Cursor = Cursors.Hand;
                    else
                        model1.Cursor = Cursors.Default;
                }

                // if we found an entity and the left mouse button is down
                if (entityIndex != -1 && e.Button == MouseButtons.Left)
                {
                    // gets the entity reference
                    Entity entity = model1.Entities[entityIndex] as Entity;

                    // current 3D point
                    //var moveTo = model1.FindClosestVertex(selectedBone, e.Location, 5);


                    model1.FindClosestTriangle((IFace)selectedBone, e.Location, out Point3D moveTo, out int triInd);
                    var moveInd = selectedBone.Triangles[triInd].V1;

                    //entity.Vertices[selectedPoint.VertexIndex] = (PointRGB)selectedBone.Vertices[moveInd];
                    entity.Vertices[selectedPoint.VertexIndex].X = selectedBone.Vertices[moveInd].X;
                    entity.Vertices[selectedPoint.VertexIndex].Y = selectedBone.Vertices[moveInd].Y;
                    entity.Vertices[selectedPoint.VertexIndex].Z = selectedBone.Vertices[moveInd].Z;
                    entity.Regen(0.05);

                    // regens curved entities it with a loosen tol to go faster
                    model1.Entities.Regen();

                    // refresh the screen 
                    model1.Invalidate();

                    var index = int.Parse(Regex.Match(entity.EntityData.ToString(), @"\d+$").Value);
                    var foot = FindFoot();
                    BoneStructurePoints[foot][index][selectedPoint.VertexIndex] = moveInd;//moveTo.VertexIndex;
                }
            }
            catch (Exception exp)
            { }
        }

        string FindFoot()
        {
            var foot = "";
            if (radioRorL.SelectedIndex == 0)
            {
                if (radioNormalHigh.SelectedIndex == 0)
                    foot = "Left";
                else
                    foot = "LeftHigh";
            }
            else
            {
                if (radioNormalHigh.SelectedIndex == 0)
                    foot = "Right";
                else
                    foot = "RightHigh";
            }
            return foot;
        }

        private void SimpleButton1_Click(object sender, EventArgs e)
        {
            var activeFoot = FindFoot();
            SaveChanges(activeFoot, "");
        }

        private void SaveChanges(string RL, string NH)
        {
            var foot = RL + NH;
            string pntPath = Application.StartupPath + "\\RegistrationAlgorithm\\Bones\\" + foot + "\\" + structType + ".txt";

            var structure = new LoadBoneStructure();
            structure.WriteBonePoints(pntPath, BoneStructurePoints[foot]);
        }

        public void DrawArrows(string foot)
        {
            Dictionary<int, List<int>> pointsList = BoneStructurePoints[foot];

            model1.Layers.Remove(foot + "Structure");
            model1.Layers.Add(foot + "Structure", Color.Yellow, false);

            var iteration = 0;
            var activeFoot = FindFoot();
            var arrowPoints = new List<Point3D>();
            var indexTri = new List<IndexTriangle>();

            foreach (var item in pointsList)
            {
                if (item.Value.Count == 0)
                    continue;

                var bones = model1.Entities.FirstOrDefault(m => m.LayerName == foot && m.EntityData.ToString() == "bones" + item.Key);

                var point1 = bones.Vertices[item.Value[0]];
                var point2 = bones.Vertices[item.Value[1]];

                //if (point1.Y > point2.Y)
                //{
                //    var temp = point1;
                //    point1 = point2;
                //    point2 = temp;
                //}

                var d = Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2) + Math.Pow(point1.Z - point2.Z, 2));
                //var dp = 0.1 * d;
                var dp = 5;

                var x = 0.9 * point1.X + 0.1 * point2.X;
                var y = 0.9 * point1.Y + 0.1 * point2.Y;
                var z = 0.9 * point1.Z + 0.1 * point2.Z;

                var npoint = new Point3D(x, y, z);
                var srfc = new Plane(npoint, new Vector3D(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z));

                var sp1 = srfc.PointAt(new Point2D(dp, 0));
                var sp2 = srfc.PointAt(new Point2D(0, dp));
                var sp3 = srfc.PointAt(new Point2D(-dp, 0));
                var sp4 = srfc.PointAt(new Point2D(0, -dp));

                arrowPoints.AddRange(new[] { point1, sp1, sp2, sp3, sp4, point2 });

                indexTri.Add(new IndexTriangle(0 + (iteration * 6), 1 + (iteration * 6), 2 + (iteration * 6)));
                indexTri.Add(new IndexTriangle(0 + (iteration * 6), 2 + (iteration * 6), 3 + (iteration * 6)));
                indexTri.Add(new IndexTriangle(0 + (iteration * 6), 3 + (iteration * 6), 4 + (iteration * 6)));
                indexTri.Add(new IndexTriangle(0 + (iteration * 6), 4 + (iteration * 6), 1 + (iteration * 6)));
                indexTri.Add(new IndexTriangle(5 + (iteration * 6), 1 + (iteration * 6), 2 + (iteration * 6)));
                indexTri.Add(new IndexTriangle(5 + (iteration * 6), 2 + (iteration * 6), 3 + (iteration * 6)));
                indexTri.Add(new IndexTriangle(5 + (iteration * 6), 3 + (iteration * 6), 4 + (iteration * 6)));
                indexTri.Add(new IndexTriangle(5 + (iteration * 6), 4 + (iteration * 6), 1 + (iteration * 6)));

                iteration++;
            }

            var arrowMesh = new Mesh(arrowPoints.ToArray(), indexTri.ToArray());
            arrowMesh.Regen(0.05);

            if (foot == activeFoot)
                model1.Layers[foot + "Structure"].Visible = checkStructure.Checked;

            model1.Entities.Add(arrowMesh, foot + "Structure", Color.Yellow);
        }

        private void radioRorL_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBones.Checked = true;

            if (radioRorL.SelectedIndex == 1)
            {
                if (radioNormalHigh.SelectedIndex == 0)
                {
                    model1.Layers["Right"].Visible = true;
                    model1.Layers["Left"].Visible = false;
                }
                else
                {
                    model1.Layers["RightHigh"].Visible = true;
                    model1.Layers["LeftHigh"].Visible = false;
                }
            }
            else
            {
                if (radioNormalHigh.SelectedIndex == 0)
                {
                    model1.Layers["Right"].Visible = false;
                    model1.Layers["Left"].Visible = true;
                }
                else
                {
                    model1.Layers["RightHigh"].Visible = false;
                    model1.Layers["LeftHigh"].Visible = true;
                }
            }

            CheckArrows_CheckedChanged(sender, e);
            CheckPoints_CheckedChanged(sender, e);

            model1.ZoomFit();
            model1.Invalidate();

            SaveChanges(radioRorL.SelectedIndex == 0 ? "Right" : "Left", radioNormalHigh.SelectedIndex == 0 ? "" : "High");
        }

        private void RadioNormalHigh_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBones.Checked = true;

            if (radioNormalHigh.SelectedIndex == 1)
            {
                if (radioRorL.SelectedIndex == 1)
                {
                    model1.Layers["RightHigh"].Visible = true;
                    model1.Layers["LeftHigh"].Visible = false;
                }
                else
                {
                    model1.Layers["RightHigh"].Visible = false;
                    model1.Layers["LeftHigh"].Visible = true;
                }
                model1.Layers["Right"].Visible = false;
                model1.Layers["Left"].Visible = false;
            }
            else
            {
                if (radioRorL.SelectedIndex == 1)
                {
                    model1.Layers["Right"].Visible = true;
                    model1.Layers["Left"].Visible = false;
                }
                else
                {
                    model1.Layers["Right"].Visible = false;
                    model1.Layers["Left"].Visible = true;
                }
                model1.Layers["RightHigh"].Visible = false;
                model1.Layers["LeftHigh"].Visible = false;
            }
            model1.Entities.Regen();

            CheckArrows_CheckedChanged(sender, e);
            CheckPoints_CheckedChanged(sender, e);

            SaveChanges(radioRorL.SelectedIndex == 1 ? "Right" : "Left", radioNormalHigh.SelectedIndex == 1 ? "" : "High");
        }

        private void CheckArrows_CheckedChanged(object sender, EventArgs e)
        {
            model1.Layers["RightStructure"].Visible = false;
            model1.Layers["LeftStructure"].Visible = false;
            model1.Layers["RightHighStructure"].Visible = false;
            model1.Layers["LeftHighStructure"].Visible = false;

            if (checkStructure.Checked)
                if (radioRorL.SelectedIndex == 1)
                {
                    if (radioNormalHigh.SelectedIndex == 0)
                    {
                        model1.Layers["RightStructure"].Visible = true;
                        model1.Layers["LeftStructure"].Visible = false;
                    }
                    else
                    {
                        model1.Layers["RightHighStructure"].Visible = true;
                        model1.Layers["LeftHighStructure"].Visible = false;
                    }
                }
                else
                {
                    if (radioNormalHigh.SelectedIndex == 0)
                    {
                        model1.Layers["RightStructure"].Visible = false;
                        model1.Layers["LeftStructure"].Visible = true;
                    }
                    else
                    {
                        model1.Layers["RightHighStructure"].Visible = false;
                        model1.Layers["LeftHighStructure"].Visible = true;
                    }
                }

            model1.Invalidate();
        }

        private void CheckBones_CheckedChanged(object sender, EventArgs e)
        {
            model1.Layers["Right"].Visible = false;
            model1.Layers["Left"].Visible = false;
            model1.Layers["RightHigh"].Visible = false;
            model1.Layers["LeftHigh"].Visible = false;

            if (checkBones.Checked)
            {
                if (radioRorL.SelectedIndex == 1)
                {
                    if (radioNormalHigh.SelectedIndex == 0)
                    {
                        model1.Layers["Right"].Visible = true;
                        model1.Layers["Left"].Visible = false;
                    }
                    else
                    {
                        model1.Layers["RightHigh"].Visible = true;
                        model1.Layers["LeftHigh"].Visible = false;
                    }
                }
                else
                {
                    if (radioNormalHigh.SelectedIndex == 0)
                    {
                        model1.Layers["Right"].Visible = false;
                        model1.Layers["Left"].Visible = true;
                    }
                    else
                    {
                        model1.Layers["RightHigh"].Visible = false;
                        model1.Layers["LeftHigh"].Visible = true;
                    }
                }
            }

            model1.Invalidate();
        }

        private void CheckPoints_CheckedChanged(object sender, EventArgs e)
        {
            model1.Layers["RightArrowsPoints"].Visible = false;
            model1.Layers["LeftArrowsPoints"].Visible = false;
            model1.Layers["RightHighArrowsPoints"].Visible = false;
            model1.Layers["LeftHighArrowsPoints"].Visible = false;

            model1.Layers["RightTetraPoints"].Visible = false;
            model1.Layers["LeftTetraPoints"].Visible = false;
            model1.Layers["RightHighTetraPoints"].Visible = false;
            model1.Layers["LeftHighTetraPoints"].Visible = false;

            if (checkPoints.Checked)
            {
                if (radioRorL.SelectedIndex == 1)
                {
                    if (radioNormalHigh.SelectedIndex == 0)
                    {
                        model1.Layers["Right" + structType.ToString() + "Points"].Visible = true;
                        model1.Layers["Left" + structType.ToString() + "Points"].Visible = false;
                    }
                    else
                    {
                        model1.Layers["RightHigh" + structType.ToString() + "Points"].Visible = true;
                        model1.Layers["LeftHigh" + structType.ToString() + "Points"].Visible = false;
                    }
                }
                else
                {
                    if (radioNormalHigh.SelectedIndex == 0)
                    {
                        model1.Layers["Right" + structType.ToString() + "Points"].Visible = false;
                        model1.Layers["Left" + structType.ToString() + "Points"].Visible = true;
                    }
                    else
                    {
                        model1.Layers["RightHigh" + structType.ToString() + "Points"].Visible = false;
                        model1.Layers["LeftHigh" + structType.ToString() + "Points"].Visible = true;
                    }
                }
            }
            model1.Invalidate();
        }

        private void BtnArrowView_Click(object sender, EventArgs e)
        {
            if (structType != StructureTypes.None)
                SaveChanges(FindFoot(), "");
            structType = StructureTypes.Arrows;
            RemovePoints();
            checkChangeBone.Enabled = false;

            LoadBoneStructure LeftArrowStruct = new LoadBoneStructure(LeftInputBonesNormal, "Left", model1); ;
            LeftArrowStruct.LoadBones(structType);
            BoneStructurePoints["Left"] = LeftArrowStruct.BoneSelectedPoints;
            DrawArrows("Left");

            LoadBoneStructure RightArrowStruct = new LoadBoneStructure(RightInputBonesNormal, "Right", model1); ;
            RightArrowStruct.LoadBones(structType);
            BoneStructurePoints["Right"] = RightArrowStruct.BoneSelectedPoints;
            DrawArrows("Right");

            LoadBoneStructure LeftHighArrowStruct = new LoadBoneStructure(LeftInputBonesHigh, "LeftHigh", model1);
            LeftHighArrowStruct.LoadBones(structType);
            BoneStructurePoints["LeftHigh"] = LeftHighArrowStruct.BoneSelectedPoints;
            DrawArrows("LeftHigh");

            LoadBoneStructure RightHighArrowStruct = new LoadBoneStructure(RightInputBonesHigh, "RightHigh", model1); ;
            RightHighArrowStruct.LoadBones(structType);
            BoneStructurePoints["RightHigh"] = RightHighArrowStruct.BoneSelectedPoints;
            DrawArrows("RightHigh");

            //ArrowViewClass leftarrowview = new ArrowViewClass();
            //leftarrowview.RenderView(LeftInputBonesNormal, "Left", model1);

            //ArrowViewClass rightarrowview = new ArrowViewClass();
            //rightarrowview.RenderView(RightInputBonesNormal, "Right", model1);
            //leftarrowview.ShowPoints(false);

            model1.Entities.Regen();
            model1.ZoomFit();
            model1.Invalidate();
        }

        private void BtnTetraView_Click(object sender, EventArgs e)
        {
            if (structType != StructureTypes.None)
                SaveChanges(FindFoot(), "");
            structType = StructureTypes.Tetra;
            RemovePoints();

            layoutControl3.Visible = true;
            ChangeBone = false;

            LoadBoneStructure LeftArrowStruct = new LoadBoneStructure(LeftInputBonesNormal, "Left", model1); ;
            LeftArrowStruct.LoadBones(structType);
            BoneStructurePoints["Left"] = LeftArrowStruct.BoneSelectedPoints;
            DrawTetra("Left");

            LoadBoneStructure RightArrowStruct = new LoadBoneStructure(RightInputBonesNormal, "Right", model1); ;
            RightArrowStruct.LoadBones(structType);
            BoneStructurePoints["Right"] = RightArrowStruct.BoneSelectedPoints;
            DrawTetra("Right");

            LoadBoneStructure LeftHighArrowStruct = new LoadBoneStructure(LeftInputBonesHigh, "LeftHigh", model1);
            LeftHighArrowStruct.LoadBones(structType);
            BoneStructurePoints["LeftHigh"] = LeftHighArrowStruct.BoneSelectedPoints;
            DrawTetra("LeftHigh");

            LoadBoneStructure RightHighArrowStruct = new LoadBoneStructure(RightInputBonesHigh, "RightHigh", model1); ;
            RightHighArrowStruct.LoadBones(structType);
            BoneStructurePoints["RightHigh"] = RightHighArrowStruct.BoneSelectedPoints;
            DrawTetra("RightHigh");

            //var lefttetraview = new TetraViewClass();
            //lefttetraview.RenderView(LeftInputBonesNormal, "Left", model1);

            //var righttetraview = new TetraViewClass();
            //righttetraview.RenderView(RightInputBonesNormal, "Right", model1);

            //lefttetraview.ShowTetra(false);

            model1.Entities.Regen();
            model1.ZoomFit();
            model1.Invalidate();
        }

        void DrawTetra(string foot)
        {
            Dictionary<int, List<int>> pointsList = BoneStructurePoints[foot];

            model1.Labels.Clear();
            model1.Layers.Remove(foot + "Structure");
            model1.Layers.Add(foot + "Structure", Color.Black, false);

            var activeFoot = FindFoot();

            var tetraPoints = new List<Point3D>();
            var indexTri = new List<IndexTriangle>();

            foreach (var item in pointsList)
            {
                if (item.Value.Count == 0)
                    continue;

                var bones = model1.Entities.FirstOrDefault(m => m.LayerName == foot && m.EntityData.ToString() == "bones" + item.Key);
                var point = bones.Vertices[item.Value[0]];

                tetraPoints.Add(point);
            }

            indexTri.Add(new IndexTriangle(0, 1, 2));
            indexTri.Add(new IndexTriangle(0, 2, 3));
            indexTri.Add(new IndexTriangle(0, 1, 3));
            indexTri.Add(new IndexTriangle(1, 2, 3));

            var tetraMesh = new Mesh(tetraPoints.ToArray(), indexTri.ToArray());
            tetraMesh.Regen(0.05);

            if (foot == activeFoot)
                model1.Layers[foot + "Structure"].Visible = checkStructure.Checked;

            //model1.Entities.Add(tetraMesh, foot + "Structure", Color.Yellow);

            AddDimension(tetraPoints[0], tetraPoints[1], foot + "Structure");
            AddDimension(tetraPoints[2], tetraPoints[1], foot + "Structure");
            AddDimension(tetraPoints[0], tetraPoints[2], foot + "Structure");
            AddDimension(tetraPoints[0], tetraPoints[3], foot + "Structure");
            AddDimension(tetraPoints[3], tetraPoints[1], foot + "Structure");
            AddDimension(tetraPoints[2], tetraPoints[3], foot + "Structure");

            model1.Entities.Regen();
        }

        void AddDimension(Point3D pnt1, Point3D pnt2, string layer)
        {
            //////var vnormal = new Vector3D(pnt1.X - pnt2.X, pnt1.Y - pnt2.Y, pnt1.Z - pnt2.Z);
            ////var vnormal = new Vector3D(0, 0, pnt1.Z - pnt2.Z);
            ////var tplane = new Plane(vnormal);
            ////tplane.Origin = pnt1;
            //////tplane.Rotate(Math.PI / 2, vnormal);
            ////var textPos = new Point3D((pnt1.X + pnt2.X) / 2, (pnt1.Y + pnt2.Y) / 2, (pnt1.Z + pnt2.Z) / 2 + 10);
            //////OrdinateDim ad = new OrdinateDim(tplane, new Point3D(pnt1.X, pnt1.Y, pnt1.Z + 10), new Point3D(pnt2.X, pnt2.Y, pnt2.Z + 10),false, 3);
            ////LinearDim ad = new LinearDim(tplane, new Point3D(pnt1.X, pnt1.Y, pnt1.Z + 1), new Point3D(pnt2.X, pnt2.Y, pnt2.Z + 10), textPos, 3);
            //////LinearDim ad = new LinearDim(tplane, pnt1, pnt2, textPos, 3);
            ////model1.Entities.Add(new PlanarEntity( tplane));

            ////ad.TextSuffix = " mm";
            ////model1.Entities.Add(ad, layer);

            var line = new Line(pnt1, pnt2);
            line.LineWeightMethod = colorMethodType.byEntity;
            line.LineWeight = configs.LineWeight;
            model1.Entities.Add(line, layer);

            var textPos = new Point3D((pnt1.X + pnt2.X) / 2, (pnt1.Y + pnt2.Y) / 2, (pnt1.Z + pnt2.Z) / 2);
            var distance = Math.Round(Math.Sqrt(Math.Pow(pnt1.X - pnt2.X, 2) + Math.Pow(pnt1.Y - pnt2.Y, 2) + Math.Pow(pnt1.Z - pnt2.Z, 2)));

            LeaderAndText lbl = new LeaderAndText(textPos,
                                  distance + " mm", new Font("Tahoma", 8.25f), Color.White, new Vector2D(0, 10));

            lbl.FillColor = Color.Black;
            lbl.Visible = false;
            labels.Add(lbl);
            //model1.Labels.Add(lbl);
            model1.Labels = labels;
        }

        void RemovePoints()
        {
            for (int i = model1.Entities.Count - 1; i > 0; i--)
            {
                if (model1.Entities[i].LayerName.EndsWith("Points"))
                    model1.Entities.Remove(model1.Entities[i]);
            }
        }

        private void CheckChangeBone_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkChangeBone.Checked)
            {
                ChangeBone = true;
                comboBoxPoints.Enabled = true;
                InitComboBox();
            }
            else
            {
                model1.Entities.ClearSelection();
                DrawTetra(FindFoot());
                ChangeBone = false;
                comboBoxBones.Enabled = false;
                comboBoxPoints.Enabled = false;
                comboBoxPoints.SelectedIndex = -1;
                comboBoxBones.SelectedIndex = -1;
                model1.Invalidate();
            }
        }

        private void InitComboBox()
        {
            comboBoxBones.Properties.Items.Clear();
            comboBoxPoints.Properties.Items.Clear();

            var foot = FindFoot();

            foreach (var item in BoneStructurePoints[foot])
            {
                comboBoxBones.Properties.Items.Add("Bone" + item.Key);

                if (item.Value.Count != 0)
                {
                    comboBoxPoints.Properties.Items.Add("Point" + item.Key);
                }
            }
        }

        private void ComboBoxPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxPoints.SelectedIndex == -1)
                return;

            model1.Entities.ClearSelection();
            model1.SelectionColor = Color.Brown;

            comboBoxBones.Enabled = true;
            //comboBoxBones.SelectedIndex = -1;

            var pointIndex = int.Parse(Regex.Replace(comboBoxPoints.SelectedItem.ToString(), "[^0-9]", ""));
            var selectedModel = model1.Entities.FirstOrDefault(m => m.EntityData.ToString() == "bones" + pointIndex && m is PointCloud);
            selectedModel.Selected = true;

            model1.Entities.Regen();
            model1.Invalidate();
        }

        private void ComboBoxBones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxBones.SelectedIndex == -1)
                return;

            var foot = FindFoot();
            var oldIndex = int.Parse(Regex.Replace(comboBoxPoints.SelectedItem.ToString(), "[^0-9]", ""));
            var newIndex = int.Parse(Regex.Replace(comboBoxBones.SelectedText, "[^0-9]", ""));

            if (comboBoxPoints.Properties.Items.Contains("Point" + newIndex))
            {
                MessageBox.Show("Duplicate Bone!");
                comboBoxBones.SelectedIndex = -1;
                return;
            }

            var selectedBone = model1.Entities.FirstOrDefault(b => b.LayerName == foot && b.EntityData.ToString() == "bones" + newIndex);
            var oldSelectedModel = model1.Entities.FirstOrDefault(m => m.LayerName == foot + structType.ToString() + "Points" && m.EntityData.ToString() == "bones" + oldIndex);
            var newSelectedModel = model1.Entities.FirstOrDefault(m => m.LayerName == foot + structType.ToString() + "Points" && m.EntityData.ToString() == "bones" + newIndex);

            oldSelectedModel.Vertices = new Point3D[0];
            oldSelectedModel.Regen(0.5);

            newSelectedModel.Vertices = new PointRGB[] { new PointRGB(selectedBone.Vertices[0].X, selectedBone.Vertices[0].Y, selectedBone.Vertices[0].Z, (new LoadBoneStructure()).colors[0]) };
            newSelectedModel.Regen(0.5);

            BoneStructurePoints[foot][oldIndex].Clear();
            BoneStructurePoints[foot][newIndex].Add(0);

            DrawTetra(foot);
            model1.Invalidate();

            comboBoxPoints.Properties.Items.Remove(comboBoxPoints.SelectedItem);
            comboBoxPoints.Properties.Items.Add("Point" + newIndex);
            comboBoxPoints.SelectedIndex = comboBoxPoints.Properties.Items.Count - 1;
        }
    }

}