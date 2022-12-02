using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisClientNorm
{
    public class Score
    {
        public string Name { get; set; }
        public string FieldSize { get; set; }
        public string ScoreNum { get; set; }
        public string Time { get; set; }

        public Score(string name, string fieldSize, string scoreNum, string time)
        {
            Name = name;
            FieldSize = fieldSize;
            ScoreNum = scoreNum;
            Time = time;
        }

    }
}
