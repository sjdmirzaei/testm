using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsolesetting
{
    public class ArrowViewClass
    {
        string Foot;
        Model ViewModel;

        public void RenderView(Dictionary<string, Mesh> bones, string foot, Model viewmodel)
        {
            Foot = foot;
            ViewModel = viewmodel;
            viewmodel.Layers.Add(foot + StructureTypes.Arrows.ToString(), Color.Yellow, true);
            viewmodel.Layers.Add(foot + StructureTypes.Arrows.ToString() + "Points", Color.Red, true);

            var structureLoad = new LoadBoneStructure(bones, foot, viewmodel);
            structureLoad.LoadBones(StructureTypes.Arrows);

            DrawArrows(structureLoad.BoneSelectedPoints, foot, viewmodel);
        }

        void DrawArrows(Dictionary<int, List<int>> pointsList, string foot, Model model1)
        {
            var iteration = 0;
            var arrowPoints = new List<Point3D>();
            var indexTri = new List<IndexTriangle>();

            foreach (var item in pointsList)
            {
                if (item.Value.Count == 0)
                    continue;

                var bones = model1.Entities.FirstOrDefault(m => m.LayerName == foot && m.EntityData.ToString() == "bones" + item.Key);

                var point1 = bones.Vertices[item.Value[0]];
                var point2 = bones.Vertices[item.Value[1]];

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

            model1.Entities.Add(arrowMesh, foot + StructureTypes.Arrows.ToString(), Color.Yellow);
        }

        public void ShowPoints(bool state)
        {
            ViewModel.Layers[Foot + StructureTypes.Arrows.ToString() + "Points"].Visible = state;
        }

        public void ShowArrows(bool state)
        {
            ViewModel.Layers[Foot + StructureTypes.Arrows.ToString()].Visible = state;
        }
    }
}
