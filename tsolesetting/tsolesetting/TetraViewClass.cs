using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Labels;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsolesetting
{
    public class TetraViewClass
    {
        string Foot;
        Model ViewModel;
        devDept.Eyeshot.Labels.LabelList labels = new devDept.Eyeshot.Labels.LabelList();

        ConfigParams_FormSetting configs = new ConfigParams_FormSetting();

        public void RenderView(Dictionary<string, Mesh> bones, string foot, Model viewmodel)
        {
            Foot = foot;
            ViewModel = viewmodel;
            viewmodel.Layers.Add(foot + StructureTypes.Tetra.ToString(), Color.Black, true);
            viewmodel.Layers.Add(foot + StructureTypes.Tetra.ToString() + "Points", Color.Red, true);

            var structureLoad = new LoadBoneStructure(bones, foot, viewmodel);
            structureLoad.LoadBones(StructureTypes.Tetra);

            DrawTetra(structureLoad.BoneSelectedPoints, foot, viewmodel);
        }

        void DrawTetra(Dictionary<int, List<int>> pointsList, string foot, Model model1)
        {
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

            //model1.Entities.Add(tetraMesh, foot + "Structure", Color.Yellow);

            AddDimension(tetraPoints[0], tetraPoints[1], foot + "Structure");
            AddDimension(tetraPoints[2], tetraPoints[1], foot + "Structure");
            AddDimension(tetraPoints[0], tetraPoints[2], foot + "Structure");
            AddDimension(tetraPoints[0], tetraPoints[3], foot + "Structure");
            AddDimension(tetraPoints[3], tetraPoints[1], foot + "Structure");
            AddDimension(tetraPoints[2], tetraPoints[3], foot + "Structure");

            model1.Labels.AddRange(labels);
            model1.Entities.Regen();
        }

        void AddDimension(Point3D pnt1, Point3D pnt2, string layer)
        {
            var line = new Line(pnt1, pnt2);
            line.LineWeightMethod = colorMethodType.byEntity;
            line.LineWeight = configs.LineWeight;
            ViewModel.Entities.Add(line, layer);

            var textPos = new Point3D((pnt1.X + pnt2.X) / 2, (pnt1.Y + pnt2.Y) / 2, (pnt1.Z + pnt2.Z) / 2);
            var distance = Math.Round(Math.Sqrt(Math.Pow(pnt1.X - pnt2.X, 2) + Math.Pow(pnt1.Y - pnt2.Y, 2) + Math.Pow(pnt1.Z - pnt2.Z, 2)));

            LeaderAndText lbl = new LeaderAndText(textPos,
                                  distance + " mm", new Font("Tahoma", 8.25f), Color.White, new Vector2D(0, 10));

            lbl.FillColor = Color.Black;
            labels.Add(lbl);
        }

        public void ShowPoints(bool state)
        {
            ViewModel.Layers[Foot + StructureTypes.Tetra.ToString() + "Points"].Visible = state;
        }

        public void ShowTetra(bool state)
        {
            ViewModel.Layers[Foot + StructureTypes.Tetra.ToString()].Visible = state;

            foreach (var item in labels)
            {
                item.Visible = state;
            }
        }

    }
}
