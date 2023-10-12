using System;
using System.Collections.Generic;
using System.Text;

namespace Gs.Application.Models
{
    public class TeamLevel
    {
        //  {
        //  "Name": "环保达人",
        //  "AuthCount": 10,
        //  "TeamCount": 300,
        //  "LianMenTeamCount": 0,
        //  "Rate": 0.1
        //}
        public string Name { get; set; }
        public int AuthCount { get; set; }
        public int TeamCount { get; set; }
        public int LianMenTeamCount { get; set; }
        public int TeamStart { get; set; }
        public decimal Rate { get; set; }

    }
}
