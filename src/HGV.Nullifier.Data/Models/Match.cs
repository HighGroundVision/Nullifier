﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HGV.Nullifier.Data.Models
{
    public class Match
    {
        [Key]
        public int id { get; set; }

        [Index]
        public long match_id { get; set; }

        [Index]
        public long match_number { get; set; }

        public int league_id { get; set; }
        public double duration { get; set; }
        public DateTime date { get; set; }
        public int day_of_week { get; set; }
        public int hour_of_day { get; set; }
        public int cluster { get; set; }
        public int region { get; set; }
        public int victory_dire { get; set; }
        public int victory_radiant { get; set; }
        public bool valid { get; set; }

    }
}
