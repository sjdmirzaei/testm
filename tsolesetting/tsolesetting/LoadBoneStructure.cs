using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tsolesetting
{
    public class LoadBoneStructure
    {
        public Dictionary<int, List<int>> BoneSelectedPoints = new Dictionary<int, List<int>>();
        Dictionary<string, Mesh> BonesMesh;
        Model model1;
        string foot;

       public List<Color> colors = new List<Color>() { Color.Blue, Color.Red, Color.Black };

        public LoadBoneStructure() { }

        public LoadBoneStructure(Dictionary<string, Mesh> bones, string Foot, Model viewport)
        {
            BonesMesh = bones;
            model1 = viewport;
            foot = Foot;
        }

        public void LoadBones(StructureTypes type)
        {
            int pointsNeeded = CheckType(type);

            List<string> pointIDList = new List<string>();
            var NoBones = BonesMesh.Count;

            string pntPath = Application.StartupPath + "\\RegistrationAlgorithm\\Bones\\" + foot + "\\" + type + ".txt";
            var fileExist = System.IO.File.Exists(pntPath);

            if (fileExist)
            {
                pointIDList = ReadBonePoints(pntPath);
            }

            for (int i = 1; i <= NoBones; i++)
            {
                List<Point3D> pointList = new List<Point3D>();

                if (!fileExist)
                {
                    List<int> idList = new List<int>();

                    for (int a = 0; a < pointsNeeded; a++)
                    {
                        idList.Add(a);
                        var pnt = BonesMesh["bones" + i].Vertices[a];
                        pointList.Add(new PointRGB(pnt.X, pnt.Y, pnt.Z, colors[a]));
                    }
                    BoneSelectedPoints[i] = idList;
                }
                else
                {
                    try
                    {
                        var ln = pointIDList.FirstOrDefault(l => l.StartsWith("bones" + i + " "));
                        var splitedLine = ln.Split(' ');
                        splitedLine = splitedLine[1].Split(',');
                        var idList = new List<int>();

                        var pcntr = 0;
                        foreach (var item in splitedLine)
                        {
                            if (item == "")
                                continue;

                            idList.Add(int.Parse(item));
                            var pnt = BonesMesh["bones" + i].Vertices[int.Parse(item)];
                            pointList.Add(new PointRGB(pnt.X, pnt.Y, pnt.Z, colors[pcntr]));

                            pcntr++;
                        }
                        BoneSelectedPoints[i] = idList;
                    }
                    catch (Exception e)
                    {
                        BoneSelectedPoints[i] = new List<int>();
                    }
                }

                var ent = new PointCloud(pointList, 10);

                //if (type == "arrows")
                //{
                //    ent.Vertices[0] = new PointRGB(ent.Vertices[0].X, ent.Vertices[0].Y, ent.Vertices[0].Z, Color.Blue);
                //    ent.Vertices[1] = new PointRGB(ent.Vertices[1].X, ent.Vertices[1].Y, ent.Vertices[1].Z, Color.Red);
                //}

                ent.EntityData = "bones" + i;
                model1.Entities.Add(ent, foot + type.ToString() + "Points");
            }

            if (!fileExist)
                WriteBonePoints(pntPath, BoneSelectedPoints);
        }

        private int CheckType(StructureTypes type)
        {
            int pointsNeeded = 2;

            switch (type)
            {
                case StructureTypes.Arrows:
                    pointsNeeded = 2;
                    break;
                case StructureTypes.Tetra:
                    pointsNeeded = 1;
                    break;
            }
            return pointsNeeded;
        }

        List<string> ReadBonePoints(string path)
        {
            List<string> linesList = new List<string>();

            using (System.IO.StreamReader pntReadr = new System.IO.StreamReader(path))
            {
                var line = pntReadr.ReadLine();
                while (line != null)
                {
                    linesList.Add(line);
                    line = pntReadr.ReadLine();
                }
            }
            return linesList;
        }

        public void WriteBonePoints(string path, Dictionary<int, List<int>> values)
        {
            System.IO.StreamWriter pwrtr = new System.IO.StreamWriter(path);
            foreach (var item in values)
            {
                var line = "bones" + item.Key + " ";

                foreach (var id in item.Value)
                {
                    line += id + ",";
                }
                pwrtr.WriteLine(line);
            }
            pwrtr.Close();
        }

        public void ReadXML(string FilePath)
        {
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(LoadBoneStructure));
            System.IO.StreamReader file = new System.IO.StreamReader(FilePath);
            var temp = (LoadBoneStructure)reader.Deserialize(file);
            file.Close();

            this.BoneSelectedPoints = temp.BoneSelectedPoints;
        }

        public void WriteXML(string FilePath)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(LoadBoneStructure));
            System.IO.FileStream file = System.IO.File.Create(FilePath);
            writer.Serialize(file, this);
            file.Close();
        }


    }
}
