using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yolov5Net.Scorer.Models.Abstract
{
    /// <summary>
    /// Model descriptor.
    /// </summary>
    public abstract class YoloModel
    {
        public abstract int Width { get; set; }
        public abstract int Height { get; set; }
        public abstract int Depth { get; set; }

        public abstract int Dimensions { get; set; }

        public abstract int[] Strides { get; set; }
        public abstract int[][][] Anchors { get; set; }
        public abstract int[] Shapes { get; set; }

        public abstract float Confidence { get; set; }
        public abstract float MulConfidence { get; set; }
        public abstract float Overlap { get; set; }

        public abstract string[] Outputs { get; set; }
        public abstract List<YoloLabel> Labels { get; set; }
        public abstract bool UseDetect { get; set; }

        public virtual void LoadLabelFromTxtFile(string txtFile)
        {
            if (!File.Exists(txtFile))
            {
                throw new FileNotFoundException(txtFile);
            }

            var labels = File.ReadLines(txtFile).Select((line, index) => new YoloLabel { Id = index, Name = line });
            Labels.AddRange(labels);
            Dimensions = this.Labels.Count + 5;
        }

    }
}
