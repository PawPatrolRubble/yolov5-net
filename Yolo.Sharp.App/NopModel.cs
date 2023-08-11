using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yolov5Net.Scorer;
using Yolov5Net.Scorer.Models.Abstract;

namespace Yolo.Sharp.App
{
    public class NopModel : YoloModel
    {
        public override int Width { get; set; } = 320;
        public override int Height { get; set; } = 320;
        public override int Depth { get; set; } = 3;

        public override int Dimensions { get; set; } = 85;

        public override int[] Strides { get; set; } = new int[] { 8, 16, 32 };

        public override int[][][] Anchors { get; set; } = new int[][][]
        {
            new int[][] { new int[] { 010, 13 }, new int[] { 016, 030 }, new int[] { 033, 023 } },
            new int[][] { new int[] { 030, 61 }, new int[] { 062, 045 }, new int[] { 059, 119 } },
            new int[][] { new int[] { 116, 90 }, new int[] { 156, 198 }, new int[] { 373, 326 } }
        };

        public override int[] Shapes { get; set; } = new int[] { 80, 40, 20 };

        public override float Confidence { get; set; } = 0.20f;
        public override float MulConfidence { get; set; } = 0.25f;
        public override float Overlap { get; set; } = 0.45f;

        public override string[] Outputs { get; set; } = new[] { "output" };

        public override List<YoloLabel> Labels { get; set; } 
        public override bool UseDetect { get; set; } = true;
        public override void LoadLabelFromTxtFile(string txtFile)
        {
            if (!File.Exists(txtFile))
            {
                throw new FileNotFoundException(txtFile);
            }

            var labels = File.ReadLines(txtFile).Select((line, index) => new YoloLabel { Id = index, Name = line });
            Labels.AddRange(labels);
            Dimensions = this.Labels.Count + 5;
        }

        public NopModel()
        {
            Labels = new List<YoloLabel>();
        }

    }
}
